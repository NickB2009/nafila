using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Domain.Services;
using GrandeTech.QueueHub.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GrandeTech.QueueHub.API.Infrastructure.Repositories
{
    /// <summary>
    /// Implementation of the ServiceType repository
    /// </summary>
    public class ServiceTypeRepository : BaseRepository<ServiceType>, IServiceTypeRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Application database context</param>
        public ServiceTypeRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Gets all service types for a service provider
        /// </summary>
        public async Task<IReadOnlyList<ServiceType>> GetByServiceProviderAsync(Guid serviceProviderId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(s => s.ServiceProviderId == serviceProviderId)
                .ToListAsync(cancellationToken);
        }
        
        /// <summary>
        /// Gets all active service types for a service provider
        /// </summary>
        public async Task<IReadOnlyList<ServiceType>> GetActiveServiceTypesAsync(Guid serviceProviderId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(s => 
                    s.ServiceProviderId == serviceProviderId && 
                    s.IsActive)
                .ToListAsync(cancellationToken);
        }
        
        /// <summary>
        /// Gets the most popular service types for a service provider
        /// </summary>
        public async Task<IReadOnlyList<ServiceType>> GetPopularServiceTypesAsync(
            Guid serviceProviderId,
            int count,
            CancellationToken cancellationToken = default)
        {
            // For now, return all active service types since we're using bogus data
            return await GetActiveServiceTypesAsync(serviceProviderId, cancellationToken);
        }
    }
} 