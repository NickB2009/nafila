using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Domain.Common;
using GrandeTech.QueueHub.API.Domain.Queues;

namespace GrandeTech.QueueHub.API.Infrastructure.Repositories.Bogus
{
    public class BogusQueueRepository : BogusBaseRepository<Queue>, IQueueRepository
    {
        public override async Task<Queue?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await base.GetByIdAsync(id, cancellationToken);
        }

        public override async Task<IReadOnlyList<Queue>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await base.GetAllAsync(cancellationToken);
        }

        public override async Task<IReadOnlyList<Queue>> FindAsync(System.Linq.Expressions.Expression<Func<Queue, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await base.FindAsync(predicate, cancellationToken);
        }

        public override async Task<Queue> AddAsync(Queue entity, CancellationToken cancellationToken = default)
        {
            return await base.AddAsync(entity, cancellationToken);
        }

        public override async Task<Queue> UpdateAsync(Queue entity, CancellationToken cancellationToken = default)
        {
            return await base.UpdateAsync(entity, cancellationToken);
        }

        public override async Task<bool> DeleteAsync(Queue entity, CancellationToken cancellationToken = default)
        {
            return await base.DeleteAsync(entity, cancellationToken);
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
                entity.ServicesProviderId,
                entity.MaxSize,
                entity.LateClientCapTimeInMinutes,
                entity.CreatedBy);
            
            // Set the ID using reflection since it's protected
            var idProperty = typeof(BaseEntity).GetProperty("Id");
            idProperty?.SetValue(queue, id);
            
            return queue;
        }

        public async Task<Queue?> GetActiveQueueAsync(Guid ServicesProviderId, DateTime? date = null, CancellationToken cancellationToken = default)
        {
            var queueDate = date ?? DateTime.Today;
            var queues = await GetAllAsync(cancellationToken);
            return queues.FirstOrDefault(q => 
                q.ServicesProviderId == ServicesProviderId && 
                q.QueueDate.Date == queueDate.Date && 
                q.IsActive);
        }

        public async Task<IReadOnlyList<Queue>> GetQueuesByDateRangeAsync(Guid ServicesProviderId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            var queues = await GetAllAsync(cancellationToken);
            return queues.Where(q => 
                q.ServicesProviderId == ServicesProviderId && 
                q.QueueDate.Date >= startDate.Date && 
                q.QueueDate.Date <= endDate.Date)
                .ToList();
        }

        public async Task<IReadOnlyList<Queue>> GetSimilarQueuesByLoadAsync(Guid queueId, int maxEntries, CancellationToken cancellationToken = default)
        {
            var queue = await GetByIdAsync(queueId, cancellationToken);
            if (queue == null)
            {
                return new List<Queue>();
            }

            var entryCount = queue.Entries.Count;
            var targetMax = entryCount + maxEntries;

            var queues = await GetAllAsync(cancellationToken);
            return queues.Where(q => 
                q.Id != queueId && 
                q.IsActive && 
                q.Entries.Count <= targetMax)
                .ToList();
        }
    }
} 