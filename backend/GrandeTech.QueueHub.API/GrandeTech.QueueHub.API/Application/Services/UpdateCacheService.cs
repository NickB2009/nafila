using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Grande.Fila.API.Domain.Locations;
using Grande.Fila.API.Application.Services.Cache;

namespace Grande.Fila.API.Application.Services
{
    public class UpdateCacheService
    {
        private readonly ILocationRepository _locationRepository;
        private readonly IAverageWaitTimeCache _cache;
        private readonly ILogger<UpdateCacheService> _logger;

        public UpdateCacheService(ILocationRepository locationRepository, IAverageWaitTimeCache cache, ILogger<UpdateCacheService> logger)
        {
            _locationRepository = locationRepository;
            _cache = cache;
            _logger = logger;
        }

        public async Task<UpdateCacheResult> ExecuteAsync(UpdateCacheRequest request, string updatedBy, CancellationToken cancellationToken = default)
        {
            var result = new UpdateCacheResult { Success = false };

            // Validate LocationId
            if (!Guid.TryParse(request.LocationId, out var locationId))
            {
                result.FieldErrors["LocationId"] = "Invalid GUID format";
                return result;
            }

            // Validate average time
            if (request.AverageServiceTimeMinutes < 0)
            {
                result.FieldErrors["AverageServiceTimeMinutes"] = "Average time cannot be negative";
                return result;
            }

            // Check location exists
            var exists = await _locationRepository.ExistsAsync(l => l.Id == locationId, cancellationToken);
            if (!exists)
            {
                result.Errors.Add("Location not found");
                return result;
            }

            try
            {
                _cache.SetAverage(locationId, request.AverageServiceTimeMinutes);
                result.Success = true;
                _logger.LogInformation("Updated wait average cache for location {LocationId} to {Average}", locationId, request.AverageServiceTimeMinutes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating average wait time cache");
                result.Errors.Add("Unable to update cache");
            }

            return result;
        }
    }
} 