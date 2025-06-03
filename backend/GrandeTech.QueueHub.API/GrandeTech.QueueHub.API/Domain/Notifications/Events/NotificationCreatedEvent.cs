using System;
using GrandeTech.QueueHub.API.Domain.Common;

namespace GrandeTech.QueueHub.API.Domain.Notifications.Events
{
    /// <summary>
    /// Event raised when a notification is created
    /// </summary>
    public class NotificationCreatedEvent : DomainEvent
    {
        public Guid NotificationId { get; }
        public string NotificationType { get; }
        public string RecipientType { get; }
        public string RecipientId { get; }
        public string Content { get; }
        
        public NotificationCreatedEvent(
            Guid notificationId, 
            string notificationType,
            string recipientType,
            string recipientId,
            string content)
        {
            NotificationId = notificationId;
            NotificationType = notificationType;
            RecipientType = recipientType;
            RecipientId = recipientId;
            Content = content;
        }
    }
}
