using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Grande.Fila.API.Domain.Queues;

namespace Grande.Fila.API.Application.Public;

/// <summary>
/// Service for retrieving the status of a specific queue entry for anonymous users
/// </summary>
public class GetQueueEntryStatusService
{
    private readonly IQueueRepository _queueRepository;
    private readonly ILogger<GetQueueEntryStatusService> _logger;

    public GetQueueEntryStatusService(
        IQueueRepository queueRepository,
        ILogger<GetQueueEntryStatusService> logger)
    {
        _queueRepository = queueRepository ?? throw new ArgumentNullException(nameof(queueRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetQueueEntryStatusResult> ExecuteAsync(string entryId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving queue entry status for entry {EntryId}", entryId);

            // Validate entry ID
            if (string.IsNullOrWhiteSpace(entryId))
            {
                return new GetQueueEntryStatusResult
                {
                    Success = false,
                    FieldErrors = { { "EntryId", "Entry ID is required" } }
                };
            }

            if (!Guid.TryParse(entryId, out var queueEntryId))
            {
                return new GetQueueEntryStatusResult
                {
                    Success = false,
                    FieldErrors = { { "EntryId", "Invalid entry ID format" } }
                };
            }

            // Find the queue containing this entry
            var allQueues = await _queueRepository.GetAllAsync(cancellationToken);
            var queueWithEntry = allQueues.FirstOrDefault(q => q.Entries.Any(e => e.Id == queueEntryId));

            if (queueWithEntry == null)
            {
                return new GetQueueEntryStatusResult
                {
                    Success = false,
                    Errors = { "Queue entry not found" }
                };
            }

            var entry = queueWithEntry.Entries.First(e => e.Id == queueEntryId);

            // Calculate position (only count waiting and called entries)
            var waitingEntries = queueWithEntry.Entries
                .Where(e => e.Status == QueueEntryStatus.Waiting || e.Status == QueueEntryStatus.Called)
                .OrderBy(e => e.EnteredAt)
                .ToList();

            var position = waitingEntries.FindIndex(e => e.Id == queueEntryId) + 1;

            // Estimate wait time (basic calculation - could be enhanced with historical data)
            var estimatedWaitMinutes = Math.Max(1, position * 15); // 15 minutes per position as default

            var result = new QueueEntryStatusDto
            {
                Id = entry.Id.ToString(),
                Position = position,
                EstimatedWaitMinutes = estimatedWaitMinutes,
                Status = entry.Status.ToString().ToLower(),
                JoinedAt = entry.EnteredAt,
                ServiceRequested = "General Service", // QueueEntry doesn't have ServiceRequested property
                CustomerName = entry.CustomerName
            };

            _logger.LogInformation("Successfully retrieved queue entry status for entry {EntryId}", entryId);

            return new GetQueueEntryStatusResult
            {
                Success = true,
                EntryStatus = result
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving queue entry status for entry {EntryId}", entryId);
            return new GetQueueEntryStatusResult
            {
                Success = false,
                Errors = { "An unexpected error occurred while retrieving queue entry status" }
            };
        }
    }
}
