namespace Grande.Fila.API.Application.Queues
{
    public class BarberAddRequest
    {
        public string QueueId { get; set; } = string.Empty;
        public string StaffMemberId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? ServiceTypeId { get; set; }
        public string? Notes { get; set; }
    }
} 