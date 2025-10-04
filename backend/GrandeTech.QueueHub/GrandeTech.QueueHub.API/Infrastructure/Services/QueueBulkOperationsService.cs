using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.Customers;
using Grande.Fila.API.Domain.Staff;
using Grande.Fila.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Grande.Fila.API.Infrastructure.Services
{
    /// <summary>
    /// Implementation of specialized queue bulk operations service
    /// </summary>
    public class QueueBulkOperationsService : IQueueBulkOperationsService
    {
        private readonly QueueHubDbContext _context;
        private readonly IBulkOperationsService _bulkOperationsService;
        private readonly ILogger<QueueBulkOperationsService> _logger;

        public QueueBulkOperationsService(
            QueueHubDbContext context,
            IBulkOperationsService bulkOperationsService,
            ILogger<QueueBulkOperationsService> logger)
        {
            _context = context;
            _bulkOperationsService = bulkOperationsService;
            _logger = logger;
        }

        public async Task<BulkOperationResult> BulkJoinQueueAsync(
            Guid queueId, 
            IEnumerable<BulkJoinRequest> joinRequests, 
            string createdBy, 
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new BulkOperationResult();

            try
            {
                _logger.LogInformation("Starting bulk join for queue {QueueId} with {Count} customers", queueId, joinRequests.Count());

                // Get the queue to validate it exists and get current position
                var queue = await _context.Queues
                    .Include(q => q.Entries)
                    .FirstOrDefaultAsync(q => q.Id == queueId, cancellationToken);

                if (queue == null)
                {
                    result.Success = false;
                    result.Errors.Add("Queue not found");
                    return result;
                }

                var queueEntries = new List<QueueEntry>();
                var currentPosition = queue.Entries.Count + 1;

                foreach (var joinRequest in joinRequests)
                {
                    try
                    {
                        // Create or get customer
                        var customer = await GetOrCreateCustomer(joinRequest, cancellationToken);
                        
                        // Create queue entry
                        var queueEntry = new QueueEntry(
                            queueId,
                            customer.Id,
                            joinRequest.CustomerName,
                            currentPosition++,
                            null, // staffMemberId
                            joinRequest.ServiceTypeId,
                            joinRequest.Notes);

                        queueEntries.Add(queueEntry);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to process join request for customer {CustomerName}", joinRequest.CustomerName);
                        result.Errors.Add($"Failed to join {joinRequest.CustomerName}: {ex.Message}");
                    }
                }

                if (queueEntries.Any())
                {
                    // Use bulk insert for queue entries
                    var insertResult = await _bulkOperationsService.BulkInsertAsync(queueEntries, cancellationToken: cancellationToken);
                    
                    result.RecordsAffected = insertResult.RecordsAffected;
                    result.Success = insertResult.Success && result.Errors.Count == 0;
                    result.Errors.AddRange(insertResult.Errors);
                }

                stopwatch.Stop();
                result.TotalDuration = stopwatch.Elapsed;
                result.RecordsPerSecond = result.TotalDuration.TotalSeconds > 0 ? result.RecordsAffected / result.TotalDuration.TotalSeconds : 0;

                _logger.LogInformation("Bulk join completed for queue {QueueId}: {RecordsAffected} customers joined in {Duration}ms", 
                    queueId, result.RecordsAffected, result.TotalDuration.TotalMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.Success = false;
                result.TotalDuration = stopwatch.Elapsed;
                result.Errors.Add($"Bulk join failed: {ex.Message}");
                
                _logger.LogError(ex, "Bulk join failed for queue {QueueId}", queueId);
                return result;
            }
        }

        public async Task<BulkOperationResult> BulkUpdateStatusAsync(
            IEnumerable<Guid> queueEntryIds, 
            QueueEntryStatus newStatus, 
            string updatedBy, 
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new BulkOperationResult();

            try
            {
                _logger.LogInformation("Starting bulk status update to {NewStatus} for {Count} queue entries", newStatus, queueEntryIds.Count());

                var idList = queueEntryIds.ToList();
                var batchSize = _bulkOperationsService.GetOptimalBatchSize<QueueEntry>();

                var batches = idList.Chunk(batchSize);
                var batchesProcessed = 0;

                foreach (var batch in batches)
                {
                    try
                    {
                        // Get queue entries
                        var queueEntries = await _context.QueueEntries
                            .Where(qe => batch.Contains(qe.Id))
                            .ToListAsync(cancellationToken);

                        // Update status
                        foreach (var entry in queueEntries)
                        {
                            switch (newStatus)
                            {
                                case QueueEntryStatus.Called:
                                    // Note: Call method requires staffMemberId, using a default for bulk operations
                                    entry.Call(Guid.Empty); // This will need to be handled differently
                                    break;
                                case QueueEntryStatus.CheckedIn:
                                    entry.CheckIn();
                                    break;
                                case QueueEntryStatus.Completed:
                                    entry.Complete(30); // Default 30 minutes service time
                                    break;
                                case QueueEntryStatus.Cancelled:
                                    entry.Cancel();
                                    break;
                                default:
                                    throw new ArgumentException($"Invalid status for bulk update: {newStatus}");
                            }
                        }

                        await _context.SaveChangesAsync(cancellationToken);
                        batchesProcessed++;
                        result.RecordsAffected += queueEntries.Count;

                        _logger.LogDebug("Processed status update batch {BatchNumber} with {RecordCount} records", batchesProcessed, queueEntries.Count);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing status update batch {BatchNumber}", batchesProcessed + 1);
                        result.Errors.Add($"Batch {batchesProcessed + 1}: {ex.Message}");
                        continue;
                    }
                }

                stopwatch.Stop();
                result.Success = result.Errors.Count == 0;
                result.BatchesProcessed = batchesProcessed;
                result.TotalDuration = stopwatch.Elapsed;
                result.RecordsPerSecond = result.TotalDuration.TotalSeconds > 0 ? result.RecordsAffected / result.TotalDuration.TotalSeconds : 0;

                _logger.LogInformation("Bulk status update completed: {RecordsAffected} entries updated to {NewStatus} in {Duration}ms", 
                    result.RecordsAffected, newStatus, result.TotalDuration.TotalMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.Success = false;
                result.TotalDuration = stopwatch.Elapsed;
                result.Errors.Add($"Bulk status update failed: {ex.Message}");
                
                _logger.LogError(ex, "Bulk status update failed for status {NewStatus}", newStatus);
                return result;
            }
        }

        public async Task<BulkOperationResult> BulkAssignStaffAsync(
            IEnumerable<Guid> queueEntryIds, 
            Guid staffMemberId, 
            string updatedBy, 
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new BulkOperationResult();

            try
            {
                _logger.LogInformation("Starting bulk staff assignment for {Count} queue entries to staff {StaffMemberId}", queueEntryIds.Count(), staffMemberId);

                // Validate staff member exists
                var staffMember = await _context.StaffMembers
                    .FirstOrDefaultAsync(s => s.Id == staffMemberId, cancellationToken);

                if (staffMember == null)
                {
                    result.Success = false;
                    result.Errors.Add("Staff member not found");
                    return result;
                }

                var idList = queueEntryIds.ToList();
                var batchSize = _bulkOperationsService.GetOptimalBatchSize<QueueEntry>();

                var batches = idList.Chunk(batchSize);
                var batchesProcessed = 0;

                foreach (var batch in batches)
                {
                    try
                    {
                        // Get queue entries
                        var queueEntries = await _context.QueueEntries
                            .Where(qe => batch.Contains(qe.Id))
                            .ToListAsync(cancellationToken);

                        // Assign staff - this would require a custom method or direct property update
                        // For now, we'll update the property directly since there's no AssignStaff method
                        foreach (var entry in queueEntries)
                        {
                            // Note: This is a workaround since QueueEntry doesn't have an AssignStaff method
                            // In a real implementation, you'd add this method to the domain entity
                            entry.GetType().GetProperty("StaffMemberId")?.SetValue(entry, staffMemberId);
                        }

                        await _context.SaveChangesAsync(cancellationToken);
                        batchesProcessed++;
                        result.RecordsAffected += queueEntries.Count;

                        _logger.LogDebug("Processed staff assignment batch {BatchNumber} with {RecordCount} records", batchesProcessed, queueEntries.Count);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing staff assignment batch {BatchNumber}", batchesProcessed + 1);
                        result.Errors.Add($"Batch {batchesProcessed + 1}: {ex.Message}");
                        continue;
                    }
                }

                stopwatch.Stop();
                result.Success = result.Errors.Count == 0;
                result.BatchesProcessed = batchesProcessed;
                result.TotalDuration = stopwatch.Elapsed;
                result.RecordsPerSecond = result.TotalDuration.TotalSeconds > 0 ? result.RecordsAffected / result.TotalDuration.TotalSeconds : 0;

                _logger.LogInformation("Bulk staff assignment completed: {RecordsAffected} entries assigned to staff {StaffMemberId} in {Duration}ms", 
                    result.RecordsAffected, staffMemberId, result.TotalDuration.TotalMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.Success = false;
                result.TotalDuration = stopwatch.Elapsed;
                result.Errors.Add($"Bulk staff assignment failed: {ex.Message}");
                
                _logger.LogError(ex, "Bulk staff assignment failed for staff {StaffMemberId}", staffMemberId);
                return result;
            }
        }

        public async Task<BulkOperationResult> BulkCompleteAsync(
            IEnumerable<Guid> queueEntryIds, 
            string completedBy, 
            CancellationToken cancellationToken = default)
        {
            return await BulkUpdateStatusAsync(queueEntryIds, QueueEntryStatus.Completed, completedBy, cancellationToken);
        }

        public async Task<BulkOperationResult> BulkCancelAsync(
            IEnumerable<Guid> queueEntryIds, 
            string cancelledBy, 
            CancellationToken cancellationToken = default)
        {
            return await BulkUpdateStatusAsync(queueEntryIds, QueueEntryStatus.Cancelled, cancelledBy, cancellationToken);
        }

        public async Task<BulkOperationResult> BulkMovePositionsAsync(
            IEnumerable<QueuePositionUpdate> positionUpdates, 
            string updatedBy, 
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new BulkOperationResult();

            try
            {
                _logger.LogInformation("Starting bulk position updates for {Count} queue entries", positionUpdates.Count());

                var updateList = positionUpdates.ToList();
                var batchSize = _bulkOperationsService.GetOptimalBatchSize<QueueEntry>();

                var batches = updateList.Chunk(batchSize);
                var batchesProcessed = 0;

                foreach (var batch in batches)
                {
                    try
                    {
                        var queueEntryIds = batch.Select(u => u.QueueEntryId).ToList();
                        
                        // Get queue entries
                        var queueEntries = await _context.QueueEntries
                            .Where(qe => queueEntryIds.Contains(qe.Id))
                            .ToDictionaryAsync(qe => qe.Id, cancellationToken);

                        // Update positions - direct property update since there's no UpdatePosition method
                        foreach (var update in batch)
                        {
                            if (queueEntries.TryGetValue(update.QueueEntryId, out var entry))
                            {
                                // Note: This is a workaround since QueueEntry doesn't have an UpdatePosition method
                                // In a real implementation, you'd add this method to the domain entity
                                entry.GetType().GetProperty("Position")?.SetValue(entry, update.NewPosition);
                            }
                        }

                        await _context.SaveChangesAsync(cancellationToken);
                        batchesProcessed++;
                        result.RecordsAffected += batch.Length;

                        _logger.LogDebug("Processed position update batch {BatchNumber} with {RecordCount} records", batchesProcessed, batch.Length);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing position update batch {BatchNumber}", batchesProcessed + 1);
                        result.Errors.Add($"Batch {batchesProcessed + 1}: {ex.Message}");
                        continue;
                    }
                }

                stopwatch.Stop();
                result.Success = result.Errors.Count == 0;
                result.BatchesProcessed = batchesProcessed;
                result.TotalDuration = stopwatch.Elapsed;
                result.RecordsPerSecond = result.TotalDuration.TotalSeconds > 0 ? result.RecordsAffected / result.TotalDuration.TotalSeconds : 0;

                _logger.LogInformation("Bulk position updates completed: {RecordsAffected} entries updated in {Duration}ms", 
                    result.RecordsAffected, result.TotalDuration.TotalMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.Success = false;
                result.TotalDuration = stopwatch.Elapsed;
                result.Errors.Add($"Bulk position updates failed: {ex.Message}");
                
                _logger.LogError(ex, "Bulk position updates failed");
                return result;
            }
        }

        public async Task<BulkOperationResult> BulkCleanupOldEntriesAsync(
            DateTime cutoffDate, 
            string cleanedBy, 
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new BulkOperationResult();

            try
            {
                _logger.LogInformation("Starting bulk cleanup of queue entries older than {CutoffDate}", cutoffDate);

                // Find old completed entries
                var oldEntries = await _context.QueueEntries
                    .Where(qe => qe.Status == QueueEntryStatus.Completed && 
                               qe.CompletedAt.HasValue && 
                               qe.CompletedAt.Value < cutoffDate)
                    .Select(qe => qe.Id)
                    .ToListAsync(cancellationToken);

                if (oldEntries.Any())
                {
                    // Use bulk delete
                    var deleteResult = await _bulkOperationsService.BulkDeleteAsync<QueueEntry>(
                        oldEntries.Cast<object>(), 
                        cancellationToken: cancellationToken);
                    
                    result.RecordsAffected = deleteResult.RecordsAffected;
                    result.Success = deleteResult.Success;
                    result.Errors.AddRange(deleteResult.Errors);
                }

                stopwatch.Stop();
                result.TotalDuration = stopwatch.Elapsed;
                result.RecordsPerSecond = result.TotalDuration.TotalSeconds > 0 ? result.RecordsAffected / result.TotalDuration.TotalSeconds : 0;

                _logger.LogInformation("Bulk cleanup completed: {RecordsAffected} old entries removed in {Duration}ms", 
                    result.RecordsAffected, result.TotalDuration.TotalMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.Success = false;
                result.TotalDuration = stopwatch.Elapsed;
                result.Errors.Add($"Bulk cleanup failed: {ex.Message}");
                
                _logger.LogError(ex, "Bulk cleanup failed");
                return result;
            }
        }

        #region Private Methods

        private async Task<Customer> GetOrCreateCustomer(BulkJoinRequest joinRequest, CancellationToken cancellationToken)
        {
            // Try to find existing customer by email or phone
            Customer? existingCustomer = null;

            if (!string.IsNullOrEmpty(joinRequest.Email))
            {
                existingCustomer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Email == joinRequest.Email, cancellationToken);
            }

            if (existingCustomer == null && !string.IsNullOrEmpty(joinRequest.PhoneNumber))
            {
                existingCustomer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.PhoneNumber == joinRequest.PhoneNumber, cancellationToken);
            }

            if (existingCustomer != null)
            {
                return existingCustomer;
            }

            // Create new customer
            var customer = new Customer(
                joinRequest.CustomerName,
                joinRequest.PhoneNumber,
                joinRequest.Email,
                joinRequest.IsAnonymous,
                null); // userId for anonymous customers

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync(cancellationToken);

            return customer;
        }

        #endregion
    }
}
