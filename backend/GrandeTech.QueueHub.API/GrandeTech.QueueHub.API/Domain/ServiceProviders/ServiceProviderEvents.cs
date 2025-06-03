using System;
using GrandeTech.QueueHub.API.Domain.Common;

namespace GrandeTech.QueueHub.API.Domain.ServiceProviders
{
    public class ServiceProviderCreatedEvent : DomainEvent
    {
        public Guid ServiceProviderId { get; }
        public string ServiceProviderName { get; }
        public Guid OrganizationId { get; }

        public ServiceProviderCreatedEvent(Guid serviceProviderId, string serviceProviderName, Guid organizationId)
        {
            ServiceProviderId = serviceProviderId;
            ServiceProviderName = serviceProviderName;
            OrganizationId = organizationId;
        }
    }

    public class ServiceProviderUpdatedEvent : DomainEvent
    {
        public Guid ServiceProviderId { get; }

        public ServiceProviderUpdatedEvent(Guid serviceProviderId)
        {
            ServiceProviderId = serviceProviderId;
        }
    }

    public class ServiceProviderBrandingUpdatedEvent : DomainEvent
    {
        public Guid ServiceProviderId { get; }

        public ServiceProviderBrandingUpdatedEvent(Guid serviceProviderId)
        {
            ServiceProviderId = serviceProviderId;
        }
    }

    public class ServiceProviderQueueEnabledEvent : DomainEvent
    {
        public Guid ServiceProviderId { get; }

        public ServiceProviderQueueEnabledEvent(Guid serviceProviderId)
        {
            ServiceProviderId = serviceProviderId;
        }
    }

    public class ServiceProviderQueueDisabledEvent : DomainEvent
    {
        public Guid ServiceProviderId { get; }

        public ServiceProviderQueueDisabledEvent(Guid serviceProviderId)
        {
            ServiceProviderId = serviceProviderId;
        }
    }

    public class ServiceProviderQueueSettingsUpdatedEvent : DomainEvent
    {
        public Guid ServiceProviderId { get; }

        public ServiceProviderQueueSettingsUpdatedEvent(Guid serviceProviderId)
        {
            ServiceProviderId = serviceProviderId;
        }
    }

    public class ServiceProviderActivatedEvent : DomainEvent
    {
        public Guid ServiceProviderId { get; }

        public ServiceProviderActivatedEvent(Guid serviceProviderId)
        {
            ServiceProviderId = serviceProviderId;
        }
    }

    public class ServiceProviderDeactivatedEvent : DomainEvent
    {
        public Guid ServiceProviderId { get; }

        public ServiceProviderDeactivatedEvent(Guid serviceProviderId)
        {
            ServiceProviderId = serviceProviderId;
        }
    }

    public class StaffMemberAddedToServiceProviderEvent : DomainEvent
    {
        public Guid ServiceProviderId { get; }
        public Guid StaffMemberId { get; }

        public StaffMemberAddedToServiceProviderEvent(Guid serviceProviderId, Guid staffMemberId)
        {
            ServiceProviderId = serviceProviderId;
            StaffMemberId = staffMemberId;
        }
    }

    public class StaffMemberRemovedFromServiceProviderEvent : DomainEvent
    {
        public Guid ServiceProviderId { get; }
        public Guid StaffMemberId { get; }

        public StaffMemberRemovedFromServiceProviderEvent(Guid serviceProviderId, Guid staffMemberId)
        {
            ServiceProviderId = serviceProviderId;
            StaffMemberId = staffMemberId;
        }
    }

    public class ServiceTypeAddedToServiceProviderEvent : DomainEvent
    {
        public Guid ServiceProviderId { get; }
        public Guid ServiceTypeId { get; }

        public ServiceTypeAddedToServiceProviderEvent(Guid serviceProviderId, Guid serviceTypeId)
        {
            ServiceProviderId = serviceProviderId;
            ServiceTypeId = serviceTypeId;
        }
    }

    public class ServiceTypeRemovedFromServiceProviderEvent : DomainEvent
    {
        public Guid ServiceProviderId { get; }
        public Guid ServiceTypeId { get; }

        public ServiceTypeRemovedFromServiceProviderEvent(Guid serviceProviderId, Guid serviceTypeId)
        {
            ServiceProviderId = serviceProviderId;
            ServiceTypeId = serviceTypeId;
        }
    }

    public class AdvertisementAddedToServiceProviderEvent : DomainEvent
    {
        public Guid ServiceProviderId { get; }
        public Guid AdvertisementId { get; }

        public AdvertisementAddedToServiceProviderEvent(Guid serviceProviderId, Guid advertisementId)
        {
            ServiceProviderId = serviceProviderId;
            AdvertisementId = advertisementId;
        }
    }

    public class AdvertisementRemovedFromServiceProviderEvent : DomainEvent
    {
        public Guid ServiceProviderId { get; }
        public Guid AdvertisementId { get; }

        public AdvertisementRemovedFromServiceProviderEvent(Guid serviceProviderId, Guid advertisementId)
        {
            ServiceProviderId = serviceProviderId;
            AdvertisementId = advertisementId;
        }
    }

    public class ServiceProviderAverageTimeUpdatedEvent : DomainEvent
    {
        public Guid ServiceProviderId { get; }
        public double NewAverageTimeInMinutes { get; }

        public ServiceProviderAverageTimeUpdatedEvent(Guid serviceProviderId, double newAverageTimeInMinutes)
        {
            ServiceProviderId = serviceProviderId;
            NewAverageTimeInMinutes = newAverageTimeInMinutes;
        }
    }

    public class ServiceProviderAverageTimeResetEvent : DomainEvent
    {
        public Guid ServiceProviderId { get; }

        public ServiceProviderAverageTimeResetEvent(Guid serviceProviderId)
        {
            ServiceProviderId = serviceProviderId;
        }
    }
}
