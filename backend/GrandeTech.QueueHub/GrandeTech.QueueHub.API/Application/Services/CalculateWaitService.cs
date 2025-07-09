using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Grande.Fila.API.Domain.Locations;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.Staff;
using Grande.Fila.API.Application.Services.Cache;

namespace Grande.Fila.API.Application.Services
{
    public class CalculateWaitService
    {
        private readonly ILocationRepository _locationRepository;
        private readonly IQueueRepository _queueRepository;
        private readonly IStaffMemberRepository _staffMemberRepository;
        private readonly IAverageWaitTimeCache _cache;
        private readonly ILogger<CalculateWaitService> _logger;

        public CalculateWaitService(
            ILocationRepository locationRepository,
            IQueueRepository queueRepository,
            IStaffMemberRepository staffMemberRepository,
            IAverageWaitTimeCache cache,
            ILogger<CalculateWaitService> logger)
        {
            _locationRepository = locationRepository;
            _queueRepository = queueRepository;
            _staffMemberRepository = staffMemberRepository;
            _cache = cache;
            _logger = logger;
        }

        public async Task<CalculateWaitResult> ExecuteAsync(CalculateWaitRequest request, string updatedBy, CancellationToken cancellationToken = default)
        {
            var result = new CalculateWaitResult { Success = false };

            // Validate LocationId
            if (!Guid.TryParse(request.LocationId, out var locationId))
            {
                result.FieldErrors["LocationId"] = "Invalid GUID format";
                return result;
            }

            // Validate QueueId
            if (!Guid.TryParse(request.QueueId, out var queueId))
            {
                result.FieldErrors["QueueId"] = "Invalid GUID format";
                return result;
            }

            // Validate EntryId
            if (!Guid.TryParse(request.EntryId, out var entryId))
            {
                result.FieldErrors["EntryId"] = "Invalid GUID format";
                return result;
            }

            try
            {
                // Get location to check if it exists
                var location = await _locationRepository.GetByIdAsync(locationId, cancellationToken);
                if (location == null)
                {
                    result.Errors.Add("Location not found");
                    return result;
                }

                // Get queue
                var queue = await _queueRepository.GetByIdAsync(queueId, cancellationToken);
                if (queue == null)
                {
                    result.Errors.Add("Queue not found");
                    return result;
                }

                // Find entry in queue
                var entry = queue.Entries.FirstOrDefault(e => e.Id == entryId);
                if (entry == null)
                {
                    result.Errors.Add("Queue entry not found");
                    return result;
                }

                // Get average service time from cache or use location default
                var averageTimeMinutes = location.AverageServiceTimeInMinutes;
                if (_cache.TryGetAverage(locationId, out var cachedAverage))
                {
                    averageTimeMinutes = cachedAverage;
                }

                // Get active staff count
                var staffMembers = await _staffMemberRepository.GetByLocationAsync(locationId, cancellationToken);
                var activeStaffCount = staffMembers.Count(s => s.IsActive && !s.IsOnBreak());

                if (activeStaffCount == 0)
                {
                    result.EstimatedWaitMinutes = -1; // No staff available
                }
                else
                {
                    // Count customers ahead in the queue
                    var customersAhead = queue.Entries
                        .Where(e => e.Status == QueueEntryStatus.Waiting || e.Status == QueueEntryStatus.Called)
                        .Where(e => e.Position < entry.Position)
                        .Count();

                    // Calculate estimated wait time
                    result.EstimatedWaitMinutes = (int)Math.Ceiling((customersAhead / (double)activeStaffCount) * averageTimeMinutes);
                }

                result.Success = true;
                _logger.LogInformation("Calculated wait time {Minutes} minutes for entry {EntryId}", result.EstimatedWaitMinutes, entryId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating wait time for entry {EntryId}", entryId);
                result.Errors.Add("Unable to calculate wait time");
            }

            return result;
        }
    }
} 