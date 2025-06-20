using System;

namespace Grande.Fila.API.Application.Queues
{
    public class AddQueueResult
    {
        public bool Success { get; set; }
        public Guid QueueId { get; set; }
        public string? ErrorMessage { get; set; }
    }
} 