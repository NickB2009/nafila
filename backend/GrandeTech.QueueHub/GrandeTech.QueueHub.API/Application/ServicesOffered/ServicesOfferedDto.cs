using System;
using Grande.Fila.API.Domain.Common.ValueObjects;

namespace Grande.Fila.API.Application.ServicesOffered
{
    public class ServicesOfferedDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid LocationId { get; set; }
        public int EstimatedDurationMinutes { get; set; }
        public decimal? PriceAmount { get; set; }
        public string PriceCurrency { get; set; } = "BRL";
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
    }
} 