using System;
using System.Collections.Generic;

namespace Grande.Fila.API.Application.Organizations
{
    /// <summary>
    /// Result DTO for UC-TRACKQ: Admin/Owner track live activity
    /// </summary>
    public class TrackLiveActivityResult
    {
        public bool Success { get; set; }
        public LiveActivityDto? LiveActivity { get; set; }
        public List<string> Errors { get; set; } = new();
        public Dictionary<string, string> FieldErrors { get; set; } = new();
    }

    public class LiveActivityDto
    {
        public string OrganizationId { get; set; } = string.Empty;
        public string OrganizationName { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
        public OrganizationSummaryDto Summary { get; set; } = new();
        public List<LocationActivityDto> Locations { get; set; } = new();
    }

    public class OrganizationSummaryDto
    {
        public int TotalLocations { get; set; }
        public int TotalActiveQueues { get; set; }
        public int TotalCustomersWaiting { get; set; }
        public int TotalStaffMembers { get; set; }
        public int TotalAvailableStaff { get; set; }
        public int TotalBusyStaff { get; set; }
        public double AverageWaitTimeMinutes { get; set; }
    }

    public class LocationActivityDto
    {
        public string LocationId { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public bool IsOpen { get; set; }
        public int TotalCustomersWaiting { get; set; }
        public int TotalStaffMembers { get; set; }
        public int AvailableStaffMembers { get; set; }
        public int BusyStaffMembers { get; set; }
        public double AverageWaitTimeMinutes { get; set; }
        public List<QueueActivityDto> Queues { get; set; } = new();
        public List<StaffActivityDto> Staff { get; set; } = new();
    }

    public class QueueActivityDto
    {
        public string QueueId { get; set; } = string.Empty;
        public DateTime QueueDate { get; set; }
        public bool IsActive { get; set; }
        public int CustomersWaiting { get; set; }
        public int CustomersBeingServed { get; set; }
        public int TotalCustomersToday { get; set; }
        public double AverageWaitTimeMinutes { get; set; }
        public int MaxSize { get; set; }
    }

    public class StaffActivityDto
    {
        public string StaffId { get; set; } = string.Empty;
        public string StaffName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? LastActivity { get; set; }
        public int CustomersServedToday { get; set; }
        public double AverageServiceTimeMinutes { get; set; }
        public bool IsOnBreak { get; set; }
        public DateTime? BreakStartTime { get; set; }
    }
}