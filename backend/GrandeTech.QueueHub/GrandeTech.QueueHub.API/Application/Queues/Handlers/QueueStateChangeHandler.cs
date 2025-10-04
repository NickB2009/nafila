using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Grande.Fila.API.Application.Kiosk;
using Grande.Fila.API.Infrastructure.Services;
using Grande.Fila.API.Domain.Queues;

namespace Grande.Fila.API.Application.Queues.Handlers
{
    public class QueueStateChangeHandler : IQueueMessageHandler<QueueStateChangeMessage>
    {
        private readonly ILogger<QueueStateChangeHandler> _logger;
        private readonly KioskDisplayService _kioskDisplayService;
        private readonly IKioskNotificationService _kioskNotificationService;

        public QueueStateChangeHandler(
            ILogger<QueueStateChangeHandler> logger,
            KioskDisplayService kioskDisplayService,
            IKioskNotificationService kioskNotificationService)
        {
            _logger = logger;
            _kioskDisplayService = kioskDisplayService;
            _kioskNotificationService = kioskNotificationService;
        }

        public async Task<bool> HandleAsync(QueueStateChangeMessage message, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Processing queue state change: {ChangeType} for queue {QueueId}, entry {QueueEntryId}", 
                    message.ChangeType, message.QueueId, message.QueueEntryId);

                // Validate required fields
                if (string.IsNullOrWhiteSpace(message.QueueId))
                {
                    _logger.LogError("Queue state change message missing required QueueId");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(message.ChangeType))
                {
                    _logger.LogError("Queue state change message missing required ChangeType");
                    return false;
                }

                // Process different types of state changes
                switch (message.ChangeType.ToLower())
                {
                    case "join":
                        return await HandleJoinEvent(message);
                    
                    case "callnext":
                        return await HandleCallNextEvent(message);
                    
                    case "checkin":
                        return await HandleCheckInEvent(message);
                    
                    case "finish":
                        return await HandleFinishEvent(message);
                    
                    case "cancel":
                        return await HandleCancelEvent(message);
                    
                    default:
                        _logger.LogWarning("Unknown queue state change type: {ChangeType}", message.ChangeType);
                        return true; // Return true to acknowledge the message was processed
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing queue state change: {ChangeType} for queue {QueueId}", 
                    message.ChangeType, message.QueueId);
                return false;
            }
        }

        private async Task<bool> HandleJoinEvent(QueueStateChangeMessage message)
        {
            _logger.LogInformation("Customer {CustomerName} joined queue {QueueId} at position {Position}", 
                message.CustomerName, message.QueueId, message.Position);

            // Send notification to kiosk displays
            await SendKioskUpdateAsync(message.QueueId, $"Customer {message.CustomerName} joined the queue at position {message.Position}");
            
            return true;
        }

        private Task<bool> HandleCallNextEvent(QueueStateChangeMessage message)
        {
            _logger.LogInformation("Customer {CustomerName} called next by staff {StaffMemberId} in queue {QueueId}", 
                message.CustomerName, message.StaffMemberId, message.QueueId);

            // Here you would typically:
            // - Send SMS/push notifications to the customer
            // - Update kiosk displays
            // - Update staff dashboards
            // - Log the event for analytics
            
            return Task.FromResult(true);
        }

        private Task<bool> HandleCheckInEvent(QueueStateChangeMessage message)
        {
            _logger.LogInformation("Customer {CustomerName} checked in for queue {QueueId}", 
                message.CustomerName, message.QueueId);

            // Here you would typically:
            // - Update queue displays
            // - Notify waiting customers about updated wait times
            // - Update analytics
            // - Send confirmation to customer
            
            return Task.FromResult(true);
        }

        private Task<bool> HandleFinishEvent(QueueStateChangeMessage message)
        {
            _logger.LogInformation("Service completed for customer {CustomerName} in queue {QueueId}", 
                message.CustomerName, message.QueueId);

            // Here you would typically:
            // - Update queue displays
            // - Recalculate wait times for remaining customers
            // - Update staff availability
            // - Send completion notifications
            // - Update analytics and metrics
            
            return Task.FromResult(true);
        }

        private Task<bool> HandleCancelEvent(QueueStateChangeMessage message)
        {
            _logger.LogInformation("Customer {CustomerName} cancelled queue entry {QueueEntryId} in queue {QueueId}", 
                message.CustomerName, message.QueueEntryId, message.QueueId);

            // Here you would typically:
            // - Update queue displays
            // - Recalculate positions for remaining customers
            // - Update wait time estimates
            // - Send cancellation confirmation
            // - Update analytics
            
            return Task.FromResult(true);
        }

        /// <summary>
        /// Send updated kiosk display data to all connected clients for a location
        /// </summary>
        private async Task SendKioskUpdateAsync(string queueId, string? notificationMessage = null)
        {
            try
            {
                // Extract location ID from queue ID (assuming queue ID contains location info)
                // This is a simplified approach - in production you might need to look up the location
                if (!Guid.TryParse(queueId, out var parsedQueueId))
                {
                    _logger.LogWarning("Invalid queue ID format for kiosk update: {QueueId}", queueId);
                    return;
                }

                // Get updated display data
                var request = new KioskDisplayRequest
                {
                    LocationId = queueId, // This assumes queue ID is the same as location ID
                    IncludeCompletedEntries = false
                };

                var displayData = await _kioskDisplayService.ExecuteAsync(request, "queue-state-change", CancellationToken.None);

                if (displayData.Success)
                {
                    // Send the updated display data
                    await _kioskNotificationService.SendQueueUpdateAsync(queueId, displayData);

                    // Send notification message if provided
                    if (!string.IsNullOrWhiteSpace(notificationMessage))
                    {
                        await _kioskNotificationService.SendNotificationAsync(queueId, notificationMessage, "queue-update");
                    }
                }
                else
                {
                    _logger.LogWarning("Failed to get kiosk display data for queue {QueueId}: {Errors}", 
                        queueId, string.Join(", ", displayData.Errors));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send kiosk update for queue {QueueId}", queueId);
            }
        }
    }
} 