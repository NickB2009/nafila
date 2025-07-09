using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Domain.Common;
using Grande.Fila.API.Domain.Locations;

namespace Grande.Fila.API.Infrastructure.Repositories.Bogus
{
    public class BogusLocationRepository : BogusBaseRepository<Domain.Locations.Location>, ILocationRepository
    {
        public override async Task<Domain.Locations.Location?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await base.GetByIdAsync(id, cancellationToken);
        }

        public override async Task<IReadOnlyList<Domain.Locations.Location>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await base.GetAllAsync(cancellationToken);
        }

        public override async Task<IReadOnlyList<Domain.Locations.Location>> FindAsync(System.Linq.Expressions.Expression<Func<Domain.Locations.Location, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await base.FindAsync(predicate, cancellationToken);
        }

        public override async Task<Domain.Locations.Location> AddAsync(Domain.Locations.Location entity, CancellationToken cancellationToken = default)
        {
            return await base.AddAsync(entity, cancellationToken);
        }

        public override async Task<Domain.Locations.Location> UpdateAsync(Domain.Locations.Location entity, CancellationToken cancellationToken = default)
        {
            return await base.UpdateAsync(entity, cancellationToken);
        }

        public override async Task<bool> DeleteAsync(Domain.Locations.Location entity, CancellationToken cancellationToken = default)
        {
            return await base.DeleteAsync(entity, cancellationToken);
        }

        public override async Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await base.DeleteByIdAsync(id, cancellationToken);
        }

        public override async Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<Domain.Locations.Location, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await base.ExistsAsync(predicate, cancellationToken);
        }

        protected override Domain.Locations.Location CreateNewEntityWithId(Domain.Locations.Location entity, Guid id)
        {
            var location = new Domain.Locations.Location(
                entity.Name,
                entity.Slug.Value,
                entity.Description ?? string.Empty,
                entity.OrganizationId,
                entity.Address,
                entity.ContactPhone?.Value,
                entity.ContactEmail?.Value,
                entity.BusinessHours.Start,
                entity.BusinessHours.End,
                entity.MaxQueueSize,
                entity.LateClientCapTimeInMinutes,
                entity.CreatedBy ?? "system");
            
            // Set the ID using reflection since it's protected
            var idProperty = typeof(BaseEntity).GetProperty("Id");
            idProperty?.SetValue(location, id);
            
            return location;
        }

        public async Task<IReadOnlyList<Domain.Locations.Location>> GetByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default)
        {
            var locations = await GetAllAsync(cancellationToken);
            return locations.Where(l => l.OrganizationId == organizationId).ToList();
        }

        public async Task<Domain.Locations.Location?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            var locations = await GetAllAsync(cancellationToken);
            return locations.FirstOrDefault(l => l.Slug.Value == slug);
        }

        public async Task<IReadOnlyList<Domain.Locations.Location>> GetActiveLocationsAsync(CancellationToken cancellationToken = default)
        {
            var locations = await GetAllAsync(cancellationToken);
            return locations.Where(l => l.IsActive).ToList();
        }

        public async Task<IReadOnlyList<Domain.Locations.Location>> GetNearbyLocationsAsync(double latitude, double longitude, double radiusInKm, CancellationToken cancellationToken = default)
        {
            var locations = await GetAllAsync(cancellationToken);
            return locations
                .Where(l => l.IsActive && 
                    l.Address.Latitude.HasValue && 
                    l.Address.Longitude.HasValue && 
                    CalculateDistance(
                        latitude, longitude,
                        l.Address.Latitude.Value, l.Address.Longitude.Value) <= radiusInKm)
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

        public async Task<IReadOnlyList<Domain.Locations.Location>> GetLocationsByOrganizationIdsAsync(List<Guid> organizationIds, CancellationToken cancellationToken = default)
        {
            var locations = await GetAllAsync(cancellationToken);
            return locations.Where(l => organizationIds.Contains(l.OrganizationId)).ToList();
        }

        public async Task<IReadOnlyList<Domain.Locations.Location>> GetLocationsByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default)
        {
            var locations = await GetAllAsync(cancellationToken);
            return locations.Where(l => l.OrganizationId == organizationId).ToList();
        }
    }
}
