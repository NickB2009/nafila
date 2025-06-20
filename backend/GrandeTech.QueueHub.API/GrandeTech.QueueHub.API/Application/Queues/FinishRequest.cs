namespace Grande.Fila.API.Application.Queues
{
    public class FinishRequest
    {
        public string QueueEntryId { get; set; } = string.Empty;
        public int ServiceDurationMinutes { get; set; }
        public string? Notes { get; set; }
    }
} 