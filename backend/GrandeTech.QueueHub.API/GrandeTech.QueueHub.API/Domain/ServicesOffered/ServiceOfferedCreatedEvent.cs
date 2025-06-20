using System;
using Grande.Fila.API.Domain.Common;

namespace Grande.Fila.API.Domain.ServicesOffered
{    public class ServiceOfferedCreatedEvent : DomainEvent
    {
        public Guid ServiceTypeId { get; }
        public string ServiceTypeName { get; }
        public Guid LocationId { get; }

        public ServiceOfferedCreatedEvent(Guid serviceTypeId, string serviceTypeName, Guid locationId)
        {
            ServiceTypeId = serviceTypeId;
            ServiceTypeName = serviceTypeName;
            LocationId = locationId;
        }
    }

    public class ServiceOfferedUpdatedEvent : DomainEvent
    {
        public Guid ServiceTypeId { get; }

        public ServiceOfferedUpdatedEvent(Guid serviceTypeId)
        {
            ServiceTypeId = serviceTypeId;
        }
    }

    public class ServiceOfferedActivatedEvent : DomainEvent
    {
        public Guid ServiceTypeId { get; }

        public ServiceOfferedActivatedEvent(Guid serviceTypeId)
        {
            ServiceTypeId = serviceTypeId;
        }
    }

    public class ServiceOfferedDeactivatedEvent : DomainEvent
    {
        public Guid ServiceTypeId { get; }

        public ServiceOfferedDeactivatedEvent(Guid serviceTypeId)
        {
            ServiceTypeId = serviceTypeId;
        }
    }

    public class ServiceOfferedProvidedEvent : DomainEvent
    {
        public Guid ServiceTypeId { get; }
        public int ActualDurationMinutes { get; }
        public double NewAverageDurationMinutes { get; }

        public ServiceOfferedProvidedEvent(
            Guid serviceTypeId,
            int actualDurationMinutes,
            double newAverageDurationMinutes)
        {
            ServiceTypeId = serviceTypeId;
            ActualDurationMinutes = actualDurationMinutes;
            NewAverageDurationMinutes = newAverageDurationMinutes;
        }
    }
}
