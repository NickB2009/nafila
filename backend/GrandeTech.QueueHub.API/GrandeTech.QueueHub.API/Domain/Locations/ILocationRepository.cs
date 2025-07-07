using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Domain.Common;
using Grande.Fila.API.Domain.Locations;

namespace Grande.Fila.API.Domain.Locations
{
    /// <summary>
    /// Repository interface for Location aggregate root
    /// </summary>
    public interface ILocationRepository : IRepository<Location>
    {
        /// <summary>
        /// Gets a location by its slug
        /// </summary>
        /// <param name="slug">The location slug</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The location or null if not found</returns>
        Task<Location?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets all locations for an organization
        /// </summary>
        /// <param name="organizationId">The organization ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of locations for the specified organization</returns>
        Task<IReadOnlyList<Location>> GetByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets all active locations
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of active locations</returns>
        Task<IReadOnlyList<Location>> GetActiveLocationsAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets locations within a certain distance of coordinates
        /// </summary>
        /// <param name="latitude">Latitude coordinate</param>
        /// <param name="longitude">Longitude coordinate</param>
        /// <param name="radiusInKm">Search radius in kilometers</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of nearby locations</returns>
        Task<IReadOnlyList<Location>> GetNearbyLocationsAsync(
            double latitude, 
            double longitude, 
            double radiusInKm, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets locations by organization IDs
        /// </summary>
        /// <param name="organizationIds">List of organization IDs</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of locations for the specified organizations</returns>
        Task<IReadOnlyList<Location>> GetLocationsByOrganizationIdsAsync(List<Guid> organizationIds, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets locations by organization ID
        /// </summary>
        /// <param name="organizationId">Organization ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of locations for the specified organization</returns>
        Task<IReadOnlyList<Location>> GetLocationsByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default);
    }
}
