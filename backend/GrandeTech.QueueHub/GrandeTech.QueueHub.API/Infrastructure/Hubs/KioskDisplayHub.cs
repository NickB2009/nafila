using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Grande.Fila.API.Infrastructure.Hubs
{
    /// <summary>
    /// SignalR hub for real-time kiosk display updates
    /// </summary>
    public class KioskDisplayHub : Hub
    {
        private readonly ILogger<KioskDisplayHub> _logger;
        private static readonly ConcurrentDictionary<string, HashSet<string>> _locationGroups = new();

        public KioskDisplayHub(ILogger<KioskDisplayHub> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Subscribe to queue updates for a specific location
        /// </summary>
        /// <param name="locationId">The location ID to subscribe to</param>
        public async Task SubscribeToLocation(string locationId)
        {
            if (string.IsNullOrWhiteSpace(locationId))
            {
                _logger.LogWarning("Client {ConnectionId} attempted to subscribe to location with empty ID", Context.ConnectionId);
                return;
            }

            var groupName = GetLocationGroupName(locationId);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            
            // Track the connection for this location
            _locationGroups.AddOrUpdate(locationId, 
                new HashSet<string> { Context.ConnectionId },
                (key, existing) => { existing.Add(Context.ConnectionId); return existing; });

            _logger.LogInformation("Client {ConnectionId} subscribed to location {LocationId}", 
                Context.ConnectionId, locationId);

            // Send acknowledgment
            await Clients.Caller.SendAsync("Subscribed", new { locationId, success = true });
        }

        /// <summary>
        /// Unsubscribe from queue updates for a specific location
        /// </summary>
        /// <param name="locationId">The location ID to unsubscribe from</param>
        public async Task UnsubscribeFromLocation(string locationId)
        {
            if (string.IsNullOrWhiteSpace(locationId))
            {
                return;
            }

            var groupName = GetLocationGroupName(locationId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            // Remove the connection from tracking
            if (_locationGroups.TryGetValue(locationId, out var connections))
            {
                connections.Remove(Context.ConnectionId);
                if (connections.Count == 0)
                {
                    _locationGroups.TryRemove(locationId, out _);
                }
            }

            _logger.LogInformation("Client {ConnectionId} unsubscribed from location {LocationId}", 
                Context.ConnectionId, locationId);

            await Clients.Caller.SendAsync("Unsubscribed", new { locationId, success = true });
        }

        /// <summary>
        /// Handle client disconnection
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            
            // Remove this connection from all location groups
            foreach (var kvp in _locationGroups.ToList())
            {
                if (kvp.Value.Remove(connectionId))
                {
                    var groupName = GetLocationGroupName(kvp.Key);
                    await Groups.RemoveFromGroupAsync(connectionId, groupName);
                    
                    if (kvp.Value.Count == 0)
                    {
                        _locationGroups.TryRemove(kvp.Key, out _);
                    }
                }
            }

            if (exception != null)
            {
                _logger.LogWarning(exception, "Client {ConnectionId} disconnected with exception", connectionId);
            }
            else
            {
                _logger.LogInformation("Client {ConnectionId} disconnected gracefully", connectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Get the number of active connections for a location
        /// </summary>
        /// <param name="locationId">The location ID</param>
        /// <returns>Number of active connections</returns>
        public static int GetActiveConnectionCount(string locationId)
        {
            return _locationGroups.TryGetValue(locationId, out var connections) ? connections.Count : 0;
        }

        /// <summary>
        /// Get all active location subscriptions
        /// </summary>
        /// <returns>Dictionary of location ID to connection count</returns>
        public static Dictionary<string, int> GetActiveSubscriptions()
        {
            return _locationGroups.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count);
        }

        private static string GetLocationGroupName(string locationId)
        {
            return $"location_{locationId}";
        }
    }
}
