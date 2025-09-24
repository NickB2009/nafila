using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;

namespace Grande.Fila.API.Infrastructure.Monitoring
{
    /// <summary>
    /// Azure Application Insights monitoring provider
    /// </summary>
    public class AzureMonitoringProvider : IMonitoringProvider
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger<AzureMonitoringProvider> _logger;

        public AzureMonitoringProvider(TelemetryClient telemetryClient, ILogger<AzureMonitoringProvider> logger)
        {
            _telemetryClient = telemetryClient;
            _logger = logger;
        }

        public void TrackEvent(string eventName, Dictionary<string, string> properties = null)
        {
            try
            {
                _telemetryClient.TrackEvent(eventName, properties);
                _logger.LogDebug("Event tracked in Azure: {EventName}", eventName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to track event in Azure: {EventName}", eventName);
            }
        }

        public void TrackMetric(string metricName, double value)
        {
            try
            {
                _telemetryClient.TrackMetric(metricName, value);
                _logger.LogDebug("Metric tracked in Azure: {MetricName} = {Value}", metricName, value);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to track metric in Azure: {MetricName}", metricName);
            }
        }

        public void TrackException(Exception exception)
        {
            try
            {
                _telemetryClient.TrackException(exception);
                _logger.LogDebug("Exception tracked in Azure: {ExceptionType}", exception.GetType().Name);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to track exception in Azure");
            }
        }

        public void TrackDependency(string dependencyName, string commandName, DateTime startTime, TimeSpan duration, bool success)
        {
            try
            {
                _telemetryClient.TrackDependency(dependencyName, commandName, startTime, duration, success);
                _logger.LogDebug("Dependency tracked in Azure: {DependencyName}.{CommandName}", dependencyName, commandName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to track dependency in Azure: {DependencyName}.{CommandName}", dependencyName, commandName);
            }
        }
    }
}


