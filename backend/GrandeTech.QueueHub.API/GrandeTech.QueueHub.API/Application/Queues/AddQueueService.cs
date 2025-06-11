using System;
using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Domain.Queues;

namespace GrandeTech.QueueHub.API.Application.Queues
{
    public class AddQueueService
    {
        private readonly IQueueRepository _queueRepository;

        public AddQueueService(IQueueRepository queueRepository)
        {
            _queueRepository = queueRepository;
        }

        public async Task<AddQueueResult> AddQueueAsync(AddQueueRequest request, CancellationToken cancellationToken)
        {
            var queue = new Queue(
                request.LocationId,
                request.MaxSize,
                request.LateClientCapTimeInMinutes,
                "system" // TODO: Get from authenticated user
            );

            await _queueRepository.AddAsync(queue, cancellationToken);

            return new AddQueueResult
            {
                Success = true,
                QueueId = queue.Id
            };
        }
    }
} 