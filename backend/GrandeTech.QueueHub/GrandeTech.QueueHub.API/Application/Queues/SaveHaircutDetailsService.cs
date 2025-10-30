using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.Customers;
using Microsoft.Extensions.Logging;

namespace Grande.Fila.API.Application.Queues
{
    /// <summary>
    /// Service to save haircut details when completing a service
    /// </summary>
    public class SaveHaircutDetailsService
    {
        private readonly IQueueRepository _queueRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<SaveHaircutDetailsService> _logger;

        public SaveHaircutDetailsService(
            IQueueRepository queueRepository,
            ICustomerRepository customerRepository,
            ILogger<SaveHaircutDetailsService> logger)
        {
            _queueRepository = queueRepository ?? throw new ArgumentNullException(nameof(queueRepository));
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<SaveHaircutDetailsResult> ExecuteAsync(
            SaveHaircutDetailsRequest request,
            string currentUserId,
            CancellationToken cancellationToken = default)
        {
            var result = new SaveHaircutDetailsResult();

            try
            {
                // Validate request
                if (!Guid.TryParse(request.QueueEntryId, out var queueEntryId))
                {
                    result.FieldErrors["QueueEntryId"] = "Invalid queue entry ID format";
                    return result;
                }

                if (string.IsNullOrWhiteSpace(request.HaircutDetails))
                {
                    result.FieldErrors["HaircutDetails"] = "Haircut details are required";
                    return result;
                }

                // Get all queues to find the entry
                var queues = await _queueRepository.GetAllAsync(cancellationToken);
                QueueEntry? queueEntry = null;
                Queue? queue = null;

                foreach (var q in queues)
                {
                    queueEntry = q.Entries.FirstOrDefault(e => e.Id == queueEntryId);
                    if (queueEntry != null)
                    {
                        queue = q;
                        break;
                    }
                }

                if (queueEntry == null || queue == null)
                {
                    result.Errors.Add("Queue entry not found");
                    return result;
                }

                // Verify the queue entry is completed
                if (queueEntry.Status != QueueEntryStatus.Completed)
                {
                    result.Errors.Add("Haircut details can only be saved for completed services");
                    return result;
                }

                // Get the customer
                var customer = await _customerRepository.GetByIdAsync(queueEntry.CustomerId, cancellationToken);
                if (customer == null)
                {
                    result.Errors.Add("Customer not found");
                    return result;
                }

                // Combine all notes
                var combinedNotes = $"Haircut: {request.HaircutDetails}";
                if (!string.IsNullOrWhiteSpace(request.AdditionalNotes))
                {
                    combinedNotes += $"\nAdditional Notes: {request.AdditionalNotes}";
                }
                if (!string.IsNullOrWhiteSpace(request.PhotoUrl))
                {
                    combinedNotes += $"\nPhoto: {request.PhotoUrl}";
                }

                // ServiceHistory removed during simplification
                // Service history tracking needs to be reimplemented if needed

                // Update the queue entry notes as well
                queueEntry.UpdateNotes(combinedNotes);

                // Save changes
                await _customerRepository.UpdateAsync(customer, cancellationToken);
                await _queueRepository.UpdateAsync(queue, cancellationToken);

                // Return success result
                result.Success = true;
                result.ServiceHistoryId = null; // ServiceHistory removed during simplification
                result.CustomerId = customer.Id.ToString();

                _logger.LogInformation(
                    "Haircut details saved for customer {CustomerId} by user {UserId}",
                    customer.Id,
                    currentUserId
                );

                return result;
            }
            catch (Exception ex)
            {
                result.Errors.Add("An unexpected error occurred while saving haircut details");
                _logger.LogError(ex, "Error saving haircut details for queue entry {QueueEntryId}", request.QueueEntryId);
                return result;
            }
        }
    }
}