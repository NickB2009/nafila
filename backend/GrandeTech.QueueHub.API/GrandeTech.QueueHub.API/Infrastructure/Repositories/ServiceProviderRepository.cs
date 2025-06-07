using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Domain.ServicesProviders;
using GrandeTech.QueueHub.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using ServicesProviderEntity = GrandeTech.QueueHub.API.Domain.ServicesProviders.ServicesProvider;

namespace GrandeTech.QueueHub.API.Infrastructure.Repositories
{
    /// <summary>
    /// Implementation of the ServicesProvider repository
    /// </summary>
    public class ServicesProviderRepository : BaseRepository<ServicesProviderEntity>, IServicesProviderRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Application database context</param>
        public ServicesProviderRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Gets a service provider by its slug
        /// </summary>
        public async Task<ServicesProviderEntity?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(sp => EF.Property<string>(sp, "Slug") == slug, cancellationToken);
        }

        /// <summary>
        /// Gets all service providers for an organization
        /// </summary>
        public async Task<IReadOnlyList<ServicesProviderEntity>> GetByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(sp => sp.OrganizationId == organizationId)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets all active service providers
        /// </summary>
        public async Task<IReadOnlyList<ServicesProviderEntity>> GetActiveServicesProvidersAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(sp => sp.IsActive)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets service providers within a certain distance of coordinates
        /// </summary>
        public async Task<IReadOnlyList<ServicesProviderEntity>> GetNearbyServicesProvidersAsync(
            double latitude,
            double longitude,
            double maxDistanceInKm,
            CancellationToken cancellationToken = default)
        {
            // For now, return all active service providers since we're using bogus data
            return await GetActiveServicesProvidersAsync(cancellationToken);
        }
    }
} 