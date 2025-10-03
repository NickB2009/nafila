using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Grande.Fila.API.Domain.ServicesOffered;
using Grande.Fila.API.Infrastructure.Data;

namespace Grande.Fila.API.Infrastructure.Repositories.Sql
{
    public class SqlServicesOfferedRepository : SqlBaseRepository<ServiceOffered>, IServicesOfferedRepository
    {
        public SqlServicesOfferedRepository(QueueHubDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<ServiceOffered>> GetByLocationAsync(Guid locationId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(s => s.LocationId == locationId)
                .OrderBy(s => s.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ServiceOffered>> GetActiveServiceTypesAsync(Guid locationId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(s => s.LocationId == locationId && s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ServiceOffered>> GetPopularServiceTypesAsync(
            Guid locationId, 
            int count = 5, 
            CancellationToken cancellationToken = default)
        {
            // For now, return active services ordered by name
            // In a real implementation, this would join with queue entries or usage statistics
            return await _dbSet
                .Where(s => s.LocationId == locationId && s.IsActive)
                .OrderBy(s => s.Name)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ServiceOffered>> GetServiceTypesByPriceRangeAsync(
            Guid locationId, 
            decimal minPrice, 
            decimal maxPrice, 
            CancellationToken cancellationToken = default)
        {
            // This would require converting Money value object to decimal for comparison
            // For now, return all active services for the location
            return await _dbSet
                .Where(s => s.LocationId == locationId && s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ServiceOffered>> GetServiceTypesByDurationAsync(
            Guid locationId, 
            int maxDurationInMinutes, 
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(s => s.LocationId == locationId && 
                          s.IsActive && 
                          s.EstimatedDurationMinutes <= maxDurationInMinutes)
                .OrderBy(s => s.EstimatedDurationMinutes)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> IsServiceNameUniqueForLocationAsync(
            string serviceName, 
            Guid locationId, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
                return false;

            return !await _dbSet
                .AnyAsync(s => s.Name == serviceName && s.LocationId == locationId, cancellationToken);
        }

        public async Task<ServiceOffered?> GetByNameAndLocationAsync(
            string serviceName, 
            Guid locationId, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
                throw new ArgumentException("Service name cannot be null or whitespace", nameof(serviceName));

            return await _dbSet
                .Where(s => s.Name == serviceName && s.LocationId == locationId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<decimal> GetAveragePriceForLocationAsync(Guid locationId, CancellationToken cancellationToken = default)
        {
            var services = await _dbSet
                .Where(s => s.LocationId == locationId && s.IsActive && s.PriceAmount != null)
                .ToListAsync(cancellationToken);

            if (!services.Any())
                return 0;

            // Use flattened price properties for calculation
            var prices = services.Where(s => s.PriceAmount != null).Select(s => s.PriceAmount!.Value).ToList();
            return prices.Any() ? prices.Average() : 0;
        }
    }
} 