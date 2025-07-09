using System.Collections.Generic;

namespace Grande.Fila.API.Application.Organizations
{
    public class OrganizationOperationResult
    {
        public bool Success { get; set; }
        public required Dictionary<string, string> FieldErrors { get; set; }
        public required List<string> Errors { get; set; }
    }
}
