using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.Locations;

namespace Grande.Fila.API.Application.Kiosk
{
    public class KioskDisplayService
    {
        private readonly IQueueRepository _queueRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly ILogger<KioskDisplayService> _logger;

        public KioskDisplayService(
            IQueueRepository queueRepository,
            ILocationRepository locationRepository,
            ILogger<KioskDisplayService> logger)
        {
            _queueRepository = queueRepository;
            _locationRepository = locationRepository;
            _logger = logger;
        }

        public async Task<KioskDisplayResult> ExecuteAsync(KioskDisplayRequest request, string updatedBy, CancellationToken cancellationToken = default)
        {
            var result = new KioskDisplayResult { Success = false };

            // Validate LocationId
            if (!Guid.TryParse(request.LocationId, out var locationId))
            {
                result.FieldErrors["LocationId"] = "Invalid GUID format";
                return result;
            }

            try
            {
                // Check location exists
                var locationExists = await _locationRepository.ExistsAsync(l => l.Id == locationId, cancellationToken);
                if (!locationExists)
                {
                    result.Errors.Add("Location not found");
                    return result;
                }

                // Get all queues for this location
                var queues = await _queueRepository.GetByLocationAsync(locationId, cancellationToken);
                
                // Aggregate all queue entries
                var allEntries = queues.SelectMany(q => q.Entries).ToList();

                // Filter active entries (waiting, called, checked-in)
                var activeEntries = allEntries.Where(e => 
                    e.Status == QueueEntryStatus.Waiting || 
                    e.Status == QueueEntryStatus.Called || 
                    e.Status == QueueEntryStatus.CheckedIn).ToList();

                // Sort by position
                activeEntries = activeEntries.OrderBy(e => e.Position).ToList();

                // Map to DTOs
                result.QueueEntries = activeEntries.Select(e => new KioskQueueEntryDto
                {
                    Id = e.Id.ToString(),
                    CustomerName = e.CustomerName,
                    Position = e.Position,
                    Status = e.Status.ToString(),
                    TokenNumber = e.TokenNumber,
                    EstimatedWaitTime = CalculateEstimatedWaitTime(e)
                }).ToList();

                // Find currently being served
                var currentlyServing = activeEntries.FirstOrDefault(e => e.Status == QueueEntryStatus.CheckedIn);
                result.CurrentlyServing = currentlyServing?.CustomerName;

                // Count waiting customers
                result.TotalWaiting = activeEntries.Count(e => e.Status == QueueEntryStatus.Waiting);

                result.Success = true;
                _logger.LogInformation("Retrieved kiosk display data for location {LocationId} with {Count} active entries", locationId, activeEntries.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving kiosk display data for location {LocationId}", locationId);
                result.Errors.Add("Unable to retrieve queue display data");
            }

            return result;
        }

        private string CalculateEstimatedWaitTime(QueueEntry entry)
        {
            // Simple calculation - could be enhanced with real-time data
            var waitMinutes = entry.Position * 15; // Assume 15 minutes per person
            
            if (waitMinutes < 60)
                return $"{waitMinutes} min";
            
            var hours = waitMinutes / 60;
            var minutes = waitMinutes % 60;
            return minutes > 0 ? $"{hours}h {minutes}m" : $"{hours}h";
        }
    }
} 