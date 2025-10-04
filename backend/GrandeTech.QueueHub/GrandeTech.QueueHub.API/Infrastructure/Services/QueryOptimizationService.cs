using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using Grande.Fila.API.Infrastructure.Data;

namespace Grande.Fila.API.Infrastructure.Services
{
    /// <summary>
    /// Implementation of query optimization service with performance monitoring and retry logic
    /// </summary>
    public class QueryOptimizationService : IQueryOptimizationService
    {
        private readonly ILogger<QueryOptimizationService> _logger;
        private readonly QueueHubDbContext _context;
        
        // Thread-safe collections for performance tracking
        private static readonly ConcurrentDictionary<string, List<double>> _executionTimes = new();
        private static readonly ConcurrentQueue<SlowQueryInfo> _slowQueries = new();
        private static readonly ConcurrentDictionary<string, int> _queryCounts = new();
        
        // Configuration
        private const double SlowQueryThresholdMs = 1000.0; // 1 second
        private const int MaxSlowQueryHistory = 100;

        public QueryOptimizationService(ILogger<QueryOptimizationService> logger, QueueHubDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<T> ExecuteWithMonitoringAsync<T>(Func<Task<T>> query, string operationName, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var queryId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                _logger.LogDebug("Starting query execution: {OperationName} [QueryId: {QueryId}]", operationName, queryId);
                
                var result = await query();
                
                stopwatch.Stop();
                var executionTime = stopwatch.Elapsed.TotalMilliseconds;
                
                // Track performance metrics
                TrackQueryPerformance(operationName, executionTime);
                
                _logger.LogInformation("Query completed: {OperationName} in {ExecutionTime}ms [QueryId: {QueryId}]", 
                    operationName, executionTime, queryId);
                
                // Log slow queries
                if (executionTime > SlowQueryThresholdMs)
                {
                    var slowQueryInfo = new SlowQueryInfo
                    {
                        OperationName = operationName,
                        ExecutionTimeMs = executionTime,
                        ExecutedAt = DateTime.UtcNow,
                        QueryText = null, // Could be populated with actual SQL if needed
                        Parameters = new Dictionary<string, object>()
                    };
                    
                    _slowQueries.Enqueue(slowQueryInfo);
                    
                    // Keep only recent slow queries
                    while (_slowQueries.Count > MaxSlowQueryHistory)
                    {
                        _slowQueries.TryDequeue(out _);
                    }
                    
                    _logger.LogWarning("Slow query detected: {OperationName} took {ExecutionTime}ms [QueryId: {QueryId}]", 
                        operationName, executionTime, queryId);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                var executionTime = stopwatch.Elapsed.TotalMilliseconds;
                
                _logger.LogError(ex, "Query failed: {OperationName} after {ExecutionTime}ms [QueryId: {QueryId}]", 
                    operationName, executionTime, queryId);
                
                throw;
            }
        }

        public async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> query, string operationName, int maxRetries = 3, CancellationToken cancellationToken = default)
        {
            var attempt = 0;
            Exception? lastException = null;
            
            while (attempt <= maxRetries)
            {
                try
                {
                    return await ExecuteWithMonitoringAsync(query, $"{operationName}_Attempt_{attempt + 1}", cancellationToken);
                }
                catch (Exception ex) when (IsRetryableException(ex) && attempt < maxRetries)
                {
                    lastException = ex;
                    attempt++;
                    
                    var delay = CalculateRetryDelay(attempt);
                    _logger.LogWarning("Query failed, retrying in {Delay}ms: {OperationName} [Attempt {Attempt}/{MaxRetries}]", 
                        delay, operationName, attempt, maxRetries);
                    
                    await Task.Delay(delay, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Query failed after {MaxRetries} retries: {OperationName}", maxRetries, operationName);
                    throw;
                }
            }
            
            throw lastException ?? new InvalidOperationException($"Query failed after {maxRetries} retries");
        }

        public async Task<QueryPerformanceStats> GetPerformanceStatsAsync(CancellationToken cancellationToken = default)
        {
            var stats = new QueryPerformanceStats();
            
            // Calculate overall statistics
            var allExecutionTimes = _executionTimes.Values.SelectMany(times => times).ToList();
            
            if (allExecutionTimes.Any())
            {
                stats.TotalQueries = allExecutionTimes.Count;
                stats.AverageExecutionTimeMs = allExecutionTimes.Average();
                stats.MaxExecutionTimeMs = allExecutionTimes.Max();
                stats.MinExecutionTimeMs = allExecutionTimes.Min();
                stats.SlowQueries = allExecutionTimes.Count(t => t > SlowQueryThresholdMs);
            }
            
            // Calculate per-operation statistics
            foreach (var kvp in _executionTimes)
            {
                var operation = kvp.Key;
                var times = kvp.Value;
                
                stats.QueryCountsByOperation[operation] = times.Count;
                stats.AverageTimesByOperation[operation] = times.Average();
            }
            
            // Get recent slow queries
            stats.RecentSlowQueries = _slowQueries.ToList();
            
            return await Task.FromResult(stats);
        }

        public IQueryable<T> OptimizeQuery<T>(IQueryable<T> query, string optimizationType = "default") where T : class
        {
            switch (optimizationType.ToLower())
            {
                case "split":
                    // Use split queries for better performance with multiple includes
                    return query.TagWith("Optimization: SplitQuery");
                
                case "stream":
                    // Use streaming for large result sets
                    return query.TagWith("Optimization: Streaming");
                
                case "cache":
                    // Tag for potential caching
                    return query.TagWith("Optimization: Cacheable");
                
                case "readonly":
                    // Optimize for read-only operations
                    return query.TagWith("Optimization: ReadOnly").AsNoTracking();
                
                default:
                    // Default optimization - add tracking hints
                    return query.TagWith("Optimization: Default");
            }
        }

        #region Private Methods

        private static void TrackQueryPerformance(string operationName, double executionTime)
        {
            _executionTimes.AddOrUpdate(
                operationName,
                new List<double> { executionTime },
                (key, existingList) =>
                {
                    lock (existingList)
                    {
                        existingList.Add(executionTime);
                        
                        // Keep only recent execution times (last 1000 per operation)
                        if (existingList.Count > 1000)
                        {
                            existingList.RemoveAt(0);
                        }
                    }
                    return existingList;
                });
            
            _queryCounts.AddOrUpdate(operationName, 1, (key, count) => count + 1);
        }

        private static bool IsRetryableException(Exception ex)
        {
            return ex is DbUpdateConcurrencyException ||
                   ex is DbUpdateException ||
                   (ex is InvalidOperationException && ex.Message.Contains("connection")) ||
                   ex is TimeoutException;
        }

        private static int CalculateRetryDelay(int attempt)
        {
            // Exponential backoff: 100ms, 200ms, 400ms, 800ms, etc.
            return (int)(100 * Math.Pow(2, attempt - 1));
        }

        #endregion
    }
}
