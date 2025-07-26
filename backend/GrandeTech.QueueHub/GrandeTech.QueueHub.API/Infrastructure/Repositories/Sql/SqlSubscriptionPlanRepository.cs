using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Grande.Fila.API.Domain.Subscriptions;
using Grande.Fila.API.Infrastructure.Data;

namespace Grande.Fila.API.Infrastructure.Repositories.Sql
{
    public class SqlSubscriptionPlanRepository : SqlBaseRepository<SubscriptionPlan>, ISubscriptionPlanRepository
    {
        public SqlSubscriptionPlanRepository(QueueHubDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<SubscriptionPlan>> GetActiveSubscriptionPlansAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.IsActive)
                .OrderBy(p => p.Price)
                .ToListAsync(cancellationToken);
        }

        public async Task<SubscriptionPlan?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be null or whitespace", nameof(name));

            return await _dbSet
                .Where(p => p.Name == name)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<SubscriptionPlan?> GetDefaultSubscriptionPlanAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.IsDefault && p.IsActive)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<SubscriptionPlan>> GetFeaturedPlansAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.IsFeatured && p.IsActive)
                .OrderBy(p => p.Price)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<SubscriptionPlan>> GetPlansByPriceRangeAsync(
            decimal minPrice, 
            decimal maxPrice, 
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.IsActive && p.Price >= minPrice && p.Price <= maxPrice)
                .OrderBy(p => p.Price)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<SubscriptionPlan>> GetPlansWithAnalyticsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.IsActive && p.IncludesAnalytics)
                .OrderBy(p => p.Price)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<SubscriptionPlan>> GetPlansWithMultipleLocationsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.IsActive && p.IncludesMultipleLocations)
                .OrderBy(p => p.Price)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> IsNameUniqueAsync(string name, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            return !await _dbSet
                .AnyAsync(p => p.Name == name, cancellationToken);
        }

        public async Task<bool> HasDefaultPlanAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(p => p.IsDefault && p.IsActive, cancellationToken);
        }

        public async Task<int> GetTotalActiveSubscriptionsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .CountAsync(p => p.IsActive, cancellationToken);
        }
    }
} 