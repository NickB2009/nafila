namespace Grande.Fila.API.Application.QrCode
{
    public class QrJoinRequest
    {
        public required string LocationId { get; set; }
        public string? ServiceTypeId { get; set; }
        public string? ExpiryMinutes { get; set; } = "60"; // Default 1 hour
    }
} 