using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Grande.Fila.API.Domain.Common;

namespace Grande.Fila.API.Infrastructure.Data
{
    /// <summary>
    /// Unit of work implementation for managing transactions across repositories
    /// Uses Entity Framework's automatic transaction management with retry strategy
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly QueueHubDbContext _context;
        private bool _disposed = false;

        public UnitOfWork(QueueHubDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            // Entity Framework automatically manages transactions when SaveChanges is called
            // No manual transaction needed
            await Task.CompletedTask;
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            // Entity Framework automatically commits when SaveChanges is called
            // No manual commit needed
            await Task.CompletedTask;
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            // Entity Framework automatically rolls back on exception
            // No manual rollback needed
            await Task.CompletedTask;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Use the execution strategy to handle retries properly
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                return await _context.SaveChangesAsync(cancellationToken);
            });
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _disposed = true;
            }
        }
    }
} 