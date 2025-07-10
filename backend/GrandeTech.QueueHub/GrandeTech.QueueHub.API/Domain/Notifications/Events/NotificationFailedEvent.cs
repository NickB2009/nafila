using System;
using Grande.Fila.API.Domain.Common;

namespace Grande.Fila.API.Domain.Notifications.Events
{
    /// <summary>
    /// Event raised when a notification fails to send
    /// </summary>
    public class NotificationFailedEvent : DomainEvent
    {
        public Guid NotificationId { get; }
        public DateTime FailedAt { get; }
        public string? ErrorMessage { get; }
        
        public NotificationFailedEvent(Guid notificationId, string? errorMessage = null)
        {
            NotificationId = notificationId;
            FailedAt = DateTime.UtcNow;
            ErrorMessage = errorMessage;
        }
    }
}
