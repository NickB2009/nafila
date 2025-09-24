using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Application.Common;
using Grande.Fila.API.Application.Queues;
using Grande.Fila.API.Infrastructure.Repositories;

namespace Grande.Fila.API.Application.Public
{
    public class LeaveQueueService
    {
        private readonly CancelQueueService _cancelQueueService;

        public LeaveQueueService(CancelQueueService cancelQueueService)
        {
            _cancelQueueService = cancelQueueService ?? throw new ArgumentNullException(nameof(cancelQueueService));
        }

        public async Task<LeaveQueueResult> ExecuteAsync(string entryId, CancellationToken cancellationToken = default)
        {
            var result = new LeaveQueueResult();

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

                var cancelRequest = new CancelQueueRequest
                {
                    QueueEntryId = entryId
                };

                var cancelResult = await _cancelQueueService.CancelQueueAsync(cancelRequest, "anonymous-user", cancellationToken);

                if (!cancelResult.Success)
                {
                    result.Errors.AddRange(cancelResult.Errors);
                    foreach (var fieldError in cancelResult.FieldErrors)
                    {
                        result.Errors.Add($"{fieldError.Key}: {fieldError.Value}");
                    }
                    return result;
                }

                result.Success = true;
                result.QueueEntryId = entryId;
                result.LeftAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"An error occurred while leaving queue: {ex.Message}");
            }

            return result;
        }
    }

    public class LeaveQueueResult : BaseResult
    {
        public string? QueueEntryId { get; set; }
        public DateTime? LeftAt { get; set; }
    }
}

