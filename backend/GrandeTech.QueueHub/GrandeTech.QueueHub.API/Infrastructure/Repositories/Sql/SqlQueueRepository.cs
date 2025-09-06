using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Infrastructure.Data;

namespace Grande.Fila.API.Infrastructure.Repositories.Sql
{
    public class SqlQueueRepository : SqlBaseRepository<Queue>, IQueueRepository
    {
        public SqlQueueRepository(QueueHubDbContext context) : base(context)
        {
        }

        public async Task<Queue?> GetByLocationIdAsync(Guid locationId, CancellationToken cancellationToken)
        {
            return await _dbSet
                .Include(q => q.Entries)
                .Where(q => q.LocationId == locationId && q.QueueDate == DateTime.UtcNow.Date)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Queue?> GetActiveQueueByLocationIdAsync(Guid locationId, CancellationToken cancellationToken)
        {
            return await _dbSet
                .Include(q => q.Entries)
                .Where(q => q.LocationId == locationId && q.IsActive && q.QueueDate == DateTime.UtcNow.Date)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IList<Queue>> GetAllByLocationIdAsync(Guid locationId, CancellationToken cancellationToken)
        {
            return await _dbSet
                .Where(q => q.LocationId == locationId)
                .OrderByDescending(q => q.QueueDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Queue>> GetByLocationAsync(Guid locationId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(q => q.LocationId == locationId)
                .OrderByDescending(q => q.QueueDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<QueueEntry?> GetQueueEntryById(Guid queueEntryId, CancellationToken cancellationToken)
        {
            return await _context.QueueEntries
                .FirstOrDefaultAsync(qe => qe.Id == queueEntryId, cancellationToken);
        }

        public void UpdateQueueEntry(QueueEntry queueEntry)
        {
            if (queueEntry == null)
                throw new ArgumentNullException(nameof(queueEntry));

            _context.Entry(queueEntry).State = EntityState.Modified;
        }

        public async Task<IReadOnlyList<Queue>> GetQueuesByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(q => q.QueueDate >= startDate.Date && q.QueueDate <= endDate.Date)
                .OrderBy(q => q.QueueDate)
                .ThenBy(q => q.LocationId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Queue>> GetQueuesByOrganizationIdAsync(Guid organizationId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            // TODO: This requires joining with Locations to get the OrganizationId - temporarily disabled
            // var queuesWithOrganization = await (from queue in _dbSet
            //                                    join location in _context.Locations on queue.LocationId equals location.Id
            //                                    where location.OrganizationId == organizationId 
            //                                          && queue.QueueDate >= startDate.Date 
            //                                          && queue.QueueDate <= endDate.Date
            //                                    orderby queue.QueueDate, queue.LocationId
            //                                    select queue)
            //                                    .ToListAsync(cancellationToken);

            // Return empty list for now until Locations entity is added
            return await Task.FromResult<IReadOnlyList<Queue>>(new List<Queue>());
        }

        // Additional methods for enhanced queue operations
        public async Task<Queue?> GetActiveQueueByLocationAsync(Guid locationId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(q => q.LocationId == locationId && q.IsActive)
                .OrderByDescending(q => q.QueueDate)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<QueueEntry>> GetQueueEntriesAsync(Guid queueId, CancellationToken cancellationToken = default)
        {
            return await _context.QueueEntries
                .Where(qe => qe.QueueId == queueId)
                .OrderBy(qe => qe.Position)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<QueueEntry>> GetActiveQueueEntriesAsync(Guid queueId, CancellationToken cancellationToken = default)
        {
            return await _context.QueueEntries
                .Where(qe => qe.QueueId == queueId && 
                            (qe.Status == QueueEntryStatus.Waiting || 
                             qe.Status == QueueEntryStatus.Called || 
                             qe.Status == QueueEntryStatus.CheckedIn))
                .OrderBy(qe => qe.Position)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetNextPositionAsync(Guid queueId, CancellationToken cancellationToken = default)
        {
            var maxPosition = await _context.QueueEntries
                .Where(qe => qe.QueueId == queueId)
                .MaxAsync(qe => (int?)qe.Position, cancellationToken);

            return (maxPosition ?? 0) + 1;
        }

        public async Task<QueueEntry?> GetQueueEntryByCustomerAndQueueAsync(Guid customerId, Guid queueId, CancellationToken cancellationToken = default)
        {
            return await _context.QueueEntries
                .Where(qe => qe.QueueId == queueId && qe.CustomerId == customerId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<QueueEntry>> GetCustomerQueueHistoryAsync(Guid customerId, int limit = 50, CancellationToken cancellationToken = default)
        {
            return await _context.QueueEntries
                .Where(qe => qe.CustomerId == customerId)
                .OrderByDescending(qe => qe.EnteredAt)
                .Take(limit)
                .ToListAsync(cancellationToken);
        }

        // Analytics and reporting methods - Updated to work with actual Queue properties
        public async Task<Dictionary<DateTime, int>> GetDailyQueueCountsAsync(Guid locationId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            var queueCounts = await (from queue in _dbSet
                                   where queue.LocationId == locationId 
                                         && queue.QueueDate >= startDate.Date 
                                         && queue.QueueDate <= endDate.Date
                                   join entry in _context.QueueEntries on queue.Id equals entry.QueueId into entries
                                   group entries by queue.QueueDate into g
                                   select new { Date = g.Key, Count = g.Count() })
                                   .ToDictionaryAsync(x => x.Date, x => x.Count, cancellationToken);

            return queueCounts;
        }

        public async Task<double> GetAverageWaitTimeAsync(Guid locationId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            // Calculate average wait time based on completed queue entries
            var completedEntries = await (from queue in _dbSet
                                        join entry in _context.QueueEntries on queue.Id equals entry.QueueId
                                        where queue.LocationId == locationId 
                                              && queue.QueueDate >= startDate.Date 
                                              && queue.QueueDate <= endDate.Date
                                              && entry.CompletedAt.HasValue
                                              && entry.EnteredAt != default
                                        select new { 
                                            WaitTime = EF.Functions.DateDiffMinute(entry.EnteredAt, entry.CompletedAt!.Value)
                                        })
                                        .ToListAsync(cancellationToken);

            return completedEntries.Any() ? completedEntries.Average(e => e.WaitTime) : 0;
        }

        // Override AddAsync to handle queue entry management
        public override async Task<Queue> AddAsync(Queue entity, CancellationToken cancellationToken = default)
        {
            var result = await base.AddAsync(entity, cancellationToken);
            // Removed immediate SaveChanges - let service layer handle transactions
            return result;
        }

        // Override UpdateAsync to handle queue entry management
        public override async Task<Queue> UpdateAsync(Queue entity, CancellationToken cancellationToken = default)
        {
            // Load the existing queue with its entries to properly handle navigation properties
            var existingQueue = await _dbSet
                .Include(q => q.Entries)
                .FirstOrDefaultAsync(q => q.Id == entity.Id, cancellationToken);
                
            if (existingQueue == null)
            {
                throw new KeyNotFoundException($"Queue with ID {entity.Id} not found");
            }

            // Update scalar properties
            _context.Entry(existingQueue).CurrentValues.SetValues(entity);

            // Handle queue entries - find new entries that need to be added
            var existingEntryIds = existingQueue.Entries.Select(e => e.Id).ToHashSet();
            var newEntries = entity.Entries.Where(e => !existingEntryIds.Contains(e.Id)).ToList();

            // Add new queue entries
            foreach (var newEntry in newEntries)
            {
                _context.QueueEntries.Add(newEntry);
            }

            // Update existing entries if needed
            foreach (var updatedEntry in entity.Entries.Where(e => existingEntryIds.Contains(e.Id)))
            {
                var existingEntry = existingQueue.Entries.First(e => e.Id == updatedEntry.Id);
                _context.Entry(existingEntry).CurrentValues.SetValues(updatedEntry);
            }

            // Removed immediate SaveChanges - let service layer handle transactions
            return existingQueue;
        }
    }
} 