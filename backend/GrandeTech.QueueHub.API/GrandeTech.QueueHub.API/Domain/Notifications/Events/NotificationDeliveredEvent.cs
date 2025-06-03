using System;
using GrandeTech.QueueHub.API.Domain.Common;

namespace GrandeTech.QueueHub.API.Domain.Notifications.Events
{
    /// <summary>
    /// Event raised when a notification is delivered to the recipient
    /// </summary>
    public class NotificationDeliveredEvent : DomainEvent
    {
        public Guid NotificationId { get; }
        public DateTime DeliveredAt { get; }
        
        public NotificationDeliveredEvent(Guid notificationId)
        {
            NotificationId = notificationId;
            DeliveredAt = DateTime.UtcNow;
        }
    }
}
