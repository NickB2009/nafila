using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Domain.Common;
using GrandeTech.QueueHub.API.Domain.ServicesOffered;

namespace GrandeTech.QueueHub.API.Infrastructure.Repositories.Bogus
{
    public class BogusServiceTypeRepository : BogusBaseRepository<ServiceOffered>, IServicesOfferedRepository
    {
        public override async Task<ServiceOffered?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await base.GetByIdAsync(id, cancellationToken);
        }

        public override async Task<IReadOnlyList<ServiceOffered>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await base.GetAllAsync(cancellationToken);
        }

        public override async Task<IReadOnlyList<ServiceOffered>> FindAsync(System.Linq.Expressions.Expression<Func<ServiceOffered, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await base.FindAsync(predicate, cancellationToken);
        }

        public override async Task<ServiceOffered> AddAsync(ServiceOffered entity, CancellationToken cancellationToken = default)
        {
            return await base.AddAsync(entity, cancellationToken);
        }

        public override async Task<ServiceOffered> UpdateAsync(ServiceOffered entity, CancellationToken cancellationToken = default)
        {
            return await base.UpdateAsync(entity, cancellationToken);
        }

        public override async Task<bool> DeleteAsync(ServiceOffered entity, CancellationToken cancellationToken = default)
        {
            return await base.DeleteAsync(entity, cancellationToken);
        }

        public override async Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await base.DeleteByIdAsync(id, cancellationToken);
        }

        public override async Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<ServiceOffered, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await base.ExistsAsync(predicate, cancellationToken);
        }

        protected override ServiceOffered CreateNewEntityWithId(ServiceOffered entity, Guid id)
        {
            var serviceType = new ServiceOffered(
                entity.Name,
                entity.Description,
                entity.LocationId,
                entity.EstimatedDurationMinutes,
                entity.Price?.Amount,
                entity.ImageUrl,
                entity.CreatedBy ?? "system");
            
            // Set the ID using reflection since it's protected
            var idProperty = typeof(BaseEntity).GetProperty("Id");
            idProperty?.SetValue(serviceType, id);
            
            return serviceType;
        }        public async Task<IReadOnlyList<ServiceOffered>> GetByLocationAsync(Guid locationId, CancellationToken cancellationToken = default)
        {
            var serviceTypes = await base.GetAllAsync(cancellationToken);
            return serviceTypes.Where(st => st.LocationId == locationId).ToList();
        }

        public async Task<IReadOnlyList<ServiceOffered>> GetActiveServiceTypesAsync(Guid locationId, CancellationToken cancellationToken = default)
        {
            var serviceTypes = await base.GetAllAsync(cancellationToken);
            return serviceTypes.Where(st => st.LocationId == locationId && st.IsActive).ToList();
        }

        public async Task<IReadOnlyList<ServiceOffered>> GetPopularServiceTypesAsync(Guid locationId, int count = 5, CancellationToken cancellationToken = default)
        {
            var serviceTypes = await base.GetAllAsync(cancellationToken);
            return serviceTypes
                .Where(st => st.LocationId == locationId && st.IsActive)
                .OrderByDescending(st => st.TimesProvided)
                .Take(count)
                .ToList();
        }
    }
}