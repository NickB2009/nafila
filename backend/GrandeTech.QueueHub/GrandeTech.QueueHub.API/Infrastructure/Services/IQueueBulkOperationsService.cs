using Grande.Fila.API.Domain.Queues;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Grande.Fila.API.Infrastructure.Services
{
    /// <summary>
    /// Specialized service for bulk queue operations
    /// </summary>
    public interface IQueueBulkOperationsService
    {
        /// <summary>
        /// Bulk join customers to a queue
        /// </summary>
        Task<BulkOperationResult> BulkJoinQueueAsync(
            Guid queueId, 
            IEnumerable<BulkJoinRequest> joinRequests, 
            string createdBy, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Bulk update queue entry statuses
        /// </summary>
        Task<BulkOperationResult> BulkUpdateStatusAsync(
            IEnumerable<Guid> queueEntryIds, 
            QueueEntryStatus newStatus, 
            string updatedBy, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Bulk assign staff to queue entries
        /// </summary>
        Task<BulkOperationResult> BulkAssignStaffAsync(
            IEnumerable<Guid> queueEntryIds, 
            Guid staffMemberId, 
            string updatedBy, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Bulk process queue entries (mark as completed)
        /// </summary>
        Task<BulkOperationResult> BulkCompleteAsync(
            IEnumerable<Guid> queueEntryIds, 
            string completedBy, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Bulk cancel queue entries
        /// </summary>
        Task<BulkOperationResult> BulkCancelAsync(
            IEnumerable<Guid> queueEntryIds, 
            string cancelledBy, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Bulk move queue entries to different positions
        /// </summary>
        Task<BulkOperationResult> BulkMovePositionsAsync(
            IEnumerable<QueuePositionUpdate> positionUpdates, 
            string updatedBy, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Bulk cleanup old completed queue entries
        /// </summary>
        Task<BulkOperationResult> BulkCleanupOldEntriesAsync(
            DateTime cutoffDate, 
            string cleanedBy, 
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Request for bulk queue join
    /// </summary>
    public class BulkJoinRequest
    {
        public string CustomerName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public Guid? ServiceTypeId { get; set; }
        public bool IsAnonymous { get; set; } = true;
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Request for queue position updates
    /// </summary>
    public class QueuePositionUpdate
    {
        public Guid QueueEntryId { get; set; }
        public int NewPosition { get; set; }
    }
}
