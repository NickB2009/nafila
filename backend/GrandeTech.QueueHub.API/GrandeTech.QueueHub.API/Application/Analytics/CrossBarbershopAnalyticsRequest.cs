using System;
using System.Collections.Generic;

namespace Grande.Fila.API.Application.Analytics
{
    public class CrossBarbershopAnalyticsRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IncludeActiveQueues { get; set; } = true;
        public bool IncludeCompletedServices { get; set; } = true;
        public bool IncludeAverageWaitTimes { get; set; } = true;
        public bool IncludeStaffUtilization { get; set; } = true;
        public bool IncludeLocationMetrics { get; set; } = true;
        public List<string> OrganizationIds { get; set; } = new List<string>();
    }
} 