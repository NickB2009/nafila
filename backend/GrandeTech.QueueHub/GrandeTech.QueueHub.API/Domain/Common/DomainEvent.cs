using System;

namespace Grande.Fila.API.Domain.Common
{
    /// <summary>
    /// Base class for all domain events
    /// </summary>
    public abstract class DomainEvent : IDomainEvent
    {
        public Guid Id { get; }
        public DateTime OccurredOn { get; }

        protected DomainEvent()
        {
            Id = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
        }
    }
}
