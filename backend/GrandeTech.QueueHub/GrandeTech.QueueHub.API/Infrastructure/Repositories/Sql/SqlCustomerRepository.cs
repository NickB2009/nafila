using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Grande.Fila.API.Domain.Customers;
using Grande.Fila.API.Infrastructure.Data;

namespace Grande.Fila.API.Infrastructure.Repositories.Sql
{
    public class SqlCustomerRepository : SqlBaseRepository<Customer>, ICustomerRepository
    {
        public SqlCustomerRepository(QueueHubDbContext context) : base(context)
        {
        }

        public async Task<Customer?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("Phone number cannot be null or whitespace", nameof(phoneNumber));

            // PhoneNumber is a value object, so we need to compare the underlying value
            return await _dbSet
                .Where(c => c.PhoneNumber != null && c.PhoneNumber.Value == phoneNumber)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or whitespace", nameof(email));

            // Email is a value object, so we need to compare the underlying value
            return await _dbSet
                .Where(c => c.Email != null && c.Email.Value == email)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Customer?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or whitespace", nameof(userId));

            return await _dbSet
                .Where(c => c.UserId == userId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Customer>> GetFrequentCustomersAsync(
            Guid locationId, 
            int minVisits = 5, 
            CancellationToken cancellationToken = default)
        {
            // This would typically involve a join with queue entries or visit history
            // For now, return customers who have this location in their favorites
            // Note: FavoriteLocationIds is IReadOnlyCollection<Guid>, need to handle differently
            return await _dbSet
                .Where(c => c.FavoriteLocationIds.Any(id => id == locationId))
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Customer>> GetAnonymousCustomersAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(c => c.IsAnonymous)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Customer>> GetCustomersWithNotificationsEnabledAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(c => c.NotificationsEnabled)
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Customer>> GetCustomersByLocationPreferenceAsync(
            Guid locationId, 
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(c => c.FavoriteLocationIds.Any(id => id == locationId))
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> IsPhoneNumberUniqueAsync(string phoneNumber, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            return !await _dbSet
                .AnyAsync(c => c.PhoneNumber != null && c.PhoneNumber.Value == phoneNumber, cancellationToken);
        }

        public async Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            return !await _dbSet
                .AnyAsync(c => c.Email != null && c.Email.Value == email, cancellationToken);
        }
    }
} 