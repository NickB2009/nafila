using Microsoft.Extensions.Logging;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System.Diagnostics;

namespace Grande.Fila.API.Application.Logging;

public class LoggingService : ILoggingService
{
    private readonly ILogger<LoggingService> _logger;
    private readonly TelemetryClient _telemetryClient;

    public LoggingService(ILogger<LoggingService> logger, TelemetryClient telemetryClient)
    {
        _logger = logger;
        _telemetryClient = telemetryClient;
    }

    public void LogInformation(string message, params object[] args)
    {
        _logger.LogInformation(message, args);
    }

    public void LogWarning(string message, params object[] args)
    {
        _logger.LogWarning(message, args);
    }

    public void LogError(string message, Exception? exception = null, params object[] args)
    {
        if (exception != null)
        {
            _logger.LogError(exception, message, args);
            _telemetryClient.TrackException(exception);
        }
        else
        {
            _logger.LogError(message, args);
        }
    }

    public void LogCritical(string message, Exception? exception = null, params object[] args)
    {
        if (exception != null)
        {
            _logger.LogCritical(exception, message, args);
            _telemetryClient.TrackException(exception);
        }
        else
        {
            _logger.LogCritical(message, args);
        }
    }

    public void LogDebug(string message, params object[] args)
    {
        _logger.LogDebug(message, args);
    }

    public void LogTrace(string message, params object[] args)
    {
        _logger.LogTrace(message, args);
    }

    public void LogApiRequest(string method, string path, string? userId = null, string? organizationId = null, string? locationId = null)
    {
        var properties = new Dictionary<string, object>
        {
            ["Method"] = method,
            ["Path"] = path,
            ["EventType"] = "ApiRequest"
        };

        if (!string.IsNullOrEmpty(userId))
            properties["UserId"] = userId;
        if (!string.IsNullOrEmpty(organizationId))
            properties["OrganizationId"] = organizationId;
        if (!string.IsNullOrEmpty(locationId))
            properties["LocationId"] = locationId;

        _logger.LogInformation("API Request: {Method} {Path}", method, path);
        TrackCustomEvent("ApiRequest", properties);
    }

    public void LogApiResponse(string method, string path, int statusCode, long durationMs, string? userId = null)
    {
        var properties = new Dictionary<string, object>
        {
            ["Method"] = method,
            ["Path"] = path,
            ["StatusCode"] = statusCode,
            ["DurationMs"] = durationMs,
            ["EventType"] = "ApiResponse"
        };

        if (!string.IsNullOrEmpty(userId))
            properties["UserId"] = userId;

        var logLevel = statusCode >= 500 ? LogLevel.Error : statusCode >= 400 ? LogLevel.Warning : LogLevel.Information;
        _logger.Log(logLevel, "API Response: {Method} {Path} - {StatusCode} ({DurationMs}ms)", method, path, statusCode, durationMs);
        
        TrackCustomEvent("ApiResponse", properties);
        TrackCustomMetric("ApiResponseTime", durationMs, new Dictionary<string, object> { ["StatusCode"] = statusCode });
    }

    public void LogDatabaseOperation(string operation, string table, long durationMs, bool success, string? error = null)
    {
        var properties = new Dictionary<string, object>
        {
            ["Operation"] = operation,
            ["Table"] = table,
            ["DurationMs"] = durationMs,
            ["Success"] = success,
            ["EventType"] = "DatabaseOperation"
        };

        if (!string.IsNullOrEmpty(error))
            properties["Error"] = error;

        var logLevel = success ? LogLevel.Debug : LogLevel.Error;
        _logger.Log(logLevel, "Database Operation: {Operation} on {Table} - {Success} ({DurationMs}ms)", operation, table, success, durationMs);
        
        TrackCustomEvent("DatabaseOperation", properties);
        TrackCustomMetric("DatabaseOperationTime", durationMs, new Dictionary<string, object> { ["Operation"] = operation, ["Success"] = success });
    }

    public void LogBusinessEvent(string eventName, string? userId = null, string? organizationId = null, string? locationId = null, object? additionalData = null)
    {
        var properties = new Dictionary<string, object>
        {
            ["EventName"] = eventName,
            ["EventType"] = "BusinessEvent"
        };

        if (!string.IsNullOrEmpty(userId))
            properties["UserId"] = userId;
        if (!string.IsNullOrEmpty(organizationId))
            properties["OrganizationId"] = organizationId;
        if (!string.IsNullOrEmpty(locationId))
            properties["LocationId"] = locationId;
        if (additionalData != null)
            properties["AdditionalData"] = additionalData.ToString() ?? "";

        _logger.LogInformation("Business Event: {EventName}", eventName);
        TrackCustomEvent("BusinessEvent", properties);
    }

    public void LogSecurityEvent(string eventName, string? userId = null, string? ipAddress = null, bool success = true, string? details = null)
    {
        var properties = new Dictionary<string, object>
        {
            ["EventName"] = eventName,
            ["Success"] = success,
            ["EventType"] = "SecurityEvent"
        };

        if (!string.IsNullOrEmpty(userId))
            properties["UserId"] = userId;
        if (!string.IsNullOrEmpty(ipAddress))
            properties["IpAddress"] = ipAddress;
        if (!string.IsNullOrEmpty(details))
            properties["Details"] = details;

        var logLevel = success ? LogLevel.Information : LogLevel.Warning;
        _logger.Log(logLevel, "Security Event: {EventName} - {Success}", eventName, success);
        TrackCustomEvent("SecurityEvent", properties);
    }

    public void LogPerformanceMetric(string metricName, double value, string? unit = null, string? context = null)
    {
        var properties = new Dictionary<string, object>
        {
            ["MetricName"] = metricName,
            ["Value"] = value,
            ["EventType"] = "PerformanceMetric"
        };

        if (!string.IsNullOrEmpty(unit))
            properties["Unit"] = unit;
        if (!string.IsNullOrEmpty(context))
            properties["Context"] = context;

        _logger.LogDebug("Performance Metric: {MetricName} = {Value} {Unit}", metricName, value, unit ?? "");
        TrackCustomMetric(metricName, value, properties);
    }

    public void TrackCustomEvent(string eventName, Dictionary<string, object>? properties = null)
    {
        try
        {
            var stringProperties = properties?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString() ?? "");
            _telemetryClient.TrackEvent(eventName, stringProperties);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to track custom event: {EventName}", eventName);
        }
    }

    public void TrackCustomMetric(string metricName, double value, Dictionary<string, object>? properties = null)
    {
        try
        {
            var stringProperties = properties?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString() ?? "");
            _telemetryClient.TrackMetric(metricName, value, stringProperties);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to track custom metric: {MetricName}", metricName);
        }
    }

    public void TrackDependency(string dependencyType, string target, string operation, DateTime startTime, TimeSpan duration, bool success)
    {
        try
        {
            var dependency = new DependencyTelemetry
            {
                Type = dependencyType,
                Target = target,
                Name = operation,
                Timestamp = startTime,
                Duration = duration,
                Success = success
            };

            _telemetryClient.TrackDependency(dependency);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to track dependency: {DependencyType} {Target} {Operation}", dependencyType, target, operation);
        }
    }
} 