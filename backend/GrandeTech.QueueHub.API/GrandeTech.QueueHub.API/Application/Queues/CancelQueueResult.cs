using System;
using System.Collections.Generic;

namespace GrandeTech.QueueHub.API.Application.Queues
{
    public class CancelQueueResult
    {
        public bool Success { get; set; }
        public string? QueueEntryId { get; set; }
        public string? CustomerName { get; set; }
        public DateTime? CancelledAt { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public Dictionary<string, string> FieldErrors { get; set; } = new Dictionary<string, string>();
    }
} 