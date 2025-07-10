using System;

namespace Grande.Fila.API.Application.ServicesOffered
{
    public class UpdateServiceOfferedRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid LocationId { get; set; }
        public int EstimatedDurationMinutes { get; set; }
        public decimal? Price { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
    }
} 