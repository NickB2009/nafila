using System;
using Grande.Fila.API.Domain.Common;

namespace Grande.Fila.API.Domain.Notifications.Events
{
    /// <summary>
    /// Event raised when a notification is sent
    /// </summary>
    public class NotificationSentEvent : DomainEvent
    {
        public Guid NotificationId { get; }
        public DateTime SentAt { get; }
        public string? ProviderReference { get; }
        
        public NotificationSentEvent(Guid notificationId, string? providerReference = null)
        {
            NotificationId = notificationId;
            SentAt = DateTime.UtcNow;
            ProviderReference = providerReference;
        }
    }
}
