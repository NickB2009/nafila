using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Grande.Fila.API.Infrastructure.Services;
using System.Collections.Generic; // Added for List
using System.Linq; // Added for Count()

namespace Grande.Fila.API.Controllers
{
    /// <summary>
    /// Controller for performance monitoring and metrics
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PerformanceController : ControllerBase
    {
        private readonly ILogger<PerformanceController> _logger;
        private readonly IPerformanceMonitoringService _performanceService;

        public PerformanceController(
            ILogger<PerformanceController> logger,
            IPerformanceMonitoringService performanceService)
        {
            _logger = logger;
            _performanceService = performanceService;
        }

        /// <summary>
        /// Get current performance metrics
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCurrentMetrics()
        {
            try
            {
                var metrics = _performanceService.GetCurrentMetrics();
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get current performance metrics");
                return StatusCode(500, new { Error = "Failed to retrieve performance metrics" });
            }
        }

        /// <summary>
        /// Get system health score
        /// </summary>
        [HttpGet("health-score")]
        public async Task<IActionResult> GetHealthScore()
        {
            try
            {
                var score = _performanceService.GetSystemHealthScore();
                var healthStatus = new
                {
                    Score = score,
                    Status = GetHealthStatus(score),
                    Timestamp = DateTime.UtcNow,
                    Recommendations = GetHealthRecommendations(score)
                };

                return Ok(healthStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get system health score");
                return StatusCode(500, new { Error = "Failed to calculate health score" });
            }
        }

        /// <summary>
        /// Get HTTP performance metrics
        /// </summary>
        [HttpGet("http")]
        public async Task<IActionResult> GetHttpMetrics()
        {
            try
            {
                var metrics = _performanceService.GetCurrentMetrics();
                return Ok(metrics.HttpMetrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get HTTP performance metrics");
                return StatusCode(500, new { Error = "Failed to retrieve HTTP metrics" });
            }
        }

        /// <summary>
        /// Get queue performance metrics
        /// </summary>
        [HttpGet("queues")]
        public async Task<IActionResult> GetQueueMetrics()
        {
            try
            {
                var metrics = _performanceService.GetCurrentMetrics();
                return Ok(metrics.QueueMetrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get queue performance metrics");
                return StatusCode(500, new { Error = "Failed to retrieve queue metrics" });
            }
        }

        /// <summary>
        /// Get system resource metrics
        /// </summary>
        [HttpGet("system")]
        public async Task<IActionResult> GetSystemMetrics()
        {
            try
            {
                var metrics = _performanceService.GetCurrentMetrics();
                return Ok(metrics.SystemMetrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get system metrics");
                return StatusCode(500, new { Error = "Failed to retrieve system metrics" });
            }
        }

        /// <summary>
        /// Get custom metrics
        /// </summary>
        [HttpGet("custom")]
        public async Task<IActionResult> GetCustomMetrics()
        {
            try
            {
                var metrics = _performanceService.GetCurrentMetrics();
                return Ok(metrics.CustomMetrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get custom metrics");
                return StatusCode(500, new { Error = "Failed to retrieve custom metrics" });
            }
        }

        /// <summary>
        /// Get metric history for a specific metric
        /// </summary>
        [HttpGet("history/{metricName}")]
        public async Task<IActionResult> GetMetricHistory(
            string metricName,
            [FromQuery] int hours = 24)
        {
            try
            {
                if (hours <= 0 || hours > 168) // Max 1 week
                {
                    return BadRequest(new { Error = "Hours must be between 1 and 168" });
                }

                var duration = TimeSpan.FromHours(hours);
                var history = _performanceService.GetMetricHistory(metricName, duration);

                var response = new
                {
                    MetricName = metricName,
                    Duration = duration,
                    DataPoints = history,
                    Count = history.Count()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get metric history for {MetricName}", metricName);
                return StatusCode(500, new { Error = "Failed to retrieve metric history" });
            }
        }

        /// <summary>
        /// Get performance summary with key indicators
        /// </summary>
        [HttpGet("summary")]
        public async Task<IActionResult> GetPerformanceSummary()
        {
            try
            {
                var metrics = _performanceService.GetCurrentMetrics();
                var healthScore = _performanceService.GetSystemHealthScore();

                var summary = new
                {
                    Timestamp = DateTime.UtcNow,
                    HealthScore = healthScore,
                    HealthStatus = GetHealthStatus(healthScore),
                    KeyMetrics = new
                    {
                        AverageResponseTime = $"{metrics.HttpMetrics.AverageResponseTimeMs:F2}ms",
                        ErrorRate = $"{metrics.HttpMetrics.ErrorRate:P2}",
                        TotalRequests = metrics.HttpMetrics.TotalRequests,
                        QueueOperations = metrics.QueueMetrics.TotalOperations,
                        MemoryUsage = $"{metrics.SystemMetrics.MemoryUsagePercent:F1}%",
                        Uptime = $"{metrics.SystemMetrics.ProcessUptimeSeconds / 3600:F1}h"
                    },
                    Alerts = GetPerformanceAlerts(metrics, healthScore)
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get performance summary");
                return StatusCode(500, new { Error = "Failed to generate performance summary" });
            }
        }

        private string GetHealthStatus(double score)
        {
            return score switch
            {
                >= 90 => "Excellent",
                >= 80 => "Good",
                >= 70 => "Fair",
                >= 60 => "Poor",
                _ => "Critical"
            };
        }

        private string[] GetHealthRecommendations(double score)
        {
            if (score >= 90) return new[] { "System is performing excellently" };

            var recommendations = new List<string>();

            if (score < 80)
            {
                recommendations.Add("Consider optimizing response times");
                recommendations.Add("Review error handling and logging");
            }

            if (score < 70)
            {
                recommendations.Add("Investigate system resource usage");
                recommendations.Add("Check for memory leaks or performance bottlenecks");
            }

            if (score < 60)
            {
                recommendations.Add("Immediate attention required");
                recommendations.Add("Consider scaling or infrastructure improvements");
            }

            return recommendations.ToArray();
        }

        private string[] GetPerformanceAlerts(Infrastructure.Services.PerformanceMetrics metrics, double healthScore)
        {
            var alerts = new List<string>();

            // Response time alerts
            if (metrics.HttpMetrics.AverageResponseTimeMs > 1000)
            {
                alerts.Add("High average response time detected");
            }

            // Error rate alerts
            if (metrics.HttpMetrics.ErrorRate > 0.05)
            {
                alerts.Add("High error rate detected");
            }

            // Memory usage alerts
            if (metrics.SystemMetrics.MemoryUsagePercent > 80)
            {
                alerts.Add("High memory usage detected");
            }

            // Queue performance alerts
            if (metrics.QueueMetrics.AverageOperationTimeMs > 2000)
            {
                alerts.Add("Slow queue operations detected");
            }

            // Health score alerts
            if (healthScore < 70)
            {
                alerts.Add("System health score is below acceptable threshold");
            }

            return alerts.ToArray();
        }
    }
}
