using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Grande.Fila.API.Domain.AuditLogs;
using Grande.Fila.API.Infrastructure.Data;

namespace Grande.Fila.API.Infrastructure.Repositories.Sql
{
    public class SqlAuditLogRepository : IAuditLogRepository
    {
        private readonly QueueHubDbContext _context;
        private readonly DbSet<AuditLogEntry> _dbSet;

        public SqlAuditLogRepository(QueueHubDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<AuditLogEntry>();
        }

        public async Task LogAsync(AuditLogEntry entry, CancellationToken cancellationToken = default)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            await _dbSet.AddAsync(entry, cancellationToken);
            // Note: The actual saving to database should be handled by Unit of Work pattern
            // or by calling SaveChangesAsync on the DbContext from the calling service
        }
    }
} 