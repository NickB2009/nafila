namespace Grande.Fila.API.Application.Promotions
{
    public class CouponNotificationRequest
    {
        public required string CustomerId { get; set; }
        public required string CouponId { get; set; }
        public required string Message { get; set; }
        public string NotificationChannel { get; set; } = "SMS";
    }
} 