using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Grande.Fila.API.Application.Queues;
using Grande.Fila.API.Application.Queues.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic; // Added for List

namespace Grande.Fila.API.Controllers
{
    /// <summary>
    /// Controller for queue analytics and metrics
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class QueueAnalyticsController : ControllerBase
    {
        private readonly IQueueAnalyticsService _analyticsService;
        private readonly ILogger<QueueAnalyticsController> _logger;

        public QueueAnalyticsController(
            IQueueAnalyticsService analyticsService,
            ILogger<QueueAnalyticsController> logger)
        {
            _analyticsService = analyticsService;
            _logger = logger;
        }

        /// <summary>
        /// Calculate estimated wait time for a new queue entry
        /// </summary>
        [HttpGet("wait-time/{salonId}")]
        [ProducesResponseType(typeof(WaitTimeEstimate), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetEstimatedWaitTime(
            Guid salonId,
            [FromQuery] string serviceType = "haircut",
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (salonId == Guid.Empty)
                {
                    return BadRequest("Invalid salon ID");
                }

                var estimatedWaitTime = await _analyticsService.CalculateEstimatedWaitTimeAsync(
                    salonId, serviceType, cancellationToken);

                var result = new WaitTimeEstimate
                {
                    SalonId = salonId,
                    ServiceType = serviceType,
                    EstimatedWaitTime = estimatedWaitTime,
                    CalculatedAt = DateTime.UtcNow,
                    Confidence = "High" // Based on current queue data
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to calculate estimated wait time for salon {SalonId}", salonId);
                return StatusCode(500, "Failed to calculate wait time");
            }
        }

        /// <summary>
        /// Get queue performance metrics for a salon
        /// </summary>
        [HttpGet("performance/{salonId}")]
        [ProducesResponseType(typeof(QueuePerformanceMetrics), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetQueuePerformance(
            Guid salonId,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (salonId == Guid.Empty)
                {
                    return BadRequest("Invalid salon ID");
                }

                var metrics = await _analyticsService.GetQueuePerformanceMetricsAsync(
                    salonId, fromDate, toDate, cancellationToken);

                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get performance metrics for salon {SalonId}", salonId);
                return StatusCode(500, "Failed to retrieve performance metrics");
            }
        }

        /// <summary>
        /// Get wait time trends for a salon
        /// </summary>
        [HttpGet("trends/{salonId}")]
        [ProducesResponseType(typeof(List<WaitTimeTrend>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetWaitTimeTrends(
            Guid salonId,
            [FromQuery] int days = 7,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (salonId == Guid.Empty)
                {
                    return BadRequest("Invalid salon ID");
                }

                if (days < 1 || days > 30)
                {
                    return BadRequest("Days must be between 1 and 30");
                }

                var trends = await _analyticsService.GetWaitTimeTrendsAsync(
                    salonId, days, cancellationToken);

                return Ok(trends);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get wait time trends for salon {SalonId}", salonId);
                return StatusCode(500, "Failed to retrieve wait time trends");
            }
        }

        /// <summary>
        /// Record customer satisfaction feedback
        /// </summary>
        [HttpPost("satisfaction")]
        [ProducesResponseType(typeof(CustomerSatisfactionResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> RecordCustomerSatisfaction(
            [FromBody] CustomerSatisfactionRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Request body is required");
                }

                if (request.Rating < 1 || request.Rating > 5)
                {
                    return BadRequest("Rating must be between 1 and 5");
                }

                var success = await _analyticsService.RecordCustomerSatisfactionAsync(
                    request.QueueEntryId,
                    request.Rating,
                    request.Feedback,
                    cancellationToken);

                if (success)
                {
                    var response = new CustomerSatisfactionResponse
                    {
                        Success = true,
                        Message = "Thank you for your feedback!",
                        RecordedAt = DateTime.UtcNow
                    };

                    return Ok(response);
                }

                return StatusCode(500, "Failed to record feedback");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to record customer satisfaction for entry {EntryId}", 
                    request?.QueueEntryId);
                return StatusCode(500, "Failed to record feedback");
            }
        }

        /// <summary>
        /// Get queue recommendations for a salon
        /// </summary>
        [HttpGet("recommendations/{salonId}")]
        [ProducesResponseType(typeof(QueueRecommendations), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetQueueRecommendations(
            Guid salonId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (salonId == Guid.Empty)
                {
                    return BadRequest("Invalid salon ID");
                }

                var recommendations = await _analyticsService.GetQueueRecommendationsAsync(
                    salonId, cancellationToken);

                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get recommendations for salon {SalonId}", salonId);
                return StatusCode(500, "Failed to retrieve recommendations");
            }
        }

        /// <summary>
        /// Get queue health status for a salon
        /// </summary>
        [HttpGet("health/{salonId}")]
        [ProducesResponseType(typeof(QueueAnalyticsHealthStatus), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetQueueHealth(
            Guid salonId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (salonId == Guid.Empty)
                {
                    return BadRequest("Invalid salon ID");
                }

                // For now, return a mock health status
                // In a real implementation, this would calculate based on actual data
                var healthStatus = new QueueAnalyticsHealthStatus
                {
                    SalonId = salonId,
                    Status = "Healthy",
                    LastUpdated = DateTime.UtcNow,
                    CurrentQueueLength = 5,
                    CurrentAverageWaitTime = TimeSpan.FromMinutes(15),
                    HealthScore = 85.0,
                    Issues = new List<string>(),
                    Recommendations = new List<string> { "Queue is operating normally" }
                };

                return Ok(healthStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get health status for salon {SalonId}", salonId);
                return StatusCode(500, "Failed to retrieve health status");
            }
        }
    }

    #region Request/Response Models

    public class WaitTimeEstimate
    {
        public Guid SalonId { get; set; }
        public string ServiceType { get; set; } = string.Empty;
        public TimeSpan EstimatedWaitTime { get; set; }
        public DateTime CalculatedAt { get; set; }
        public string Confidence { get; set; } = string.Empty;
    }

    public class CustomerSatisfactionRequest
    {
        public Guid QueueEntryId { get; set; }
        public int Rating { get; set; }
        public string? Feedback { get; set; }
    }

    public class CustomerSatisfactionResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime RecordedAt { get; set; }
    }

    #endregion
}
