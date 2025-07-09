using System;

namespace Grande.Fila.API.Application.Organizations
{
    public class UpdateOrganizationRequest
    {
        public required string OrganizationId { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? WebsiteUrl { get; set; }
    }
}
