using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Application.Common;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Infrastructure.Repositories;

namespace Grande.Fila.API.Application.Public
{
    public class UpdateQueueEntryService
    {
        private readonly IQueueEntryRepository _queueEntryRepository;

        public UpdateQueueEntryService(IQueueEntryRepository queueEntryRepository)
        {
            _queueEntryRepository = queueEntryRepository ?? throw new ArgumentNullException(nameof(queueEntryRepository));
        }

        public async Task<UpdateQueueEntryResult> ExecuteAsync(string entryId, UpdateQueueEntryRequest request, CancellationToken cancellationToken = default)
        {
            var result = new UpdateQueueEntryResult();

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

                if (request == null)
                {
                    result.Errors.Add("Update request is required");
                    return result;
                }

                var queueEntry = await _queueEntryRepository.GetByIdAsync(queueEntryGuid, cancellationToken);
                if (queueEntry == null)
                {
                    result.Errors.Add("Queue entry not found");
                    return result;
                }

                // Update allowed fields for anonymous users
                var hasUpdates = false;

                if (!string.IsNullOrWhiteSpace(request.Name) && request.Name != queueEntry.CustomerName)
                {
                    queueEntry.UpdateCustomerName(request.Name, "anonymous-user");
                    hasUpdates = true;
                }

                if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != queueEntry.ContactEmail)
                {
                    queueEntry.UpdateContactEmail(request.Email, "anonymous-user");
                    hasUpdates = true;
                }

                if (request.EmailNotifications.HasValue && request.EmailNotifications != queueEntry.EmailNotifications)
                {
                    queueEntry.UpdateEmailNotifications(request.EmailNotifications.Value, "anonymous-user");
                    hasUpdates = true;
                }

                if (request.BrowserNotifications.HasValue && request.BrowserNotifications != queueEntry.BrowserNotifications)
                {
                    queueEntry.UpdateBrowserNotifications(request.BrowserNotifications.Value, "anonymous-user");
                    hasUpdates = true;
                }

                if (hasUpdates)
                {
                    await _queueEntryRepository.UpdateAsync(queueEntry, cancellationToken);
                }

                result.Success = true;
                result.QueueEntryId = entryId;
                result.UpdatedAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"An error occurred while updating queue entry: {ex.Message}");
            }

            return result;
        }
    }

    public class UpdateQueueEntryResult : BaseResult
    {
        public string? QueueEntryId { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class UpdateQueueEntryRequest
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public bool? EmailNotifications { get; set; }
        public bool? BrowserNotifications { get; set; }
    }
}


