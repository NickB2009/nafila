using System;
using System.Collections.Generic;

namespace Grande.Fila.API.Application.Queues
{
    public class CallNextResult
    {
        public bool Success { get; set; }
        public string? QueueEntryId { get; set; }
        public string? CustomerName { get; set; }
        public int Position { get; set; }
        public string? StaffMemberId { get; set; }
        public DateTime? CalledAt { get; set; }
        public Dictionary<string, string> FieldErrors { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }
} 