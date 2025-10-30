using System;
using Grande.Fila.API.Domain.Common;
using Grande.Fila.API.Domain.Common.ValueObjects;

namespace Grande.Fila.API.Domain.ServicesOffered
{
    /// <summary>
    /// Represents a type of service (e.g., haircut style) offered by a service provider
    /// </summary>
    public class ServiceOffered : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public Guid LocationId { get; private set; }
        public int EstimatedDurationMinutes { get; private set; }
        // Flattened price properties (replacing Money value object)
        public decimal? PriceAmount { get; private set; }
        public string PriceCurrency { get; private set; } = "BRL";
        public string? ImageUrl { get; private set; }
        public bool IsActive { get; private set; }
        public int TimesProvided { get; private set; }        public double ActualAverageDurationMinutes { get; private set; }

        // For EF Core
        private ServiceOffered() { }

        public ServiceOffered(
            string name,
            string? description,
            Guid locationId,
            int estimatedDurationMinutes,
            decimal? priceValue,
            string? imageUrl,
            string createdBy)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Service type name is required", nameof(name));

            if (locationId == Guid.Empty)
                throw new ArgumentException("Location ID is required", nameof(locationId));

            if (estimatedDurationMinutes <= 0)
                throw new ArgumentException("Estimated duration must be positive", nameof(estimatedDurationMinutes));

            Name = name;
            Description = description;
            LocationId = locationId;
            EstimatedDurationMinutes = estimatedDurationMinutes;
            PriceAmount = priceValue;
            PriceCurrency = "BRL"; // Default to Brazilian Real
            ImageUrl = imageUrl;
            IsActive = true;
            TimesProvided = 0;
            ActualAverageDurationMinutes = estimatedDurationMinutes; // Start with estimate
            // AddDomainEvent(new ServiceOfferedCreatedEvent(Id, Name, LocationId));
        }

        // Domain behavior methods
        public void UpdateDetails(
            string name,
            string? description,
            int estimatedDurationMinutes,
            decimal? price,
            string? imageUrl,
            string updatedBy)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Service type name is required", nameof(name));

            if (estimatedDurationMinutes <= 0)
                throw new ArgumentException("Estimated duration must be positive", nameof(estimatedDurationMinutes));

            Name = name;
            Description = description;
            EstimatedDurationMinutes = estimatedDurationMinutes;
            PriceAmount = price;
            // Keep existing currency
            ImageUrl = imageUrl;
            
            // MarkAsModified(updatedBy);
            // AddDomainEvent(new ServiceOfferedUpdatedEvent(Id));
        }

        public void Activate(string updatedBy)
        {
            if (!IsActive)
            {
                IsActive = true;
                // MarkAsModified(updatedBy);
                // AddDomainEvent(new ServiceOfferedActivatedEvent(Id));
            }
        }

        public void Deactivate(string updatedBy)
        {
            if (IsActive)
            {
                IsActive = false;
            }
        }

        public void RecordServiceProvided(int actualDurationMinutes, string updatedBy)
        {
            if (actualDurationMinutes <= 0)
                throw new ArgumentException("Actual duration must be positive", nameof(actualDurationMinutes));

            // Update actual average duration using a weighted average
            var totalMinutes = ActualAverageDurationMinutes * TimesProvided;
            TimesProvided++;
            ActualAverageDurationMinutes = (totalMinutes + actualDurationMinutes) / TimesProvided;
        }

        public int GetEstimatedWaitTime()
        {
            return EstimatedDurationMinutes;
        }

        public int GetActualWaitTime()
        {
            return (int)Math.Ceiling(ActualAverageDurationMinutes);
        }

        public void Update(string name, string? description, Guid locationId, int estimatedDurationMinutes, decimal? price, string? imageUrl, bool isActive)
        {
            Name = name;
            Description = description;
            LocationId = locationId;
            EstimatedDurationMinutes = estimatedDurationMinutes;
            PriceAmount = price;
            PriceCurrency = "BRL"; // Default to Brazilian Real
            ImageUrl = imageUrl;
            IsActive = isActive;
        }
    }
}
