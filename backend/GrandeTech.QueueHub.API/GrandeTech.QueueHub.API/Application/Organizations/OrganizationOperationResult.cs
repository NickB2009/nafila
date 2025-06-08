using System.Collections.Generic;

namespace GrandeTech.QueueHub.API.Application.Organizations
{
    public class OrganizationOperationResult
    {
        public bool Success { get; set; }
        public required Dictionary<string, string> FieldErrors { get; set; }
        public required List<string> Errors { get; set; }
    }
}
