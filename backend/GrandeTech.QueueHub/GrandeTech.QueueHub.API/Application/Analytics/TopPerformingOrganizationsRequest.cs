using System;

namespace Grande.Fila.API.Application.Analytics
{
    public class TopPerformingOrganizationsRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string MetricType { get; set; } = "CompletedServices"; // CompletedServices, AverageWaitTime, StaffUtilization, CustomerSatisfaction
        public int MaxResults { get; set; } = 10;
        public bool IncludeDetails { get; set; } = false;
    }
} 