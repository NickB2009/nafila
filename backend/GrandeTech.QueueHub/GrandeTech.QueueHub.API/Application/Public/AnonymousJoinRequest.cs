namespace Grande.Fila.API.Application.Public
{
    public class AnonymousJoinRequest
    {
        public string SalonId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AnonymousUserId { get; set; } = string.Empty;
        public string ServiceRequested { get; set; } = string.Empty;
        public bool EmailNotifications { get; set; } = false;
        public bool BrowserNotifications { get; set; } = false;
    }
}