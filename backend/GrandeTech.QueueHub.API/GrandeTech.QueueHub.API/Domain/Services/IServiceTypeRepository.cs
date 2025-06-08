using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Domain.Common;
using GrandeTech.QueueHub.API.Domain.Services;

namespace GrandeTech.QueueHub.API.Domain.Services
{
    /// <summary>
    /// Repository interface for ServiceType aggregate root
    /// </summary>
    public interface IServiceTypeRepository : IRepository<ServiceType>
    {
        /// <summary>
        /// Gets all service types for a location
        /// </summary>
        /// <param name="locationId">The location ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of service types for the specified location</returns>
        Task<IReadOnlyList<ServiceType>> GetByLocationAsync(Guid locationId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets all active service types for a location
        /// </summary>
        /// <param name="locationId">The location ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of active service types</returns>
        Task<IReadOnlyList<ServiceType>> GetActiveServiceTypesAsync(Guid locationId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the most popular service types for a location
        /// </summary>
        /// <param name="locationId">The location ID</param>
        /// <param name="count">The number of service types to return</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of popular service types</returns>
        Task<IReadOnlyList<ServiceType>> GetPopularServiceTypesAsync(
            Guid locationId, 
            int count = 5, 
            CancellationToken cancellationToken = default);
    }
}
