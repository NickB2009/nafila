using System;

namespace GrandeTech.QueueHub.API.Application.Queues
{
    public class AddQueueRequest
    {
        public Guid LocationId { get; set; }
        public int MaxSize { get; set; }
        public int LateClientCapTimeInMinutes { get; set; }
    }
} 