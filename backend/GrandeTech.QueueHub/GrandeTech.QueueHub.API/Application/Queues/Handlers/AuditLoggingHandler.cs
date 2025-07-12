using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Grande.Fila.API.Domain.AuditLogs;

namespace Grande.Fila.API.Application.Queues.Handlers
{
    public class AuditLoggingHandler : IQueueMessageHandler<AuditLoggingMessage>
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly ILogger<AuditLoggingHandler> _logger;

        public AuditLoggingHandler(
            IAuditLogRepository auditLogRepository,
            ILogger<AuditLoggingHandler> logger)
        {
            _auditLogRepository = auditLogRepository;
            _logger = logger;
        }

        public async Task<bool> HandleAsync(AuditLoggingMessage message, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Processing audit log entry for user {UserId}, action {Action}", 
                    message.UserId, message.Action);

                // Validate required fields
                if (string.IsNullOrWhiteSpace(message.UserId))
                {
                    _logger.LogError("Audit log message missing required UserId");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(message.Action))
                {
                    _logger.LogError("Audit log message missing required Action");
                    return false;
                }

                // Create comprehensive audit log entry with all available information
                var auditLogEntry = new AuditLogEntry
                {
                    UserId = message.UserId,
                    Action = message.Action,
                    EntityType = message.EntityType,
                    EntityId = message.EntityId,
                    Details = message.Details,
                    TimestampUtc = message.TimestampUtc,
                    IpAddress = message.IpAddress,
                    UserAgent = message.UserAgent
                };

                // Save to repository
                await _auditLogRepository.LogAsync(auditLogEntry, cancellationToken);

                _logger.LogDebug("Successfully processed audit log entry for user {UserId}, action {Action}", 
                    message.UserId, message.Action);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing audit log entry for user {UserId}, action {Action}", 
                    message.UserId, message.Action);
                return false;
            }
        }
    }
} 