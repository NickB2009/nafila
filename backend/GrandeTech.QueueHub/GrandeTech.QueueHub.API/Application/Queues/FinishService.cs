using System;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Domain.Queues;

namespace Grande.Fila.API.Application.Queues
{
    /// <summary>
    /// Service for handling UC-FINISH: Complete service use case
    /// Implements TDD-driven business logic for completing services
    /// </summary>
    public class FinishService
    {
        private readonly IQueueRepository _queueRepository;

        public FinishService(IQueueRepository queueRepository)
        {
            _queueRepository = queueRepository;
        }

        /// <summary>
        /// Completes a service for a checked-in customer
        /// </summary>
        public async Task<FinishResult> FinishAsync(FinishRequest request, string userId, CancellationToken cancellationToken = default)
        {
            var result = new FinishResult();

            // Input validation
            if (string.IsNullOrWhiteSpace(request.QueueEntryId))
            {
                result.FieldErrors["QueueEntryId"] = "Queue entry ID is required.";
            }
            else if (!Guid.TryParse(request.QueueEntryId, out var queueEntryId))
            {
                result.FieldErrors["QueueEntryId"] = "Invalid queue entry ID format.";
            }

            if (request.ServiceDurationMinutes <= 0)
            {
                result.FieldErrors["ServiceDurationMinutes"] = "Service duration must be greater than 0 minutes.";
            }

            if (result.FieldErrors.Count > 0)
                return result;

            // Parse the queue entry ID since validation passed
            var parsedQueueEntryId = Guid.Parse(request.QueueEntryId);

            try
            {
                // Get the queue entry
                var queueEntry = await _queueRepository.GetQueueEntryById(parsedQueueEntryId, cancellationToken);
                if (queueEntry == null)
                {
                    result.Errors.Add("Queue entry not found.");
                    return result;
                }

                // Validate status
                if (queueEntry.Status != QueueEntryStatus.CheckedIn)
                {
                    result.Errors.Add("Customer is not checked in.");
                    return result;
                }

                // Complete the service
                queueEntry.Complete(request.ServiceDurationMinutes);
                
                // Update the queue entry in the repository
                _queueRepository.UpdateQueueEntry(queueEntry);

                // Populate the result with success data
                result.Success = true;
                result.QueueEntryId = queueEntry.Id.ToString();
                result.CustomerName = queueEntry.CustomerName;
                result.ServiceDurationMinutes = request.ServiceDurationMinutes;
                result.CompletedAt = queueEntry.CompletedAt;
                result.Notes = request.Notes;

                return result;
            }
            catch (InvalidOperationException ex)
            {
                result.Errors.Add(ex.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                result.Errors.Add(ex.Message);
                return result;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"An error occurred while completing service: {ex.Message}");
                return result;
            }
        }
    }
} 