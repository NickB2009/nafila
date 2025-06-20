using System;
using Grande.Fila.API.Domain.Common;

namespace Grande.Fila.API.Domain.Notifications.Events
{
    /// <summary>
    /// Event raised when a notification retry is attempted
    /// </summary>
    public class NotificationRetryEvent : DomainEvent
    {
        public Guid NotificationId { get; }
        public DateTime RetryAt { get; }
        public int RetryCount { get; }
        
        public NotificationRetryEvent(Guid notificationId, int retryCount)
        {
            NotificationId = notificationId;
            RetryAt = DateTime.UtcNow;
            RetryCount = retryCount;
        }
    }
}
