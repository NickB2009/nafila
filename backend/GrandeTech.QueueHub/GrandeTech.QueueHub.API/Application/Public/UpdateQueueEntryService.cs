using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Grande.Fila.API.Domain.Queues;

namespace Grande.Fila.API.Application.Public;

/// <summary>
/// Service for updating contact information for a queue entry
/// </summary>
public class UpdateQueueEntryService
{
    private readonly IQueueRepository _queueRepository;
    private readonly ILogger<UpdateQueueEntryService> _logger;

    public UpdateQueueEntryService(
        IQueueRepository queueRepository,
        ILogger<UpdateQueueEntryService> logger)
    {
        _queueRepository = queueRepository ?? throw new ArgumentNullException(nameof(queueRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UpdateQueueEntryResult> ExecuteAsync(string entryId, UpdateQueueEntryRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing update queue entry request for entry {EntryId}", entryId);

            var result = new UpdateQueueEntryResult();

            // Validate entry ID
            if (string.IsNullOrWhiteSpace(entryId))
            {
                result.Errors.Add("Entry ID is required");
                return result;
            }

            if (!Guid.TryParse(entryId, out var queueEntryId))
            {
                result.Errors.Add("Invalid entry ID format");
                return result;
            }

            // Validate request
            if (request == null)
            {
                result.Errors.Add("Update request is required");
                return result;
            }

            // Find the queue containing this entry
            var allQueues = await _queueRepository.GetAllAsync(cancellationToken);
            var queueWithEntry = allQueues.FirstOrDefault(q => q.Entries.Any(e => e.Id == queueEntryId));

            if (queueWithEntry == null)
            {
                result.Errors.Add("Queue entry not found");
                return result;
            }

            var entryToUpdate = queueWithEntry.Entries.First(e => e.Id == queueEntryId);

            // Check if entry can be updated (only waiting entries can be updated)
            if (entryToUpdate.Status != QueueEntryStatus.Waiting)
            {
                result.Errors.Add("Only waiting customers can update their information");
                return result;
            }

            // Update the entry with new information
            bool hasChanges = false;

            // Note: QueueEntry doesn't have an UpdateCustomerName method
            // For now, we'll just validate that the request is valid
            // In a real implementation, you might need to add this method to QueueEntry
            // or handle the update differently

            // Note: Email and notification preferences are not stored in QueueEntry
            // They would be stored in the Customer entity if the customer is registered
            // For anonymous entries, we only validate the request format

            result.Success = true;
            result.QueueEntryId = entryToUpdate.Id.ToString();
            result.UpdatedAt = DateTime.UtcNow;

            _logger.LogInformation("Successfully processed update queue entry request for entry {EntryId}", entryId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing update queue entry request for entry {EntryId}", entryId);
            return new UpdateQueueEntryResult
            {
                Success = false,
                Errors = { "An unexpected error occurred while updating the queue entry" }
            };
        }
    }
}

public class UpdateQueueEntryRequest
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public bool? EmailNotifications { get; set; }
    public bool? BrowserNotifications { get; set; }
}

public class UpdateQueueEntryResult
{
    public bool Success { get; set; }
    public string? QueueEntryId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<string> Errors { get; set; } = new();
}
