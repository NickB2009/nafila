using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Domain.Customers;
using Microsoft.EntityFrameworkCore;

namespace GrandeTech.QueueHub.API.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Implementation of the Customer repository
    /// </summary>
    public class CustomerRepository : BaseRepository<Customer>, ICustomerRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Application database context</param>
        public CustomerRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Gets a customer by their phone number
        /// </summary>
        public async Task<Customer?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => EF.Property<string>(c, "PhoneNumber") == phoneNumber, cancellationToken);
        }
        
        /// <summary>
        /// Gets a customer by their email address
        /// </summary>
        public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => EF.Property<string>(c, "Email") == email.ToLower(), cancellationToken);
        }
        
        /// <summary>
        /// Gets a customer by their user ID (for authenticated users)
        /// </summary>
        public async Task<Customer?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
        }
        
        /// <summary>
        /// Gets customers who frequently visit a specific service provider
        /// </summary>
        public async Task<IReadOnlyList<Customer>> GetFrequentCustomersAsync(
            Guid serviceProviderId, 
            int minVisits = 3, 
            CancellationToken cancellationToken = default)
        {
            // This query requires a join with QueueEntries
            // Load customers who have a history with this service provider
            var customersWithServiceCount = await _context.QueueEntries
                .Where(qe => 
                    _context.Queues.Any(q => q.Id == EF.Property<Guid>(qe, "QueueId") && q.ServiceProviderId == serviceProviderId) &&
                    qe.Status == Domain.Queues.QueueEntryStatus.Completed)
                .GroupBy(qe => qe.CustomerId)
                .Select(g => new { CustomerId = g.Key, VisitCount = g.Count() })
                .Where(x => x.VisitCount >= minVisits)
                .ToListAsync(cancellationToken);
                
            // Fetch the actual customer records
            var customerIds = customersWithServiceCount.Select(c => c.CustomerId).ToList();
            
            return await _dbSet
                .Where(c => customerIds.Contains(c.Id))
                .ToListAsync(cancellationToken);
        }
    }
}
