using System;
using System.Collections.Generic;

namespace Grande.Fila.API.Application.Kiosk
{
    public class KioskDisplayResult
    {
        public bool Success { get; set; }
        public string? LocationId { get; set; }
        public string? LocationName { get; set; }
        public List<KioskQueueEntryDto> QueueEntries { get; set; } = new();
        public string? CurrentlyServing { get; set; }
        public int? CurrentPosition { get; set; }
        public int TotalWaiting { get; set; }
        public int TotalActive { get; set; }
        public int ActiveStaffCount { get; set; }
        public double AverageServiceTimeMinutes { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public Dictionary<string, string> FieldErrors { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }

    public class KioskQueueEntryDto
    {
        public string Id { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public int Position { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? TokenNumber { get; set; }
        public int EstimatedWaitMinutes { get; set; }
        public string? EstimatedWaitTime { get; set; }
        public DateTime? JoinedAt { get; set; }
        public DateTime? CalledAt { get; set; }
        public DateTime? CheckedInAt { get; set; }
        public string? ServiceType { get; set; }
        public string? StaffMemberName { get; set; }
    }

    public class KioskLocationInfoDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsOpen { get; set; }
        public string? CurrentTime { get; set; }
        public string? NextOpenTime { get; set; }
        public int ActiveStaffCount { get; set; }
        public double AverageWaitTimeMinutes { get; set; }
    }
} 