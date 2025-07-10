namespace Grande.Fila.API.Application.Staff
{
    public class StartBreakRequest
    {
        public required string StaffMemberId { get; set; }
        public required int DurationMinutes { get; set; }
        public string? Reason { get; set; }
    }
} 