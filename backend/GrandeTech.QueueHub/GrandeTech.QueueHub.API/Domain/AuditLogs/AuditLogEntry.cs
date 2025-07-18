using System;

namespace Grande.Fila.API.Domain.AuditLogs
{
    public class AuditLogEntry
    {
        public string UserId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public DateTime TimestampUtc { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
    }
} 