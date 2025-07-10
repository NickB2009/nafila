using System;

namespace Grande.Fila.API.Domain.Common
{
    /// <summary>
    /// Base interface for domain events
    /// </summary>
    public interface IDomainEvent
    {
        Guid Id { get; }
        DateTime OccurredOn { get; }
    }
}
