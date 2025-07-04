using System.Collections.Generic;

namespace Grande.Fila.API.Application.Promotions
{
    public class CouponNotificationResult
    {
        public bool Success { get; set; }
        public string? NotificationId { get; set; }
        public Dictionary<string, string> FieldErrors { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }
} 