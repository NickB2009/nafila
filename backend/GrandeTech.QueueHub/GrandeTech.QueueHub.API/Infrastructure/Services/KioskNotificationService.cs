using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Grande.Fila.API.Application.Kiosk;
using Grande.Fila.API.Infrastructure.Hubs;

namespace Grande.Fila.API.Infrastructure.Services
{
    /// <summary>
    /// Service for sending real-time notifications to kiosk displays using SignalR
    /// </summary>
    public class KioskNotificationService : IKioskNotificationService
    {
        private readonly IHubContext<KioskDisplayHub> _hubContext;
        private readonly ILogger<KioskNotificationService> _logger;

        public KioskNotificationService(
            IHubContext<KioskDisplayHub> hubContext,
            ILogger<KioskNotificationService> logger)
        {
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SendQueueUpdateAsync(string locationId, KioskDisplayResult displayData, CancellationToken cancellationToken = default)
        {
            try
            {
                var groupName = GetLocationGroupName(locationId);
                
                _logger.LogDebug("Sending queue update to location {LocationId} group {GroupName}", 
                    locationId, groupName);

                await _hubContext.Clients.Group(groupName).SendAsync("QueueUpdated", displayData, cancellationToken);
                
                _logger.LogInformation("Queue update sent to location {LocationId} with {EntryCount} entries", 
                    locationId, displayData.QueueEntries.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send queue update to location {LocationId}", locationId);
                throw;
            }
        }

        public async Task SendNotificationAsync(string locationId, string message, string messageType = "info", CancellationToken cancellationToken = default)
        {
            try
            {
                var groupName = GetLocationGroupName(locationId);
                var notification = new
                {
                    message,
                    messageType,
                    timestamp = DateTime.UtcNow,
                    locationId
                };

                _logger.LogDebug("Sending notification to location {LocationId}: {Message}", 
                    locationId, message);

                await _hubContext.Clients.Group(groupName).SendAsync("Notification", notification, cancellationToken);
                
                _logger.LogInformation("Notification sent to location {LocationId}: {MessageType} - {Message}", 
                    locationId, messageType, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification to location {LocationId}", locationId);
                throw;
            }
        }

        public int GetActiveConnectionCount(string locationId)
        {
            return KioskDisplayHub.GetActiveConnectionCount(locationId);
        }

        public Dictionary<string, int> GetConnectionStatistics()
        {
            return KioskDisplayHub.GetActiveSubscriptions();
        }

        private static string GetLocationGroupName(string locationId)
        {
            return $"location_{locationId}";
        }
    }
}
