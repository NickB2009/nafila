using System;
using Grande.Fila.API.Domain.Common;

namespace Grande.Fila.API.Domain.Advertising
{
    public class AdvertisementCreatedEvent : DomainEvent
    {
        public Guid AdvertisementId { get; }
        public string Title { get; }
        public Guid LocationId { get; }

        public AdvertisementCreatedEvent(Guid advertisementId, string title, Guid locationId)
        {
            AdvertisementId = advertisementId;
            Title = title;
            LocationId = locationId;
        }
    }

    public class AdvertisementUpdatedEvent : DomainEvent
    {
        public Guid AdvertisementId { get; }

        public AdvertisementUpdatedEvent(Guid advertisementId)
        {
            AdvertisementId = advertisementId;
        }
    }

    public class AdvertisementActivatedEvent : DomainEvent
    {
        public Guid AdvertisementId { get; }

        public AdvertisementActivatedEvent(Guid advertisementId)
        {
            AdvertisementId = advertisementId;
        }
    }

    public class AdvertisementDeactivatedEvent : DomainEvent
    {
        public Guid AdvertisementId { get; }

        public AdvertisementDeactivatedEvent(Guid advertisementId)
        {
            AdvertisementId = advertisementId;
        }
    }

    public class AdvertisementVisibilityChangedEvent : DomainEvent
    {
        public Guid AdvertisementId { get; }
        public bool IsGlobal { get; }

        public AdvertisementVisibilityChangedEvent(Guid advertisementId, bool isGlobal)
        {
            AdvertisementId = advertisementId;
            IsGlobal = isGlobal;
        }
    }
}
