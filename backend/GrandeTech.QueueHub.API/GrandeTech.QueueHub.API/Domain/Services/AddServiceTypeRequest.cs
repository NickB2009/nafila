namespace GrandeTech.QueueHub.API.Domain.Services
{
    public class AddServiceTypeRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid LocationId { get; set; }
        public int EstimatedDurationMinutes { get; set; }
        public decimal? Price { get; set; }
        public string? ImageUrl { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }
} 