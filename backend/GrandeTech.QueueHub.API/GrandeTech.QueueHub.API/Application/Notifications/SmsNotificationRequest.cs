namespace Grande.Fila.API.Application.Notifications
{
    public class SmsNotificationRequest
    {
        public required string QueueEntryId { get; set; }
        public required string Message { get; set; }
        public required string NotificationType { get; set; }
    }
} 