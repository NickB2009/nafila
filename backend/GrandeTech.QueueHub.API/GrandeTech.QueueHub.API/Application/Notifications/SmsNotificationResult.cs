using System.Collections.Generic;

namespace Grande.Fila.API.Application.Notifications
{
    public class SmsNotificationResult
    {
        public bool Success { get; set; }
        public string? MessageId { get; set; }
        public Dictionary<string, string> FieldErrors { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }
} 