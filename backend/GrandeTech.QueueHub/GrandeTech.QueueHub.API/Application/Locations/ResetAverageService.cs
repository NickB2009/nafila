using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using Grande.Fila.API.Domain.Locations;

namespace Grande.Fila.API.Application.Locations
{
    public class ResetAverageService
    {
        private readonly ILocationRepository _locationRepository;
        private readonly ILogger<ResetAverageService> _logger;
        private static readonly TimeSpan ResetThreshold = TimeSpan.FromDays(90);

        public ResetAverageService(ILocationRepository locationRepository, ILogger<ResetAverageService> logger)
        {
            _locationRepository = locationRepository;
            _logger = logger;
        }

        public async Task<ResetAverageResult> ExecuteAsync(ResetAverageRequest request, string updatedBy, CancellationToken cancellationToken = default)
        {
            var result = new ResetAverageResult { Success = false, ResetCount = 0 };

            try
            {
                var allLocations = await _locationRepository.GetAllAsync(cancellationToken);
                var now = DateTime.UtcNow;
                var toReset = allLocations.Where(l => (now - l.LastAverageTimeReset) >= ResetThreshold).ToList();

                foreach (var location in toReset)
                {
                    location.ResetAverageServiceTime(updatedBy);
                    await _locationRepository.UpdateAsync(location, cancellationToken);
                    result.ResetCount++;
                }

                result.Success = true;
                _logger.LogInformation("Reset average service time for {Count} locations", result.ResetCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting average service time");
                result.Errors.Add("An unexpected error occurred while resetting averages.");
            }

            return result;
        }
    }
} 