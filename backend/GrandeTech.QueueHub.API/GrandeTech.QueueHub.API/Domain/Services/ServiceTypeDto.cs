using System;

namespace GrandeTech.QueueHub.API.Domain.Services
{
    public class ServiceTypeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid LocationId { get; set; }
        public int EstimatedDurationMinutes { get; set; }
        public decimal? Price { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
    }
} 