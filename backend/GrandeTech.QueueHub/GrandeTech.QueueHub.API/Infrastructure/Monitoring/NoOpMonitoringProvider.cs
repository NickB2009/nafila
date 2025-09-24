using System;
using System.Collections.Generic;

namespace Grande.Fila.API.Infrastructure.Monitoring
{
    /// <summary>
    /// No-operation monitoring provider for development/testing
    /// </summary>
    public class NoOpMonitoringProvider : IMonitoringProvider
    {
        public void TrackEvent(string eventName, Dictionary<string, string> properties = null)
        {
            // No-op for development
        }

        public void TrackMetric(string metricName, double value)
        {
            // No-op for development
        }

        public void TrackException(Exception exception)
        {
            // No-op for development
        }

        public void TrackDependency(string dependencyName, string commandName, DateTime startTime, TimeSpan duration, bool success)
        {
            // No-op for development
        }
    }
}


