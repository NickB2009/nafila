using System;

namespace GrandeTech.QueueHub.API.Domain.Services
{
    public class AddQueueResult
    {
        public bool Success { get; set; }
        public Guid QueueId { get; set; }
        public string? ErrorMessage { get; set; }
    }
} 