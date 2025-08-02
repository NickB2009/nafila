using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Grande.Fila.API.Domain.Locations;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.ServicesOffered;

namespace Grande.Fila.API.Application.Public;

public class GetPublicSalonDetailService
{
    private readonly ILocationRepository _locationRepository;
    private readonly IQueueRepository _queueRepository;
    private readonly IServicesOfferedRepository _serviceRepository;
    private readonly ILogger<GetPublicSalonDetailService> _logger;

    public GetPublicSalonDetailService(
        ILocationRepository locationRepository,
        IQueueRepository queueRepository,
        IServicesOfferedRepository serviceRepository,
        ILogger<GetPublicSalonDetailService> logger)
    {
        _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
        _queueRepository = queueRepository ?? throw new ArgumentNullException(nameof(queueRepository));
        _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetPublicSalonDetailResult> ExecuteAsync(string salonId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate salon ID
            if (!Guid.TryParse(salonId, out var locationId))
            {
                return new GetPublicSalonDetailResult
                {
                    Success = false,
                    FieldErrors = { { "SalonId", "Invalid salon ID format" } }
                };
            }

            _logger.LogInformation("Retrieving detailed information for salon {SalonId}", salonId);

            // Get the location
            var location = await _locationRepository.GetByIdAsync(locationId, cancellationToken);
            if (location == null)
            {
                return new GetPublicSalonDetailResult
                {
                    Success = false,
                    Errors = { "Salon not found" }
                };
            }

            // Get today's queue
            var queue = await GetTodaysQueueForLocation(locationId, cancellationToken);
            
            // Get active services
            var services = await _serviceRepository.GetActiveServiceTypesAsync(locationId, cancellationToken);

            // Map to DTO
            var salon = await MapToPublicSalonDto(location, queue, services);

            _logger.LogInformation("Successfully retrieved detailed information for salon {SalonId}", salonId);

            return new GetPublicSalonDetailResult
            {
                Success = true,
                Salon = salon
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving salon details for {SalonId}", salonId);
            return new GetPublicSalonDetailResult
            {
                Success = false,
                Errors = { "An unexpected error occurred while retrieving salon details" }
            };
        }
    }

    private async Task<Queue?> GetTodaysQueueForLocation(Guid locationId, CancellationToken cancellationToken)
    {
        try
        {
            var queue = await _queueRepository.GetActiveQueueByLocationIdAsync(locationId, cancellationToken);
            
            // The GetActiveQueueByLocationIdAsync already filters for today's date and active status
            // so we just need to return the queue if it exists
            return queue;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not retrieve queue for location {LocationId}", locationId);
            return null;
        }
    }

    private static Task<PublicSalonDto> MapToPublicSalonDto(Location location, Queue? queue, IReadOnlyList<ServiceOffered> services)
    {
        // Calculate queue metrics
        var queueLength = queue?.Entries?.Count(e => e.Status == QueueEntryStatus.Waiting || e.Status == QueueEntryStatus.Called) ?? 0;
        var currentWaitTime = CalculateWaitTime(queueLength, location.AverageServiceTimeInMinutes, 1); // Assume 1 staff member

        var salon = new PublicSalonDto
        {
            Id = location.Id.ToString(),
            Name = location.Name,
            Address = location.Address.ToString(),
            Latitude = location.Address.Latitude ?? 0,
            Longitude = location.Address.Longitude ?? 0,
            IsOpen = location.IsOpen(),
            CurrentWaitTimeMinutes = currentWaitTime,
            QueueLength = queueLength,
            IsFast = currentWaitTime < 10,
            IsPopular = queueLength > 5, // Simple heuristic: more than 5 people waiting
            Rating = 4.5, // TODO: Implement actual rating system
            ReviewCount = 0, // TODO: Implement actual review system
            Services = services.Select(s => s.Name).ToList(),
            BusinessHours = GetBusinessHoursDto(location)
        };

        return Task.FromResult(salon);
    }

    private static int CalculateWaitTime(int queueLength, double averageServiceTime, int activeStaff)
    {
        if (queueLength == 0) return 0;
        
        var effectiveStaff = Math.Max(1, activeStaff);
        return (int)Math.Ceiling(queueLength / (double)effectiveStaff * averageServiceTime);
    }

    private static Dictionary<string, string> GetBusinessHoursDto(Location location)
    {
        var businessHours = new Dictionary<string, string>();
        
        // For now, assume same hours every day except Sunday (closed)
        var hours = $"{location.BusinessHours.Start:hh\\:mm}-{location.BusinessHours.End:hh\\:mm}";
        
        businessHours["monday"] = hours;
        businessHours["tuesday"] = hours;
        businessHours["wednesday"] = hours;
        businessHours["thursday"] = hours;
        businessHours["friday"] = hours;
        businessHours["saturday"] = hours;
        businessHours["sunday"] = "closed";
        
        return businessHours;
    }
}