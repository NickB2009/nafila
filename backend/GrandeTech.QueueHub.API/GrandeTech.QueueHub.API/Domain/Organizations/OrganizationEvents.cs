using System;
using GrandeTech.QueueHub.API.Domain.Common;

namespace GrandeTech.QueueHub.API.Domain.Organizations
{
    public class OrganizationCreatedEvent : DomainEvent
    {
        public Guid OrganizationId { get; }
        public string OrganizationName { get; }
        public string OrganizationSlug { get; }

        public OrganizationCreatedEvent(Guid organizationId, string organizationName, string organizationSlug)
        {
            OrganizationId = organizationId;
            OrganizationName = organizationName;
            OrganizationSlug = organizationSlug;
        }
    }

    public class OrganizationUpdatedEvent : DomainEvent
    {
        public Guid OrganizationId { get; }

        public OrganizationUpdatedEvent(Guid organizationId)
        {
            OrganizationId = organizationId;
        }
    }

    public class OrganizationBrandingUpdatedEvent : DomainEvent
    {
        public Guid OrganizationId { get; }

        public OrganizationBrandingUpdatedEvent(Guid organizationId)
        {
            OrganizationId = organizationId;
        }
    }

    public class OrganizationSubscriptionChangedEvent : DomainEvent
    {
        public Guid OrganizationId { get; }
        public Guid NewSubscriptionPlanId { get; }

        public OrganizationSubscriptionChangedEvent(Guid organizationId, Guid newSubscriptionPlanId)
        {
            OrganizationId = organizationId;
            NewSubscriptionPlanId = newSubscriptionPlanId;
        }
    }

    public class OrganizationAnalyticsSharingChangedEvent : DomainEvent
    {
        public Guid OrganizationId { get; }
        public bool SharesData { get; }

        public OrganizationAnalyticsSharingChangedEvent(Guid organizationId, bool sharesData)
        {
            OrganizationId = organizationId;
            SharesData = sharesData;
        }
    }

    public class OrganizationActivatedEvent : DomainEvent
    {
        public Guid OrganizationId { get; }

        public OrganizationActivatedEvent(Guid organizationId)
        {
            OrganizationId = organizationId;
        }
    }

    public class OrganizationDeactivatedEvent : DomainEvent
    {
        public Guid OrganizationId { get; }

        public OrganizationDeactivatedEvent(Guid organizationId)
        {
            OrganizationId = organizationId;
        }
    }

    public class ServiceProviderAddedToOrganizationEvent : DomainEvent
    {
        public Guid OrganizationId { get; }
        public Guid ServiceProviderId { get; }

        public ServiceProviderAddedToOrganizationEvent(Guid organizationId, Guid serviceProviderId)
        {
            OrganizationId = organizationId;
            ServiceProviderId = serviceProviderId;
        }
    }

    public class ServiceProviderRemovedFromOrganizationEvent : DomainEvent
    {
        public Guid OrganizationId { get; }
        public Guid ServiceProviderId { get; }

        public ServiceProviderRemovedFromOrganizationEvent(Guid organizationId, Guid serviceProviderId)
        {
            OrganizationId = organizationId;
            ServiceProviderId = serviceProviderId;
        }
    }
}
