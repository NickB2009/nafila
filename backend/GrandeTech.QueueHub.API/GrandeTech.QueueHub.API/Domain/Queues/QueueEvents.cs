using System;
using GrandeTech.QueueHub.API.Domain.Common;

namespace GrandeTech.QueueHub.API.Domain.Queues
{
    public class QueueCreatedEvent : DomainEvent
    {
        public Guid QueueId { get; }
        public Guid ServiceProviderId { get; }
        public DateTime QueueDate { get; }

        public QueueCreatedEvent(Guid queueId, Guid serviceProviderId, DateTime queueDate)
        {
            QueueId = queueId;
            ServiceProviderId = serviceProviderId;
            QueueDate = queueDate;
        }
    }

    public class QueueSettingsUpdatedEvent : DomainEvent
    {
        public Guid QueueId { get; }

        public QueueSettingsUpdatedEvent(Guid queueId)
        {
            QueueId = queueId;
        }
    }

    public class QueueActivatedEvent : DomainEvent
    {
        public Guid QueueId { get; }

        public QueueActivatedEvent(Guid queueId)
        {
            QueueId = queueId;
        }
    }

    public class QueueDeactivatedEvent : DomainEvent
    {
        public Guid QueueId { get; }

        public QueueDeactivatedEvent(Guid queueId)
        {
            QueueId = queueId;
        }
    }

    public class CustomerAddedToQueueEvent : DomainEvent
    {
        public Guid QueueId { get; }
        public Guid QueueEntryId { get; }
        public Guid CustomerId { get; }
        public int Position { get; }

        public CustomerAddedToQueueEvent(Guid queueId, Guid queueEntryId, Guid customerId, int position)
        {
            QueueId = queueId;
            QueueEntryId = queueEntryId;
            CustomerId = customerId;
            Position = position;
        }
    }

    public class CustomerCalledFromQueueEvent : DomainEvent
    {
        public Guid QueueId { get; }
        public Guid QueueEntryId { get; }
        public Guid CustomerId { get; }
        public Guid StaffMemberId { get; }

        public CustomerCalledFromQueueEvent(Guid queueId, Guid queueEntryId, Guid customerId, Guid staffMemberId)
        {
            QueueId = queueId;
            QueueEntryId = queueEntryId;
            CustomerId = customerId;
            StaffMemberId = staffMemberId;
        }
    }

    public class CustomerCheckedInEvent : DomainEvent
    {
        public Guid QueueId { get; }
        public Guid QueueEntryId { get; }
        public Guid CustomerId { get; }

        public CustomerCheckedInEvent(Guid queueId, Guid queueEntryId, Guid customerId)
        {
            QueueId = queueId;
            QueueEntryId = queueEntryId;
            CustomerId = customerId;
        }
    }

    public class CustomerServiceCompletedEvent : DomainEvent
    {
        public Guid QueueId { get; }
        public Guid QueueEntryId { get; }
        public Guid CustomerId { get; }
        public Guid StaffMemberId { get; }
        public int ServiceDurationMinutes { get; }

        public CustomerServiceCompletedEvent(
            Guid queueId,
            Guid queueEntryId,
            Guid customerId,
            Guid staffMemberId,
            int serviceDurationMinutes)
        {
            QueueId = queueId;
            QueueEntryId = queueEntryId;
            CustomerId = customerId;
            StaffMemberId = staffMemberId;
            ServiceDurationMinutes = serviceDurationMinutes;
        }
    }

    public class CustomerCancelledFromQueueEvent : DomainEvent
    {
        public Guid QueueId { get; }
        public Guid QueueEntryId { get; }
        public Guid CustomerId { get; }

        public CustomerCancelledFromQueueEvent(Guid queueId, Guid queueEntryId, Guid customerId)
        {
            QueueId = queueId;
            QueueEntryId = queueEntryId;
            CustomerId = customerId;
        }
    }

    public class CustomerMarkedAsNoShowEvent : DomainEvent
    {
        public Guid QueueId { get; }
        public Guid QueueEntryId { get; }
        public Guid CustomerId { get; }

        public CustomerMarkedAsNoShowEvent(Guid queueId, Guid queueEntryId, Guid customerId)
        {
            QueueId = queueId;
            QueueEntryId = queueEntryId;
            CustomerId = customerId;
        }
    }

    public class LateCustomersRemovedEvent : DomainEvent
    {
        public Guid QueueId { get; }
        public int CustomerCount { get; }

        public LateCustomersRemovedEvent(Guid queueId, int customerCount)
        {
            QueueId = queueId;
            CustomerCount = customerCount;
        }
    }

    public class CustomerNotificationSentEvent : DomainEvent
    {
        public Guid QueueId { get; }
        public Guid QueueEntryId { get; }
        public Guid CustomerId { get; }
        public string NotificationChannel { get; }

        public CustomerNotificationSentEvent(
            Guid queueId,
            Guid queueEntryId,
            Guid customerId,
            string notificationChannel)
        {
            QueueId = queueId;
            QueueEntryId = queueEntryId;
            CustomerId = customerId;
            NotificationChannel = notificationChannel;
        }
    }
}
