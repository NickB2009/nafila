using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.Locations;
using Grande.Fila.API.Domain.Staff;
using Grande.Fila.API.Application.Services.Cache;
using System.Collections.Generic; // Added for List<QueueEntry>

namespace Grande.Fila.API.Application.Kiosk
{
    public class KioskDisplayService
    {
        private readonly IQueueRepository _queueRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly IStaffMemberRepository _staffMemberRepository;
        private readonly IAverageWaitTimeCache _cache;
        private readonly ILogger<KioskDisplayService> _logger;

        public KioskDisplayService(
            IQueueRepository queueRepository,
            ILocationRepository locationRepository,
            IStaffMemberRepository staffMemberRepository,
            IAverageWaitTimeCache cache,
            ILogger<KioskDisplayService> logger)
        {
            _queueRepository = queueRepository;
            _locationRepository = locationRepository;
            _staffMemberRepository = staffMemberRepository;
            _cache = cache;
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
                // Get location
                var location = await _locationRepository.GetByIdAsync(locationId, cancellationToken);
                if (location == null)
                {
                    result.Errors.Add("Location not found");
                    return result;
                }

                // Get all queues for this location
                var queues = await _queueRepository.GetByLocationAsync(locationId, cancellationToken);
                
                // Get staff members for wait time calculation
                var staffMembers = await _staffMemberRepository.GetByLocationAsync(locationId, cancellationToken);
                var activeStaffCount = staffMembers.Count(s => s.IsActive && !s.IsOnBreak());

                // Get average service time from cache or use location default
                var averageTimeMinutes = location.AverageServiceTimeInMinutes;
                if (_cache.TryGetAverage(locationId, out var cachedAverage))
                {
                    averageTimeMinutes = cachedAverage;
                }

                // Set location information
                result.LocationId = locationId.ToString();
                result.LocationName = location.Name;
                result.ActiveStaffCount = activeStaffCount;
                result.AverageServiceTimeMinutes = averageTimeMinutes;

                // Aggregate all queue entries
                var allEntries = queues.SelectMany(q => q.Entries).ToList();

                // Filter entries based on request
                var activeEntries = allEntries.Where(e => 
                    e.Status == QueueEntryStatus.Waiting || 
                    e.Status == QueueEntryStatus.Called || 
                    e.Status == QueueEntryStatus.CheckedIn ||
                    (request.IncludeCompletedEntries && e.Status == QueueEntryStatus.Completed)).ToList();

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
                    EstimatedWaitMinutes = CalculateEstimatedWaitMinutes(e, activeEntries, activeStaffCount, averageTimeMinutes),
                    EstimatedWaitTime = FormatEstimatedWaitTime(CalculateEstimatedWaitMinutes(e, activeEntries, activeStaffCount, averageTimeMinutes)),
                    JoinedAt = e.EnteredAt,
                    CalledAt = e.CalledAt,
                    CheckedInAt = e.CheckedInAt,
                    ServiceType = e.ServiceTypeId?.ToString(), // Convert GUID to string
                    StaffMemberName = e.StaffMemberId.HasValue ? 
                        staffMembers.FirstOrDefault(s => s.Id == e.StaffMemberId)?.Name : null
                }).ToList();

                // Find currently being served
                var currentlyServing = activeEntries.FirstOrDefault(e => e.Status == QueueEntryStatus.CheckedIn);
                result.CurrentlyServing = currentlyServing?.CustomerName;
                result.CurrentPosition = currentlyServing?.Position;

                // Count customers by status
                result.TotalWaiting = activeEntries.Count(e => e.Status == QueueEntryStatus.Waiting);
                result.TotalActive = activeEntries.Count(e => 
                    e.Status == QueueEntryStatus.Waiting || 
                    e.Status == QueueEntryStatus.Called || 
                    e.Status == QueueEntryStatus.CheckedIn);

                result.Success = true;
                _logger.LogInformation("Retrieved kiosk display data for location {LocationId} with {Count} active entries, {Waiting} waiting", 
                    locationId, activeEntries.Count, result.TotalWaiting);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving kiosk display data for location {LocationId}", locationId);
                result.Errors.Add("Unable to retrieve queue display data");
            }

            return result;
        }

        private int CalculateEstimatedWaitMinutes(QueueEntry entry, List<QueueEntry> allActiveEntries, int activeStaffCount, double averageTimeMinutes)
        {
            // If no staff available, return -1 (consistent with CalculateWaitService)
            if (activeStaffCount == 0)
                return -1;

            // Count customers ahead in the queue (same logic as CalculateWaitService)
            var customersAhead = allActiveEntries
                .Where(e => e.Status == QueueEntryStatus.Waiting || e.Status == QueueEntryStatus.Called)
                .Where(e => e.Position < entry.Position)
                .Count();

            // Calculate estimated wait time (same logic as CalculateWaitService)
            return (int)Math.Ceiling((customersAhead / (double)activeStaffCount) * averageTimeMinutes);
        }

        private string FormatEstimatedWaitTime(int waitMinutes)
        {
            // Handle special cases
            if (waitMinutes < 0)
                return "N/A";
            
            if (waitMinutes == 0)
                return "0 min";
            
            // Format as before for backward compatibility
            if (waitMinutes < 60)
                return $"{waitMinutes} min";
            
            var hours = waitMinutes / 60;
            var minutes = waitMinutes % 60;
            return minutes > 0 ? $"{hours}h {minutes}m" : $"{hours}h";
        }
    }
} 