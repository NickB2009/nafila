using System;

namespace GrandeTech.QueueHub.API.Application.Organizations
{
    public class UpdateBrandingRequest
    {
        public required string OrganizationId { get; set; }
        public required string PrimaryColor { get; set; }
        public required string SecondaryColor { get; set; }
        public string? LogoUrl { get; set; }
        public string? FaviconUrl { get; set; }
        public string? TagLine { get; set; }
    }
}
