using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Application.Queues.Models;

namespace Grande.Fila.API.Application.Queues
{
    /// <summary>
    /// Interface for queue analytics and metrics services
    /// </summary>
    public interface IQueueAnalyticsService
    {
        /// <summary>
        /// Calculate estimated wait time for a new queue entry
        /// </summary>
        Task<TimeSpan> CalculateEstimatedWaitTimeAsync(
            Guid salonId, 
            string serviceType, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get queue performance metrics for a salon
        /// </summary>
        Task<QueuePerformanceMetrics> GetQueuePerformanceMetricsAsync(
            Guid salonId, 
            DateTime? fromDate = null, 
            DateTime? toDate = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get wait time trends for a salon
        /// </summary>
        Task<List<WaitTimeTrend>> GetWaitTimeTrendsAsync(
            Guid salonId, 
            int days = 7,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Record customer satisfaction feedback
        /// </summary>
        Task<bool> RecordCustomerSatisfactionAsync(
            Guid queueEntryId, 
            int rating, 
            string? feedback = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get queue recommendations for a salon
        /// </summary>
        Task<QueueRecommendations> GetQueueRecommendationsAsync(
            Guid salonId,
            CancellationToken cancellationToken = default);
    }
}
