using System;
using System.Collections.Generic;

namespace Grande.Fila.API.Application.Queues.Models
{
    /// <summary>
    /// Queue performance metrics for a salon
    /// </summary>
    public class QueuePerformanceMetrics
    {
        public Guid SalonId { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int TotalCustomers { get; set; }
        public TimeSpan AverageWaitTime { get; set; }
        public TimeSpan AverageServiceTime { get; set; }
        public TimeSpan? PeakHour { get; set; }
        public double CustomerSatisfaction { get; set; } // 1.0 to 5.0 scale
        public double QueueEfficiency { get; set; } // 0.0 to 1.0 scale
        public TimeSpan PeakWaitTime { get; set; }
        public int PeakHourCustomers { get; set; }
        public int CompletedServices { get; set; }
        public int CancelledServices { get; set; }
        public double CancellationRate => TotalCustomers > 0 ? (double)CancelledServices / TotalCustomers : 0.0;
    }

    /// <summary>
    /// Wait time trend data for a specific date
    /// </summary>
    public class WaitTimeTrend
    {
        public DateTime Date { get; set; }
        public TimeSpan AverageWaitTime { get; set; }
        public TimeSpan PeakWaitTime { get; set; }
        public int TotalCustomers { get; set; }
        public TimeSpan? BusiestHour { get; set; }
        public int BusiestHourCustomers { get; set; }
    }

    /// <summary>
    /// Queue recommendations for a salon
    /// </summary>
    public class QueueRecommendations
    {
        public Guid SalonId { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public List<string> Recommendations { get; set; } = new();
        public string Priority { get; set; } = "Medium"; // Low, Medium, High
        public double ConfidenceScore { get; set; } = 0.8; // 0.0 to 1.0
    }

    /// <summary>
    /// Customer satisfaction feedback
    /// </summary>
    public class CustomerSatisfactionFeedback
    {
        public Guid QueueEntryId { get; set; }
        public Guid SalonId { get; set; }
        public int Rating { get; set; } // 1 to 5 stars
        public string? Feedback { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public string? ServiceType { get; set; }
        public TimeSpan? ActualWaitTime { get; set; }
        public TimeSpan? EstimatedWaitTime { get; set; }
        public bool WaitTimeMetExpectation => 
            ActualWaitTime.HasValue && EstimatedWaitTime.HasValue && 
            ActualWaitTime.Value <= EstimatedWaitTime.Value.Add(TimeSpan.FromMinutes(5));
    }

    /// <summary>
    /// Service type analytics
    /// </summary>
    public class ServiceTypeAnalytics
    {
        public string ServiceType { get; set; } = string.Empty;
        public int TotalCustomers { get; set; }
        public TimeSpan AverageServiceTime { get; set; }
        public TimeSpan AverageWaitTime { get; set; }
        public double CustomerSatisfaction { get; set; }
        public double PopularityScore { get; set; } // 0.0 to 1.0
        public decimal AverageRevenue { get; set; }
    }

    /// <summary>
    /// Hourly queue analytics
    /// </summary>
    public class HourlyQueueAnalytics
    {
        public int Hour { get; set; } // 0-23
        public string HourLabel { get; set; } = string.Empty; // "9:00 AM", "2:00 PM", etc.
        public int TotalCustomers { get; set; }
        public TimeSpan AverageWaitTime { get; set; }
        public TimeSpan PeakWaitTime { get; set; }
        public double QueueEfficiency { get; set; }
        public bool IsPeakHour { get; set; }
        public string Recommendation { get; set; } = string.Empty;
    }

    /// <summary>
    /// Queue health status for analytics
    /// </summary>
    public class QueueAnalyticsHealthStatus
    {
        public Guid SalonId { get; set; }
        public string Status { get; set; } = "Healthy"; // Healthy, Warning, Critical
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public int CurrentQueueLength { get; set; }
        public TimeSpan CurrentAverageWaitTime { get; set; }
        public List<string> Issues { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
        public double HealthScore { get; set; } // 0.0 to 100.0
    }
}
