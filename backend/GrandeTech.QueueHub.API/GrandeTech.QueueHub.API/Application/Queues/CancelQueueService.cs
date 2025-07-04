using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Domain.Queues;

namespace Grande.Fila.API.Application.Queues
{
    /// <summary>
    /// Service for handling UC-CANCEL: Client cancels queue spot use case
    /// </summary>
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

            try
            {
                // Validate request
                if (string.IsNullOrWhiteSpace(request.QueueEntryId))
                {
                    result.Success = false;
                    result.FieldErrors.Add("QueueEntryId", "Queue entry ID is required.");
                    return result;
                }

                if (!Guid.TryParse(request.QueueEntryId, out var queueEntryId))
                {
                    result.Success = false;
                    result.FieldErrors.Add("QueueEntryId", "Invalid queue entry ID format.");
                    return result;
                }

                // Find the queue entry
                var queueEntry = await _queueRepository.GetQueueEntryById(queueEntryId, cancellationToken);
                if (queueEntry == null)
                {
                    result.Success = false;
                    result.Errors.Add("Queue entry not found.");
                    return result;
                }

                // Cancel the queue entry using domain logic
                queueEntry.Cancel();

                // Update queue entry in repository
                _queueRepository.UpdateQueueEntry(queueEntry);

                // Return success result
                result.Success = true;
                result.QueueEntryId = queueEntry.Id.ToString();
                result.CustomerName = queueEntry.CustomerName;
                result.CancelledAt = queueEntry.CancelledAt;

                return result;
            }
            catch (InvalidOperationException ex)
            {
                result.Success = false;
                result.Errors.Add(ex.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                result.Success = false;
                result.FieldErrors.Add("QueueEntryId", ex.Message);
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"An error occurred while canceling queue entry: {ex.Message}");
                return result;
            }
        }
    }
} 