using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Grande.Fila.API.Domain.Locations;
using Grande.Fila.API.Domain.Organizations;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.ServicesOffered;

namespace Grande.Fila.API.Application.Public;

public class GetPublicSalonsService
{
    private readonly ILocationRepository _locationRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IQueueRepository _queueRepository;
    private readonly IServicesOfferedRepository _serviceRepository;
    private readonly ILogger<GetPublicSalonsService> _logger;

    public GetPublicSalonsService(
        ILocationRepository locationRepository,
        IOrganizationRepository organizationRepository,
        IQueueRepository queueRepository,
        IServicesOfferedRepository serviceRepository,
        ILogger<GetPublicSalonsService> logger)
    {
        _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
        _organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
        _queueRepository = queueRepository ?? throw new ArgumentNullException(nameof(queueRepository));
        _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetPublicSalonsResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving all active salons for public access");

            // Get all active locations
            var locations = await _locationRepository.GetActiveLocationsAsync(cancellationToken);
            
            var salons = new List<PublicSalonDto>();

            foreach (var location in locations)
            {
                var salon = await MapToPublicSalonDto(location, cancellationToken);
                salons.Add(salon);
            }

            _logger.LogInformation("Successfully retrieved {SalonCount} salons", salons.Count);

            return new GetPublicSalonsResult
            {
                Success = true,
                Salons = salons
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving salons");
            return new GetPublicSalonsResult
            {
                Success = false,
                Errors = { "An unexpected error occurred while retrieving salons" }
            };
        }
    }

    private async Task<PublicSalonDto> MapToPublicSalonDto(Location location, CancellationToken cancellationToken)
    {
        // Get today's queue for this location
        var queue = await GetTodaysQueueForLocation(location.Id, cancellationToken);
        
        // Get active services for this location
        var services = await _serviceRepository.GetActiveServiceTypesAsync(location.Id, cancellationToken);

        // Calculate queue metrics
        var queueLength = queue?.Entries?.Count(e => e.Status == QueueEntryStatus.Waiting || e.Status == QueueEntryStatus.Called) ?? 0;
        var currentWaitTime = CalculateWaitTime(queueLength, location.AverageServiceTimeInMinutes, 1); // Assume 1 staff member for now
        
        return new PublicSalonDto
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
    }

    private async Task<Queue?> GetTodaysQueueForLocation(Guid locationId, CancellationToken cancellationToken)
    {
        try
        {
            // Try to get today's queue - using the method from the interface
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

    private static Dictionary<string, string> GetBusinessHoursDto(Location location)
    {
        // Use the new weekly hours from the domain model
        return location.GetBusinessHoursDictionary();
    }
}