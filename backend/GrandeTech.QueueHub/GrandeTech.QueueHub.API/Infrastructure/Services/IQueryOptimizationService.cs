using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Grande.Fila.API.Infrastructure.Services
{
    /// <summary>
    /// Service for optimizing database queries and monitoring query performance
    /// </summary>
    public interface IQueryOptimizationService
    {
        /// <summary>
        /// Execute a query with performance monitoring
        /// </summary>
        Task<T> ExecuteWithMonitoringAsync<T>(Func<Task<T>> query, string operationName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Execute a query with automatic retry on transient failures
        /// </summary>
        Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> query, string operationName, int maxRetries = 3, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get query performance statistics
        /// </summary>
        Task<QueryPerformanceStats> GetPerformanceStatsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Optimize a query by adding hints or modifying execution plan
        /// </summary>
        IQueryable<T> OptimizeQuery<T>(IQueryable<T> query, string optimizationType = "default") where T : class;
    }

    /// <summary>
    /// Query performance statistics
    /// </summary>
    public class QueryPerformanceStats
    {
        public int TotalQueries { get; set; }
        public int SlowQueries { get; set; }
        public double AverageExecutionTimeMs { get; set; }
        public double MaxExecutionTimeMs { get; set; }
        public double MinExecutionTimeMs { get; set; }
        public Dictionary<string, int> QueryCountsByOperation { get; set; } = new();
        public Dictionary<string, double> AverageTimesByOperation { get; set; } = new();
        public List<SlowQueryInfo> RecentSlowQueries { get; set; } = new();
    }

    /// <summary>
    /// Information about a slow query
    /// </summary>
    public class SlowQueryInfo
    {
        public string OperationName { get; set; } = string.Empty;
        public double ExecutionTimeMs { get; set; }
        public DateTime ExecutedAt { get; set; }
        public string? QueryText { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new();
    }
}
