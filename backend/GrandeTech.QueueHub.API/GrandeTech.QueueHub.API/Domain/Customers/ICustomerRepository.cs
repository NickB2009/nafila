using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Domain.Common;
using Grande.Fila.API.Domain.Customers;

namespace Grande.Fila.API.Domain.Customers
{
    /// <summary>
    /// Repository interface for Customer aggregate root
    /// </summary>
    public interface ICustomerRepository : IRepository<Customer>
    {
        /// <summary>
        /// Gets a customer by their phone number
        /// </summary>
        /// <param name="phoneNumber">The phone number</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The customer or null if not found</returns>
        Task<Customer?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a customer by their email address
        /// </summary>
        /// <param name="email">The email address</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The customer or null if not found</returns>
        Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a customer by their user ID (for authenticated users)
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The customer or null if not found</returns>
        Task<Customer?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets customers who frequently visit a specific service provider
        /// </summary>
        /// <param name="LocationId">The service provider ID</param>
        /// <param name="minVisits">Minimum number of visits to be considered frequent</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of frequent customers</returns>
        Task<IReadOnlyList<Customer>> GetFrequentCustomersAsync(
            Guid LocationId, 
            int minVisits = 3, 
            CancellationToken cancellationToken = default);
    }
}
