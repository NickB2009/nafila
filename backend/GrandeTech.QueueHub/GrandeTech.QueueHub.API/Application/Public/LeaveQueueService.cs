using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Grande.Fila.API.Domain.Queues;

namespace Grande.Fila.API.Application.Public;

/// <summary>
/// Service for allowing anonymous users to leave a queue
/// </summary>
public class LeaveQueueService
{
    private readonly IQueueRepository _queueRepository;
    private readonly ILogger<LeaveQueueService> _logger;

    public LeaveQueueService(
        IQueueRepository queueRepository,
        ILogger<LeaveQueueService> logger)
    {
        _queueRepository = queueRepository ?? throw new ArgumentNullException(nameof(queueRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<LeaveQueueResult> ExecuteAsync(string entryId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing leave queue request for entry {EntryId}", entryId);

            var result = new LeaveQueueResult();

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

            // Find the queue containing this entry
            var allQueues = await _queueRepository.GetAllAsync(cancellationToken);
            var queueWithEntry = allQueues.FirstOrDefault(q => q.Entries.Any(e => e.Id == queueEntryId));

            if (queueWithEntry == null)
            {
                result.Errors.Add("Queue entry not found");
                return result;
            }

            var entryToCancel = queueWithEntry.Entries.First(e => e.Id == queueEntryId);

            // Check if entry can be cancelled
            if (entryToCancel.Status != QueueEntryStatus.Waiting)
            {
                result.Errors.Add("Only waiting customers can leave the queue");
                return result;
            }

            // Cancel the entry
            entryToCancel.Cancel();
            await _queueRepository.UpdateAsync(queueWithEntry, cancellationToken);

            result.Success = true;
            result.QueueEntryId = entryToCancel.Id.ToString();
            result.LeftAt = entryToCancel.CancelledAt;
            result.CustomerName = entryToCancel.CustomerName;

            _logger.LogInformation("Successfully processed leave queue request for entry {EntryId}", entryId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing leave queue request for entry {EntryId}", entryId);
            return new LeaveQueueResult
            {
                Success = false,
                Errors = { "An unexpected error occurred while leaving the queue" }
            };
        }
    }
}

public class LeaveQueueResult
{
    public bool Success { get; set; }
    public string? QueueEntryId { get; set; }
    public DateTime? LeftAt { get; set; }
    public string? CustomerName { get; set; }
    public List<string> Errors { get; set; } = new();
}
