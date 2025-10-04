using Grande.Fila.API.Application.Kiosk;

namespace Grande.Fila.API.Infrastructure.Services
{
    /// <summary>
    /// Service for sending real-time notifications to kiosk displays
    /// </summary>
    public interface IKioskNotificationService
    {
        /// <summary>
        /// Send queue update to all kiosk displays for a specific location
        /// </summary>
        /// <param name="locationId">The location ID</param>
        /// <param name="displayData">The updated display data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task SendQueueUpdateAsync(string locationId, KioskDisplayResult displayData, CancellationToken cancellationToken = default);

        /// <summary>
        /// Send a custom notification to kiosk displays
        /// </summary>
        /// <param name="locationId">The location ID</param>
        /// <param name="message">The notification message</param>
        /// <param name="messageType">The type of notification</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task SendNotificationAsync(string locationId, string message, string messageType = "info", CancellationToken cancellationToken = default);

        /// <summary>
        /// Get the number of active kiosk connections for a location
        /// </summary>
        /// <param name="locationId">The location ID</param>
        /// <returns>Number of active connections</returns>
        int GetActiveConnectionCount(string locationId);

        /// <summary>
        /// Get statistics about all kiosk connections
        /// </summary>
        /// <returns>Dictionary of location ID to connection count</returns>
        Dictionary<string, int> GetConnectionStatistics();
    }
}
