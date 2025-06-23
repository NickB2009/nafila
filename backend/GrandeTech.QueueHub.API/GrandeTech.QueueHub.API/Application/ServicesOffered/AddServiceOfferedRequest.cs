namespace Grande.Fila.API.Application.ServicesOffered
{
    public class AddServiceOfferedRequest
    {
        public string LocationId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int EstimatedDurationMinutes { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
    }
}
