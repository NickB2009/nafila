using System.Threading;
using System.Threading.Tasks;

namespace GrandeTech.QueueHub.API.Domain.AuditLogs
{
    public interface IAuditLogRepository
    {
        Task LogAsync(AuditLogEntry entry, CancellationToken cancellationToken = default);
    }
} 