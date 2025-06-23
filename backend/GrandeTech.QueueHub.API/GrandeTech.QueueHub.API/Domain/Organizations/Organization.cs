using System;
using System.Collections.Generic;
using Grande.Fila.API.Domain.Common;
using Grande.Fila.API.Domain.Common.ValueObjects;

namespace Grande.Fila.API.Domain.Organizations
{
    /// <summary>
    /// Represents the top-level organization that can own multiple service providers (locations)
    /// </summary>
    public class Organization : BaseEntity, IAggregateRoot
    {
        public string Name { get; private set; } = string.Empty;
        public Slug Slug { get; private set; } = null!;
        public string? Description { get; private set; }
        public Email? ContactEmail { get; private set; }
        public PhoneNumber? ContactPhone { get; private set; }
        public string? WebsiteUrl { get; private set; }
        public BrandingConfig BrandingConfig { get; private set; } = null!;
        public Guid SubscriptionPlanId { get; private set; }
        public bool IsActive { get; private set; }
        public bool SharesDataForAnalytics { get; private set; }

        // Navigation properties
        private readonly List<Guid> _LocationIds = new();
        public IReadOnlyCollection<Guid> LocationIds => _LocationIds.AsReadOnly();
        
        // For EF Core
        private Organization() { }

        public Organization(
            string name,
            string slug,
            string? description,
            string? contactEmail,
            string? contactPhone,
            string? websiteUrl,
            BrandingConfig? brandingConfig,
            Guid subscriptionPlanId,
            string createdBy)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Organization name is required", nameof(name));

            Name = name;
            Slug = Slug.Create(slug ?? name);
            Description = description;
            ContactEmail = contactEmail != null ? Email.Create(contactEmail) : null;
            ContactPhone = contactPhone != null ? PhoneNumber.Create(contactPhone) : null;
            WebsiteUrl = websiteUrl;
            BrandingConfig = brandingConfig ?? BrandingConfig.Default;
            SubscriptionPlanId = subscriptionPlanId;
            IsActive = true;
            SharesDataForAnalytics = false;
            CreatedBy = createdBy;

            AddDomainEvent(new OrganizationCreatedEvent(Id, Name, Slug.Value));
        }

        // Domain behavior methods
        public void UpdateDetails(
            string name,
            string? description,
            string? contactEmail,
            string? contactPhone,
            string? websiteUrl,
            string updatedBy)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Organization name is required", nameof(name));

            Name = name;
            Description = description;
            ContactEmail = contactEmail != null ? Email.Create(contactEmail) : null;
            ContactPhone = contactPhone != null ? PhoneNumber.Create(contactPhone) : null;
            WebsiteUrl = websiteUrl;

            MarkAsModified(updatedBy);
            AddDomainEvent(new OrganizationUpdatedEvent(Id));
        }

        public void UpdateBranding(
            string primaryColor,
            string secondaryColor,
            string? logoUrl,
            string? faviconUrl,
            string? tagLine,
            string updatedBy)
        {
            BrandingConfig = BrandingConfig.Create(
                primaryColor,
                secondaryColor,
                logoUrl ?? BrandingConfig.LogoUrl,
                faviconUrl ?? BrandingConfig.FaviconUrl,
                Name,
                tagLine ?? BrandingConfig.TagLine
            );

            MarkAsModified(updatedBy);
            AddDomainEvent(new OrganizationBrandingUpdatedEvent(Id));
        }

        public void ChangeSubscriptionPlan(Guid newSubscriptionPlanId, string updatedBy)
        {
            if (newSubscriptionPlanId == Guid.Empty)
                throw new ArgumentException("Subscription plan ID cannot be empty", nameof(newSubscriptionPlanId));

            SubscriptionPlanId = newSubscriptionPlanId;
            MarkAsModified(updatedBy);
            AddDomainEvent(new OrganizationSubscriptionChangedEvent(Id, newSubscriptionPlanId));
        }

        public void SetAnalyticsSharing(bool sharesData, string updatedBy)
        {
            if (SharesDataForAnalytics != sharesData)
            {
                SharesDataForAnalytics = sharesData;
                MarkAsModified(updatedBy);
                AddDomainEvent(new OrganizationAnalyticsSharingChangedEvent(Id, sharesData));
            }
        }

        public void AddLocation(Guid LocationId)
        {
            if (!_LocationIds.Contains(LocationId))
            {
                _LocationIds.Add(LocationId);
                AddDomainEvent(new LocationAddedToOrganizationEvent(Id, LocationId));
            }
        }

        public void RemoveLocation(Guid LocationId)
        {
            if (_LocationIds.Contains(LocationId))
            {
                _LocationIds.Remove(LocationId);
                AddDomainEvent(new LocationRemovedFromOrganizationEvent(Id, LocationId));
            }
        }

        public void Activate(string updatedBy)
        {
            if (!IsActive)
            {
                IsActive = true;
                MarkAsModified(updatedBy);
                AddDomainEvent(new OrganizationActivatedEvent(Id));
            }
        }

        public void Deactivate(string updatedBy)
        {
            if (IsActive)
            {
                IsActive = false;
                MarkAsModified(updatedBy);
                AddDomainEvent(new OrganizationDeactivatedEvent(Id));
            }
        }
    }
}
