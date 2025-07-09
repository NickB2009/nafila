using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Domain.Queues;

namespace Grande.Fila.API.Application.Queues
{
    public class CancelQueueService
    {
        private readonly IQueueRepository _queueRepository;

        public CancelQueueService(IQueueRepository queueRepository)
        {
            _queueRepository = queueRepository ?? throw new ArgumentNullException(nameof(queueRepository));
        }

        public async Task<CancelQueueResult> CancelQueueAsync(CancelQueueRequest request, string userId, CancellationToken cancellationToken)
        {
            var result = new CancelQueueResult();

            if (string.IsNullOrWhiteSpace(request.QueueEntryId) || !Guid.TryParse(request.QueueEntryId, out var queueEntryId))
            {
                result.FieldErrors["QueueEntryId"] = "Invalid queue entry ID format.";
                return result;
            }

            var allQueues = await _queueRepository.GetAllAsync(cancellationToken);
            var queueWithEntry = allQueues.FirstOrDefault(q => q.Entries.Any(e => e.Id == queueEntryId));

            if (queueWithEntry == null)
            {
                result.Errors.Add("Queue entry not found.");
                return result;
            }

            var entryToCancel = queueWithEntry.Entries.First(e => e.Id == queueEntryId);

            if (entryToCancel.Status != QueueEntryStatus.Waiting)
            {
                result.Errors.Add("Only waiting customers can be cancelled.");
                return result;
            }

            entryToCancel.Cancel();
            await _queueRepository.UpdateAsync(queueWithEntry, cancellationToken);

            result.Success = true;
            result.QueueEntryId = entryToCancel.Id.ToString();
            result.CustomerName = entryToCancel.CustomerName;
            result.CancelledAt = entryToCancel.CancelledAt;

            return result;
        }
    }
} 