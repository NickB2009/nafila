using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace GrandeTech.QueueHub.API.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Implementation of the ServiceType repository
    /// </summary>
    public class ServiceTypeRepository : BaseRepository<ServiceType>, IServiceTypeRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Application database context</param>
        public ServiceTypeRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Gets all service types for a service provider
        /// </summary>
        public async Task<IReadOnlyList<ServiceType>> GetByServiceProviderAsync(Guid serviceProviderId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(s => s.ServiceProviderId == serviceProviderId)
                .ToListAsync(cancellationToken);
        }
        
        /// <summary>
        /// Gets all active service types for a service provider
        /// </summary>
        public async Task<IReadOnlyList<ServiceType>> GetActiveServiceTypesAsync(Guid serviceProviderId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(s => 
                    s.ServiceProviderId == serviceProviderId && 
                    s.IsActive)
                .ToListAsync(cancellationToken);
        }
        
        /// <summary>
        /// Gets the most popular service types for a service provider
        /// </summary>
        public async Task<IReadOnlyList<ServiceType>> GetPopularServiceTypesAsync(
            Guid serviceProviderId, 
            int count = 5, 
            CancellationToken cancellationToken = default)
        {
            // Get service provider's queues
            var queueIds = await _context.Queues
                .Where(q => q.ServiceProviderId == serviceProviderId)
                .Select(q => q.Id)
                .ToListAsync(cancellationToken);
                
            // Get the most popular service types based on completed queue entries
            var popularServiceTypeIds = await _context.QueueEntries
                .Where(qe => 
                    queueIds.Contains(EF.Property<Guid>(qe, "QueueId")) && 
                    qe.Status == Domain.Queues.QueueEntryStatus.Completed)
                .GroupBy(qe => qe.ServiceTypeId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(count)
                .ToListAsync(cancellationToken);
                
            // Get the actual service type records
            return await _dbSet
                .Where(st => popularServiceTypeIds.Contains(st.Id))
                .ToListAsync(cancellationToken);
        }
    }
}
