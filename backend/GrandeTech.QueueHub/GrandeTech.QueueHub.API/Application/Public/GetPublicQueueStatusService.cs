using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Grande.Fila.API.Domain.Locations;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.ServicesOffered;

namespace Grande.Fila.API.Application.Public;

public class GetPublicQueueStatusService
{
    private readonly ILocationRepository _locationRepository;
    private readonly IQueueRepository _queueRepository;
    private readonly IServicesOfferedRepository _serviceRepository;
    private readonly ILogger<GetPublicQueueStatusService> _logger;

    public GetPublicQueueStatusService(
        ILocationRepository locationRepository,
        IQueueRepository queueRepository,
        IServicesOfferedRepository serviceRepository,
        ILogger<GetPublicQueueStatusService> logger)
    {
        _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
        _queueRepository = queueRepository ?? throw new ArgumentNullException(nameof(queueRepository));
        _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetPublicQueueStatusResult> ExecuteAsync(string salonId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate salon ID
            if (!Guid.TryParse(salonId, out var locationId))
            {
                return new GetPublicQueueStatusResult
                {
                    Success = false,
                    FieldErrors = { { "SalonId", "Invalid salon ID format" } }
                };
            }

            _logger.LogInformation("Retrieving queue status for salon {SalonId}", salonId);

            // Get the location
            var location = await _locationRepository.GetByIdAsync(locationId, cancellationToken);
            if (location == null)
            {
                return new GetPublicQueueStatusResult
                {
                    Success = false,
                    Errors = { "Salon not found" }
                };
            }

            // Get today's queue
            var queue = await GetTodaysQueueForLocation(locationId, cancellationToken);
            
            // Get active services
            var services = await _serviceRepository.GetActiveServiceTypesAsync(locationId, cancellationToken);

            // Calculate queue metrics
            var queueLength = queue?.Entries?.Count(e => e.Status == QueueEntryStatus.Waiting || e.Status == QueueEntryStatus.Called) ?? 0;
            var estimatedWaitTime = CalculateWaitTime(queueLength, location.AverageServiceTimeInMinutes, 1); // Assume 1 staff member

            var queueStatus = new PublicQueueStatusDto
            {
                SalonId = location.Id.ToString(),
                SalonName = location.Name,
                QueueLength = queueLength,
                EstimatedWaitTimeMinutes = estimatedWaitTime,
                IsAcceptingCustomers = location.CanAcceptQueueEntries(),
                LastUpdated = DateTime.UtcNow,
                AvailableServices = services.Select(s => s.Name).ToList()
            };

            _logger.LogInformation("Successfully retrieved queue status for salon {SalonId}", salonId);

            return new GetPublicQueueStatusResult
            {
                Success = true,
                QueueStatus = queueStatus
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving queue status for salon {SalonId}", salonId);
            return new GetPublicQueueStatusResult
            {
                Success = false,
                Errors = { "An unexpected error occurred while retrieving queue status" }
            };
        }
    }

    private async Task<Queue?> GetTodaysQueueForLocation(Guid locationId, CancellationToken cancellationToken)
    {
        try
        {
            var queue = await _queueRepository.GetByLocationIdAsync(locationId, cancellationToken);
            
            // Check if the queue is for today
            if (queue?.QueueDate.Date == DateTime.UtcNow.Date)
            {
                return queue;
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not retrieve queue for location {LocationId}", locationId);
            return null;
        }
    }

    private static int CalculateWaitTime(int queueLength, double averageServiceTime, int activeStaff)
    {
        if (queueLength == 0) return 0;
        
        var effectiveStaff = Math.Max(1, activeStaff);
        return (int)Math.Ceiling(queueLength / (double)effectiveStaff * averageServiceTime);
    }
}