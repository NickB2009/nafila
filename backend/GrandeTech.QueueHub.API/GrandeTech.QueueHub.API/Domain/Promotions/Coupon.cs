using System;
using System.Collections.Generic;
using Grande.Fila.API.Domain.Common;
using Grande.Fila.API.Domain.Common.ValueObjects;

namespace Grande.Fila.API.Domain.Promotions
{    /// <summary>
    /// Represents a coupon that can be used by customers to receive discounts
    /// </summary>
    public class Coupon : BaseEntity, IAggregateRoot
    {
        public string Code { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public Guid LocationId { get; private set; }
        public decimal DiscountPercentage { get; private set; }
        public Money? FixedDiscountAmount { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public int MaxUsageCount { get; private set; }
        public int CurrentUsageCount { get; private set; }
        public bool IsActive { get; private set; }
        public bool RequiresLogin { get; private set; }

        // Navigation properties
        private readonly List<Guid>? _applicableServiceTypeIds;
        public IReadOnlyCollection<Guid>? ApplicableServiceTypeIds => _applicableServiceTypeIds?.AsReadOnly();        // For EF Core
        private Coupon() { }

        public Coupon(
            string code,
            string description,
            Guid locationId,
            decimal discountPercentage,
            decimal? fixedDiscountAmount,
            DateTime startDate,
            DateTime endDate,
            int maxUsageCount,
            bool requiresLogin,
            IEnumerable<Guid>? applicableServiceTypeIds,
            string createdBy)
        {            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Coupon code is required", nameof(code));

            if (locationId == Guid.Empty)
                throw new ArgumentException("Location ID is required", nameof(locationId));

            if (discountPercentage < 0 || discountPercentage > 100)
                throw new ArgumentException("Discount percentage must be between 0 and 100", nameof(discountPercentage));

            if (fixedDiscountAmount.HasValue && fixedDiscountAmount.Value <= 0)
                throw new ArgumentException("Fixed discount amount must be positive", nameof(fixedDiscountAmount));

            if (endDate <= startDate)
                throw new ArgumentException("End date must be after start date", nameof(endDate));

            if (maxUsageCount <= 0)
                throw new ArgumentException("Max usage count must be positive", nameof(maxUsageCount));            Code = code.ToUpperInvariant();
            Description = description ?? string.Empty;
            LocationId = locationId;
            DiscountPercentage = discountPercentage;
            FixedDiscountAmount = fixedDiscountAmount.HasValue ? Money.Create(fixedDiscountAmount.Value) : null;
            StartDate = startDate;
            EndDate = endDate;
            MaxUsageCount = maxUsageCount;
            CurrentUsageCount = 0;
            IsActive = true;
            RequiresLogin = requiresLogin;
            CreatedBy = createdBy;            if (applicableServiceTypeIds != null)
            {
                _applicableServiceTypeIds = new List<Guid>(applicableServiceTypeIds);
            }

            AddDomainEvent(new CouponCreatedEvent(Id, Code, LocationId));
        }

        // Domain behavior methods
        public void UpdateDetails(
            string description,
            decimal discountPercentage,
            decimal? fixedDiscountAmount,
            DateTime startDate,
            DateTime endDate,
            int maxUsageCount,
            bool requiresLogin,
            IEnumerable<Guid>? applicableServiceTypeIds,
            string updatedBy)
        {
            if (discountPercentage < 0 || discountPercentage > 100)
                throw new ArgumentException("Discount percentage must be between 0 and 100", nameof(discountPercentage));

            if (fixedDiscountAmount.HasValue && fixedDiscountAmount.Value <= 0)
                throw new ArgumentException("Fixed discount amount must be positive", nameof(fixedDiscountAmount));

            if (endDate <= startDate)
                throw new ArgumentException("End date must be after start date", nameof(endDate));

            if (maxUsageCount <= 0)
                throw new ArgumentException("Max usage count must be positive", nameof(maxUsageCount));

            Description = description ?? string.Empty;
            DiscountPercentage = discountPercentage;
            FixedDiscountAmount = fixedDiscountAmount.HasValue ? Money.Create(fixedDiscountAmount.Value) : null;
            StartDate = startDate;
            EndDate = endDate;
            MaxUsageCount = maxUsageCount;
            RequiresLogin = requiresLogin;

            if (_applicableServiceTypeIds != null)
            {
                _applicableServiceTypeIds.Clear();
                if (applicableServiceTypeIds != null)
                {
                    _applicableServiceTypeIds.AddRange(applicableServiceTypeIds);
                }
            }
            
            MarkAsModified(updatedBy);
            AddDomainEvent(new CouponUpdatedEvent(Id));
        }

        public void Activate(string updatedBy)
        {
            if (!IsActive)
            {
                IsActive = true;
                MarkAsModified(updatedBy);
                AddDomainEvent(new CouponActivatedEvent(Id));
            }
        }

        public void Deactivate(string updatedBy)
        {
            if (IsActive)
            {
                IsActive = false;
                MarkAsModified(updatedBy);
                AddDomainEvent(new CouponDeactivatedEvent(Id));
            }
        }

        public bool IsApplicableToServiceType(Guid serviceTypeId)
        {
            return _applicableServiceTypeIds == null || _applicableServiceTypeIds.Count == 0 || _applicableServiceTypeIds.Contains(serviceTypeId);
        }

        public bool IsValid()
        {
            var now = DateTime.UtcNow;
            
            if (!IsActive)
                return false;
            
            if (now < StartDate || now > EndDate)
                return false;
            
            if (CurrentUsageCount >= MaxUsageCount)
                return false;
            
            return true;
        }

        public CouponRedemption Redeem(Guid customerId, Guid? queueEntryId = null)
        {
            if (!IsValid())
                throw new InvalidOperationException("Coupon is not valid");

            CurrentUsageCount++;
            
            var redemption = new CouponRedemption(Id, customerId, queueEntryId);
            AddDomainEvent(new CouponRedeemedEvent(Id, customerId, queueEntryId));
            
            return redemption;
        }

        public Money CalculateDiscount(Money originalPrice)
        {
            if (FixedDiscountAmount != null)
            {
                // Prevent discount being larger than original price
                var maxDiscount = originalPrice.Amount;
                var discountAmount = Math.Min(FixedDiscountAmount.Amount, maxDiscount);
                return Money.Create(discountAmount, originalPrice.Currency);
            }
            else
            {
                var discountAmount = originalPrice.Amount * DiscountPercentage / 100;
                return Money.Create(discountAmount, originalPrice.Currency);
            }
        }

        public Money GetFinalPrice(Money originalPrice)
        {
            return originalPrice.Subtract(CalculateDiscount(originalPrice));
        }
    }

    public class CouponRedemption
    {
        public Guid Id { get; private set; }
        public Guid CouponId { get; private set; }
        public Guid CustomerId { get; private set; }
        public Guid? QueueEntryId { get; private set; }
        public DateTime RedeemedAt { get; private set; }

        // For EF Core
        private CouponRedemption() { }

        public CouponRedemption(
            Guid couponId,
            Guid customerId,
            Guid? queueEntryId = null)
        {
            Id = Guid.NewGuid();
            CouponId = couponId;
            CustomerId = customerId;
            QueueEntryId = queueEntryId;
            RedeemedAt = DateTime.UtcNow;
        }
    }
}
