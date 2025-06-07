using System;
using GrandeTech.QueueHub.API.Domain.Common;
using GrandeTech.QueueHub.API.Domain.Notifications.Events;

namespace GrandeTech.QueueHub.API.Domain.Notifications
{
    public enum NotificationType
    {
        QueueTurn,
        CouponPromotion,
        ReturnReminder,
        ServiceComplete,
        StatusUpdate,
        SystemMessage
    }

    public enum NotificationChannel
    {
        SMS,
        Email,
        WhatsApp,
        Push,
        InApp
    }

    public enum NotificationStatus
    {
        Pending,
        Sent,
        Failed,
        Delivered,
        Read
    }

    /// <summary>
    /// Represents a notification sent to customers or staff members
    /// </summary>
    public class Notification : BaseEntity, IAggregateRoot
    {
        public Guid RecipientId { get; private set; }
        public string RecipientType { get; private set; } = string.Empty; // Customer, StaffMember
        public string Title { get; private set; } = string.Empty;
        public string Content { get; private set; } = string.Empty;
        public NotificationType Type { get; private set; }
        public NotificationChannel Channel { get; private set; }
        public NotificationStatus Status { get; private set; }
        public Guid? ReferenceId { get; private set; } // Could be QueueEntryId, CouponId, etc.
        public string? ReferenceType { get; private set; } // The type of entity referenced
        public DateTime? ScheduledFor { get; private set; }
        public DateTime? SentAt { get; private set; }
        public DateTime? DeliveredAt { get; private set; }
        public DateTime? ReadAt { get; private set; }
        public string? ErrorMessage { get; private set; }
        public int RetryCount { get; private set; }
        public string? DeepLink { get; private set; }

        // For EF Core
        private Notification() { }

        public Notification(
            Guid recipientId,
            string recipientType,
            string title,
            string content,
            NotificationType type,
            NotificationChannel channel,
            Guid? referenceId = null,
            string? referenceType = null,
            DateTime? scheduledFor = null,
            string? deepLink = null)
        {
            if (recipientId == Guid.Empty)
                throw new ArgumentException("Recipient ID is required", nameof(recipientId));

            if (string.IsNullOrWhiteSpace(recipientType))
                throw new ArgumentException("Recipient type is required", nameof(recipientType));

            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title is required", nameof(title));

            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Content is required", nameof(content));

            RecipientId = recipientId;
            RecipientType = recipientType;
            Title = title;
            Content = content;
            Type = type;
            Channel = channel;
            Status = NotificationStatus.Pending;
            ReferenceId = referenceId;
            ReferenceType = referenceType;
            ScheduledFor = scheduledFor;
            DeepLink = deepLink;
            RetryCount = 0;

            AddDomainEvent(new NotificationCreatedEvent(Id, Type.ToString(), RecipientType, RecipientId.ToString(), Content));
        }

        // Domain behavior methods
        public void MarkAsSent(string? error = null)
        {
            if (Status == NotificationStatus.Sent || Status == NotificationStatus.Delivered || Status == NotificationStatus.Read)
                throw new InvalidOperationException($"Notification status cannot be changed from {Status} to Sent");

            Status = error == null ? NotificationStatus.Sent : NotificationStatus.Failed;
            SentAt = DateTime.UtcNow;
            ErrorMessage = error;            if (Status == NotificationStatus.Sent)
            {
                AddDomainEvent(new NotificationSentEvent(Id));
            }
            else
            {
                AddDomainEvent(new NotificationFailedEvent(Id, ErrorMessage));
            }
        }

        public void MarkAsDelivered()
        {
            if (Status != NotificationStatus.Sent)
                throw new InvalidOperationException($"Notification must be in Sent status to mark as Delivered, current status: {Status}");

            Status = NotificationStatus.Delivered;
            DeliveredAt = DateTime.UtcNow;
            AddDomainEvent(new NotificationDeliveredEvent(Id));
        }

        public void MarkAsRead()
        {
            if (Status != NotificationStatus.Sent && Status != NotificationStatus.Delivered)
                throw new InvalidOperationException($"Notification must be in Sent or Delivered status to mark as Read, current status: {Status}");

            Status = NotificationStatus.Read;
            ReadAt = DateTime.UtcNow;
            AddDomainEvent(new NotificationReadEvent(Id));
        }

        public void IncrementRetryCount(string? error = null)
        {
            RetryCount++;
            ErrorMessage = error;
            AddDomainEvent(new NotificationRetryEvent(Id, RetryCount));
        }

        public bool ShouldSend()
        {
            var now = DateTime.UtcNow;
            
            if (Status != NotificationStatus.Pending)
                return false;
            
            if (ScheduledFor.HasValue && now < ScheduledFor.Value)
                return false;
            
            return true;
        }

        public bool CanRetry(int maxRetries)
        {
            return Status == NotificationStatus.Failed && RetryCount < maxRetries;
        }

        public static Notification CreateQueueTurnNotification(
            Guid customerId,
            int position,
            string customerName,
            string ServicesProviderName,
            NotificationChannel channel,
            Guid queueEntryId,
            string deepLink)
        {
            var title = $"É quase sua vez, {customerName}!";
            var content = $"Você está na {position}ª posição na fila da {ServicesProviderName}. Em breve será atendido.";
            
            return new Notification(
                customerId,
                "Customer",
                title,
                content,
                NotificationType.QueueTurn,
                channel,
                queueEntryId,
                "QueueEntry",
                null,
                deepLink);
        }

        public static Notification CreateCouponNotification(
            Guid customerId,
            string customerName,
            string couponCode,
            string discount,
            string expirationDate,
            string ServicesProviderName,
            NotificationChannel channel,
            Guid couponId,
            string deepLink)
        {
            var title = $"Cupom especial para você, {customerName}!";
            var content = $"Use o código {couponCode} para ganhar {discount}% de desconto na {ServicesProviderName}. Válido até {expirationDate}.";
            
            return new Notification(
                customerId,
                "Customer",
                title,
                content,
                NotificationType.CouponPromotion,
                channel,
                couponId,
                "Coupon",
                null,
                deepLink);
        }

        public static Notification CreateReturnReminderNotification(
            Guid staffMemberId,
            string staffMemberName,
            int minutesLeft,
            NotificationChannel channel,
            Guid breakId)
        {
            var title = "Hora de voltar";
            var content = $"{staffMemberName}, seu intervalo termina em {minutesLeft} minutos.";
            
            return new Notification(
                staffMemberId,
                "StaffMember",
                title,
                content,
                NotificationType.ReturnReminder,
                channel,
                breakId,
                "StaffBreak");
        }
    }
}
