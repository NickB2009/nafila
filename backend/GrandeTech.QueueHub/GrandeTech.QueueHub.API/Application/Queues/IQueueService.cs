using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grande.Fila.API.Application.Queues
{
    public interface IQueueService
    {
        /// <summary>
        /// Enqueue a message for background processing
        /// </summary>
        Task<bool> EnqueueAsync<T>(T message, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Enqueue a message with delay
        /// </summary>
        Task<bool> EnqueueDelayedAsync<T>(T message, TimeSpan delay, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Enqueue a message with priority (higher number = higher priority)
        /// </summary>
        Task<bool> EnqueueWithPriorityAsync<T>(T message, int priority = 0, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Get approximate queue length for monitoring
        /// </summary>
        Task<int> GetQueueLengthAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get queue health status
        /// </summary>
        Task<QueueHealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default);
    }

    public class QueueHealthStatus
    {
        public bool IsHealthy { get; set; }
        public int PendingMessages { get; set; }
        public int ProcessedMessages { get; set; }
        public int FailedMessages { get; set; }
        public DateTime LastProcessedAt { get; set; }
        public string[] Errors { get; set; } = Array.Empty<string>();
    }
} 