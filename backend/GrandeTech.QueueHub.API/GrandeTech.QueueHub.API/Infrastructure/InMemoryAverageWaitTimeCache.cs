using System;
using System.Collections.Concurrent;
using Grande.Fila.API.Application.Services.Cache;

namespace Grande.Fila.API.Infrastructure
{
    public class InMemoryAverageWaitTimeCache : IAverageWaitTimeCache
    {
        private readonly ConcurrentDictionary<Guid, double> _cache = new();

        public void SetAverage(Guid locationId, double averageMinutes)
        {
            _cache[locationId] = averageMinutes;
        }

        public bool TryGetAverage(Guid locationId, out double averageMinutes)
        {
            return _cache.TryGetValue(locationId, out averageMinutes);
        }
    }
} 