using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Domain.ServiceProviders;
using Microsoft.EntityFrameworkCore;
using ServiceProviderEntity = GrandeTech.QueueHub.API.Domain.ServiceProviders.ServiceProvider;

namespace GrandeTech.QueueHub.API.Infrastructure.Persistence.Repositories
{    /// <summary>
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
        }        /// <summary>
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
            double radiusInKm, 
            CancellationToken cancellationToken = default)
        {
            // Calculate bounding box to optimize query (simple approximation)
            // 1 degree of latitude = ~111km, 1 degree of longitude = ~111km * cos(latitude)
            double latDegrees = radiusInKm / 111.0;
            double longDegrees = radiusInKm / (111.0 * Math.Cos(latitude * Math.PI / 180));
            
            double minLat = latitude - latDegrees;
            double maxLat = latitude + latDegrees;
            double minLong = longitude - longDegrees;
            double maxLong = longitude + longDegrees;
            
            // First filter by bounding box
            var providers = await _dbSet
                .Where(sp => 
                    EF.Property<double>(sp, "Latitude") >= minLat &&
                    EF.Property<double>(sp, "Latitude") <= maxLat &&
                    EF.Property<double>(sp, "Longitude") >= minLong &&
                    EF.Property<double>(sp, "Longitude") <= maxLong &&
                    sp.IsActive)
                .ToListAsync(cancellationToken);
                
            // Then calculate exact distances and filter by actual radius
            return providers
                .Where(sp => CalculateDistance(
                    latitude, 
                    longitude, 
                    EF.Property<double>(sp, "Latitude"),
                    EF.Property<double>(sp, "Longitude")) <= radiusInKm)
                .ToList();
        }
        
        /// <summary>
        /// Calculate the distance between two points using the Haversine formula
        /// </summary>
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double earthRadiusKm = 6371.0;
            
            var dLat = (lat2 - lat1) * Math.PI / 180;
            var dLon = (lon2 - lon1) * Math.PI / 180;
            
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
                    
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            
            return earthRadiusKm * c;
        }
    }
}
