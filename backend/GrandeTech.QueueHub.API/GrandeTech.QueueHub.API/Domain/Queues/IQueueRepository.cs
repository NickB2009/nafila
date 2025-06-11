using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Domain.Common;

namespace GrandeTech.QueueHub.API.Domain.Queues
{
    public interface IQueueRepository : IRepository<Queue>
    {
        Task<Queue?> GetByLocationIdAsync(Guid locationId, CancellationToken cancellationToken);
        Task<IList<Queue>> GetAllByLocationIdAsync(Guid locationId, CancellationToken cancellationToken);
    }
} 