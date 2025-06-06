using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Domain.Common;
using GrandeTech.QueueHub.API.Domain.Customers;

namespace GrandeTech.QueueHub.API.Infrastructure.Repositories.Bogus
{
    public class BogusCustomerRepository : BogusBaseRepository<Customer>, ICustomerRepository
    {
        public override async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await base.GetByIdAsync(id, cancellationToken);
        }

        public override async Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await base.GetAllAsync(cancellationToken);
        }

        public override async Task<IReadOnlyList<Customer>> FindAsync(System.Linq.Expressions.Expression<Func<Customer, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await base.FindAsync(predicate, cancellationToken);
        }

        public override async Task<Customer> AddAsync(Customer entity, CancellationToken cancellationToken = default)
        {
            return await base.AddAsync(entity, cancellationToken);
        }

        public override async Task<Customer> UpdateAsync(Customer entity, CancellationToken cancellationToken = default)
        {
            return await base.UpdateAsync(entity, cancellationToken);
        }

        public override async Task<bool> DeleteAsync(Customer entity, CancellationToken cancellationToken = default)
        {
            return await base.DeleteAsync(entity, cancellationToken);
        }

        public override async Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await base.DeleteByIdAsync(id, cancellationToken);
        }

        public override async Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<Customer, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await base.ExistsAsync(predicate, cancellationToken);
        }

        protected override Customer CreateNewEntityWithId(Customer entity, Guid id)
        {
            var customer = new Customer(
                entity.Name,
                entity.PhoneNumber?.Value,
                entity.Email?.Value,
                entity.IsAnonymous,
                entity.UserId);
            
            // Set the ID using reflection since it's protected
            var idProperty = typeof(BaseEntity).GetProperty("Id");
            idProperty?.SetValue(customer, id);
            
            return customer;
        }

        public async Task<Customer?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
        {
            var customers = await GetAllAsync(cancellationToken);
            return customers.FirstOrDefault(c => c.PhoneNumber?.Value == phoneNumber);
        }

        public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var customers = await GetAllAsync(cancellationToken);
            return customers.FirstOrDefault(c => c.Email?.Value == email.ToLower());
        }

        public async Task<Customer?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            var customers = await GetAllAsync(cancellationToken);
            return customers.FirstOrDefault(c => c.UserId == userId);
        }

        public async Task<IReadOnlyList<Customer>> GetFrequentCustomersAsync(Guid serviceProviderId, int minVisits, CancellationToken cancellationToken = default)
        {
            var customers = await GetAllAsync(cancellationToken);
            return customers
                .Where(c => c.ServiceHistory.Count(h => h.ServiceProviderId == serviceProviderId) >= minVisits)
                .ToList();
        }
    }
} 