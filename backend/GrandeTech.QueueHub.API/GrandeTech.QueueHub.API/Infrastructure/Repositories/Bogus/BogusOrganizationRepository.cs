using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Domain.Common;
using GrandeTech.QueueHub.API.Domain.Organizations;

namespace GrandeTech.QueueHub.API.Infrastructure.Repositories.Bogus
{
    public class BogusOrganizationRepository : BogusBaseRepository<Organization>, IOrganizationRepository
    {
        public override async Task<Organization?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await base.GetByIdAsync(id, cancellationToken);
        }

        public override async Task<IReadOnlyList<Organization>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await base.GetAllAsync(cancellationToken);
        }

        public override async Task<IReadOnlyList<Organization>> FindAsync(System.Linq.Expressions.Expression<Func<Organization, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await base.FindAsync(predicate, cancellationToken);
        }

        public override async Task<Organization> AddAsync(Organization entity, CancellationToken cancellationToken = default)
        {
            return await base.AddAsync(entity, cancellationToken);
        }

        public override async Task<Organization> UpdateAsync(Organization entity, CancellationToken cancellationToken = default)
        {
            return await base.UpdateAsync(entity, cancellationToken);
        }

        public override async Task<bool> DeleteAsync(Organization entity, CancellationToken cancellationToken = default)
        {
            return await base.DeleteAsync(entity, cancellationToken);
        }

        public override async Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await base.DeleteByIdAsync(id, cancellationToken);
        }

        public override async Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<Organization, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await base.ExistsAsync(predicate, cancellationToken);
        }

        protected override Organization CreateNewEntityWithId(Organization entity, Guid id)
        {
            var organization = new Organization(
                entity.Name,
                entity.Slug.Value,
                entity.Description,
                entity.ContactEmail?.Value,
                entity.ContactPhone?.Value,
                entity.WebsiteUrl,
                entity.BrandingConfig,
                entity.SubscriptionPlanId,
                entity.CreatedBy);
            
            // Set the ID using reflection since it's protected
            var idProperty = typeof(BaseEntity).GetProperty("Id");
            idProperty?.SetValue(organization, id);
            
            return organization;
        }

        public async Task<Organization?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            var organizations = await GetAllAsync(cancellationToken);
            return organizations.FirstOrDefault(o => o.Slug.Value == slug);
        }

        public async Task<IReadOnlyList<Organization>> GetActiveOrganizationsAsync(CancellationToken cancellationToken = default)
        {
            var organizations = await GetAllAsync(cancellationToken);
            return organizations.Where(o => o.IsActive).ToList();
        }

        public async Task<IReadOnlyList<Organization>> GetBySubscriptionPlanAsync(Guid subscriptionPlanId, CancellationToken cancellationToken = default)
        {
            var organizations = await GetAllAsync(cancellationToken);
            return organizations.Where(o => o.SubscriptionPlanId == subscriptionPlanId).ToList();
        }
    }
} 