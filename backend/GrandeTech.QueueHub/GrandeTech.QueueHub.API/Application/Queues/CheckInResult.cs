using System;
using System.Collections.Generic;

namespace Grande.Fila.API.Application.Queues
{
    public class CheckInResult
    {
        public bool Success { get; set; }
        public string? QueueEntryId { get; set; }
        public string? CustomerName { get; set; }
        public int Position { get; set; }
        public DateTime? CheckedInAt { get; set; }
        public Dictionary<string, string> FieldErrors { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }
} 