using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Grande.Fila.API.Infrastructure.Logging
{
    /// <summary>
    /// Enhanced logging service with correlation IDs, performance tracking, and audit trails
    /// </summary>
    public class EnhancedLoggingService
    {
        private readonly ILogger<EnhancedLoggingService> _logger;
        private readonly AsyncLocal<string> _correlationId = new();
        private readonly AsyncLocal<Dictionary<string, object>> _contextData = new();

        public EnhancedLoggingService(ILogger<EnhancedLoggingService> logger)
        {
            _logger = logger ?? NullLogger<EnhancedLoggingService>.Instance;
        }

        /// <summary>
        /// Gets or sets the current correlation ID for request tracing
        /// </summary>
        public string CorrelationId
        {
            get => _correlationId.Value ?? Guid.NewGuid().ToString();
            set => _correlationId.Value = value;
        }

        /// <summary>
        /// Adds context data for the current request
        /// </summary>
        public void AddContextData(string key, object value)
        {
            _contextData.Value ??= new Dictionary<string, object>();
            _contextData.Value[key] = value;
        }

        /// <summary>
        /// Logs an operation with performance tracking
        /// </summary>
        public async Task<T> LogOperationAsync<T>(
            string operationName,
            Func<Task<T>> operation,
            Dictionary<string, object>? metadata = null,
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var startTime = DateTime.UtcNow;

            try
            {
                _logger.LogInformation(
                    "Starting operation {OperationName} at {StartTime} with correlation ID {CorrelationId}",
                    operationName, startTime, CorrelationId);

                if (metadata != null)
                {
                    foreach (var kvp in metadata)
                    {
                        AddContextData(kvp.Key, kvp.Value);
                    }
                }

                var result = await operation();
                stopwatch.Stop();

                _logger.LogInformation(
                    "Operation {OperationName} completed successfully in {ElapsedMs}ms with correlation ID {CorrelationId}",
                    operationName, stopwatch.ElapsedMilliseconds, CorrelationId);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                _logger.LogError(ex,
                    "Operation {OperationName} failed after {ElapsedMs}ms with correlation ID {CorrelationId}. Error: {ErrorMessage}",
                    operationName, stopwatch.ElapsedMilliseconds, CorrelationId, ex.Message);

                throw;
            }
        }

        /// <summary>
        /// Logs a business event with audit trail
        /// </summary>
        public void LogBusinessEvent(
            string eventType,
            string entityType,
            string entityId,
            string userId,
            Dictionary<string, object>? changes = null)
        {
            var logData = new Dictionary<string, object>
            {
                ["EventType"] = eventType,
                ["EntityType"] = entityType,
                ["EntityId"] = entityId,
                ["UserId"] = userId,
                ["CorrelationId"] = CorrelationId,
                ["Timestamp"] = DateTime.UtcNow
            };

            if (changes != null)
            {
                logData["Changes"] = changes;
            }

            _logger.LogInformation(
                "Business event: {EventType} on {EntityType} {EntityId} by user {UserId}",
                eventType, entityType, entityId, userId);

            // TODO: Send to audit log storage
        }

        /// <summary>
        /// Logs performance metrics
        /// </summary>
        public void LogPerformanceMetric(
            string metricName,
            double value,
            string unit = "ms",
            Dictionary<string, object>? tags = null)
        {
            var logData = new Dictionary<string, object>
            {
                ["MetricName"] = metricName,
                ["Value"] = value,
                ["Unit"] = unit,
                ["CorrelationId"] = CorrelationId,
                ["Timestamp"] = DateTime.UtcNow
            };

            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    logData[$"Tag_{tag.Key}"] = tag.Value;
                }
            }

            _logger.LogInformation(
                "Performance metric: {MetricName} = {Value}{Unit}",
                metricName, value, unit);

            // TODO: Send to metrics collection system
        }

        /// <summary>
        /// Logs security events
        /// </summary>
        public void LogSecurityEvent(
            string eventType,
            string userId,
            string ipAddress,
            string userAgent,
            bool isSuccess,
            string? failureReason = null)
        {
            var logData = new Dictionary<string, object>
            {
                ["EventType"] = eventType,
                ["UserId"] = userId,
                ["IpAddress"] = ipAddress,
                ["UserAgent"] = userAgent,
                ["IsSuccess"] = isSuccess,
                ["CorrelationId"] = CorrelationId,
                ["Timestamp"] = DateTime.UtcNow
            };

            if (!isSuccess && !string.IsNullOrEmpty(failureReason))
            {
                logData["FailureReason"] = failureReason;
            }

            var logLevel = isSuccess ? LogLevel.Information : LogLevel.Warning;
            
            _logger.Log(logLevel,
                "Security event: {EventType} for user {UserId} from {IpAddress} - Success: {IsSuccess}",
                eventType, userId, ipAddress, isSuccess);

            // TODO: Send to security monitoring system
        }

        /// <summary>
        /// Logs API request/response for debugging
        /// </summary>
        public void LogApiRequest(
            string method,
            string path,
            string? queryString,
            Dictionary<string, string>? headers,
            object? requestBody = null)
        {
            var logData = new Dictionary<string, object>
            {
                ["Method"] = method,
                ["Path"] = path,
                ["QueryString"] = queryString ?? "",
                ["CorrelationId"] = CorrelationId,
                ["Timestamp"] = DateTime.UtcNow
            };

            if (headers != null)
            {
                logData["Headers"] = headers;
            }

            if (requestBody != null)
            {
                logData["RequestBody"] = requestBody;
            }

            _logger.LogDebug(
                "API Request: {Method} {Path} with correlation ID {CorrelationId}",
                method, path, CorrelationId);
        }

        /// <summary>
        /// Logs API response for debugging
        /// </summary>
        public void LogApiResponse(
            int statusCode,
            string? responseBody,
            TimeSpan duration)
        {
            var logData = new Dictionary<string, object>
            {
                ["StatusCode"] = statusCode,
                ["Duration"] = duration.TotalMilliseconds,
                ["CorrelationId"] = CorrelationId,
                ["Timestamp"] = DateTime.UtcNow
            };

            if (!string.IsNullOrEmpty(responseBody))
            {
                logData["ResponseBody"] = responseBody;
            }

            var logLevel = statusCode >= 400 ? LogLevel.Warning : LogLevel.Debug;
            
            _logger.Log(logLevel,
                "API Response: {StatusCode} in {Duration}ms with correlation ID {CorrelationId}",
                statusCode, duration.TotalMilliseconds, CorrelationId);
        }

        /// <summary>
        /// Logs queue-specific events
        /// </summary>
        public void LogQueueEvent(
            string eventType,
            string queueId,
            string? customerId = null,
            string? staffId = null,
            Dictionary<string, object>? metadata = null)
        {
            var logData = new Dictionary<string, object>
            {
                ["EventType"] = eventType,
                ["QueueId"] = queueId,
                ["CorrelationId"] = CorrelationId,
                ["Timestamp"] = DateTime.UtcNow
            };

            if (!string.IsNullOrEmpty(customerId))
            {
                logData["CustomerId"] = customerId;
            }

            if (!string.IsNullOrEmpty(staffId))
            {
                logData["StaffId"] = staffId;
            }

            if (metadata != null)
            {
                foreach (var kvp in metadata)
                {
                    logData[kvp.Key] = kvp.Value;
                }
            }

            _logger.LogInformation(
                "Queue event: {EventType} on queue {QueueId} with correlation ID {CorrelationId}",
                eventType, queueId, CorrelationId);

            // TODO: Send to queue analytics system
        }

        /// <summary>
        /// Logs transfer events with analytics
        /// </summary>
        public void LogTransferEvent(
            string transferType,
            string originalQueueId,
            string targetQueueId,
            string customerId,
            bool isSuccessful,
            int? timeSaved = null,
            int? positionImproved = null)
        {
            var logData = new Dictionary<string, object>
            {
                ["TransferType"] = transferType,
                ["OriginalQueueId"] = originalQueueId,
                ["TargetQueueId"] = targetQueueId,
                ["CustomerId"] = customerId,
                ["IsSuccessful"] = isSuccessful,
                ["CorrelationId"] = CorrelationId,
                ["Timestamp"] = DateTime.UtcNow
            };

            if (timeSaved.HasValue)
            {
                logData["TimeSaved"] = timeSaved.Value;
            }

            if (positionImproved.HasValue)
            {
                logData["PositionImproved"] = positionImproved.Value;
            }

            var logLevel = isSuccessful ? LogLevel.Information : LogLevel.Warning;
            
            _logger.Log(logLevel,
                "Transfer event: {TransferType} from {OriginalQueueId} to {TargetQueueId} for customer {CustomerId} - Success: {IsSuccessful}",
                transferType, originalQueueId, targetQueueId, customerId, isSuccessful);

            // TODO: Send to transfer analytics system
        }

        /// <summary>
        /// Clears context data for the current request
        /// </summary>
        public void ClearContext()
        {
            _contextData.Value?.Clear();
            _correlationId.Value = null;
        }
    }
}
