namespace Grande.Fila.API.Application.Queues
{
    public class CallNextRequest
    {
        public string StaffMemberId { get; set; } = string.Empty;
        public string QueueId { get; set; } = string.Empty;
    }
} 