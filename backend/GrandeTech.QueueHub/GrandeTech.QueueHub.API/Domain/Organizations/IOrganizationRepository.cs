using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Domain.Common;
using Grande.Fila.API.Domain.Organizations;

namespace Grande.Fila.API.Domain.Organizations
{
    /// <summary>
    /// Repository interface for Organization aggregate root
    /// </summary>
    public interface IOrganizationRepository : IRepository<Organization>
    {
        /// <summary>
        /// Gets an organization by its slug
        /// </summary>
        /// <param name="slug">The organization slug</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The organization or null if not found</returns>
        Task<Organization?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets all active organizations
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of active organizations</returns>
        Task<IReadOnlyList<Organization>> GetActiveOrganizationsAsync(CancellationToken cancellationToken = default);
          /// <summary>
        /// Gets organizations by subscription plan ID
        /// </summary>
        /// <param name="subscriptionPlanId">The subscription plan ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of organizations with the specified subscription plan</returns>
        Task<IReadOnlyList<Organization>> GetBySubscriptionPlanAsync(Guid subscriptionPlanId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Checks if a slug is unique across all organizations
        /// </summary>
        /// <param name="slug">The slug to check</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if the slug is unique, false otherwise</returns>
        Task<bool> IsSlugUniqueAsync(string slug, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets all organizations that share data for analytics
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of organizations that share data for analytics</returns>
        Task<IReadOnlyList<Organization>> GetOrganizationsWithAnalyticsSharingAsync(CancellationToken cancellationToken = default);
    }
}
