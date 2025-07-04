using System.Collections.Generic;

namespace Grande.Fila.API.Application.Locations.Results
{
    /// <summary>
    /// Result of toggling queue status for a location
    /// </summary>
    public class ToggleQueueResult
    {
        public bool Success { get; set; }
        public string? LocationId { get; set; }
        public bool IsQueueEnabled { get; set; }
        public List<string> Errors { get; set; } = new();
        public Dictionary<string, string> FieldErrors { get; set; } = new();
    }
}