using System.Threading;
using System.Threading.Tasks;

namespace Grande.Fila.API.Domain.AuditLogs
{
    public interface IAuditLogRepository
    {
        Task LogAsync(AuditLogEntry entry, CancellationToken cancellationToken = default);
    }
} 