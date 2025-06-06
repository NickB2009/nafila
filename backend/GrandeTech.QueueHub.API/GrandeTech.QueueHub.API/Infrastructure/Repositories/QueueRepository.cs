using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Domain.Queues;
using GrandeTech.QueueHub.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GrandeTech.QueueHub.API.Infrastructure.Repositories
{
    /// <summary>
    /// Implementation of the Queue repository
    /// </summary>
    public class QueueRepository : BaseRepository<Queue>, IQueueRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Application database context</param>
        public QueueRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Gets the active queue for a service provider on a specific date
        /// </summary>
        public async Task<Queue?> GetActiveQueueAsync(
            Guid serviceProviderId, 
            DateTime? date = null, 
            CancellationToken cancellationToken = default)
        {
            var queueDate = date ?? DateTime.Today;
            
            return await _dbSet
                .Include(q => q.Entries)
                .FirstOrDefaultAsync(q => 
                    q.ServiceProviderId == serviceProviderId && 
                    q.QueueDate.Date == queueDate.Date && 
                    q.IsActive, 
                    cancellationToken);
        }
            
        /// <summary>
        /// Gets all queues for a service provider within a date range
        /// </summary>
        public async Task<IReadOnlyList<Queue>> GetQueuesByDateRangeAsync(
            Guid serviceProviderId,
            DateTime startDate,
            DateTime endDate, 
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(q => q.Entries)
                .Where(q => 
                    q.ServiceProviderId == serviceProviderId && 
                    q.QueueDate.Date >= startDate.Date && 
                    q.QueueDate.Date <= endDate.Date)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets active queues with similar load to specified queue for analytics
        /// </summary>
        public async Task<IReadOnlyList<Queue>> GetSimilarQueuesByLoadAsync(
            Guid queueId,
            int maxEntries,
            CancellationToken cancellationToken = default)
        {
            var queue = await _dbSet
                .Include(q => q.Entries)
                .FirstOrDefaultAsync(q => q.Id == queueId, cancellationToken);

            if (queue == null)
            {
                return new List<Queue>();
            }

            var entryCount = queue.Entries.Count;
            var targetMax = entryCount + maxEntries;

            return await _dbSet
                .Include(q => q.Entries)
                .Where(q => 
                    q.Id != queueId && 
                    q.IsActive && 
                    q.Entries.Count <= targetMax)
                .ToListAsync(cancellationToken);
        }
    }
} 