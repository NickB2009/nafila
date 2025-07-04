using System.Collections.Generic;

namespace Grande.Fila.API.Application.Queues
{
    /// <summary>
    /// Result of saving haircut details
    /// </summary>
    public class SaveHaircutDetailsResult
    {
        public bool Success { get; set; }
        public string? ServiceHistoryId { get; set; }
        public string? CustomerId { get; set; }
        public List<string> Errors { get; set; } = new();
        public Dictionary<string, string> FieldErrors { get; set; } = new();
    }
}