using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Domain.Organizations;
using Microsoft.EntityFrameworkCore;

namespace GrandeTech.QueueHub.API.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Implementation of the Organization repository
    /// </summary>
    public class OrganizationRepository : BaseRepository<Organization>, IOrganizationRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Application database context</param>
        public OrganizationRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Gets an organization by its slug
        /// </summary>
        public async Task<Organization?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(o => EF.Property<string>(o, "Slug") == slug, cancellationToken);
        }
        
        /// <summary>
        /// Gets all active organizations
        /// </summary>
        public async Task<IReadOnlyList<Organization>> GetActiveOrganizationsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(o => o.IsActive)
                .ToListAsync(cancellationToken);
        }
        
        /// <summary>
        /// Gets organizations by subscription plan ID
        /// </summary>
        public async Task<IReadOnlyList<Organization>> GetBySubscriptionPlanAsync(Guid subscriptionPlanId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(o => o.SubscriptionPlanId == subscriptionPlanId)
                .ToListAsync(cancellationToken);
        }
    }
}
