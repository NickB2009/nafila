using System;
using System.Collections.Generic;

namespace Grande.Fila.API.Application.Analytics
{
    public class OrganizationAnalyticsResult
    {
        public bool Success { get; set; } = true;
        public List<string> Errors { get; set; } = new List<string>();
        public Dictionary<string, string> FieldErrors { get; set; } = new Dictionary<string, string>();
        public OrganizationAnalyticsData? AnalyticsData { get; set; }
    }

    public class OrganizationAnalyticsData
    {
        public Guid OrganizationId { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
        public int TotalLocations { get; set; }
        public int TotalStaffMembers { get; set; }
        public int TotalCompletedServices { get; set; }
        public int TotalActiveQueues { get; set; }
        public double AverageWaitTimeMinutes { get; set; }
        public double StaffUtilizationPercentage { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public List<LocationMetrics> LocationMetrics { get; set; } = new List<LocationMetrics>();
        public List<StaffMetrics> StaffMetrics { get; set; } = new List<StaffMetrics>();
        public List<ServiceMetrics> ServiceMetrics { get; set; } = new List<ServiceMetrics>();
        public List<DailyMetrics> DailyTrends { get; set; } = new List<DailyMetrics>();
    }

    public class LocationMetrics
    {
        public Guid LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public int CompletedServices { get; set; }
        public double AverageWaitTimeMinutes { get; set; }
        public double StaffUtilizationPercentage { get; set; }
        public int TotalStaffMembers { get; set; }
    }

    public class StaffMetrics
    {
        public Guid StaffMemberId { get; set; }
        public string StaffMemberName { get; set; } = string.Empty;
        public int CompletedServices { get; set; }
        public double AverageServiceDurationMinutes { get; set; }
        public double UtilizationPercentage { get; set; }
    }

    public class DailyMetrics
    {
        public DateTime Date { get; set; }
        public int TotalServices { get; set; }
        public double AverageWaitTimeMinutes { get; set; }
        public double StaffUtilizationPercentage { get; set; }
    }
} 