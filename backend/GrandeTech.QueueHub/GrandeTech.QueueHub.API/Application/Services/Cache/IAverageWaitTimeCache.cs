using System;

namespace Grande.Fila.API.Application.Services.Cache
{
    public interface IAverageWaitTimeCache
    {
        void SetAverage(Guid locationId, double averageMinutes);
        bool TryGetAverage(Guid locationId, out double averageMinutes);
    }
} 