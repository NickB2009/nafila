using System;
using System.Collections.Generic;

namespace GrandeTech.QueueHub.API.Application.Queues
{
    public class JoinQueueResult
    {
        public bool Success { get; set; }
        public string? QueueEntryId { get; set; }
        public int Position { get; set; }
        public int EstimatedWaitTimeMinutes { get; set; }
        public string? TokenNumber { get; set; }
        public Dictionary<string, string> FieldErrors { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }
}
