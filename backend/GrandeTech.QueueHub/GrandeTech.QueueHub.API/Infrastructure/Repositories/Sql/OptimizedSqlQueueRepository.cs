using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Infrastructure.Data;
using Grande.Fila.API.Infrastructure.Services;

namespace Grande.Fila.API.Infrastructure.Repositories.Sql
{
    /// <summary>
    /// Optimized queue repository with caching, query optimization, and performance monitoring
    /// </summary>
    public class OptimizedSqlQueueRepository : SqlBaseRepository<Queue>, IQueueRepository
    {
        private readonly IQueryCacheService _cacheService;
        private readonly IQueryOptimizationService _optimizationService;
        private readonly ILogger<OptimizedSqlQueueRepository> _logger;

        public OptimizedSqlQueueRepository(
            QueueHubDbContext context,
            IQueryCacheService cacheService,
            IQueryOptimizationService optimizationService,
            ILogger<OptimizedSqlQueueRepository> logger) : base(context)
        {
            _cacheService = cacheService;
            _optimizationService = optimizationService;
            _logger = logger;
        }

        public async Task<Queue?> GetByLocationIdAsync(Guid locationId, CancellationToken cancellationToken)
        {
            var cacheKey = $"Queue_ByLocation_{locationId}";
            
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                return await _optimizationService.ExecuteWithMonitoringAsync(async () =>
                {
                    var query = _dbSet
                        .Include(q => q.Entries)
                        .Where(q => q.LocationId == locationId)
                        .OrderByDescending(q => q.QueueDate);

                    return await _optimizationService.OptimizeQuery(query, "readonly")
                        .FirstOrDefaultAsync(cancellationToken);
                }, "GetByLocationId", cancellationToken);
            }, TimeSpan.FromMinutes(2), cancellationToken);
        }

        public async Task<Queue?> GetActiveQueueByLocationIdAsync(Guid locationId, CancellationToken cancellationToken)
        {
            var cacheKey = $"Queue_ActiveByLocation_{locationId}";
            
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                return await _optimizationService.ExecuteWithMonitoringAsync(async () =>
                {
                    var query = _dbSet
                        .Include(q => q.Entries)
                        .Where(q => q.LocationId == locationId && q.IsActive)
                        .OrderByDescending(q => q.QueueDate);

                    return await _optimizationService.OptimizeQuery(query, "readonly")
                        .FirstOrDefaultAsync(cancellationToken);
                }, "GetActiveQueueByLocationId", cancellationToken);
            }, TimeSpan.FromMinutes(1), cancellationToken); // Shorter cache for active queues
        }

        public async Task<IList<Queue>> GetAllByLocationIdAsync(Guid locationId, CancellationToken cancellationToken)
        {
            var cacheKey = $"Queues_AllByLocation_{locationId}";
            
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                return await _optimizationService.ExecuteWithMonitoringAsync(async () =>
                {
                    var query = _dbSet
                        .Where(q => q.LocationId == locationId)
                        .OrderByDescending(q => q.QueueDate);

                    return await _optimizationService.OptimizeQuery(query, "readonly")
                        .ToListAsync(cancellationToken);
                }, "GetAllByLocationId", cancellationToken);
            }, TimeSpan.FromMinutes(5), cancellationToken);
        }

        public async Task<IReadOnlyList<Queue>> GetByLocationAsync(Guid locationId, CancellationToken cancellationToken = default)
        {
            var result = await GetAllByLocationIdAsync(locationId, cancellationToken);
            return result.ToList();
        }

        public async Task<QueueEntry?> GetQueueEntryById(Guid queueEntryId, CancellationToken cancellationToken)
        {
            var cacheKey = $"QueueEntry_ById_{queueEntryId}";
            
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                return await _optimizationService.ExecuteWithMonitoringAsync(async () =>
                {
                    return await _context.QueueEntries
                        .FirstOrDefaultAsync(qe => qe.Id == queueEntryId, cancellationToken);
                }, "GetQueueEntryById", cancellationToken);
            }, TimeSpan.FromMinutes(5), cancellationToken);
        }

        public void UpdateQueueEntry(QueueEntry queueEntry)
        {
            if (queueEntry == null)
                throw new ArgumentNullException(nameof(queueEntry));

            _context.Entry(queueEntry).State = EntityState.Modified;
            
            // Invalidate related cache entries
            _ = Task.Run(async () =>
            {
                await _cacheService.InvalidateEntityCacheAsync<QueueEntry>(queueEntry.Id);
                await _cacheService.RemoveAsync($"Queue_ActiveByLocation_{queueEntry.QueueId}");
                await _cacheService.RemoveAsync($"Queue_ByLocation_{queueEntry.QueueId}");
            });
        }

        public async Task<IReadOnlyList<Queue>> GetQueuesByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"Queues_ByDateRange_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";
            
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                return await _optimizationService.ExecuteWithMonitoringAsync(async () =>
                {
                    var query = _dbSet
                        .Where(q => q.QueueDate >= startDate.Date && q.QueueDate <= endDate.Date)
                        .OrderBy(q => q.QueueDate)
                        .ThenBy(q => q.LocationId);

                    return await _optimizationService.OptimizeQuery(query, "readonly")
                        .ToListAsync(cancellationToken);
                }, "GetQueuesByDateRange", cancellationToken);
            }, TimeSpan.FromMinutes(10), cancellationToken);
        }

        public async Task<IReadOnlyList<Queue>> GetQueuesByOrganizationIdAsync(Guid organizationId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            // This method is currently disabled in the base implementation
            // Return empty list for now until Locations entity relationship is properly configured
            return await Task.FromResult<IReadOnlyList<Queue>>(new List<Queue>());
        }

        public async Task<Queue?> GetActiveQueueByLocationAsync(Guid locationId, CancellationToken cancellationToken = default)
        {
            return await GetActiveQueueByLocationIdAsync(locationId, cancellationToken);
        }

        public async Task<IReadOnlyList<QueueEntry>> GetQueueEntriesAsync(Guid queueId, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"QueueEntries_ByQueue_{queueId}";
            
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                return await _optimizationService.ExecuteWithMonitoringAsync(async () =>
                {
                    var query = _context.QueueEntries
                        .Where(qe => qe.QueueId == queueId)
                        .OrderBy(qe => qe.Position);

                    return await _optimizationService.OptimizeQuery(query, "readonly")
                        .ToListAsync(cancellationToken);
                }, "GetQueueEntries", cancellationToken);
            }, TimeSpan.FromMinutes(1), cancellationToken);
        }

        public async Task<IReadOnlyList<QueueEntry>> GetActiveQueueEntriesAsync(Guid queueId, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"QueueEntries_ActiveByQueue_{queueId}";
            
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                return await _optimizationService.ExecuteWithMonitoringAsync(async () =>
                {
                    var query = _context.QueueEntries
                        .Where(qe => qe.QueueId == queueId && 
                                   (qe.Status == QueueEntryStatus.Waiting || 
                                    qe.Status == QueueEntryStatus.Called || 
                                    qe.Status == QueueEntryStatus.CheckedIn))
                        .OrderBy(qe => qe.Position);

                    return await _optimizationService.OptimizeQuery(query, "readonly")
                        .ToListAsync(cancellationToken);
                }, "GetActiveQueueEntries", cancellationToken);
            }, TimeSpan.FromSeconds(30), cancellationToken); // Very short cache for active entries
        }

        public async Task<int> GetNextPositionAsync(Guid queueId, CancellationToken cancellationToken = default)
        {
            // Don't cache this as it needs to be real-time
            return await _optimizationService.ExecuteWithMonitoringAsync(async () =>
            {
                var maxPosition = await _context.QueueEntries
                    .Where(qe => qe.QueueId == queueId)
                    .MaxAsync(qe => (int?)qe.Position, cancellationToken);

                return (maxPosition ?? 0) + 1;
            }, "GetNextPosition", cancellationToken);
        }

        public async Task<QueueEntry?> GetQueueEntryByCustomerAndQueueAsync(Guid customerId, Guid queueId, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"QueueEntry_ByCustomerAndQueue_{customerId}_{queueId}";
            
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                return await _optimizationService.ExecuteWithMonitoringAsync(async () =>
                {
                    var query = _context.QueueEntries
                        .Where(qe => qe.QueueId == queueId && qe.CustomerId == customerId);

                    return await _optimizationService.OptimizeQuery(query, "readonly")
                        .FirstOrDefaultAsync(cancellationToken);
                }, "GetQueueEntryByCustomerAndQueue", cancellationToken);
            }, TimeSpan.FromMinutes(2), cancellationToken);
        }

        public async Task<IReadOnlyList<QueueEntry>> GetCustomerQueueHistoryAsync(Guid customerId, int limit = 50, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"QueueEntries_CustomerHistory_{customerId}_{limit}";
            
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                return await _optimizationService.ExecuteWithMonitoringAsync(async () =>
                {
                    var query = _context.QueueEntries
                        .Where(qe => qe.CustomerId == customerId)
                        .OrderByDescending(qe => qe.EnteredAt)
                        .Take(limit);

                    return await _optimizationService.OptimizeQuery(query, "readonly")
                        .ToListAsync(cancellationToken);
                }, "GetCustomerQueueHistory", cancellationToken);
            }, TimeSpan.FromMinutes(5), cancellationToken);
        }

        public async Task<Dictionary<DateTime, int>> GetDailyQueueCountsAsync(Guid locationId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"QueueCounts_Daily_{locationId}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";
            
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                return await _optimizationService.ExecuteWithMonitoringAsync(async () =>
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
                }, "GetDailyQueueCounts", cancellationToken);
            }, TimeSpan.FromMinutes(15), cancellationToken);
        }

        public async Task<double> GetAverageWaitTimeAsync(Guid locationId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"AverageWaitTime_{locationId}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";
            
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                return await _optimizationService.ExecuteWithMonitoringAsync(async () =>
                {
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
                }, "GetAverageWaitTime", cancellationToken);
            }, TimeSpan.FromMinutes(30), cancellationToken);
        }

        public override async Task<Queue> AddAsync(Queue entity, CancellationToken cancellationToken = default)
        {
            var result = await base.AddAsync(entity, cancellationToken);
            
            // Invalidate related cache entries
            await _cacheService.RemoveAsync($"Queue_ByLocation_{entity.LocationId}");
            await _cacheService.RemoveAsync($"Queues_AllByLocation_{entity.LocationId}");
            
            return result;
        }

        public override async Task<Queue> UpdateAsync(Queue entity, CancellationToken cancellationToken = default)
        {
            var result = await base.UpdateAsync(entity, cancellationToken);
            
            // Invalidate related cache entries
            await _cacheService.InvalidateEntityCacheAsync<Queue>(entity.Id);
            await _cacheService.RemoveAsync($"Queue_ByLocation_{entity.LocationId}");
            await _cacheService.RemoveAsync($"Queue_ActiveByLocation_{entity.LocationId}");
            await _cacheService.RemoveAsync($"Queues_AllByLocation_{entity.LocationId}");
            
            return result;
        }
    }
}
