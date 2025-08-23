using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Application.Queues.Models;

namespace Grande.Fila.API.Application.Queues
{
    /// <summary>
    /// Interface for queue transfer operations
    /// </summary>
    public interface IQueueTransferService
    {
        /// <summary>
        /// Transfer a queue entry to another salon, service, or time slot
        /// </summary>
        Task<QueueTransferResponse> TransferQueueEntryAsync(
            QueueTransferRequest request, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get transfer suggestions for a queue entry
        /// </summary>
        Task<TransferSuggestionsResponse> GetTransferSuggestionsAsync(
            TransferSuggestionsRequest request, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if a queue entry is eligible for transfer
        /// </summary>
        Task<TransferEligibilityResponse> CheckTransferEligibilityAsync(
            TransferEligibilityRequest request, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Perform bulk transfer of multiple queue entries
        /// </summary>
        Task<BulkTransferResponse> BulkTransferAsync(
            BulkTransferRequest request, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get transfer analytics for a specific transfer
        /// </summary>
        Task<QueueTransferAnalytics?> GetTransferAnalyticsAsync(
            string transferId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get all transfers for a customer
        /// </summary>
        Task<List<QueueTransferAnalytics>> GetCustomerTransferHistoryAsync(
            string customerId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Cancel a pending transfer
        /// </summary>
        Task<bool> CancelTransferAsync(
            string transferId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get transfer statistics for a salon
        /// </summary>
        Task<Dictionary<string, object>> GetSalonTransferStatsAsync(
            string salonId, 
            DateTime? fromDate = null, 
            DateTime? toDate = null, 
            CancellationToken cancellationToken = default);
    }
}
