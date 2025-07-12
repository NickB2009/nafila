using System;
using System.Collections.Generic;

namespace Grande.Fila.API.Application.Queues
{
    public enum QueueMessageType
    {
        SmsNotification,
        AnalyticsProcessing,
        AuditLogging,
        CacheInvalidation,
        QueueStateChange,
        EmailNotification,
        WebhookDelivery
    }

    public enum MessagePriority
    {
        Low = 0,
        Normal = 1,
        High = 2,
        Critical = 3
    }

    public abstract class QueueMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public QueueMessageType MessageType { get; set; }
        public MessagePriority Priority { get; set; } = MessagePriority.Normal;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = string.Empty;
        public Dictionary<string, string> Metadata { get; set; } = new();
        public int RetryCount { get; set; } = 0;
        public int MaxRetries { get; set; } = 3;
        public TimeSpan? DelayUntil { get; set; }
    }

    // SMS Notification Messages
    public class SmsNotificationMessage : QueueMessage
    {
        public SmsNotificationMessage()
        {
            MessageType = QueueMessageType.SmsNotification;
        }

        public string QueueEntryId { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string NotificationType { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
    }

    // Analytics Processing Messages
    public class AnalyticsProcessingMessage : QueueMessage
    {
        public AnalyticsProcessingMessage()
        {
            MessageType = QueueMessageType.AnalyticsProcessing;
            Priority = MessagePriority.Low; // Analytics can be processed with lower priority
        }

        public string AnalyticsType { get; set; } = string.Empty; // "CrossBarbershop", "Organization", "TopPerforming"
        public string OrganizationId { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new();
        public string RequestId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
    }

    // Audit Logging Messages
    public class AuditLoggingMessage : QueueMessage
    {
        public AuditLoggingMessage()
        {
            MessageType = QueueMessageType.AuditLogging;
            Priority = MessagePriority.Normal;
        }

        public string UserId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
    }

    // Cache Invalidation Messages
    public class CacheInvalidationMessage : QueueMessage
    {
        public CacheInvalidationMessage()
        {
            MessageType = QueueMessageType.CacheInvalidation;
            Priority = MessagePriority.High; // Cache invalidation should be fast
        }

        public string CacheKey { get; set; } = string.Empty;
        public string[] CacheKeys { get; set; } = Array.Empty<string>();
        public string CachePattern { get; set; } = string.Empty;
        public string InvalidationType { get; set; } = string.Empty; // "Single", "Multiple", "Pattern"
    }

    // Queue State Change Messages
    public class QueueStateChangeMessage : QueueMessage
    {
        public QueueStateChangeMessage()
        {
            MessageType = QueueMessageType.QueueStateChange;
            Priority = MessagePriority.High;
        }

        public string QueueId { get; set; } = string.Empty;
        public string QueueEntryId { get; set; } = string.Empty;
        public string ChangeType { get; set; } = string.Empty; // "Join", "CallNext", "CheckIn", "Finish", "Cancel"
        public string PreviousStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
        public string StaffMemberId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public int Position { get; set; }
        public DateTime? EventTime { get; set; }
    }

    // Email Notification Messages
    public class EmailNotificationMessage : QueueMessage
    {
        public EmailNotificationMessage()
        {
            MessageType = QueueMessageType.EmailNotification;
        }

        public string ToEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string TemplateId { get; set; } = string.Empty;
        public Dictionary<string, string> TemplateData { get; set; } = new();
        public bool IsHtml { get; set; } = true;
    }

    // Webhook Delivery Messages
    public class WebhookDeliveryMessage : QueueMessage
    {
        public WebhookDeliveryMessage()
        {
            MessageType = QueueMessageType.WebhookDelivery;
            Priority = MessagePriority.Normal;
        }

        public string WebhookUrl { get; set; } = string.Empty;
        public string HttpMethod { get; set; } = "POST";
        public Dictionary<string, string> Headers { get; set; } = new();
        public string Payload { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string OrganizationId { get; set; } = string.Empty;
    }
} 