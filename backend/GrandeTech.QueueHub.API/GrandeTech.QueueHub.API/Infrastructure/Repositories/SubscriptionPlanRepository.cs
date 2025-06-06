using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Domain.Subscriptions;
using GrandeTech.QueueHub.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GrandeTech.QueueHub.API.Infrastructure.Repositories
{
    /// <summary>
    /// Implementation of the SubscriptionPlan repository
    /// </summary>
    public class SubscriptionPlanRepository : BaseRepository<SubscriptionPlan>, ISubscriptionPlanRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Application database context</param>
        public SubscriptionPlanRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Gets all active subscription plans
        /// </summary>
        public async Task<IReadOnlyList<SubscriptionPlan>> GetActiveSubscriptionPlansAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(sp => sp.IsActive)
                .OrderBy(sp => sp.Price)
                .ToListAsync(cancellationToken);
        }
        
        /// <summary>
        /// Gets a subscription plan by its name
        /// </summary>
        public async Task<SubscriptionPlan?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(sp => sp.Name.ToLower() == name.ToLower(), cancellationToken);
        }
        
        /// <summary>
        /// Gets the default subscription plan
        /// </summary>
        public async Task<SubscriptionPlan?> GetDefaultSubscriptionPlanAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(sp => sp.IsDefault, cancellationToken);
        }
    }
} 