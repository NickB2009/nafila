using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Domain.Common;
using GrandeTech.QueueHub.API.Domain.Queues;
using GrandeTech.QueueHub.API.Domain.Services;

namespace GrandeTech.QueueHub.API.Infrastructure.Repositories.Bogus
{
    public class BogusQueueRepository : BogusBaseRepository<Queue>, IQueueRepository
    {
        // Rely on the storage implemented in BogusBaseRepository<T> (the static _items dictionary).
        // No additional state or CRUD overrides are required here.

        public override async Task<IReadOnlyList<Queue>> FindAsync(System.Linq.Expressions.Expression<Func<Queue, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await base.FindAsync(predicate, cancellationToken);
        }

        public override async Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await base.DeleteByIdAsync(id, cancellationToken);
        }

        public override async Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<Queue, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await base.ExistsAsync(predicate, cancellationToken);
        }

        protected override Queue CreateNewEntityWithId(Queue entity, Guid id)
        {
            var queue = new Queue(
                entity.LocationId,
                entity.MaxSize,
                entity.LateClientCapTimeInMinutes,
                entity.CreatedBy ?? "system");
            
            // Set the ID using reflection since it's protected
            var idProperty = typeof(BaseEntity).GetProperty("Id");
            idProperty?.SetValue(queue, id);
            
            return queue;
        }

        public async Task<Queue?> GetActiveQueueAsync(Guid locationId, DateTime? date = null, CancellationToken cancellationToken = default)
        {
            var queueDate = date ?? DateTime.Today;
            var queues = await GetAllAsync(cancellationToken);
            return queues.FirstOrDefault(q => 
                q.LocationId == locationId && 
                q.QueueDate.Date == queueDate.Date && 
                q.IsActive);
        }

        public async Task<IReadOnlyList<Queue>> GetQueuesByDateRangeAsync(
            Guid locationId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default)
        {
            var queues = await GetAllAsync(cancellationToken);
            return queues.Where(q =>
                q.LocationId == locationId &&
                q.QueueDate.Date >= startDate.Date &&
                q.QueueDate.Date <= endDate.Date).ToList();
        }

        public async Task<IReadOnlyList<Queue>> GetSimilarQueuesByLoadAsync(
            Guid queueId,
            int daysToLookBack = 30,
            CancellationToken cancellationToken = default)
        {
            var targetQueue = await GetByIdAsync(queueId, cancellationToken);
            if (targetQueue == null)
                return new List<Queue>();

            var startDate = targetQueue.QueueDate.AddDays(-daysToLookBack);
            var endDate = targetQueue.QueueDate.AddDays(-1);

            var queues = await GetQueuesByDateRangeAsync(
                targetQueue.LocationId,
                startDate,
                endDate,
                cancellationToken);

            // TODO: Implement similarity logic based on queue load patterns
            return queues;
        }
    }
} 