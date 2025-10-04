using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Grande.Fila.API.Infrastructure.Services
{
    /// <summary>
    /// Service for optimizing bulk database operations
    /// </summary>
    public interface IBulkOperationsService
    {
        /// <summary>
        /// Bulk insert entities with optimal batching
        /// </summary>
        Task<BulkOperationResult> BulkInsertAsync<T>(IEnumerable<T> entities, int batchSize = 1000, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Bulk update entities with optimal batching
        /// </summary>
        Task<BulkOperationResult> BulkUpdateAsync<T>(IEnumerable<T> entities, int batchSize = 1000, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Bulk delete entities by IDs with optimal batching
        /// </summary>
        Task<BulkOperationResult> BulkDeleteAsync<T>(IEnumerable<object> ids, int batchSize = 1000, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Bulk upsert (insert or update) entities with conflict resolution
        /// </summary>
        Task<BulkOperationResult> BulkUpsertAsync<T>(IEnumerable<T> entities, string[] conflictColumns, int batchSize = 1000, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Execute raw SQL bulk operations for maximum performance
        /// </summary>
        Task<BulkOperationResult> ExecuteBulkSqlAsync(string sql, IEnumerable<object[]> parameters, int batchSize = 1000, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get optimal batch size for a given entity type
        /// </summary>
        int GetOptimalBatchSize<T>() where T : class;

        /// <summary>
        /// Get bulk operation statistics
        /// </summary>
        Task<BulkOperationStats> GetBulkOperationStatsAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Result of a bulk operation
    /// </summary>
    public class BulkOperationResult
    {
        public bool Success { get; set; }
        public int RecordsAffected { get; set; }
        public int BatchesProcessed { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public double RecordsPerSecond { get; set; }
        public List<string> Errors { get; set; } = new();
        public Dictionary<string, object> Metrics { get; set; } = new();
    }

    /// <summary>
    /// Bulk operation statistics
    /// </summary>
    public class BulkOperationStats
    {
        public long TotalOperations { get; set; }
        public long TotalRecordsProcessed { get; set; }
        public double AverageRecordsPerSecond { get; set; }
        public TimeSpan AverageOperationDuration { get; set; }
        public Dictionary<string, EntityTypeStats> StatsByEntityType { get; set; } = new();
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// Statistics for a specific entity type
    /// </summary>
    public class EntityTypeStats
    {
        public string EntityType { get; set; } = string.Empty;
        public long OperationCount { get; set; }
        public long RecordsProcessed { get; set; }
        public double AverageRecordsPerSecond { get; set; }
        public int OptimalBatchSize { get; set; }
        public TimeSpan AverageDuration { get; set; }
    }
}
