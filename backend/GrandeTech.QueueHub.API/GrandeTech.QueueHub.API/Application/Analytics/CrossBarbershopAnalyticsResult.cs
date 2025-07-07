using System;
using System.Collections.Generic;

namespace Grande.Fila.API.Application.Analytics
{
    public class CrossBarbershopAnalyticsResult
    {
        public bool Success { get; set; } = true;
        public List<string> Errors { get; set; } = new List<string>();
        public Dictionary<string, string> FieldErrors { get; set; } = new Dictionary<string, string>();
        public CrossBarbershopAnalyticsData? AnalyticsData { get; set; }
    }

    public class CrossBarbershopAnalyticsData
    {
        public int TotalOrganizations { get; set; }
        public int TotalActiveQueues { get; set; }
        public int TotalCompletedServices { get; set; }
        public double AverageWaitTimeMinutes { get; set; }
        public double StaffUtilizationPercentage { get; set; }
        public int TotalLocations { get; set; }
        public int TotalStaffMembers { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public List<OrganizationMetrics> TopOrganizations { get; set; } = new List<OrganizationMetrics>();
        public List<ServiceMetrics> PopularServices { get; set; } = new List<ServiceMetrics>();
        public List<HourlyMetrics> HourlyTrends { get; set; } = new List<HourlyMetrics>();
    }

    public class OrganizationMetrics
    {
        public Guid OrganizationId { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
        public int CompletedServices { get; set; }
        public double AverageWaitTimeMinutes { get; set; }
        public double StaffUtilizationPercentage { get; set; }
        public int TotalLocations { get; set; }
        public int TotalStaffMembers { get; set; }
    }

    public class ServiceMetrics
    {
        public string ServiceName { get; set; } = string.Empty;
        public int TotalBookings { get; set; }
        public double AverageDurationMinutes { get; set; }
        public double AverageWaitTimeMinutes { get; set; }
    }

    public class HourlyMetrics
    {
        public int Hour { get; set; }
        public int TotalServices { get; set; }
        public double AverageWaitTimeMinutes { get; set; }
        public double StaffUtilizationPercentage { get; set; }
    }
} 