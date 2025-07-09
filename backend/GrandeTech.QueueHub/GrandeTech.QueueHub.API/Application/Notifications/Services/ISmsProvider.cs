using System.Threading;
using System.Threading.Tasks;

namespace Grande.Fila.API.Application.Notifications.Services
{
    public interface ISmsProvider
    {
        Task<bool> SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
    }
} 