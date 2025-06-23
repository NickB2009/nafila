using System;
using Grande.Fila.API.Domain.Common;

namespace Grande.Fila.API.Domain.Promotions
{
    public class CouponCreatedEvent : DomainEvent
    {
        public Guid CouponId { get; }
        public string Code { get; }
        public Guid LocationId { get; }        public CouponCreatedEvent(Guid couponId, string code, Guid locationId)
        {
            CouponId = couponId;
            Code = code;
            LocationId = locationId;
        }
    }

    public class CouponUpdatedEvent : DomainEvent
    {
        public Guid CouponId { get; }

        public CouponUpdatedEvent(Guid couponId)
        {
            CouponId = couponId;
        }
    }

    public class CouponActivatedEvent : DomainEvent
    {
        public Guid CouponId { get; }

        public CouponActivatedEvent(Guid couponId)
        {
            CouponId = couponId;
        }
    }

    public class CouponDeactivatedEvent : DomainEvent
    {
        public Guid CouponId { get; }

        public CouponDeactivatedEvent(Guid couponId)
        {
            CouponId = couponId;
        }
    }

    public class CouponRedeemedEvent : DomainEvent
    {
        public Guid CouponId { get; }
        public Guid CustomerId { get; }
        public Guid? QueueEntryId { get; }

        public CouponRedeemedEvent(Guid couponId, Guid customerId, Guid? queueEntryId)
        {
            CouponId = couponId;
            CustomerId = customerId;
            QueueEntryId = queueEntryId;
        }
    }
}
