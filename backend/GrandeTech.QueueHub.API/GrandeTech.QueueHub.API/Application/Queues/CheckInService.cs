using System;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Domain.Queues;

namespace Grande.Fila.API.Application.Queues
{
    /// <summary>
    /// Service for handling UC-CHECKIN: Check-in client use case
    /// Implements TDD-driven business logic for checking in customers
    /// </summary>
    public class CheckInService
    {
        private readonly IQueueRepository _queueRepository;

        public CheckInService(IQueueRepository queueRepository)
        {
            _queueRepository = queueRepository;
        }

        /// <summary>
        /// Checks in a customer who has been called
        /// </summary>
        public async Task<CheckInResult> CheckInAsync(CheckInRequest request, string userId, CancellationToken cancellationToken = default)
        {
            var result = new CheckInResult();

            // Input validation
            if (string.IsNullOrWhiteSpace(request.QueueEntryId))
            {
                result.FieldErrors["QueueEntryId"] = "Queue entry ID is required.";
            }
            else if (!Guid.TryParse(request.QueueEntryId, out var queueEntryId))
            {
                result.FieldErrors["QueueEntryId"] = "Invalid queue entry ID format.";
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

                // Validate status - customer must be called to be checked in
                if (queueEntry.Status != QueueEntryStatus.Called)
                {
                    result.Errors.Add("Customer must be called before they can be checked in.");
                    return result;
                }

                // Check in the customer
                queueEntry.CheckIn();
                
                // Update the queue entry in the repository
                _queueRepository.UpdateQueueEntry(queueEntry);

                // Populate the result with success data
                result.Success = true;
                result.QueueEntryId = queueEntry.Id.ToString();
                result.CustomerName = queueEntry.CustomerName;
                result.Position = queueEntry.Position;
                result.CheckedInAt = queueEntry.CheckedInAt;

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
                result.Errors.Add($"An error occurred while checking in customer: {ex.Message}");
                return result;
            }
        }
    }
} 