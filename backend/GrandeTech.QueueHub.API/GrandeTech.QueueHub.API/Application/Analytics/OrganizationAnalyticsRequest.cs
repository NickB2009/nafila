using System;

namespace Grande.Fila.API.Application.Analytics
{
    public class OrganizationAnalyticsRequest
    {
        public string OrganizationId { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IncludeQueueMetrics { get; set; } = true;
        public bool IncludeServiceMetrics { get; set; } = true;
        public bool IncludeStaffMetrics { get; set; } = true;
        public bool IncludeLocationMetrics { get; set; } = true;
    }
} 