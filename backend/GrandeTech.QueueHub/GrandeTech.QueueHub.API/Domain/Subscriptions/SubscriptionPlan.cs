using System;
using System.Collections.Generic;
using Grande.Fila.API.Domain.Common;
using Grande.Fila.API.Domain.Common.ValueObjects;

namespace Grande.Fila.API.Domain.Subscriptions
{
    /// <summary>
    /// Represents a subscription plan that organizations can subscribe to
    /// </summary>
    public class SubscriptionPlan : BaseEntity, IAggregateRoot
    {
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;        public Money MonthlyPrice { get; private set; } = null!;
        public Money YearlyPrice { get; private set; } = null!;
        public decimal Price { get; private set; } // Simple price property for queries
        public bool IsActive { get; private set; }
        public bool IsDefault { get; private set; }
        public int MaxLocations { get; private set; }
        public int MaxStaffPerLocation { get; private set; }
        public bool IncludesAnalytics { get; private set; }
        public bool IncludesAdvancedReporting { get; private set; }
        public bool IncludesCustomBranding { get; private set; }
        public bool IncludesAdvertising { get; private set; }
        public bool IncludesMultipleLocations { get; private set; }
        public int MaxQueueEntriesPerDay { get; private set; }
        public bool IsFeatured { get; private set; }

        // For EF Core
        private SubscriptionPlan() { }

        public SubscriptionPlan(
            string name,
            string description,
            decimal monthlyPrice,
            decimal yearlyPrice,
            int maxLocations,
            int maxStaffPerLocation,
            bool includesAnalytics,
            bool includesAdvancedReporting,
            bool includesCustomBranding,
            bool includesAdvertising,
            bool includesMultipleLocations,
            int maxQueueEntriesPerDay,
            bool isFeatured,
            string createdBy)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Plan name is required", nameof(name));

            Name = name;
            Description = description ?? string.Empty;
            MonthlyPrice = Money.Create(monthlyPrice);
            YearlyPrice = Money.Create(yearlyPrice);
            IsActive = true;
            MaxLocations = maxLocations;
            MaxStaffPerLocation = maxStaffPerLocation;
            IncludesAnalytics = includesAnalytics;
            IncludesAdvancedReporting = includesAdvancedReporting;
            IncludesCustomBranding = includesCustomBranding;
            IncludesAdvertising = includesAdvertising;
            IncludesMultipleLocations = includesMultipleLocations;
            MaxQueueEntriesPerDay = maxQueueEntriesPerDay;
            IsFeatured = isFeatured;
            CreatedBy = createdBy;

            AddDomainEvent(new SubscriptionPlanCreatedEvent(Id, Name));
        }

        public void UpdateDetails(
            string name,
            string description,
            decimal monthlyPrice,
            decimal yearlyPrice,
            int maxLocations,
            int maxStaffPerLocation,
            bool includesAnalytics,
            bool includesAdvancedReporting,
            bool includesCustomBranding,
            bool includesAdvertising,
            bool includesMultipleLocations,
            int maxQueueEntriesPerDay,
            bool isFeatured,
            string updatedBy)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Plan name is required", nameof(name));

            Name = name;
            Description = description ?? string.Empty;
            MonthlyPrice = Money.Create(monthlyPrice);
            YearlyPrice = Money.Create(yearlyPrice);
            MaxLocations = maxLocations;
            MaxStaffPerLocation = maxStaffPerLocation;
            IncludesAnalytics = includesAnalytics;
            IncludesAdvancedReporting = includesAdvancedReporting;
            IncludesCustomBranding = includesCustomBranding;
            IncludesAdvertising = includesAdvertising;
            IncludesMultipleLocations = includesMultipleLocations;
            MaxQueueEntriesPerDay = maxQueueEntriesPerDay;
            IsFeatured = isFeatured;
            
            MarkAsModified(updatedBy);
            AddDomainEvent(new SubscriptionPlanUpdatedEvent(Id));
        }

        public void Activate(string updatedBy)
        {
            if (!IsActive)
            {
                IsActive = true;
                MarkAsModified(updatedBy);
                AddDomainEvent(new SubscriptionPlanActivatedEvent(Id));
            }
        }

        public void Deactivate(string updatedBy)
        {
            if (IsActive)
            {
                IsActive = false;
                MarkAsModified(updatedBy);
                AddDomainEvent(new SubscriptionPlanDeactivatedEvent(Id));
            }
        }

        public bool CanHaveMultipleLocations() => IncludesMultipleLocations;

        public bool HasReachedLocationLimit(int currentLocationCount)
        {
            return currentLocationCount >= MaxLocations;
        }

        public bool HasReachedStaffLimit(int currentStaffCount)
        {
            return currentStaffCount >= MaxStaffPerLocation;
        }
    }
}
