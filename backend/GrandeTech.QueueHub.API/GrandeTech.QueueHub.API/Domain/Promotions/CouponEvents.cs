using System;
using GrandeTech.QueueHub.API.Domain.Common;

namespace GrandeTech.QueueHub.API.Domain.Promotions
{
    public class CouponCreatedEvent : DomainEvent
    {
        public Guid CouponId { get; }
        public string Code { get; }
        public Guid ServicesProviderId { get; }

        public CouponCreatedEvent(Guid couponId, string code, Guid ServicesProviderId)
        {
            CouponId = couponId;
            Code = code;
            ServicesProviderId = ServicesProviderId;
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
