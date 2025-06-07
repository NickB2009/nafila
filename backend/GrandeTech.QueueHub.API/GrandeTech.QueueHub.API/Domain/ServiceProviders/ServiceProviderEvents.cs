using System;
using GrandeTech.QueueHub.API.Domain.Common;

namespace GrandeTech.QueueHub.API.Domain.ServicesProviders
{
    public class ServicesProviderCreatedEvent : DomainEvent
    {
        public Guid ServicesProviderId { get; }
        public string ServicesProviderName { get; }
        public Guid OrganizationId { get; }

        public ServicesProviderCreatedEvent(Guid ServicesProviderId, string ServicesProviderName, Guid organizationId)
        {
            ServicesProviderId = ServicesProviderId;
            ServicesProviderName = ServicesProviderName;
            OrganizationId = organizationId;
        }
    }

    public class ServicesProviderUpdatedEvent : DomainEvent
    {
        public Guid ServicesProviderId { get; }

        public ServicesProviderUpdatedEvent(Guid ServicesProviderId)
        {
            ServicesProviderId = ServicesProviderId;
        }
    }

    public class ServicesProviderBrandingUpdatedEvent : DomainEvent
    {
        public Guid ServicesProviderId { get; }

        public ServicesProviderBrandingUpdatedEvent(Guid ServicesProviderId)
        {
            ServicesProviderId = ServicesProviderId;
        }
    }

    public class ServicesProviderQueueEnabledEvent : DomainEvent
    {
        public Guid ServicesProviderId { get; }

        public ServicesProviderQueueEnabledEvent(Guid ServicesProviderId)
        {
            ServicesProviderId = ServicesProviderId;
        }
    }

    public class ServicesProviderQueueDisabledEvent : DomainEvent
    {
        public Guid ServicesProviderId { get; }

        public ServicesProviderQueueDisabledEvent(Guid ServicesProviderId)
        {
            ServicesProviderId = ServicesProviderId;
        }
    }

    public class ServicesProviderQueueSettingsUpdatedEvent : DomainEvent
    {
        public Guid ServicesProviderId { get; }

        public ServicesProviderQueueSettingsUpdatedEvent(Guid ServicesProviderId)
        {
            ServicesProviderId = ServicesProviderId;
        }
    }

    public class ServicesProviderActivatedEvent : DomainEvent
    {
        public Guid ServicesProviderId { get; }

        public ServicesProviderActivatedEvent(Guid ServicesProviderId)
        {
            ServicesProviderId = ServicesProviderId;
        }
    }

    public class ServicesProviderDeactivatedEvent : DomainEvent
    {
        public Guid ServicesProviderId { get; }

        public ServicesProviderDeactivatedEvent(Guid ServicesProviderId)
        {
            ServicesProviderId = ServicesProviderId;
        }
    }

    public class StaffMemberAddedToServicesProviderEvent : DomainEvent
    {
        public Guid ServicesProviderId { get; }
        public Guid StaffMemberId { get; }

        public StaffMemberAddedToServicesProviderEvent(Guid ServicesProviderId, Guid staffMemberId)
        {
            ServicesProviderId = ServicesProviderId;
            StaffMemberId = staffMemberId;
        }
    }

    public class StaffMemberRemovedFromServicesProviderEvent : DomainEvent
    {
        public Guid ServicesProviderId { get; }
        public Guid StaffMemberId { get; }

        public StaffMemberRemovedFromServicesProviderEvent(Guid ServicesProviderId, Guid staffMemberId)
        {
            ServicesProviderId = ServicesProviderId;
            StaffMemberId = staffMemberId;
        }
    }

    public class ServiceTypeAddedToServicesProviderEvent : DomainEvent
    {
        public Guid ServicesProviderId { get; }
        public Guid ServiceTypeId { get; }

        public ServiceTypeAddedToServicesProviderEvent(Guid ServicesProviderId, Guid serviceTypeId)
        {
            ServicesProviderId = ServicesProviderId;
            ServiceTypeId = serviceTypeId;
        }
    }

    public class ServiceTypeRemovedFromServicesProviderEvent : DomainEvent
    {
        public Guid ServicesProviderId { get; }
        public Guid ServiceTypeId { get; }

        public ServiceTypeRemovedFromServicesProviderEvent(Guid ServicesProviderId, Guid serviceTypeId)
        {
            ServicesProviderId = ServicesProviderId;
            ServiceTypeId = serviceTypeId;
        }
    }

    public class AdvertisementAddedToServicesProviderEvent : DomainEvent
    {
        public Guid ServicesProviderId { get; }
        public Guid AdvertisementId { get; }

        public AdvertisementAddedToServicesProviderEvent(Guid ServicesProviderId, Guid advertisementId)
        {
            ServicesProviderId = ServicesProviderId;
            AdvertisementId = advertisementId;
        }
    }

    public class AdvertisementRemovedFromServicesProviderEvent : DomainEvent
    {
        public Guid ServicesProviderId { get; }
        public Guid AdvertisementId { get; }

        public AdvertisementRemovedFromServicesProviderEvent(Guid ServicesProviderId, Guid advertisementId)
        {
            ServicesProviderId = ServicesProviderId;
            AdvertisementId = advertisementId;
        }
    }

    public class ServicesProviderAverageTimeUpdatedEvent : DomainEvent
    {
        public Guid ServicesProviderId { get; }
        public double NewAverageTimeInMinutes { get; }

        public ServicesProviderAverageTimeUpdatedEvent(Guid ServicesProviderId, double newAverageTimeInMinutes)
        {
            ServicesProviderId = ServicesProviderId;
            NewAverageTimeInMinutes = newAverageTimeInMinutes;
        }
    }

    public class ServicesProviderAverageTimeResetEvent : DomainEvent
    {
        public Guid ServicesProviderId { get; }

        public ServicesProviderAverageTimeResetEvent(Guid ServicesProviderId)
        {
            ServicesProviderId = ServicesProviderId;
        }
    }
}
