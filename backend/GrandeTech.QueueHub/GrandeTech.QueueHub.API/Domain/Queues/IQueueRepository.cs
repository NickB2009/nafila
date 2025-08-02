using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Domain.Common;

namespace Grande.Fila.API.Domain.Queues
{
    public interface IQueueRepository : IRepository<Queue>
    {
        Task<Queue?> GetByLocationIdAsync(Guid locationId, CancellationToken cancellationToken);
        Task<Queue?> GetActiveQueueByLocationIdAsync(Guid locationId, CancellationToken cancellationToken);
        Task<IList<Queue>> GetAllByLocationIdAsync(Guid locationId, CancellationToken cancellationToken);
        Task<IReadOnlyList<Queue>> GetByLocationAsync(Guid locationId, CancellationToken cancellationToken = default);
        Task<QueueEntry?> GetQueueEntryById(Guid queueEntryId, CancellationToken cancellationToken);
        void UpdateQueueEntry(QueueEntry queueEntry);
        
        /// <summary>
        /// Gets queues by date range across all organizations
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of queues within the date range</returns>
        Task<IReadOnlyList<Queue>> GetQueuesByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets queues by organization ID and date range
        /// </summary>
        /// <param name="organizationId">Organization ID</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of queues for the organization within the date range</returns>
        Task<IReadOnlyList<Queue>> GetQueuesByOrganizationIdAsync(Guid organizationId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    }
} 