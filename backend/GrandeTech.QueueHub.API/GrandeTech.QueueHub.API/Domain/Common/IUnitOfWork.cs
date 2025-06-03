using System;
using System.Threading;
using System.Threading.Tasks;

namespace GrandeTech.QueueHub.API.Domain.Common
{
    /// <summary>
    /// Unit of work interface for managing transactions across repositories
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Begins a new transaction
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A Task representing the asynchronous operation</returns>
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Commits the current transaction
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A Task representing the asynchronous operation</returns>
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Rolls back the current transaction
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A Task representing the asynchronous operation</returns>
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves all changes made to the database in the current transaction
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The number of database rows affected</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
