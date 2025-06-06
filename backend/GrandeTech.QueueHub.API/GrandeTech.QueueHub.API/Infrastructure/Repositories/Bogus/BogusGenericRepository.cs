using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Domain.Common;

namespace GrandeTech.QueueHub.API.Infrastructure.Repositories.Bogus
{
    public class BogusGenericRepository<T> : BogusBaseRepository<T> where T : BaseEntity, IAggregateRoot
    {
        protected override T CreateNewEntityWithId(T entity, Guid id)
        {
            // For generic entities, we'll just set the ID using reflection
            var idProperty = typeof(BaseEntity).GetProperty("Id");
            idProperty?.SetValue(entity, id);
            return entity;
        }
    }
} 