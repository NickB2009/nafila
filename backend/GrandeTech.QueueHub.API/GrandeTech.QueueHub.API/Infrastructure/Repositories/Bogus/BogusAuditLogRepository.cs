using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Domain.AuditLogs;

namespace GrandeTech.QueueHub.API.Infrastructure.Repositories.Bogus
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
