using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Grande.Fila.API.Domain.Customers;
using Grande.Fila.API.Domain.Common.ValueObjects;
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

            // Create PhoneNumber value object for comparison
            // EF Core will use the configured converter to translate this to the database column
            var phoneNumberValueObject = PhoneNumber.Create(phoneNumber);
            return await _dbSet
                .Where(c => c.PhoneNumber != null && c.PhoneNumber == phoneNumberValueObject)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or whitespace", nameof(email));

            // Create Email value object for comparison
            // EF Core will use the configured converter to translate this to the database column
            var emailValueObject = Email.Create(email);
            return await _dbSet
                .Where(c => c.Email != null && c.Email == emailValueObject)
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

            // Create PhoneNumber value object for comparison
            var phoneNumberValueObject = PhoneNumber.Create(phoneNumber);
            return !await _dbSet
                .AnyAsync(c => c.PhoneNumber != null && c.PhoneNumber == phoneNumberValueObject, cancellationToken);
        }

        public async Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            // Create Email value object for comparison
            var emailValueObject = Email.Create(email);
            return !await _dbSet
                .AnyAsync(c => c.Email != null && c.Email == emailValueObject, cancellationToken);
        }

        // Removed immediate SaveChanges - let service layer handle transactions
    }
} 