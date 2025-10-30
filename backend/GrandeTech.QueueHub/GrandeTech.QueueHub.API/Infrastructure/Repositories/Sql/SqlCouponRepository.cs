using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Grande.Fila.API.Domain.Promotions;
using Grande.Fila.API.Infrastructure.Data;

namespace Grande.Fila.API.Infrastructure.Repositories.Sql
{
    public class SqlCouponRepository : SqlBaseRepository<Coupon>, ICouponRepository
    {
        public SqlCouponRepository(QueueHubDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<Coupon>> GetActiveByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(c => c.IsActive && c.EndDate >= DateTime.UtcNow)
                .OrderBy(c => c.Code)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Coupon>> GetByLocationAsync(Guid locationId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(c => c.LocationId == locationId)
                .OrderBy(c => c.Code)
                .ToListAsync(cancellationToken);
        }
    }
}

