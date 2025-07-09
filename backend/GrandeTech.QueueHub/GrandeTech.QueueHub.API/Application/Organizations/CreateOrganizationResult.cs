using System.Collections.Generic;

namespace Grande.Fila.API.Application.Organizations
{
    public class CreateOrganizationResult
    {
        public bool Success { get; set; }
        public required string OrganizationId { get; set; }
        public required string Status { get; set; }
        public required string Slug { get; set; }
        public required Dictionary<string, string> FieldErrors { get; set; }
        public required List<string> Errors { get; set; }
    }
}
