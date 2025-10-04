using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Grande.Fila.API.Infrastructure.Services;
using Grande.Fila.API.Infrastructure.Authorization;

namespace Grande.Fila.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PerformanceController : ControllerBase
    {
        private readonly IQueryOptimizationService _queryOptimizationService;
        private readonly IConnectionPoolingService _connectionPoolingService;
        private readonly IQueryCacheService _cacheService;
        private readonly ILogger<PerformanceController> _logger;

        public PerformanceController(
            IQueryOptimizationService queryOptimizationService,
            IConnectionPoolingService connectionPoolingService,
            IQueryCacheService cacheService,
            ILogger<PerformanceController> logger)
        {
            _queryOptimizationService = queryOptimizationService;
            _connectionPoolingService = connectionPoolingService;
            _cacheService = cacheService;
            _logger = logger;
        }

        /// <summary>
        /// Get database query performance statistics
        /// </summary>
        /// <returns>Query performance metrics</returns>
        [HttpGet("query-stats")]
        [Authorize(Policy = "PlatformAdminOnly")]
        [ProducesResponseType(typeof(QueryPerformanceStats), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetQueryStats(CancellationToken cancellationToken = default)
        {
            try
            {
                var stats = await _queryOptimizationService.GetPerformanceStatsAsync(cancellationToken);
                
                _logger.LogInformation("Retrieved query performance statistics");
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve query performance statistics");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Failed to retrieve query statistics" });
            }
        }

        /// <summary>
        /// Get database connection pool statistics
        /// </summary>
        /// <returns>Connection pool metrics</returns>
        [HttpGet("connection-stats")]
        [Authorize(Policy = "PlatformAdminOnly")]
        [ProducesResponseType(typeof(ConnectionPoolStats), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetConnectionStats(CancellationToken cancellationToken = default)
        {
            try
            {
                var stats = await _connectionPoolingService.GetPoolStatsAsync(cancellationToken);
                
                _logger.LogInformation("Retrieved connection pool statistics");
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve connection pool statistics");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Failed to retrieve connection statistics" });
            }
        }

        /// <summary>
        /// Get cache performance statistics
        /// </summary>
        /// <returns>Cache performance metrics</returns>
        [HttpGet("cache-stats")]
        [Authorize(Policy = "PlatformAdminOnly")]
        [ProducesResponseType(typeof(CacheStatistics), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetCacheStats(CancellationToken cancellationToken = default)
        {
            try
            {
                var stats = await _cacheService.GetStatisticsAsync(cancellationToken);
                
                _logger.LogInformation("Retrieved cache performance statistics");
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve cache performance statistics");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Failed to retrieve cache statistics" });
            }
        }

        /// <summary>
        /// Get database connection health status
        /// </summary>
        /// <returns>Connection health information</returns>
        [HttpGet("health")]
        [AllowPublicAccess] // Allow public access for health checks
        [ProducesResponseType(typeof(ConnectionHealthStatus), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> GetConnectionHealth(CancellationToken cancellationToken = default)
        {
            try
            {
                var health = await _connectionPoolingService.CheckConnectionHealthAsync(cancellationToken);
                
                if (health.IsHealthy)
                {
                    return Ok(health);
                }
                else
                {
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, health);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check connection health");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { 
                    IsHealthy = false, 
                    Status = "Error", 
                    Issues = new[] { "Health check failed" },
                    Details = new { Error = ex.Message }
                });
            }
        }

        /// <summary>
        /// Optimize database connection pool settings
        /// </summary>
        /// <returns>Optimization results</returns>
        [HttpPost("optimize-connections")]
        [Authorize(Policy = "PlatformAdminOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> OptimizeConnections(CancellationToken cancellationToken = default)
        {
            try
            {
                await _connectionPoolingService.OptimizePoolSettingsAsync(cancellationToken);
                
                _logger.LogInformation("Connection pool optimization completed");
                return Ok(new { message = "Connection pool optimization completed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to optimize connection pool");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Failed to optimize connection pool" });
            }
        }

        /// <summary>
        /// Clear query cache
        /// </summary>
        /// <returns>Cache clearing results</returns>
        [HttpPost("clear-cache")]
        [Authorize(Policy = "PlatformAdminOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ClearCache(CancellationToken cancellationToken = default)
        {
            try
            {
                await _cacheService.ClearAllAsync(cancellationToken);
                
                _logger.LogInformation("Query cache cleared");
                return Ok(new { message = "Query cache cleared successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clear query cache");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Failed to clear query cache" });
            }
        }

        /// <summary>
        /// Get comprehensive performance dashboard data
        /// </summary>
        /// <returns>Complete performance metrics</returns>
        [HttpGet("dashboard")]
        [Authorize(Policy = "PlatformAdminOnly")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetPerformanceDashboard(CancellationToken cancellationToken = default)
        {
            try
            {
                var dashboard = new
                {
                    QueryStats = await _queryOptimizationService.GetPerformanceStatsAsync(cancellationToken),
                    ConnectionStats = await _connectionPoolingService.GetPoolStatsAsync(cancellationToken),
                    CacheStats = await _cacheService.GetStatisticsAsync(cancellationToken),
                    ConnectionHealth = await _connectionPoolingService.CheckConnectionHealthAsync(cancellationToken),
                    Timestamp = DateTime.UtcNow
                };
                
                _logger.LogInformation("Retrieved performance dashboard data");
                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve performance dashboard");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Failed to retrieve performance dashboard" });
            }
        }
    }
}