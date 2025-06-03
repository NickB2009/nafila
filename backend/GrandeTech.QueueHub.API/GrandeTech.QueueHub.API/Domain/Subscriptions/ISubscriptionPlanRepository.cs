using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Domain.Common;
using GrandeTech.QueueHub.API.Domain.Subscriptions;

namespace GrandeTech.QueueHub.API.Domain.Subscriptions
{
    /// <summary>
    /// Repository interface for SubscriptionPlan aggregate root
    /// </summary>
    public interface ISubscriptionPlanRepository : IRepository<SubscriptionPlan>
    {
        /// <summary>
        /// Gets all active subscription plans
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of active subscription plans</returns>
        Task<IReadOnlyList<SubscriptionPlan>> GetActiveSubscriptionPlansAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a subscription plan by its name
        /// </summary>
        /// <param name="name">The subscription plan name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The subscription plan or null if not found</returns>
        Task<SubscriptionPlan?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the default subscription plan
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The default subscription plan or null if not found</returns>
        Task<SubscriptionPlan?> GetDefaultSubscriptionPlanAsync(CancellationToken cancellationToken = default);
    }
}
