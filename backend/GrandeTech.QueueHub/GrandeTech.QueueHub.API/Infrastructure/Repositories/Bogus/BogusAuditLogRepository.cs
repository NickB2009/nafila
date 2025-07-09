using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Domain.AuditLogs;

namespace Grande.Fila.API.Infrastructure.Repositories.Bogus
{
    public class BogusAuditLogRepository : IAuditLogRepository
    {
        public async Task LogAsync(AuditLogEntry entry, CancellationToken cancellationToken = default)
        {
            // In-memory implementation - just simulate logging
            await Task.CompletedTask;
        }
    }
}
