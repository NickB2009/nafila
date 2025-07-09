namespace Grande.Fila.API.Application.Queues
{
    public class JoinQueueRequest
    {
        public string QueueId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public bool IsAnonymous { get; set; } = false;
        public string? Notes { get; set; }
        public string? ServiceTypeId { get; set; }
    }
}
