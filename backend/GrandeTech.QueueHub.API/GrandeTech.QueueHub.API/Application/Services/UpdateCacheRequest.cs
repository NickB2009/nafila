namespace Grande.Fila.API.Application.Services
{
    public class UpdateCacheRequest
    {
        public required string LocationId { get; set; }
        public required double AverageServiceTimeMinutes { get; set; }
    }
} 