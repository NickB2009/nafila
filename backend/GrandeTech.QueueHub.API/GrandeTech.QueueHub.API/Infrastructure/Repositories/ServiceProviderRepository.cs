using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Domain.ServiceProviders;
using GrandeTech.QueueHub.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using ServiceProviderEntity = GrandeTech.QueueHub.API.Domain.ServiceProviders.ServiceProvider;

namespace GrandeTech.QueueHub.API.Infrastructure.Repositories
{
    /// <summary>
    /// Implementation of the ServiceProvider repository
    /// </summary>
    public class ServiceProviderRepository : BaseRepository<ServiceProviderEntity>, IServiceProviderRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Application database context</param>
        public ServiceProviderRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Gets a service provider by its slug
        /// </summary>
        public async Task<ServiceProviderEntity?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(sp => EF.Property<string>(sp, "Slug") == slug, cancellationToken);
        }

        /// <summary>
        /// Gets all service providers for an organization
        /// </summary>
        public async Task<IReadOnlyList<ServiceProviderEntity>> GetByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(sp => sp.OrganizationId == organizationId)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets all active service providers
        /// </summary>
        public async Task<IReadOnlyList<ServiceProviderEntity>> GetActiveServiceProvidersAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(sp => sp.IsActive)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets service providers within a certain distance of coordinates
        /// </summary>
        public async Task<IReadOnlyList<ServiceProviderEntity>> GetNearbyServiceProvidersAsync(
            double latitude,
            double longitude,
            double maxDistanceInKm,
            CancellationToken cancellationToken = default)
        {
            // For now, return all active service providers since we're using bogus data
            return await GetActiveServiceProvidersAsync(cancellationToken);
        }
    }
} 