using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Domain.Common;

namespace Grande.Fila.API.Domain.Promotions
{
    public interface ICouponRepository : IRepository<Coupon>
    {
        Task<IReadOnlyList<Coupon>> GetActiveByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Coupon>> GetByLocationAsync(Guid locationId, CancellationToken cancellationToken = default);
    }
} 