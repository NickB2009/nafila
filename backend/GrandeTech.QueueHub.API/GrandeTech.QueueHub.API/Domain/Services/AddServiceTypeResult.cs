namespace GrandeTech.QueueHub.API.Domain.Services
{
    public class AddServiceTypeResult
    {
        public bool Success { get; set; }
        public Guid ServiceTypeId { get; set; }
        public string? ErrorMessage { get; set; }
    }
} 