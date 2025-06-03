using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Domain.Queues;
using Microsoft.EntityFrameworkCore;

namespace GrandeTech.QueueHub.API.Infrastructure.Persistence.Repositories
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
                .OrderBy(q => q.QueueDate)
                .ToListAsync(cancellationToken);
        }
            
        /// <summary>
        /// Gets active queues with similar load to specified queue for analytics
        /// </summary>
        public async Task<IReadOnlyList<Queue>> GetSimilarQueuesByLoadAsync(
            Guid queueId,
            int daysToLookBack = 30,
            CancellationToken cancellationToken = default)
        {
            // First, get the queue we want to compare
            var baseQueue = await _dbSet
                .Include(q => q.Entries)
                .FirstOrDefaultAsync(q => q.Id == queueId, cancellationToken);
                
            if (baseQueue == null)
            {
                return new List<Queue>();
            }
            
            var baseQueueSize = baseQueue.Entries.Count;
            
            // Get the day of week of the base queue
            var dayOfWeek = baseQueue.QueueDate.DayOfWeek;
            
            // Look for queues with similar load (±20%) on same day of week
            var minSize = baseQueueSize - (baseQueueSize * 0.2);
            var maxSize = baseQueueSize + (baseQueueSize * 0.2);
            
            var startDate = baseQueue.QueueDate.AddDays(-daysToLookBack);
            
            return await _dbSet
                .Include(q => q.Entries)
                .Where(q => 
                    q.Id != queueId &&
                    q.ServiceProviderId == baseQueue.ServiceProviderId &&
                    q.QueueDate >= startDate &&
                    q.QueueDate < baseQueue.QueueDate &&
                    q.QueueDate.DayOfWeek == dayOfWeek &&
                    q.Entries.Count >= minSize &&
                    q.Entries.Count <= maxSize)
                .OrderByDescending(q => q.QueueDate)
                .Take(5)
                .ToListAsync(cancellationToken);
        }
    }
}
