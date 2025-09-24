using System;
using System.Collections.Generic;

namespace Grande.Fila.API.Infrastructure.Monitoring
{
    /// <summary>
    /// Provider-agnostic monitoring interface for different hosting environments
    /// </summary>
    public interface IMonitoringProvider
    {
        /// <summary>
        /// Track a custom event
        /// </summary>
        void TrackEvent(string eventName, Dictionary<string, string> properties = null);

        /// <summary>
        /// Track a custom metric
        /// </summary>
        void TrackMetric(string metricName, double value);

        /// <summary>
        /// Track an exception
        /// </summary>
        void TrackException(Exception exception);

        /// <summary>
        /// Track a dependency call
        /// </summary>
        void TrackDependency(string dependencyName, string commandName, DateTime startTime, TimeSpan duration, bool success);
    }
}


