using System;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Application.Locations.Requests;
using Grande.Fila.API.Application.Locations.Results;
using Grande.Fila.API.Domain.Locations;
using Microsoft.Extensions.Logging;

namespace Grande.Fila.API.Application.Locations
{
    /// <summary>
    /// Service to handle enabling or disabling queue for a location
    /// </summary>
    public class ToggleQueueService
    {
        private readonly ILocationRepository _locationRepository;
        private readonly ILogger<ToggleQueueService> _logger;

        public ToggleQueueService(
            ILocationRepository locationRepository,
            ILogger<ToggleQueueService> logger)
        {
            _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ToggleQueueResult> ExecuteAsync(
            ToggleQueueRequest request,
            string currentUserId,
            CancellationToken cancellationToken = default)
        {
            var result = new ToggleQueueResult();

            try
            {
                // Validate request
                if (!Guid.TryParse(request.LocationId, out var locationId))
                {
                    result.FieldErrors["LocationId"] = "Invalid location ID format";
                    return result;
                }

                // Get the location
                var location = await _locationRepository.GetByIdAsync(locationId, cancellationToken);
                if (location == null)
                {
                    result.Errors.Add("Location not found");
                    return result;
                }

                // Enable or disable queue
                if (request.EnableQueue)
                {
                    location.EnableQueue(currentUserId);
                    _logger.LogInformation(
                        "Queue enabled for location {LocationId} by user {UserId}",
                        locationId,
                        currentUserId
                    );
                }
                else
                {
                    location.DisableQueue(currentUserId);
                    _logger.LogInformation(
                        "Queue disabled for location {LocationId} by user {UserId}",
                        locationId,
                        currentUserId
                    );
                }

                // Save changes
                await _locationRepository.UpdateAsync(location, cancellationToken);

                // Return success result
                result.Success = true;
                result.LocationId = location.Id.ToString();
                result.IsQueueEnabled = location.IsQueueEnabled;

                return result;
            }
            catch (Exception ex)
            {
                result.Errors.Add("An unexpected error occurred while updating queue status");
                _logger.LogError(ex, "Error toggling queue status for location {LocationId}", request.LocationId);
                return result;
            }
        }
    }
}