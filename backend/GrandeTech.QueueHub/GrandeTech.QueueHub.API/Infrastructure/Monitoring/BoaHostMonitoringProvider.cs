using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Grande.Fila.API.Infrastructure.Monitoring
{
    /// <summary>
    /// BoaHost native monitoring provider using file-based logging
    /// </summary>
    public class BoaHostMonitoringProvider : IMonitoringProvider
    {
        private readonly ILogger<BoaHostMonitoringProvider> _logger;

        public BoaHostMonitoringProvider(ILogger<BoaHostMonitoringProvider> logger)
        {
            _logger = logger;
        }

        public void TrackEvent(string eventName, Dictionary<string, string> properties = null)
        {
            var propertiesStr = properties != null ? string.Join(", ", properties) : "none";
            _logger.LogInformation("Event: {EventName}, Properties: {Properties}", eventName, propertiesStr);
        }

        public void TrackMetric(string metricName, double value)
        {
            _logger.LogInformation("Metric: {MetricName} = {Value}", metricName, value);
        }

        public void TrackException(Exception exception)
        {
            _logger.LogError(exception, "Exception tracked: {ExceptionType}", exception.GetType().Name);
        }

        public void TrackDependency(string dependencyName, string commandName, DateTime startTime, TimeSpan duration, bool success)
        {
            var status = success ? "Success" : "Failed";
            _logger.LogInformation("Dependency: {DependencyName}.{CommandName} - {Status} - {Duration}ms", 
                dependencyName, commandName, status, duration.TotalMilliseconds);
        }
    }
}

