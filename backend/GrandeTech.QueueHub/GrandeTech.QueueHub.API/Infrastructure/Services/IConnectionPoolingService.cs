using Microsoft.EntityFrameworkCore;

namespace Grande.Fila.API.Infrastructure.Services
{
    /// <summary>
    /// Service for managing database connection pooling and connection health
    /// </summary>
    public interface IConnectionPoolingService
    {
        /// <summary>
        /// Get current connection pool statistics
        /// </summary>
        Task<ConnectionPoolStats> GetPoolStatsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Health check for database connections
        /// </summary>
        Task<ConnectionHealthStatus> CheckConnectionHealthAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Optimize connection pool settings based on current load
        /// </summary>
        Task OptimizePoolSettingsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Clear connection pool (emergency recovery)
        /// </summary>
        Task ClearConnectionPoolAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Connection pool statistics
    /// </summary>
    public class ConnectionPoolStats
    {
        public int ActiveConnections { get; set; }
        public int IdleConnections { get; set; }
        public int TotalConnections { get; set; }
        public int MaxPoolSize { get; set; }
        public int MinPoolSize { get; set; }
        public double PoolUtilizationPercent { get; set; }
        public int ConnectionTimeouts { get; set; }
        public int ConnectionFailures { get; set; }
        public TimeSpan AverageConnectionLifetime { get; set; }
        public Dictionary<string, object> AdditionalMetrics { get; set; } = new();
    }

    /// <summary>
    /// Connection health status
    /// </summary>
    public class ConnectionHealthStatus
    {
        public bool IsHealthy { get; set; }
        public string Status { get; set; } = string.Empty;
        public TimeSpan ResponseTime { get; set; }
        public DateTime LastChecked { get; set; }
        public List<string> Issues { get; set; } = new();
        public Dictionary<string, object> Details { get; set; } = new();
    }
}
