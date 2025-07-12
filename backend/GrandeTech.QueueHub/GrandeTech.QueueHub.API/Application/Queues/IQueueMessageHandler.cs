using System.Threading;
using System.Threading.Tasks;

namespace Grande.Fila.API.Application.Queues
{
    public interface IQueueMessageHandler<in T> where T : QueueMessage
    {
        Task<bool> HandleAsync(T message, CancellationToken cancellationToken = default);
    }
} 