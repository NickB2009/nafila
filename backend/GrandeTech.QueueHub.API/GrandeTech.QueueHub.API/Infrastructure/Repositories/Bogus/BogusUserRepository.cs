using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using Grande.Fila.API.Domain.Common;
using Grande.Fila.API.Domain.Users;

namespace Grande.Fila.API.Infrastructure.Repositories.Bogus
{
    public class BogusUserRepository : BogusBaseRepository<User>, IUserRepository
    {        public BogusUserRepository()
        {
            lock (_lock)
            {
                // Only initialize if not already initialized
                if (_items.Count == 0)
                {
                    var faker = new Faker<User>()
                        .CustomInstantiator(f => new User(
                            f.Internet.UserName(),
                            f.Internet.Email(),
                            BCrypt.Net.BCrypt.HashPassword("password123"),
                            f.PickRandom(new[] { "Admin", "Staff", "Customer" })
                        ))
                        .RuleFor(u => u.Id, f => f.Random.Guid())
                        .RuleFor(u => u.IsActive, f => f.Random.Bool())
                        .RuleFor(u => u.CreatedAt, f => f.Date.Past())
                        .RuleFor(u => u.LastLoginAt, f => f.Date.Recent());

                    var users = faker.Generate(50);
                    foreach (var user in users)
                    {
                        _items[user.Id] = user;
                    }
                }
            }
        }public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            User? result;
            lock (_lock)
            {
                result = _items.Values.FirstOrDefault(u => u.Username == username);
            }
            return await Task.FromResult(result);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            User? result;
            lock (_lock)
            {
                result = _items.Values.FirstOrDefault(u => u.Email == email);
            }
            return await Task.FromResult(result);
        }

        public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            bool result;
            lock (_lock)
            {
                result = _items.Values.Any(u => u.Username == username);
            }
            return await Task.FromResult(result);
        }

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            bool result;
            lock (_lock)
            {
                result = _items.Values.Any(u => u.Email == email);
            }
            return await Task.FromResult(result);
        }

        public new async Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            await base.AddAsync(user, cancellationToken);
        }

        public new async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            await base.UpdateAsync(user, cancellationToken);
        }

        protected override User CreateNewEntityWithId(User entity, Guid id)
        {
            // Create a new user with the same properties using the constructor
            var newUser = new User(
                entity.Username,
                entity.Email,
                entity.PasswordHash,
                entity.Role
            );

            // Set the ID using reflection since it's protected
            var baseEntity = typeof(BaseEntity);
            var idProperty = baseEntity.GetProperty("Id");
            idProperty?.SetValue(newUser, id);

            // Copy the state of IsActive and LastLoginAt
            if (!entity.IsActive)
            {
                newUser.Deactivate();
            }
            if (entity.LastLoginAt.HasValue)
            {
                newUser.UpdateLastLogin();
            }

            return newUser;
        }
    }
} 