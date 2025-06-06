using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Domain.Common;
using GrandeTech.QueueHub.API.Domain.ServiceProviders;

namespace GrandeTech.QueueHub.API.Infrastructure.Repositories.Bogus
{
    public class BogusServiceProviderRepository : BogusBaseRepository<Domain.ServiceProviders.ServiceProvider>, IServiceProviderRepository
    {
        public override async Task<Domain.ServiceProviders.ServiceProvider?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await base.GetByIdAsync(id, cancellationToken);
        }

        public override async Task<IReadOnlyList<Domain.ServiceProviders.ServiceProvider>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await base.GetAllAsync(cancellationToken);
        }

        public override async Task<IReadOnlyList<Domain.ServiceProviders.ServiceProvider>> FindAsync(System.Linq.Expressions.Expression<Func<Domain.ServiceProviders.ServiceProvider, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await base.FindAsync(predicate, cancellationToken);
        }

        public override async Task<Domain.ServiceProviders.ServiceProvider> AddAsync(Domain.ServiceProviders.ServiceProvider entity, CancellationToken cancellationToken = default)
        {
            return await base.AddAsync(entity, cancellationToken);
        }

        public override async Task<Domain.ServiceProviders.ServiceProvider> UpdateAsync(Domain.ServiceProviders.ServiceProvider entity, CancellationToken cancellationToken = default)
        {
            return await base.UpdateAsync(entity, cancellationToken);
        }

        public override async Task<bool> DeleteAsync(Domain.ServiceProviders.ServiceProvider entity, CancellationToken cancellationToken = default)
        {
            return await base.DeleteAsync(entity, cancellationToken);
        }

        public override async Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await base.DeleteByIdAsync(id, cancellationToken);
        }

        public override async Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<Domain.ServiceProviders.ServiceProvider, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await base.ExistsAsync(predicate, cancellationToken);
        }

        protected override Domain.ServiceProviders.ServiceProvider CreateNewEntityWithId(Domain.ServiceProviders.ServiceProvider entity, Guid id)
        {
            var serviceProvider = new Domain.ServiceProviders.ServiceProvider(
                entity.Name,
                entity.Slug.Value,
                entity.Description ?? string.Empty,
                entity.OrganizationId,
                entity.Location,
                entity.ContactPhone?.Value,
                entity.ContactEmail?.Value,
                entity.BusinessHours.Start,
                entity.BusinessHours.End,
                entity.MaxQueueSize,
                entity.LateClientCapTimeInMinutes,
                entity.CreatedBy);
            
            // Set the ID using reflection since it's protected
            var idProperty = typeof(BaseEntity).GetProperty("Id");
            idProperty?.SetValue(serviceProvider, id);
            
            return serviceProvider;
        }

        public async Task<IReadOnlyList<Domain.ServiceProviders.ServiceProvider>> GetByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default)
        {
            var serviceProviders = await GetAllAsync(cancellationToken);
            return serviceProviders.Where(sp => sp.OrganizationId == organizationId).ToList();
        }

        public async Task<Domain.ServiceProviders.ServiceProvider?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            var serviceProviders = await GetAllAsync(cancellationToken);
            return serviceProviders.FirstOrDefault(sp => sp.Slug.Value == slug);
        }

        public async Task<IReadOnlyList<Domain.ServiceProviders.ServiceProvider>> GetActiveServiceProvidersAsync(CancellationToken cancellationToken = default)
        {
            var serviceProviders = await GetAllAsync(cancellationToken);
            return serviceProviders.Where(sp => sp.IsActive).ToList();
        }

        public async Task<IReadOnlyList<Domain.ServiceProviders.ServiceProvider>> GetNearbyServiceProvidersAsync(double latitude, double longitude, double radiusInKm, CancellationToken cancellationToken = default)
        {
            var serviceProviders = await GetAllAsync(cancellationToken);
            return serviceProviders
                .Where(sp => sp.IsActive && 
                    sp.Location.Latitude.HasValue && 
                    sp.Location.Longitude.HasValue && 
                    CalculateDistance(
                        latitude, longitude,
                        sp.Location.Latitude.Value, sp.Location.Longitude.Value) <= radiusInKm)
                .ToList();
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double EarthRadiusKm = 6371;
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return EarthRadiusKm * c;
        }

        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }
} 