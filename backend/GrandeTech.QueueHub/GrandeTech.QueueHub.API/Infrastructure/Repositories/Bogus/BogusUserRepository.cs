using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using Grande.Fila.API.Domain.Users;

namespace Grande.Fila.API.Infrastructure.Repositories.Bogus
{
    public class BogusUserRepository : IUserRepository
    {
        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(BogusDataStore.Get<User>(id));
        }

        public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            var users = BogusDataStore.GetAll<User>();
            return Task.FromResult(users.FirstOrDefault(u => u.FullName.Equals(username, StringComparison.OrdinalIgnoreCase))); // Map username to FullName
        }


        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var users = BogusDataStore.GetAll<User>();
            return Task.FromResult(users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));
        }

        public Task<User?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
        {
            var users = BogusDataStore.GetAll<User>();
            return Task.FromResult(users.FirstOrDefault(u => u.PhoneNumber.Equals(phoneNumber, StringComparison.OrdinalIgnoreCase)));
        }


        public Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            var users = BogusDataStore.GetAll<User>();
            return Task.FromResult(users.Any(u => u.FullName.Equals(username, StringComparison.OrdinalIgnoreCase))); // Map username to FullName
        }

        public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var users = BogusDataStore.GetAll<User>();
            return Task.FromResult(users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));
        }

        public Task<bool> ExistsByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
        {
            var users = BogusDataStore.GetAll<User>();
            return Task.FromResult(users.Any(u => u.PhoneNumber.Equals(phoneNumber, StringComparison.OrdinalIgnoreCase)));
        }

        public Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            BogusDataStore.Add(user);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            BogusDataStore.Update(user);
            return Task.CompletedTask;
        }
    }
} 