using System;
using GrandeTech.QueueHub.API.Domain.Common;

namespace GrandeTech.QueueHub.API.Domain.Services
{
    public class ServiceTypeCreatedEvent : DomainEvent
    {
        public Guid ServiceTypeId { get; }
        public string ServiceTypeName { get; }
        public Guid ServicesProviderId { get; }

        public ServiceTypeCreatedEvent(Guid serviceTypeId, string serviceTypeName, Guid ServicesProviderId)
        {
            ServiceTypeId = serviceTypeId;
            ServiceTypeName = serviceTypeName;
            ServicesProviderId = ServicesProviderId;
        }
    }

    public class ServiceTypeUpdatedEvent : DomainEvent
    {
        public Guid ServiceTypeId { get; }

        public ServiceTypeUpdatedEvent(Guid serviceTypeId)
        {
            ServiceTypeId = serviceTypeId;
        }
    }

    public class ServiceTypeActivatedEvent : DomainEvent
    {
        public Guid ServiceTypeId { get; }

        public ServiceTypeActivatedEvent(Guid serviceTypeId)
        {
            ServiceTypeId = serviceTypeId;
        }
    }

    public class ServiceTypeDeactivatedEvent : DomainEvent
    {
        public Guid ServiceTypeId { get; }

        public ServiceTypeDeactivatedEvent(Guid serviceTypeId)
        {
            ServiceTypeId = serviceTypeId;
        }
    }

    public class ServiceTypeProvidedEvent : DomainEvent
    {
        public Guid ServiceTypeId { get; }
        public int ActualDurationMinutes { get; }
        public double NewAverageDurationMinutes { get; }

        public ServiceTypeProvidedEvent(
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
