using System;
using Grande.Fila.API.Domain.Common;

namespace Grande.Fila.API.Domain.Notifications.Events
{
    /// <summary>
    /// Event raised when a notification is read by the recipient
    /// </summary>
    public class NotificationReadEvent : DomainEvent
    {
        public Guid NotificationId { get; }
        public DateTime ReadAt { get; }
        
        public NotificationReadEvent(Guid notificationId)
        {
            NotificationId = notificationId;
            ReadAt = DateTime.UtcNow;
        }
    }
}
