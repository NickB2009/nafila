using System;
using GrandeTech.QueueHub.API.Domain.Common;

namespace GrandeTech.QueueHub.API.Domain.Customers
{
    public class CustomerCreatedEvent : DomainEvent
    {
        public Guid CustomerId { get; }
        public string CustomerName { get; }
        public bool IsAnonymous { get; }

        public CustomerCreatedEvent(Guid customerId, string customerName, bool isAnonymous)
        {
            CustomerId = customerId;
            CustomerName = customerName;
            IsAnonymous = isAnonymous;
        }
    }

    public class CustomerProfileUpdatedEvent : DomainEvent
    {
        public Guid CustomerId { get; }

        public CustomerProfileUpdatedEvent(Guid customerId)
        {
            CustomerId = customerId;
        }
    }

    public class CustomerConvertedToRegisteredEvent : DomainEvent
    {
        public Guid CustomerId { get; }

        public CustomerConvertedToRegisteredEvent(Guid customerId)
        {
            CustomerId = customerId;
        }
    }

    public class CustomerNotificationsEnabledEvent : DomainEvent
    {
        public Guid CustomerId { get; }

        public CustomerNotificationsEnabledEvent(Guid customerId)
        {
            CustomerId = customerId;
        }
    }

    public class CustomerNotificationsDisabledEvent : DomainEvent
    {
        public Guid CustomerId { get; }

        public CustomerNotificationsDisabledEvent(Guid customerId)
        {
            CustomerId = customerId;
        }
    }

    public class CustomerNotificationChannelChangedEvent : DomainEvent
    {
        public Guid CustomerId { get; }
        public string Channel { get; }

        public CustomerNotificationChannelChangedEvent(Guid customerId, string channel)
        {
            CustomerId = customerId;
            Channel = channel;
        }
    }

    public class CustomerServiceHistoryAddedEvent : DomainEvent
    {
        public Guid CustomerId { get; }
        public Guid ServiceHistoryItemId { get; }

        public CustomerServiceHistoryAddedEvent(Guid customerId, Guid serviceHistoryItemId)
        {
            CustomerId = customerId;
            ServiceHistoryItemId = serviceHistoryItemId;
        }
    }

    public class CustomerFavoriteLocationAddedEvent : DomainEvent
    {
        public Guid CustomerId { get; }
        public Guid LocationId { get; }

        public CustomerFavoriteLocationAddedEvent(Guid customerId, Guid locationId)
        {
            CustomerId = customerId;
            LocationId = locationId;
        }
    }

    public class CustomerFavoriteLocationRemovedEvent : DomainEvent
    {
        public Guid CustomerId { get; }
        public Guid LocationId { get; }

        public CustomerFavoriteLocationRemovedEvent(Guid customerId, Guid locationId)
        {
            CustomerId = customerId;
            LocationId = locationId;
        }
    }

    public class CustomerConnectedToUserEvent : DomainEvent
    {
        public Guid CustomerId { get; }
        public string UserId { get; }

        public CustomerConnectedToUserEvent(Guid customerId, string userId)
        {
            CustomerId = customerId;
            UserId = userId;
        }
    }
}
