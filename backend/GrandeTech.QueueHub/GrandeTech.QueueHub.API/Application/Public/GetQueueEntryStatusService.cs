using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Application.Common;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Infrastructure.Repositories;

namespace Grande.Fila.API.Application.Public
{
    public class GetQueueEntryStatusService
    {
        private readonly IQueueEntryRepository _queueEntryRepository;
        private readonly IQueueRepository _queueRepository;

        public GetQueueEntryStatusService(
            IQueueEntryRepository queueEntryRepository,
            IQueueRepository queueRepository)
        {
            _queueEntryRepository = queueEntryRepository ?? throw new ArgumentNullException(nameof(queueEntryRepository));
            _queueRepository = queueRepository ?? throw new ArgumentNullException(nameof(queueRepository));
        }

        public async Task<GetQueueEntryStatusResult> ExecuteAsync(string entryId, CancellationToken cancellationToken = default)
        {
            var result = new GetQueueEntryStatusResult();

            try
            {
                if (string.IsNullOrWhiteSpace(entryId))
                {
                    result.Errors.Add("Entry ID is required");
                    return result;
                }

                if (!Guid.TryParse(entryId, out var queueEntryGuid))
                {
                    result.Errors.Add("Invalid entry ID format");
                    return result;
                }

                var queueEntry = await _queueEntryRepository.GetByIdAsync(queueEntryGuid, cancellationToken);
                if (queueEntry == null)
                {
                    result.Errors.Add("Queue entry not found");
                    return result;
                }

                var queue = await _queueRepository.GetByIdAsync(queueEntry.QueueId, cancellationToken);
                if (queue == null)
                {
                    result.Errors.Add("Queue not found");
                    return result;
                }

                // Calculate position in queue
                var allEntries = await _queueEntryRepository.GetByQueueIdAsync(queueEntry.QueueId, cancellationToken);
                var activeEntries = allEntries
                    .Where(e => e.Status == QueueEntryStatus.Waiting)
                    .OrderBy(e => e.JoinedAt)
                    .ToList();

                var position = activeEntries.FindIndex(e => e.Id == queueEntry.Id) + 1;

                // Calculate estimated wait time
                var estimatedWaitMinutes = queue.CalculateEstimatedWaitTime(position);

                result.QueueEntryStatus = new QueueEntryStatusDto
                {
                    Id = queueEntry.Id.ToString(),
                    Position = position,
                    EstimatedWaitMinutes = estimatedWaitMinutes,
                    Status = queueEntry.Status.ToString().ToLowerInvariant(),
                    JoinedAt = queueEntry.JoinedAt,
                    ServiceRequested = queueEntry.ServiceTypeId?.ToString() ?? "General"
                };

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"An error occurred while retrieving queue entry status: {ex.Message}");
            }

            return result;
        }
    }

    public class GetQueueEntryStatusResult : BaseResult
    {
        public QueueEntryStatusDto? QueueEntryStatus { get; set; }
    }

    public class QueueEntryStatusDto
    {
        public string? Id { get; set; }
        public int Position { get; set; }
        public int EstimatedWaitMinutes { get; set; }
        public string? Status { get; set; }
        public DateTime? JoinedAt { get; set; }
        public string? ServiceRequested { get; set; }
    }
}

