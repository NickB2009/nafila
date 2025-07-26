using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Grande.Fila.API.Domain.Users;
using Grande.Fila.API.Infrastructure.Data;

namespace Grande.Fila.API.Infrastructure.Repositories.Sql
{
    public class SqlUserRepository : IUserRepository
    {
        private readonly QueueHubDbContext _context;
        private readonly DbSet<User> _dbSet;

        public SqlUserRepository(QueueHubDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<User>();
        }

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be null or whitespace", nameof(username));

            return await _dbSet
                .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or whitespace", nameof(email));

            return await _dbSet
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }

        public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            return await _dbSet
                .AnyAsync(u => u.Username == username, cancellationToken);
        }

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            return await _dbSet
                .AnyAsync(u => u.Email == email, cancellationToken);
        }

        public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            await _dbSet.AddAsync(user, cancellationToken);
        }

        public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var existingUser = await _dbSet.FirstOrDefaultAsync(u => u.Id == user.Id, cancellationToken);
            if (existingUser == null)
            {
                throw new InvalidOperationException($"User with ID {user.Id} not found");
            }

            _context.Entry(existingUser).CurrentValues.SetValues(user);
            _context.Entry(existingUser).State = EntityState.Modified;
        }
    }
} 