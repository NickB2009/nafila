using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Grande.Fila.API.Infrastructure.Services
{
    /// <summary>
    /// Service for monitoring system performance and collecting metrics
    /// </summary>
    public interface IPerformanceMonitoringService
    {
        /// <summary>
        /// Starts monitoring an operation
        /// </summary>
        IDisposable StartOperation(string operationName, Dictionary<string, object>? metadata = null);
        
        /// <summary>
        /// Records a performance metric
        /// </summary>
        void RecordMetric(string metricName, double value, string unit = "ms", Dictionary<string, object>? tags = null);
        
        /// <summary>
        /// Records response time for an endpoint
        /// </summary>
        void RecordResponseTime(string endpoint, string method, double responseTimeMs, int statusCode);
        
        /// <summary>
        /// Records queue operation performance
        /// </summary>
        void RecordQueueOperation(string operation, string queueId, double durationMs, bool isSuccess);
        
        /// <summary>
        /// Gets current performance metrics
        /// </summary>
        PerformanceMetrics GetCurrentMetrics();
        
        /// <summary>
        /// Gets performance history for a specific metric
        /// </summary>
        IEnumerable<MetricDataPoint> GetMetricHistory(string metricName, TimeSpan duration);
        
        /// <summary>
        /// Gets system health score based on performance metrics
        /// </summary>
        double GetSystemHealthScore();
    }

    /// <summary>
    /// Implementation of performance monitoring service
    /// </summary>
    public class PerformanceMonitoringService : IPerformanceMonitoringService
    {
        private readonly ILogger<PerformanceMonitoringService> _logger;
        private readonly PerformanceMonitoringOptions _options;
        private readonly ConcurrentDictionary<string, MetricAggregator> _metrics;
        private readonly ConcurrentDictionary<string, Queue<MetricDataPoint>> _metricHistory;
        private readonly Timer _cleanupTimer;

        public PerformanceMonitoringService(
            ILogger<PerformanceMonitoringService> logger,
            IOptions<PerformanceMonitoringOptions> options)
        {
            _logger = logger;
            _options = options.Value;
            _metrics = new ConcurrentDictionary<string, MetricAggregator>();
            _metricHistory = new ConcurrentDictionary<string, Queue<MetricDataPoint>>();
            
            // Start cleanup timer
            _cleanupTimer = new Timer(CleanupOldMetrics, null, 
                TimeSpan.FromMinutes(_options.CleanupIntervalMinutes), 
                TimeSpan.FromMinutes(_options.CleanupIntervalMinutes));
        }

        public IDisposable StartOperation(string operationName, Dictionary<string, object>? metadata = null)
        {
            return new OperationTracker(this, operationName, metadata);
        }

        public void RecordMetric(string metricName, double value, string unit = "ms", Dictionary<string, object>? tags = null)
        {
            try
            {
                var aggregator = _metrics.GetOrAdd(metricName, _ => new MetricAggregator());
                aggregator.AddValue(value);

                // Store in history
                var historyKey = $"{metricName}_{unit}";
                var history = _metricHistory.GetOrAdd(historyKey, _ => new Queue<MetricDataPoint>());
                
                lock (history)
                {
                    history.Enqueue(new MetricDataPoint
                    {
                        Timestamp = DateTime.UtcNow,
                        Value = value,
                        Unit = unit,
                        Tags = tags
                    });

                    // Keep only recent data points
                    while (history.Count > _options.MaxHistoryPoints)
                    {
                        history.Dequeue();
                    }
                }

                _logger.LogDebug("Recorded metric {MetricName}: {Value}{Unit}", metricName, value, unit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to record metric {MetricName}", metricName);
            }
        }

        public void RecordResponseTime(string endpoint, string method, double responseTimeMs, int statusCode)
        {
            var metricName = $"http_response_time_{method}_{endpoint}";
            var tags = new Dictionary<string, object>
            {
                ["endpoint"] = endpoint,
                ["method"] = method,
                ["status_code"] = statusCode,
                ["status_category"] = GetStatusCategory(statusCode)
            };

            RecordMetric(metricName, responseTimeMs, "ms", tags);
        }

        public void RecordQueueOperation(string operation, string queueId, double durationMs, bool isSuccess)
        {
            var metricName = $"queue_operation_{operation}";
            var tags = new Dictionary<string, object>
            {
                ["queue_id"] = queueId,
                ["success"] = isSuccess
            };

            RecordMetric(metricName, durationMs, "ms", tags);
        }

        public PerformanceMetrics GetCurrentMetrics()
        {
            var metrics = new PerformanceMetrics
            {
                Timestamp = DateTime.UtcNow,
                SystemMetrics = GetSystemMetrics(),
                HttpMetrics = GetHttpMetrics(),
                QueueMetrics = GetQueueMetrics(),
                CustomMetrics = GetCustomMetrics()
            };

            return metrics;
        }

        public IEnumerable<MetricDataPoint> GetMetricHistory(string metricName, TimeSpan duration)
        {
            var cutoff = DateTime.UtcNow.Subtract(duration);
            var historyKey = _metricHistory.Keys.FirstOrDefault(k => k.StartsWith(metricName));

            if (string.IsNullOrEmpty(historyKey) || !_metricHistory.TryGetValue(historyKey, out var history))
                return Enumerable.Empty<MetricDataPoint>();

            lock (history)
            {
                return history.Where(p => p.Timestamp >= cutoff).ToList();
            }
        }

        public double GetSystemHealthScore()
        {
            try
            {
                var metrics = GetCurrentMetrics();
                var score = 100.0;

                // Check response time health
                var avgResponseTime = metrics.HttpMetrics.AverageResponseTimeMs;
                if (avgResponseTime > 1000) score -= 20; // Penalty for slow responses
                else if (avgResponseTime > 500) score -= 10;

                // Check error rate
                var errorRate = metrics.HttpMetrics.ErrorRate;
                if (errorRate > 0.05) score -= 30; // Penalty for high error rate
                else if (errorRate > 0.01) score -= 15;

                // Check queue performance
                var avgQueueOperationTime = metrics.QueueMetrics.AverageOperationTimeMs;
                if (avgQueueOperationTime > 2000) score -= 20; // Penalty for slow queue operations
                else if (avgQueueOperationTime > 1000) score -= 10;

                // Check system resources
                var memoryUsage = metrics.SystemMetrics.MemoryUsagePercent;
                if (memoryUsage > 90) score -= 20; // Penalty for high memory usage
                else if (memoryUsage > 80) score -= 10;

                return Math.Max(0, score);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to calculate system health score");
                return 50.0; // Default score on error
            }
        }

        private SystemMetrics GetSystemMetrics()
        {
            var process = Process.GetCurrentProcess();
            var gcInfo = GC.GetGCMemoryInfo();

            return new SystemMetrics
            {
                CpuUsagePercent = GetCpuUsage(),
                MemoryUsagePercent = (double)process.WorkingSet64 / (double)process.MaxWorkingSet * 100,
                AvailableMemoryMB = GC.GetTotalMemory(false) / 1024 / 1024,
                ThreadCount = process.Threads.Count,
                ProcessUptimeSeconds = (DateTime.UtcNow - process.StartTime.ToUniversalTime()).TotalSeconds
            };
        }

        private HttpMetrics GetHttpMetrics()
        {
            var responseTimeMetrics = _metrics.Values
                .Where(m => m.Name.StartsWith("http_response_time"))
                .ToList();

            return new HttpMetrics
            {
                TotalRequests = responseTimeMetrics.Sum(m => m.Count),
                AverageResponseTimeMs = responseTimeMetrics.Any() ? responseTimeMetrics.Average(m => m.Average) : 0,
                MinResponseTimeMs = responseTimeMetrics.Any() ? responseTimeMetrics.Min(m => m.Min) : 0,
                MaxResponseTimeMs = responseTimeMetrics.Any() ? responseTimeMetrics.Max(m => m.Max) : 0,
                ErrorRate = CalculateErrorRate()
            };
        }

        private QueueMetrics GetQueueMetrics()
        {
            var queueMetrics = _metrics.Values
                .Where(m => m.Name.StartsWith("queue_operation"))
                .ToList();

            return new QueueMetrics
            {
                TotalOperations = queueMetrics.Sum(m => m.Count),
                AverageOperationTimeMs = queueMetrics.Any() ? queueMetrics.Average(m => m.Average) : 0,
                MinOperationTimeMs = queueMetrics.Any() ? queueMetrics.Min(m => m.Min) : 0,
                MaxOperationTimeMs = queueMetrics.Any() ? queueMetrics.Max(m => m.Max) : 0
            };
        }

        private Dictionary<string, MetricSummary> GetCustomMetrics()
        {
            var customMetrics = _metrics.Values
                .Where(m => !m.Name.StartsWith("http_") && !m.Name.StartsWith("queue_"))
                .ToDictionary(m => m.Name, m => new MetricSummary
                {
                    Count = m.Count,
                    Average = m.Average,
                    Min = m.Min,
                    Max = m.Max
                });

            return customMetrics;
        }

        private double GetCpuUsage()
        {
            // Simplified CPU usage calculation
            // In production, you might want to use PerformanceCounters or other system APIs
            return 0.0; // Placeholder
        }

        private double CalculateErrorRate()
        {
            var totalRequests = _metrics.Values
                .Where(m => m.Name.StartsWith("http_response_time"))
                .Sum(m => m.Count);

            if (totalRequests == 0) return 0.0;

            var errorRequests = _metrics.Values
                .Where(m => m.Name.StartsWith("http_response_time"))
                .Count(m => m.Name.Contains("status_code") && m.Name.Contains("4") || m.Name.Contains("5"));

            return (double)errorRequests / totalRequests;
        }

        private string GetStatusCategory(int statusCode)
        {
            if (statusCode >= 200 && statusCode < 300) return "success";
            if (statusCode >= 300 && statusCode < 400) return "redirect";
            if (statusCode >= 400 && statusCode < 500) return "client_error";
            if (statusCode >= 500) return "server_error";
            return "unknown";
        }

        private void CleanupOldMetrics(object? state)
        {
            try
            {
                var cutoff = DateTime.UtcNow.Subtract(TimeSpan.FromHours(_options.MetricRetentionHours));
                
                foreach (var kvp in _metricHistory)
                {
                    lock (kvp.Value)
                    {
                        while (kvp.Value.Count > 0 && kvp.Value.Peek().Timestamp < cutoff)
                        {
                            kvp.Value.Dequeue();
                        }
                    }
                }

                _logger.LogDebug("Cleaned up old metrics, retention: {RetentionHours}h", _options.MetricRetentionHours);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cleanup old metrics");
            }
        }

        public void Dispose()
        {
            _cleanupTimer?.Dispose();
        }
    }

    /// <summary>
    /// Tracks individual operations for performance monitoring
    /// </summary>
    public class OperationTracker : IDisposable
    {
        private readonly PerformanceMonitoringService _service;
        private readonly string _operationName;
        private readonly Dictionary<string, object>? _metadata;
        private readonly Stopwatch _stopwatch;

        public OperationTracker(PerformanceMonitoringService service, string operationName, Dictionary<string, object>? metadata)
        {
            _service = service;
            _operationName = operationName;
            _metadata = metadata;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _service.RecordMetric(_operationName, _stopwatch.ElapsedMilliseconds, "ms", _metadata);
        }
    }

    /// <summary>
    /// Aggregates metric values for statistical analysis
    /// </summary>
    public class MetricAggregator
    {
        public string Name { get; set; } = string.Empty;
        public long Count { get; private set; }
        public double Sum { get; private set; }
        public double Min { get; private set; } = double.MaxValue;
        public double Max { get; private set; } = double.MinValue;
        public double Average => Count > 0 ? Sum / Count : 0;

        public void AddValue(double value)
        {
            Count++;
            Sum += value;
            Min = Math.Min(Min, value);
            Max = Math.Max(Max, value);
        }
    }

    /// <summary>
    /// Performance metrics summary
    /// </summary>
    public class PerformanceMetrics
    {
        public DateTime Timestamp { get; set; }
        public SystemMetrics SystemMetrics { get; set; } = new();
        public HttpMetrics HttpMetrics { get; set; } = new();
        public QueueMetrics QueueMetrics { get; set; } = new();
        public Dictionary<string, MetricSummary> CustomMetrics { get; set; } = new();
    }

    /// <summary>
    /// System resource metrics
    /// </summary>
    public class SystemMetrics
    {
        public double CpuUsagePercent { get; set; }
        public double MemoryUsagePercent { get; set; }
        public double AvailableMemoryMB { get; set; }
        public int ThreadCount { get; set; }
        public double ProcessUptimeSeconds { get; set; }
    }

    /// <summary>
    /// HTTP performance metrics
    /// </summary>
    public class HttpMetrics
    {
        public long TotalRequests { get; set; }
        public double AverageResponseTimeMs { get; set; }
        public double MinResponseTimeMs { get; set; }
        public double MaxResponseTimeMs { get; set; }
        public double ErrorRate { get; set; }
    }

    /// <summary>
    /// Queue operation metrics
    /// </summary>
    public class QueueMetrics
    {
        public long TotalOperations { get; set; }
        public double AverageOperationTimeMs { get; set; }
        public double MinOperationTimeMs { get; set; }
        public double MaxOperationTimeMs { get; set; }
    }

    /// <summary>
    /// Metric summary for a specific metric
    /// </summary>
    public class MetricSummary
    {
        public long Count { get; set; }
        public double Average { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
    }

    /// <summary>
    /// Individual metric data point
    /// </summary>
    public class MetricDataPoint
    {
        public DateTime Timestamp { get; set; }
        public double Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public Dictionary<string, object>? Tags { get; set; }
    }

    /// <summary>
    /// Configuration options for performance monitoring
    /// </summary>
    public class PerformanceMonitoringOptions
    {
        /// <summary>
        /// Maximum number of history points to keep per metric
        /// </summary>
        public int MaxHistoryPoints { get; set; } = 1000;

        /// <summary>
        /// How often to cleanup old metrics (in minutes)
        /// </summary>
        public int CleanupIntervalMinutes { get; set; } = 15;

        /// <summary>
        /// How long to retain metrics (in hours)
        /// </summary>
        public int MetricRetentionHours { get; set; } = 24;

        /// <summary>
        /// Whether to enable detailed performance logging
        /// </summary>
        public bool EnableDetailedLogging { get; set; } = false;
    }
}
