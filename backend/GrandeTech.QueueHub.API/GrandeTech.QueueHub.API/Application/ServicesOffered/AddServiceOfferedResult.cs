using System.Collections.Generic;

namespace GrandeTech.QueueHub.API.Application.ServicesOffered
{
    public class AddServiceOfferedResult
    {
        public bool Success { get; set; }
        public string? ServiceTypeId { get; set; }
        public Dictionary<string, string> FieldErrors { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }
}
