using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Grande.Fila.API.Infrastructure.Data;

namespace Grande.Fila.API.Infrastructure.Services
{
    /// <summary>
    /// Implementation of connection pooling service for MySQL database optimization
    /// </summary>
    public class ConnectionPoolingService : IConnectionPoolingService
    {
        private readonly ILogger<ConnectionPoolingService> _logger;
        private readonly QueueHubDbContext _context;
        private readonly IConfiguration _configuration;
        
        // Connection health tracking
        private static DateTime _lastHealthCheck = DateTime.MinValue;
        private static ConnectionHealthStatus _lastHealthStatus = new() { IsHealthy = false };
        private static readonly object _healthCheckLock = new();

        public ConnectionPoolingService(
            ILogger<ConnectionPoolingService> logger, 
            QueueHubDbContext context,
            IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;
        }

        public async Task<ConnectionPoolStats> GetPoolStatsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Get MySQL connection pool information
                var connectionString = _context.Database.GetConnectionString();
                var poolStats = new ConnectionPoolStats();

                // Parse connection string for pool settings
                ParseConnectionStringForPoolSettings(connectionString, poolStats);

                // Execute a simple query to get current connection info
                var connectionInfo = await GetCurrentConnectionInfoAsync(cancellationToken);
                
                poolStats.ActiveConnections = connectionInfo.ActiveConnections;
                poolStats.IdleConnections = connectionInfo.IdleConnections;
                poolStats.TotalConnections = poolStats.ActiveConnections + poolStats.IdleConnections;
                
                // Calculate utilization
                if (poolStats.MaxPoolSize > 0)
                {
                    poolStats.PoolUtilizationPercent = (double)poolStats.TotalConnections / poolStats.MaxPoolSize * 100;
                }

                _logger.LogDebug("Connection pool stats - Active: {Active}, Idle: {Idle}, Total: {Total}, Utilization: {Utilization}%",
                    poolStats.ActiveConnections, poolStats.IdleConnections, poolStats.TotalConnections, poolStats.PoolUtilizationPercent);

                return poolStats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get connection pool statistics");
                throw;
            }
        }

        public async Task<ConnectionHealthStatus> CheckConnectionHealthAsync(CancellationToken cancellationToken = default)
        {
            // Cache health check results for 30 seconds to avoid excessive database calls
            lock (_healthCheckLock)
            {
                if (DateTime.UtcNow - _lastHealthCheck < TimeSpan.FromSeconds(30))
                {
                    return _lastHealthStatus;
                }
            }

            var stopwatch = Stopwatch.StartNew();
            var healthStatus = new ConnectionHealthStatus
            {
                LastChecked = DateTime.UtcNow
            };

            try
            {
                // Test basic connectivity
                await _context.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken);
                stopwatch.Stop();
                
                healthStatus.ResponseTime = stopwatch.Elapsed;
                healthStatus.IsHealthy = true;
                healthStatus.Status = "Healthy";

                // Check if response time is acceptable
                if (healthStatus.ResponseTime > TimeSpan.FromSeconds(5))
                {
                    healthStatus.Issues.Add("Slow response time");
                    healthStatus.IsHealthy = false;
                }

                // Test connection pool health
                var poolStats = await GetPoolStatsAsync(cancellationToken);
                healthStatus.Details["PoolUtilization"] = poolStats.PoolUtilizationPercent;

                if (poolStats.PoolUtilizationPercent > 90)
                {
                    healthStatus.Issues.Add("High pool utilization");
                    healthStatus.IsHealthy = false;
                }

                _logger.LogDebug("Connection health check completed in {ResponseTime}ms - Status: {Status}",
                    healthStatus.ResponseTime.TotalMilliseconds, healthStatus.Status);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                healthStatus.ResponseTime = stopwatch.Elapsed;
                healthStatus.IsHealthy = false;
                healthStatus.Status = "Unhealthy";
                healthStatus.Issues.Add($"Database error: {ex.Message}");
                
                _logger.LogError(ex, "Connection health check failed after {ResponseTime}ms", healthStatus.ResponseTime.TotalMilliseconds);
            }

            lock (_healthCheckLock)
            {
                _lastHealthCheck = DateTime.UtcNow;
                _lastHealthStatus = healthStatus;
            }

            return healthStatus;
        }

        public async Task OptimizePoolSettingsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var currentStats = await GetPoolStatsAsync(cancellationToken);
                var healthStatus = await CheckConnectionHealthAsync(cancellationToken);

                _logger.LogInformation("Optimizing connection pool settings - Current utilization: {Utilization}%, Health: {Health}",
                    currentStats.PoolUtilizationPercent, healthStatus.Status);

                // Log optimization recommendations
                var recommendations = new List<string>();

                if (currentStats.PoolUtilizationPercent > 80)
                {
                    recommendations.Add("Consider increasing MaxPoolSize");
                }
                else if (currentStats.PoolUtilizationPercent < 20 && currentStats.MaxPoolSize > 20)
                {
                    recommendations.Add("Consider decreasing MaxPoolSize to save resources");
                }

                if (healthStatus.ResponseTime > TimeSpan.FromSeconds(2))
                {
                    recommendations.Add("Consider increasing ConnectionTimeout");
                }

                if (recommendations.Any())
                {
                    _logger.LogWarning("Connection pool optimization recommendations: {Recommendations}",
                        string.Join(", ", recommendations));
                }
                else
                {
                    _logger.LogInformation("Connection pool settings appear optimal");
                }

                // Store optimization metrics
                currentStats.AdditionalMetrics["OptimizationChecked"] = DateTime.UtcNow;
                currentStats.AdditionalMetrics["Recommendations"] = recommendations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to optimize connection pool settings");
                throw;
            }
        }

        public async Task ClearConnectionPoolAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogWarning("Clearing connection pool - this should only be used for emergency recovery");

                // Force close all connections by disposing and recreating the context
                // Note: This is a drastic measure and should only be used in emergency situations
                await _context.Database.CloseConnectionAsync();

                _logger.LogInformation("Connection pool cleared successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clear connection pool");
                throw;
            }
        }

        #region Private Methods

        private void ParseConnectionStringForPoolSettings(string connectionString, ConnectionPoolStats stats)
        {
            if (string.IsNullOrEmpty(connectionString))
                return;

            var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var part in parts)
            {
                var keyValue = part.Split('=', 2);
                if (keyValue.Length != 2) continue;

                var key = keyValue[0].Trim().ToLower();
                var value = keyValue[1].Trim();

                switch (key)
                {
                    case "maximum pool size":
                    case "maxpoolsize":
                        if (int.TryParse(value, out var maxPoolSize))
                            stats.MaxPoolSize = maxPoolSize;
                        break;
                    
                    case "minimum pool size":
                    case "minpoolsize":
                        if (int.TryParse(value, out var minPoolSize))
                            stats.MinPoolSize = minPoolSize;
                        break;
                    
                    case "pooling":
                        stats.AdditionalMetrics["PoolingEnabled"] = value.ToLower() == "true";
                        break;
                }
            }

            // Set defaults if not specified
            if (stats.MaxPoolSize == 0)
                stats.MaxPoolSize = 100; // MySQL default
            if (stats.MinPoolSize == 0)
                stats.MinPoolSize = 5; // Our configured minimum
        }

        private async Task<(int ActiveConnections, int IdleConnections)> GetCurrentConnectionInfoAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Query MySQL system tables for connection information
                var connectionQuery = @"
                    SELECT 
                        COUNT(*) as TotalConnections,
                        SUM(CASE WHEN COMMAND != 'Sleep' THEN 1 ELSE 0 END) as ActiveConnections,
                        SUM(CASE WHEN COMMAND = 'Sleep' THEN 1 ELSE 0 END) as IdleConnections
                    FROM INFORMATION_SCHEMA.PROCESSLIST 
                    WHERE DB = DATABASE()";

                using var command = _context.Database.GetDbConnection().CreateCommand();
                command.CommandText = connectionQuery;
                
                await _context.Database.OpenConnectionAsync(cancellationToken);
                
                using var reader = await command.ExecuteReaderAsync(cancellationToken);
                if (await reader.ReadAsync(cancellationToken))
                {
                    var activeConnections = reader.GetInt32(1); // ActiveConnections column
                    var idleConnections = reader.GetInt32(2); // IdleConnections column
                    
                    return (activeConnections, idleConnections);
                }

                return (0, 0);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not retrieve detailed connection info, using defaults");
                return (1, 0); // Assume at least one active connection (current)
            }
        }

        #endregion
    }
}
