using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Grande.Fila.API.Domain.Locations;
using Grande.Fila.API.Domain.Common.ValueObjects;
using Grande.Fila.API.Infrastructure.Data;

namespace Grande.Fila.API.Infrastructure.Repositories.Sql
{
    public class SqlLocationRepository : SqlBaseRepository<Location>, ILocationRepository
    {
        public SqlLocationRepository(QueueHubDbContext context) : base(context)
        {
        }

        public async Task<Location?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(slug))
                throw new ArgumentException("Slug cannot be null or whitespace", nameof(slug));

            // Create Slug value object for comparison
            var slugValueObject = Slug.Create(slug);
            return await _dbSet
                .Where(l => l.Slug == slugValueObject)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Location>> GetByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(l => l.OrganizationId == organizationId)
                .OrderBy(l => l.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Location>> GetLocationsByOrganizationIdsAsync(List<Guid> organizationIds, CancellationToken cancellationToken = default)
        {
            if (organizationIds == null || !organizationIds.Any())
                return new List<Location>();

            return await _dbSet
                .Where(l => organizationIds.Contains(l.OrganizationId))
                .OrderBy(l => l.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Location>> GetLocationsByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(l => l.OrganizationId == organizationId)
                .OrderBy(l => l.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Location>> GetActiveLocationsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(l => l.IsActive)
                .OrderBy(l => l.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Location>> GetNearbyLocationsAsync(
            double latitude, 
            double longitude, 
            double radiusInKm, 
            CancellationToken cancellationToken = default)
        {
            // Since Location entity uses Address value object instead of direct latitude/longitude,
            // this is a simplified implementation that returns all active locations
            // In a real implementation, you'd need to extract coordinates from Address
            // or store them separately for geographic queries
            return await _dbSet
                .Where(l => l.IsActive)
                .OrderBy(l => l.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> IsSlugUniqueAsync(string slug, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return false;

            // Create Slug value object for comparison
            var slugValueObject = Slug.Create(slug);
            return !await _dbSet
                .AnyAsync(l => l.Slug == slugValueObject, cancellationToken);
        }

        public async Task<bool> IsSlugUniqueForOrganizationAsync(string slug, Guid organizationId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return false;

            // Create Slug value object for comparison
            var slugValueObject = Slug.Create(slug);
            return !await _dbSet
                .AnyAsync(l => l.Slug == slugValueObject && l.OrganizationId == organizationId, cancellationToken);
        }

        public async Task<IReadOnlyList<Location>> GetLocationsByQueueStatusAsync(bool isQueueEnabled, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(l => l.IsQueueEnabled == isQueueEnabled && l.IsActive)
                .OrderBy(l => l.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<Location?> GetLocationWithStaffAsync(Guid locationId, CancellationToken cancellationToken = default)
        {
            // This would require navigation properties to be set up in EF Core configuration
            // For now, just return the location without staff
            return await GetByIdAsync(locationId, cancellationToken);
        }
    }
} 