using System;
using GrandeTech.QueueHub.API.Domain.Common;

namespace GrandeTech.QueueHub.API.Domain.Subscriptions
{
    public class SubscriptionPlanCreatedEvent : DomainEvent
    {
        public Guid SubscriptionPlanId { get; }
        public string SubscriptionPlanName { get; }

        public SubscriptionPlanCreatedEvent(Guid subscriptionPlanId, string subscriptionPlanName)
        {
            SubscriptionPlanId = subscriptionPlanId;
            SubscriptionPlanName = subscriptionPlanName;
        }
    }

    public class SubscriptionPlanUpdatedEvent : DomainEvent
    {
        public Guid SubscriptionPlanId { get; }

        public SubscriptionPlanUpdatedEvent(Guid subscriptionPlanId)
        {
            SubscriptionPlanId = subscriptionPlanId;
        }
    }

    public class SubscriptionPlanActivatedEvent : DomainEvent
    {
        public Guid SubscriptionPlanId { get; }

        public SubscriptionPlanActivatedEvent(Guid subscriptionPlanId)
        {
            SubscriptionPlanId = subscriptionPlanId;
        }
    }

    public class SubscriptionPlanDeactivatedEvent : DomainEvent
    {
        public Guid SubscriptionPlanId { get; }

        public SubscriptionPlanDeactivatedEvent(Guid subscriptionPlanId)
        {
            SubscriptionPlanId = subscriptionPlanId;
        }
    }
}
