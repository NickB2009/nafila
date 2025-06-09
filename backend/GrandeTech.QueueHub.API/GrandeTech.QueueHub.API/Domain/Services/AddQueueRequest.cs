using System;

namespace GrandeTech.QueueHub.API.Domain.Services
{
    public class AddQueueRequest
    {
        public Guid LocationId { get; set; }
        public int MaxSize { get; set; }
        public int LateClientCapTimeInMinutes { get; set; }
    }
} 