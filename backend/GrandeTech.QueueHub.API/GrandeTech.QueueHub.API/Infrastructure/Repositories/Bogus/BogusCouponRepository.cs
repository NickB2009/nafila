using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Domain.Common;
using Grande.Fila.API.Domain.Promotions;

namespace Grande.Fila.API.Infrastructure.Repositories.Bogus
{
    public class BogusCouponRepository : BogusBaseRepository<Coupon>, ICouponRepository
    {
        protected override Coupon CreateNewEntityWithId(Coupon entity, Guid id)
        {
            var coupon = new Coupon(
                entity.Code,
                entity.Description,
                entity.LocationId,
                entity.DiscountPercentage,
                entity.FixedDiscountAmount?.Amount,
                entity.StartDate,
                entity.EndDate,
                entity.MaxUsageCount,
                entity.RequiresLogin,
                entity.ApplicableServiceTypeIds,
                entity.CreatedBy ?? "system");
            
            // Set the ID using reflection since it's protected
            var idProperty = typeof(BaseEntity).GetProperty("Id");
            idProperty?.SetValue(coupon, id);
            
            return coupon;
        }

        public async Task<IReadOnlyList<Coupon>> GetActiveByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            var coupons = await GetAllAsync(cancellationToken);
            return coupons.Where(c => c.IsActive && c.EndDate > DateTime.UtcNow).ToList();
        }

        public async Task<IReadOnlyList<Coupon>> GetByLocationAsync(Guid locationId, CancellationToken cancellationToken = default)
        {
            var coupons = await GetAllAsync(cancellationToken);
            // For mock data, return all coupons (in real implementation, filter by location)
            return coupons.ToList();
        }
    }
} 