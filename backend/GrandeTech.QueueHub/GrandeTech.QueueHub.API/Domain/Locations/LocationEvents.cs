using System;
using Grande.Fila.API.Domain.Common;

namespace Grande.Fila.API.Domain.Locations
{
    public class LocationCreatedEvent : DomainEvent
    {
        public Guid LocationId { get; }
        public string LocationName { get; }
        public Guid OrganizationId { get; }

        public LocationCreatedEvent(Guid locationId, string locationName, Guid organizationId)
        {
            LocationId = locationId;
            LocationName = locationName;
            OrganizationId = organizationId;
        }
    }

    public class LocationUpdatedEvent : DomainEvent
    {
        public Guid LocationId { get; }

        public LocationUpdatedEvent(Guid locationId)
        {
            LocationId = locationId;
        }
    }

    public class LocationBrandingUpdatedEvent : DomainEvent
    {
        public Guid LocationId { get; }

        public LocationBrandingUpdatedEvent(Guid locationId)
        {
            LocationId = locationId;
        }
    }

    public class LocationQueueEnabledEvent : DomainEvent
    {
        public Guid LocationId { get; }

        public LocationQueueEnabledEvent(Guid locationId)
        {
            LocationId = locationId;
        }
    }

    public class LocationQueueDisabledEvent : DomainEvent
    {
        public Guid LocationId { get; }

        public LocationQueueDisabledEvent(Guid locationId)
        {
            LocationId = locationId;
        }
    }

    public class LocationQueueSettingsUpdatedEvent : DomainEvent
    {
        public Guid LocationId { get; }

        public LocationQueueSettingsUpdatedEvent(Guid locationId)
        {
            LocationId = locationId;
        }
    }

    public class LocationActivatedEvent : DomainEvent
    {
        public Guid LocationId { get; }

        public LocationActivatedEvent(Guid locationId)
        {
            LocationId = locationId;
        }
    }

    public class LocationDeactivatedEvent : DomainEvent
    {
        public Guid LocationId { get; }

        public LocationDeactivatedEvent(Guid locationId)
        {
            LocationId = locationId;
        }
    }

    public class StaffMemberAddedToLocationEvent : DomainEvent
    {
        public Guid LocationId { get; }
        public Guid StaffMemberId { get; }

        public StaffMemberAddedToLocationEvent(Guid locationId, Guid staffMemberId)
        {
            LocationId = locationId;
            StaffMemberId = staffMemberId;
        }
    }

    public class StaffMemberRemovedFromLocationEvent : DomainEvent
    {
        public Guid LocationId { get; }
        public Guid StaffMemberId { get; }

        public StaffMemberRemovedFromLocationEvent(Guid locationId, Guid staffMemberId)
        {
            LocationId = locationId;
            StaffMemberId = staffMemberId;
        }
    }

    public class ServiceTypeAddedToLocationEvent : DomainEvent
    {
        public Guid LocationId { get; }
        public Guid ServiceTypeId { get; }

        public ServiceTypeAddedToLocationEvent(Guid locationId, Guid serviceTypeId)
        {
            LocationId = locationId;
            ServiceTypeId = serviceTypeId;
        }
    }

    public class ServiceTypeRemovedFromLocationEvent : DomainEvent
    {
        public Guid LocationId { get; }
        public Guid ServiceTypeId { get; }

        public ServiceTypeRemovedFromLocationEvent(Guid locationId, Guid serviceTypeId)
        {
            LocationId = locationId;
            ServiceTypeId = serviceTypeId;
        }
    }

    public class AdvertisementAddedToLocationEvent : DomainEvent
    {
        public Guid LocationId { get; }
        public Guid AdvertisementId { get; }

        public AdvertisementAddedToLocationEvent(Guid locationId, Guid advertisementId)
        {
            LocationId = locationId;
            AdvertisementId = advertisementId;
        }
    }

    public class AdvertisementRemovedFromLocationEvent : DomainEvent
    {
        public Guid LocationId { get; }
        public Guid AdvertisementId { get; }

        public AdvertisementRemovedFromLocationEvent(Guid locationId, Guid advertisementId)
        {
            LocationId = locationId;
            AdvertisementId = advertisementId;
        }
    }

    public class LocationAverageTimeUpdatedEvent : DomainEvent
    {
        public Guid LocationId { get; }
        public double NewAverageTimeInMinutes { get; }

        public LocationAverageTimeUpdatedEvent(Guid locationId, double newAverageTimeInMinutes)
        {
            LocationId = locationId;
            NewAverageTimeInMinutes = newAverageTimeInMinutes;
        }
    }

    public class LocationAverageTimeResetEvent : DomainEvent
    {
        public Guid LocationId { get; }

        public LocationAverageTimeResetEvent(Guid locationId)
        {
            LocationId = locationId;
        }
    }
}
