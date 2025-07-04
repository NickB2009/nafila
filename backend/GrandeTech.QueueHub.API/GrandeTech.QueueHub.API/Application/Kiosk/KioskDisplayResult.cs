using System.Collections.Generic;

namespace Grande.Fila.API.Application.Kiosk
{
    public class KioskDisplayResult
    {
        public bool Success { get; set; }
        public List<KioskQueueEntryDto> QueueEntries { get; set; } = new();
        public string? CurrentlyServing { get; set; }
        public int TotalWaiting { get; set; }
        public Dictionary<string, string> FieldErrors { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }

    public class KioskQueueEntryDto
    {
        public string Id { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public int Position { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? TokenNumber { get; set; }
        public string? EstimatedWaitTime { get; set; }
    }
} 