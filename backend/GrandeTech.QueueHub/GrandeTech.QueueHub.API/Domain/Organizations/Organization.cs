using System;
using System.Collections.Generic;
using Grande.Fila.API.Domain.Common;
using Grande.Fila.API.Domain.Common.ValueObjects;

namespace Grande.Fila.API.Domain.Organizations
{
    /// <summary>
    /// Represents the top-level organization that can own multiple service providers (locations)
    /// </summary>
    public class Organization : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public Slug Slug { get; private set; } = null!;
        public string? Description { get; private set; }
        public Email? ContactEmail { get; private set; }
        public PhoneNumber? ContactPhone { get; private set; }
        public string? WebsiteUrl { get; private set; }
        // Flattened branding properties (replacing BrandingConfig value object)
        public string PrimaryColor { get; private set; } = "#3B82F6";
        public string SecondaryColor { get; private set; } = "#1E40AF";
        public string LogoUrl { get; private set; } = string.Empty;
        public string FaviconUrl { get; private set; } = string.Empty;
        public string CompanyName { get; private set; } = string.Empty;
        public string TagLine { get; private set; } = string.Empty;
        public string FontFamily { get; private set; } = "Arial, sans-serif";
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
            // Set flattened branding properties
            if (brandingConfig != null)
            {
                PrimaryColor = brandingConfig.PrimaryColor;
                SecondaryColor = brandingConfig.SecondaryColor;
                LogoUrl = brandingConfig.LogoUrl;
                FaviconUrl = brandingConfig.FaviconUrl;
                CompanyName = brandingConfig.CompanyName;
                TagLine = brandingConfig.TagLine;
                FontFamily = brandingConfig.FontFamily;
            }
            else
            {
                // Use default values
                PrimaryColor = "#3B82F6";
                SecondaryColor = "#1E40AF";
                LogoUrl = string.Empty;
                FaviconUrl = string.Empty;
                CompanyName = name;
                TagLine = string.Empty;
                FontFamily = "Arial, sans-serif";
            }
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
            // Update flattened branding properties
            PrimaryColor = ValidateHexColor(primaryColor);
            SecondaryColor = ValidateHexColor(secondaryColor ?? "#FFFFFF");
            LogoUrl = logoUrl?.Trim() ?? LogoUrl;
            FaviconUrl = faviconUrl?.Trim() ?? FaviconUrl;
            CompanyName = Name.Trim();
            TagLine = tagLine?.Trim() ?? TagLine;
            // Keep existing font family

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

        /// <summary>
        /// Validates and formats hex color (moved from BrandingConfig)
        /// </summary>
        private static string ValidateHexColor(string color)
        {
            if (string.IsNullOrWhiteSpace(color))
                return "#000000";

            color = color.Trim();

            // If it doesn't start with #, add it
            if (!color.StartsWith("#"))
                color = "#" + color;

            // If it's a short form hex color (#RGB), convert to long form (#RRGGBB)
            if (color.Length == 4)
            {
                color = "#" +
                       color[1] + color[1] +
                       color[2] + color[2] +
                       color[3] + color[3];
            }

            // Check if it's a valid hex color
            if (color.Length != 7 || !System.Text.RegularExpressions.Regex.IsMatch(color, "^#[0-9A-Fa-f]{6}$"))
                throw new ArgumentException("Invalid hex color format. Must be #RRGGBB");

            return color.ToUpperInvariant();
        }
    }
}
