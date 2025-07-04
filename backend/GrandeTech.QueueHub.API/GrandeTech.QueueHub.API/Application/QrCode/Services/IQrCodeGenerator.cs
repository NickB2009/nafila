using System.Threading;
using System.Threading.Tasks;

namespace Grande.Fila.API.Application.QrCode
{
    public interface IQrCodeGenerator
    {
        Task<string> GenerateQrCodeAsync(string data, CancellationToken cancellationToken = default);
    }
} 