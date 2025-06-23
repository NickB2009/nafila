using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Domain.Common;

namespace Grande.Fila.API.Infrastructure.Repositories.Bogus
{
    public abstract class BogusBaseRepository<T> : IRepository<T> where T : BaseEntity, IAggregateRoot
    {
        protected static readonly Dictionary<Guid, T> _items = new();
        protected static readonly object _lock = new();

        public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(_items.TryGetValue(id, out var item) ? item : null);
        }

        public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(_items.Values.ToList());
        }

        public virtual async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(_items.Values.AsQueryable().Where(predicate).ToList());
        }

        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            T result;
            lock (_lock)
            {
                if (entity.Id == Guid.Empty)
                {
                    result = CreateNewEntityWithId(entity, Guid.NewGuid());
                    _items[result.Id] = result;
                }
                else
                {
                    _items[entity.Id] = entity;
                    result = entity;
                }
            }
            return await Task.FromResult(result);
        }

        public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (!_items.ContainsKey(entity.Id))
                {
                    throw new KeyNotFoundException($"Entity with ID {entity.Id} not found");
                }
                _items[entity.Id] = entity;
            }
            return await Task.FromResult(entity);
        }

        public virtual async Task<bool> DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            bool result;
            lock (_lock)
            {
                result = _items.Remove(entity.Id);
            }
            return await Task.FromResult(result);
        }

        public virtual async Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            bool result;
            lock (_lock)
            {
                result = _items.Remove(id);
            }
            return await Task.FromResult(result);
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(_items.Values.AsQueryable().Any(predicate));
        }

        // Helper method to create a new entity with a specific Id
        protected abstract T CreateNewEntityWithId(T entity, Guid id);
    }
} 