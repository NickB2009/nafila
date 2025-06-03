using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Domain.Common;
using GrandeTech.QueueHub.API.Domain.ServiceProviders;

namespace GrandeTech.QueueHub.API.Domain.ServiceProviders
{
    /// <summary>
    /// Repository interface for ServiceProvider aggregate root
    /// </summary>
    public interface IServiceProviderRepository : IRepository<ServiceProvider>
    {
        /// <summary>
        /// Gets a service provider by its slug
        /// </summary>
        /// <param name="slug">The service provider slug</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The service provider or null if not found</returns>
        Task<ServiceProvider?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets all service providers for an organization
        /// </summary>
        /// <param name="organizationId">The organization ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of service providers for the specified organization</returns>
        Task<IReadOnlyList<ServiceProvider>> GetByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets all active service providers
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of active service providers</returns>
        Task<IReadOnlyList<ServiceProvider>> GetActiveServiceProvidersAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets service providers within a certain distance of coordinates
        /// </summary>
        /// <param name="latitude">Latitude coordinate</param>
        /// <param name="longitude">Longitude coordinate</param>
        /// <param name="radiusInKm">Search radius in kilometers</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of nearby service providers</returns>
        Task<IReadOnlyList<ServiceProvider>> GetNearbyServiceProvidersAsync(
            double latitude, 
            double longitude, 
            double radiusInKm, 
            CancellationToken cancellationToken = default);
    }
}
