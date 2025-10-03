using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Domain.Common;
using Grande.Fila.API.Domain.Subscriptions;

namespace Grande.Fila.API.Infrastructure.Repositories.Bogus
{
    public class BogusSubscriptionPlanRepository : BogusBaseRepository<SubscriptionPlan>, ISubscriptionPlanRepository
    {
        public override async Task<SubscriptionPlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await base.GetByIdAsync(id, cancellationToken);
        }

        public override async Task<IReadOnlyList<SubscriptionPlan>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await base.GetAllAsync(cancellationToken);
        }

        public override async Task<IReadOnlyList<SubscriptionPlan>> FindAsync(System.Linq.Expressions.Expression<Func<SubscriptionPlan, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await base.FindAsync(predicate, cancellationToken);
        }

        public override async Task<SubscriptionPlan> AddAsync(SubscriptionPlan entity, CancellationToken cancellationToken = default)
        {
            return await base.AddAsync(entity, cancellationToken);
        }

        public override async Task<SubscriptionPlan> UpdateAsync(SubscriptionPlan entity, CancellationToken cancellationToken = default)
        {
            return await base.UpdateAsync(entity, cancellationToken);
        }

        public override async Task<bool> DeleteAsync(SubscriptionPlan entity, CancellationToken cancellationToken = default)
        {
            return await base.DeleteAsync(entity, cancellationToken);
        }

        public override async Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await base.DeleteByIdAsync(id, cancellationToken);
        }

        public override async Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<SubscriptionPlan, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await base.ExistsAsync(predicate, cancellationToken);
        }

        protected override SubscriptionPlan CreateNewEntityWithId(SubscriptionPlan entity, Guid id)
        {
            var subscriptionPlan = new SubscriptionPlan(
                entity.Name,
                entity.Description,
                entity.MonthlyPriceAmount,
                entity.YearlyPriceAmount,
                entity.MaxLocations,
                entity.MaxStaffPerLocation,
                entity.IncludesAnalytics,
                entity.IncludesAdvancedReporting,
                entity.IncludesCustomBranding,
                entity.IncludesAdvertising,
                entity.IncludesMultipleLocations,
                entity.MaxQueueEntriesPerDay,
                entity.IsFeatured,
                entity.CreatedBy ?? "system");
            
            // Set the ID using reflection since it's protected
            var idProperty = typeof(BaseEntity).GetProperty("Id");
            idProperty?.SetValue(subscriptionPlan, id);
            
            return subscriptionPlan;
        }

        public async Task<IReadOnlyList<SubscriptionPlan>> GetActiveSubscriptionPlansAsync(CancellationToken cancellationToken = default)
        {
            var plans = await GetAllAsync(cancellationToken);
            return plans.Where(p => p.IsActive).OrderBy(p => p.Price).ToList();
        }

        public async Task<SubscriptionPlan?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            var plans = await GetAllAsync(cancellationToken);
            return plans.FirstOrDefault(p => p.Name.ToLower() == name.ToLower());
        }

        public async Task<SubscriptionPlan?> GetDefaultSubscriptionPlanAsync(CancellationToken cancellationToken = default)
        {
            var plans = await GetAllAsync(cancellationToken);
            return plans.FirstOrDefault(p => p.IsDefault);
        }
    }
} 