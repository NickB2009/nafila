using System;
using GrandeTech.QueueHub.API.Domain.Common;

namespace GrandeTech.QueueHub.API.Domain.Notifications.Events
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
