using System;

namespace GrandeTech.QueueHub.API.Application.Organizations
{
    public class CreateOrganizationRequest
    {
        public required string Name { get; set; }
        public string? Slug { get; set; }
        public string? Description { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? PrimaryColor { get; set; }
        public string? SecondaryColor { get; set; }
        public string? LogoUrl { get; set; }
        public string? FaviconUrl { get; set; }
        public string? TagLine { get; set; }
        public required string SubscriptionPlanId { get; set; }
        public bool SharesDataForAnalytics { get; set; } = false;
    }
}
