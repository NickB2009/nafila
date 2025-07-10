using System;

namespace Grande.Fila.API.Application.SubscriptionPlans
{
    public class CreateSubscriptionPlanRequest
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public decimal MonthlyPrice { get; set; }
        public decimal YearlyPrice { get; set; }
        public int MaxLocations { get; set; }
        public int MaxStaffPerLocation { get; set; }
        public bool IncludesAnalytics { get; set; } = false;
        public bool IncludesAdvancedReporting { get; set; } = false;
        public bool IncludesCustomBranding { get; set; } = false;
        public bool IncludesAdvertising { get; set; } = false;
        public bool IncludesMultipleLocations { get; set; } = false;
        public int MaxQueueEntriesPerDay { get; set; }
        public bool IsFeatured { get; set; } = false;
        public bool IsDefault { get; set; } = false;
    }
} 