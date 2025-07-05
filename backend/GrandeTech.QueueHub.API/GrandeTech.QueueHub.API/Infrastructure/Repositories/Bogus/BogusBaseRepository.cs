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
        public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(BogusDataStore.Get<T>(id));
        }

        public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(BogusDataStore.GetAll<T>());
        }

        public virtual async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var allItems = BogusDataStore.GetAll<T>();
            return await Task.FromResult(allItems.AsQueryable().Where(predicate).ToList());
        }

        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity.Id == Guid.Empty)
            {
                var newEntity = CreateNewEntityWithId(entity, Guid.NewGuid());
                BogusDataStore.Add(newEntity);
                return await Task.FromResult(newEntity);
            }
            else
            {
                BogusDataStore.Add(entity);
                return await Task.FromResult(entity);
            }
        }

        public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            var existing = BogusDataStore.Get<T>(entity.Id);
            if (existing == null)
            {
                throw new KeyNotFoundException($"Entity with ID {entity.Id} not found");
            }
            BogusDataStore.Update(entity);
            return await Task.FromResult(entity);
        }

        public virtual async Task<bool> DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(BogusDataStore.Remove<T>(entity.Id));
        }

        public virtual async Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(BogusDataStore.Remove<T>(id));
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var allItems = BogusDataStore.GetAll<T>();
            return await Task.FromResult(allItems.AsQueryable().Any(predicate));
        }

        // Helper method to create a new entity with a specific Id
        protected abstract T CreateNewEntityWithId(T entity, Guid id);
    }
} 