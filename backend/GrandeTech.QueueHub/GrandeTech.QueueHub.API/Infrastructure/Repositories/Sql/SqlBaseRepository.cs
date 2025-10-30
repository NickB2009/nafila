using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Grande.Fila.API.Domain.Common;
using Grande.Fila.API.Infrastructure.Data;

namespace Grande.Fila.API.Infrastructure.Repositories.Sql
{
    public abstract class SqlBaseRepository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly QueueHubDbContext _context;
        protected readonly DbSet<T> _dbSet;

        protected SqlBaseRepository(QueueHubDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.ToListAsync(cancellationToken);
        }

        public virtual async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
        }

        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var entry = await _dbSet.AddAsync(entity, cancellationToken);
            return entry.Entity;
        }

        public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var existingEntity = await _dbSet.FirstOrDefaultAsync(e => e.Id == entity.Id, cancellationToken);
            if (existingEntity == null)
            {
                throw new KeyNotFoundException($"Entity with ID {entity.Id} not found");
            }

            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            _context.Entry(existingEntity).State = EntityState.Modified;

            return existingEntity;
        }

        public virtual async Task<bool> DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return await DeleteByIdAsync(entity.Id, cancellationToken);
        }

        public virtual async Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
            if (entity == null)
                return false;

            // Soft delete by default
            entity.IsDeleted = true;
            _context.Entry(entity).State = EntityState.Modified;

            return true;
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(predicate, cancellationToken);
        }

        // Additional helper methods for SQL repositories
        protected async Task<T?> GetByIdWithIncludesAsync(Guid id, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes)
        {
            var query = _dbSet.AsQueryable();
            
            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        protected async Task<IReadOnlyList<T>> FindWithIncludesAsync(
            Expression<Func<T, bool>> predicate, 
            CancellationToken cancellationToken = default, 
            params Expression<Func<T, object>>[] includes)
        {
            var query = _dbSet.AsQueryable();
            
            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.Where(predicate).ToListAsync(cancellationToken);
        }

        protected async Task<IReadOnlyList<T>> GetAllWithIncludesAsync(CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes)
        {
            var query = _dbSet.AsQueryable();
            
            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.ToListAsync(cancellationToken);
        }

        // Pagination support
        protected async Task<(IReadOnlyList<T> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? filter = null,
            Expression<Func<T, object>>? orderBy = null,
            bool orderByDescending = false,
            CancellationToken cancellationToken = default,
            params Expression<Func<T, object>>[] includes)
        {
            var query = _dbSet.AsQueryable();

            // Apply includes
            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            // Apply filter
            if (filter != null)
            {
                query = query.Where(filter);
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply ordering
            if (orderBy != null)
            {
                query = orderByDescending 
                    ? query.OrderByDescending(orderBy)
                    : query.OrderBy(orderBy);
            }

            // Apply pagination
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
    }
} 