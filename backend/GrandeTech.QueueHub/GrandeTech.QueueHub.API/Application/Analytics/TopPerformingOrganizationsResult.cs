using System;
using System.Collections.Generic;

namespace Grande.Fila.API.Application.Analytics
{
    public class TopPerformingOrganizationsResult
    {
        public bool Success { get; set; } = true;
        public List<string> Errors { get; set; } = new List<string>();
        public Dictionary<string, string> FieldErrors { get; set; } = new Dictionary<string, string>();
        public List<OrganizationMetrics> TopOrganizations { get; set; } = new List<OrganizationMetrics>();
        public string MetricType { get; set; } = string.Empty;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }
} 