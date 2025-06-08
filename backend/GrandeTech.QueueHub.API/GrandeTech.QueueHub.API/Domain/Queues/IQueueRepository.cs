using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Domain.Common;
using GrandeTech.QueueHub.API.Domain.Queues;

namespace GrandeTech.QueueHub.API.Domain.Queues
{
    /// <summary>
    /// Repository interface for Queue aggregate root
    /// </summary>
    public interface IQueueRepository : IRepository<Queue>
    {
        /// <summary>
        /// Gets the active queue for a service provider on a specific date
        /// </summary>
        /// <param name="LocationId">The service provider ID</param>
        /// <param name="date">The queue date (defaults to today)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The active queue or null if not found</returns>
        Task<Queue?> GetActiveQueueAsync(
            Guid LocationId, 
            DateTime? date = null, 
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Gets all queues for a service provider within a date range
        /// </summary>
        /// <param name="LocationId">The service provider ID</param>
        /// <param name="startDate">The start date</param>
        /// <param name="endDate">The end date</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of queues for the specified date range</returns>
        Task<IReadOnlyList<Queue>> GetQueuesByDateRangeAsync(
            Guid LocationId,
            DateTime startDate,
            DateTime endDate, 
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Gets active queues with similar load to specified queue for analytics
        /// </summary>
        /// <param name="queueId">The queue ID</param>
        /// <param name="daysToLookBack">Days to look back for similar queues</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of queues with similar patterns</returns>
        Task<IReadOnlyList<Queue>> GetSimilarQueuesByLoadAsync(
            Guid queueId,
            int daysToLookBack = 30,
            CancellationToken cancellationToken = default);
    }
}
