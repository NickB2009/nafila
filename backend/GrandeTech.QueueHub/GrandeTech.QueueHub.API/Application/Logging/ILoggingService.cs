using Microsoft.Extensions.Logging;

namespace Grande.Fila.API.Application.Logging;

public interface ILoggingService
{
    void LogInformation(string message, params object[] args);
    void LogWarning(string message, params object[] args);
    void LogError(string message, Exception? exception = null, params object[] args);
    void LogCritical(string message, Exception? exception = null, params object[] args);
    void LogDebug(string message, params object[] args);
    void LogTrace(string message, params object[] args);
    
    // Structured logging methods
    void LogApiRequest(string method, string path, string? userId = null, string? organizationId = null, string? locationId = null, string? correlationId = null);
    void LogApiResponse(string method, string path, int statusCode, long durationMs, string? userId = null, string? correlationId = null);
    void LogDatabaseOperation(string operation, string table, long durationMs, bool success, string? error = null);
    void LogBusinessEvent(string eventName, string? userId = null, string? organizationId = null, string? locationId = null, object? additionalData = null);
    void LogSecurityEvent(string eventName, string? userId = null, string? ipAddress = null, bool success = true, string? details = null);
    void LogPerformanceMetric(string metricName, double value, string? unit = null, string? context = null);
    
    // Custom telemetry
    void TrackCustomEvent(string eventName, Dictionary<string, object>? properties = null);
    void TrackCustomMetric(string metricName, double value, Dictionary<string, object>? properties = null);
    void TrackDependency(string dependencyType, string target, string operation, DateTime startTime, TimeSpan duration, bool success);
} 