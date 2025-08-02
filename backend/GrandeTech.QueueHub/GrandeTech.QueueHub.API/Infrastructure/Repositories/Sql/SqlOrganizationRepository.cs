using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Grande.Fila.API.Domain.Organizations;
using Grande.Fila.API.Domain.Common.ValueObjects;
using Grande.Fila.API.Infrastructure.Data;

namespace Grande.Fila.API.Infrastructure.Repositories.Sql
{
    public class SqlOrganizationRepository : SqlBaseRepository<Organization>, IOrganizationRepository
    {
        public SqlOrganizationRepository(QueueHubDbContext context) : base(context)
        {
        }

        public async Task<Organization?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(slug))
                throw new ArgumentException("Slug cannot be null or whitespace", nameof(slug));

            // Create Slug value object for comparison
            var slugValueObject = Slug.Create(slug);
            return await _dbSet
                .Where(o => o.Slug == slugValueObject)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Organization>> GetActiveOrganizationsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(o => o.IsActive)
                .OrderBy(o => o.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Organization>> GetBySubscriptionPlanAsync(Guid subscriptionPlanId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(o => o.SubscriptionPlanId == subscriptionPlanId)
                .OrderBy(o => o.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> IsSlugUniqueAsync(string slug, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return false;

            // Create Slug value object for comparison
            var slugValueObject = Slug.Create(slug);
            return !await _dbSet
                .AnyAsync(o => o.Slug == slugValueObject, cancellationToken);
        }

        public async Task<bool> IsSlugUniqueAsync(string slug, Guid excludeOrganizationId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return false;

            // Create Slug value object for comparison
            var slugValueObject = Slug.Create(slug);
            return !await _dbSet
                .AnyAsync(o => o.Slug == slugValueObject && o.Id != excludeOrganizationId, cancellationToken);
        }

        // Additional methods for enhanced organization operations
        public async Task<IReadOnlyList<Organization>> GetOrganizationsWithAnalyticsSharingAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(o => o.SharesDataForAnalytics && o.IsActive)
                .OrderBy(o => o.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Organization>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<Organization>();

            var searchTermLower = searchTerm.ToLower();

            return await _dbSet
                .Where(o => o.Name.ToLower().Contains(searchTermLower) ||
                           (o.Description != null && o.Description.ToLower().Contains(searchTermLower)))
                .OrderBy(o => o.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<Organization?> GetByContactEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            // Create Email value object for comparison
            var emailValueObject = Email.Create(email);
            return await _dbSet
                .Where(o => o.ContactEmail != null && o.ContactEmail == emailValueObject)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<(IReadOnlyList<Organization> Organizations, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            bool? isActive = null,
            Guid? subscriptionPlanId = null,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchTermLower = searchTerm.ToLower();
                query = query.Where(o => o.Name.ToLower().Contains(searchTermLower) ||
                                       (o.Description != null && o.Description.ToLower().Contains(searchTermLower)));
                
                // Note: Removed Slug search for now since it requires complex value object handling in LINQ
                // This could be added back with a more sophisticated approach if needed
            }

            if (isActive.HasValue)
            {
                query = query.Where(o => o.IsActive == isActive.Value);
            }

            if (subscriptionPlanId.HasValue)
            {
                query = query.Where(o => o.SubscriptionPlanId == subscriptionPlanId.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination and ordering
            var organizations = await query
                .OrderBy(o => o.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (organizations, totalCount);
        }

        public async Task<Dictionary<Guid, int>> GetLocationCountsAsync(IEnumerable<Guid> organizationIds, CancellationToken cancellationToken = default)
        {
            var organizationIdsList = organizationIds.ToList();
            
            if (!organizationIdsList.Any())
                return await Task.FromResult(new Dictionary<Guid, int>());

            // TODO: This requires joining with Locations table - temporarily disabled
            // var locationCounts = await (from org in _dbSet
            //                           where organizationIdsList.Contains(org.Id)
            //                           join location in _context.Locations on org.Id equals location.OrganizationId into locations
            //                           select new { OrganizationId = org.Id, LocationCount = locations.Count() })
            //                           .ToDictionaryAsync(x => x.OrganizationId, x => x.LocationCount, cancellationToken);

            // Return empty dictionary for now until Locations entity is added
            return await Task.FromResult(new Dictionary<Guid, int>());
        }

        // Analytics methods
        public async Task<Dictionary<string, object>> GetOrganizationStatisticsAsync(Guid organizationId, CancellationToken cancellationToken = default)
        {
            var organization = await GetByIdAsync(organizationId, cancellationToken);
            if (organization == null)
                return new Dictionary<string, object>();

            var stats = new Dictionary<string, object>
            {
                ["OrganizationId"] = organizationId,
                ["Name"] = organization.Name,
                ["IsActive"] = organization.IsActive,
                ["SharesDataForAnalytics"] = organization.SharesDataForAnalytics,
                ["LocationCount"] = 0, // TODO: await _context.Locations.CountAsync(l => l.OrganizationId == organizationId, cancellationToken),
                ["TotalStaffMembers"] = 0, // TODO: Calculate when Locations and StaffMembers are added
                ["CreatedAt"] = organization.CreatedAt
            };

            return stats;
        }

        public async Task<IReadOnlyList<Organization>> GetRecentlyCreatedAsync(int count = 10, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .OrderByDescending(o => o.CreatedAt)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        // Bulk operations
        public async Task<int> BulkUpdateSubscriptionPlanAsync(IEnumerable<Guid> organizationIds, Guid newSubscriptionPlanId, CancellationToken cancellationToken = default)
        {
            var organizationIdsList = organizationIds.ToList();
            
            if (!organizationIdsList.Any())
                return 0;

            var organizations = await _dbSet
                .Where(o => organizationIdsList.Contains(o.Id))
                .ToListAsync(cancellationToken);

            foreach (var organization in organizations)
            {
                // This would require a domain method to update subscription plan
                // For now, we'll need to implement this properly with domain logic
                organization.MarkAsModified("System"); // This needs to be implemented in the domain
            }

            return organizations.Count;
        }
    }
} 