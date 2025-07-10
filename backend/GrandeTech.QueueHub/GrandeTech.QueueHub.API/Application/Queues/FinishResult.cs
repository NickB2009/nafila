using System;
using System.Collections.Generic;

namespace Grande.Fila.API.Application.Queues
{
    public class FinishResult
    {
        public bool Success { get; set; }
        public string? QueueEntryId { get; set; }
        public string? CustomerName { get; set; }
        public int ServiceDurationMinutes { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? Notes { get; set; }
        public Dictionary<string, string> FieldErrors { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }
} 