using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Application.Queues.Models;

namespace Grande.Fila.API.Application.Queues
{
    /// <summary>
    /// Service for providing queue analytics and metrics
    /// </summary>
    public class QueueAnalyticsService : IQueueAnalyticsService
    {
        private readonly ILogger<QueueAnalyticsService> _logger;
        private readonly IQueueService _queueService;

        public QueueAnalyticsService(
            IQueueService queueService,
            ILogger<QueueAnalyticsService> logger)
        {
            _queueService = queueService;
            _logger = logger;
        }

        /// <summary>
        /// Calculate estimated wait time for a new queue entry
        /// </summary>
        public async Task<TimeSpan> CalculateEstimatedWaitTimeAsync(
            Guid salonId, 
            string serviceType, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Get current queue for the salon
                var currentQueue = await GetCurrentQueueAsync(salonId, cancellationToken);
                
                if (currentQueue == null || !currentQueue.Any())
                {
                    return TimeSpan.FromMinutes(5); // Minimal wait for empty queue
                }

                // Calculate based on service type and queue length
                var averageServiceTime = GetAverageServiceTime(serviceType);
                var queueLength = currentQueue.Count();
                var estimatedWait = TimeSpan.FromMinutes(averageServiceTime.TotalMinutes * queueLength);

                // Add some buffer time
                estimatedWait = estimatedWait.Add(TimeSpan.FromMinutes(5));

                _logger.LogInformation("Calculated wait time for salon {SalonId}: {WaitTime} minutes", 
                    salonId, estimatedWait.TotalMinutes);

                return estimatedWait;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to calculate estimated wait time for salon {SalonId}", salonId);
                return TimeSpan.FromMinutes(15); // Default fallback
            }
        }

        /// <summary>
        /// Get queue performance metrics for a salon
        /// </summary>
        public async Task<QueuePerformanceMetrics> GetQueuePerformanceMetricsAsync(
            Guid salonId, 
            DateTime? fromDate = null, 
            DateTime? toDate = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var startDate = fromDate ?? DateTime.UtcNow.AddDays(-7);
                var endDate = toDate ?? DateTime.UtcNow;

                // Get queue history for the period
                var queueHistory = await GetQueueHistoryAsync(salonId, startDate, endDate, cancellationToken);

                var metrics = new QueuePerformanceMetrics
                {
                    SalonId = salonId,
                    PeriodStart = startDate,
                    PeriodEnd = endDate,
                    TotalCustomers = queueHistory.Count(),
                    AverageWaitTime = CalculateAverageWaitTime(queueHistory),
                    AverageServiceTime = CalculateAverageServiceTime(queueHistory),
                    PeakHour = CalculatePeakHour(queueHistory),
                    CustomerSatisfaction = CalculateCustomerSatisfaction(queueHistory),
                    QueueEfficiency = CalculateQueueEfficiency(queueHistory)
                };

                _logger.LogInformation("Generated performance metrics for salon {SalonId}: {TotalCustomers} customers, {AvgWaitTime} min avg wait", 
                    salonId, metrics.TotalCustomers, metrics.AverageWaitTime.TotalMinutes);

                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate performance metrics for salon {SalonId}", salonId);
                throw;
            }
        }

        /// <summary>
        /// Get wait time trends for a salon
        /// </summary>
        public async Task<List<WaitTimeTrend>> GetWaitTimeTrendsAsync(
            Guid salonId, 
            int days = 7,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var trends = new List<WaitTimeTrend>();
                var endDate = DateTime.UtcNow.Date;
                var startDate = endDate.AddDays(-days);

                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    var dayMetrics = await GetQueuePerformanceMetricsAsync(
                        salonId, 
                        date, 
                        date.AddDays(1), 
                        cancellationToken);

                    trends.Add(new WaitTimeTrend
                    {
                        Date = date,
                        AverageWaitTime = dayMetrics.AverageWaitTime,
                        PeakWaitTime = dayMetrics.PeakWaitTime,
                        TotalCustomers = dayMetrics.TotalCustomers
                    });
                }

                return trends;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate wait time trends for salon {SalonId}", salonId);
                throw;
            }
        }

        /// <summary>
        /// Record customer satisfaction feedback
        /// </summary>
        public async Task<bool> RecordCustomerSatisfactionAsync(
            Guid queueEntryId, 
            int rating, 
            string? feedback = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // In a real implementation, this would save to a database
                // For now, we'll just log it
                _logger.LogInformation("Customer satisfaction recorded for entry {EntryId}: Rating {Rating}, Feedback: {Feedback}", 
                    queueEntryId, rating, feedback ?? "None");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to record customer satisfaction for entry {EntryId}", queueEntryId);
                return false;
            }
        }

        /// <summary>
        /// Get queue recommendations for a salon
        /// </summary>
        public async Task<QueueRecommendations> GetQueueRecommendationsAsync(
            Guid salonId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var metrics = await GetQueuePerformanceMetricsAsync(salonId, cancellationToken: cancellationToken);
                
                var recommendations = new QueueRecommendations
                {
                    SalonId = salonId,
                    Recommendations = new List<string>()
                };

                // Analyze and provide recommendations
                if (metrics.AverageWaitTime.TotalMinutes > 30)
                {
                    recommendations.Recommendations.Add("Consider adding more staff during peak hours");
                }

                if (metrics.QueueEfficiency < 0.7)
                {
                    recommendations.Recommendations.Add("Review service time allocation and optimize workflow");
                }

                if (metrics.CustomerSatisfaction < 4.0)
                {
                    recommendations.Recommendations.Add("Investigate customer complaints and improve service quality");
                }

                if (metrics.PeakHour.HasValue)
                {
                    recommendations.Recommendations.Add($"Peak hour is {metrics.PeakHour.Value:HH:mm}, consider scheduling accordingly");
                }

                return recommendations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate queue recommendations for salon {SalonId}", salonId);
                throw;
            }
        }

        #region Private Methods

        private async Task<IEnumerable<QueueEntry>> GetCurrentQueueAsync(Guid salonId, CancellationToken cancellationToken)
        {
            // This would typically query the actual queue data
            // For now, return empty collection
            return Enumerable.Empty<QueueEntry>();
        }

        private async Task<IEnumerable<QueueEntry>> GetQueueHistoryAsync(
            Guid salonId, 
            DateTime startDate, 
            DateTime endDate, 
            CancellationToken cancellationToken)
        {
            // This would typically query the actual queue history
            // For now, return empty collection
            return Enumerable.Empty<QueueEntry>();
        }

        private TimeSpan GetAverageServiceTime(string serviceType)
        {
            // Service time estimates based on service type
            return serviceType.ToLower() switch
            {
                "haircut" => TimeSpan.FromMinutes(20),
                "haircut and wash" => TimeSpan.FromMinutes(30),
                "haircut, wash and style" => TimeSpan.FromMinutes(45),
                "haircut, wash, style and color" => TimeSpan.FromMinutes(90),
                "haircut, wash, style, color and treatment" => TimeSpan.FromMinutes(120),
                _ => TimeSpan.FromMinutes(25) // Default
            };
        }

        private TimeSpan CalculateAverageWaitTime(IEnumerable<QueueEntry> queueHistory)
        {
            if (!queueHistory.Any()) return TimeSpan.Zero;

            var totalWaitTime = queueHistory
                .Where(entry => entry.EnteredAt != default && entry.CompletedAt.HasValue)
                .Sum(entry => (entry.CompletedAt.Value - entry.EnteredAt).TotalMinutes);

            var count = queueHistory.Count(entry => entry.EnteredAt != default && entry.CompletedAt.HasValue);
            
            return count > 0 ? TimeSpan.FromMinutes(totalWaitTime / count) : TimeSpan.Zero;
        }

        private TimeSpan CalculateAverageServiceTime(IEnumerable<QueueEntry> queueHistory)
        {
            if (!queueHistory.Any()) return TimeSpan.Zero;

            var totalServiceTime = queueHistory
                .Where(entry => entry.ActualStartTime.HasValue && entry.CompletionTime.HasValue)
                .Sum(entry => (entry.CompletionTime.Value - entry.ActualStartTime.Value).TotalMinutes);

            var count = queueHistory.Count(entry => entry.ActualStartTime.HasValue && entry.CompletionTime.HasValue);
            
            return count > 0 ? TimeSpan.FromMinutes(totalServiceTime / count) : TimeSpan.Zero;
        }

        private TimeSpan? CalculatePeakHour(IEnumerable<QueueEntry> queueHistory)
        {
            if (!queueHistory.Any()) return null;

            var hourlyCounts = queueHistory
                .Where(entry => entry.EnteredAt != default)
                .GroupBy(entry => entry.EnteredAt.Hour)
                .Select(g => new { Hour = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefault();

            return hourlyCounts != null ? TimeSpan.FromHours(hourlyCounts.Hour) : null;
        }

        private double CalculateCustomerSatisfaction(IEnumerable<QueueEntry> queueHistory)
        {
            // This would typically query actual satisfaction ratings
            // For now, return a mock value
            return 4.2; // Mock average rating out of 5
        }

        private double CalculateQueueEfficiency(IEnumerable<QueueEntry> queueHistory)
        {
            if (!queueHistory.Any()) return 0.0;

            var totalEntries = queueHistory.Count();
            var completedEntries = queueHistory.Count(entry => entry.Status == QueueEntryStatus.Completed);
            
            return totalEntries > 0 ? (double)completedEntries / totalEntries : 0.0;
        }

        #endregion
    }
}
