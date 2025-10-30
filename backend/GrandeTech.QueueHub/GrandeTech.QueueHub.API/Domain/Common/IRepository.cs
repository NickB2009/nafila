using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Grande.Fila.API.Domain.Common
{
    /// <summary>
    /// Generic repository interface
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public interface IRepository<T> where T : BaseEntity
    {
        /// <summary>
        /// Gets an entity by its ID
        /// </summary>
        /// <param name="id">The entity ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The entity or null if not found</returns>
        Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all entities
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of all entities</returns>
        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds entities that match the specified predicate
        /// </summary>
        /// <param name="predicate">The filter expression</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of matching entities</returns>
        Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a new entity
        /// </summary>
        /// <param name="entity">The entity to add</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The added entity</returns>
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing entity
        /// </summary>
        /// <param name="entity">The entity to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The updated entity</returns>
        Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes an entity
        /// </summary>
        /// <param name="entity">The entity to remove</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> DeleteAsync(T entity, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Removes an entity by ID
        /// </summary>
        /// <param name="id">The ID of the entity to remove</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if any entity matches the specified predicate
        /// </summary>
        /// <param name="predicate">The filter expression</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if any entity matches, false otherwise</returns>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    }
}
