using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Domain.Customers;
using GrandeTech.QueueHub.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GrandeTech.QueueHub.API.Infrastructure.Repositories
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
            int minVisits,
            CancellationToken cancellationToken = default)
        {
            // For now, return all customers since we're using bogus data
            return await GetAllAsync(cancellationToken);
        }
    }
} 