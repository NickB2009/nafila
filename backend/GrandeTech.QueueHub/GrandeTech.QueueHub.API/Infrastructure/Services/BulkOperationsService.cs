using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Grande.Fila.API.Infrastructure.Data;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.Locations;
using Grande.Fila.API.Domain.Organizations;
using Grande.Fila.API.Domain.Staff;
using Grande.Fila.API.Domain.Customers;
using Grande.Fila.API.Domain.Users;
using Grande.Fila.API.Domain.ServicesOffered;

namespace Grande.Fila.API.Infrastructure.Services
{
    /// <summary>
    /// Implementation of bulk operations service for high-performance database operations
    /// </summary>
    public class BulkOperationsService : IBulkOperationsService
    {
        private readonly QueueHubDbContext _context;
        private readonly ILogger<BulkOperationsService> _logger;
        
        // Performance tracking
        private static readonly ConcurrentDictionary<string, EntityTypeStats> _entityStats = new();
        private static long _totalOperations = 0;
        private static long _totalRecordsProcessed = 0;
        private static readonly object _statsLock = new();

        // Configuration
        private readonly Dictionary<Type, int> _optimalBatchSizes = new()
        {
            { typeof(QueueEntry), 2000 }, // Queue entries are small
            { typeof(Queue), 500 },       // Queues are medium-sized
            { typeof(Location), 200 },    // Locations have more data
            { typeof(Organization), 100 }, // Organizations are complex
            { typeof(StaffMember), 300 },  // Staff members are medium
            { typeof(Customer), 1500 },    // Customers are small
            { typeof(User), 1000 },        // Users are medium
            { typeof(ServiceOffered), 800 } // Services are medium
        };

        public BulkOperationsService(QueueHubDbContext context, ILogger<BulkOperationsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<BulkOperationResult> BulkInsertAsync<T>(IEnumerable<T> entities, int batchSize = 1000, CancellationToken cancellationToken = default) where T : class
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new BulkOperationResult();
            var entityType = typeof(T);
            var entityTypeName = entityType.Name;

            try
            {
                _logger.LogInformation("Starting bulk insert for {EntityType} with batch size {BatchSize}", entityTypeName, batchSize);

                var entityList = entities.ToList();
                var totalRecords = entityList.Count;
                
                if (totalRecords == 0)
                {
                    result.Success = true;
                    return result;
                }

                var optimalBatchSize = GetOptimalBatchSize<T>();
                var actualBatchSize = Math.Min(batchSize, optimalBatchSize);

                var batches = entityList.Chunk(actualBatchSize);
                var batchesProcessed = 0;

                foreach (var batch in batches)
                {
                    try
                    {
                        await _context.Set<T>().AddRangeAsync(batch, cancellationToken);
                        await _context.SaveChangesAsync(cancellationToken);
                        
                        batchesProcessed++;
                        result.RecordsAffected += batch.Length;

                        _logger.LogDebug("Processed batch {BatchNumber} with {RecordCount} records", batchesProcessed, batch.Length);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing batch {BatchNumber} for {EntityType}", batchesProcessed + 1, entityTypeName);
                        result.Errors.Add($"Batch {batchesProcessed + 1}: {ex.Message}");
                        
                        // Continue with next batch instead of failing completely
                        continue;
                    }
                }

                stopwatch.Stop();
                result.Success = result.Errors.Count == 0;
                result.BatchesProcessed = batchesProcessed;
                result.TotalDuration = stopwatch.Elapsed;
                result.RecordsPerSecond = result.TotalDuration.TotalSeconds > 0 ? result.RecordsAffected / result.TotalDuration.TotalSeconds : 0;

                // Update statistics
                UpdateStatistics(entityTypeName, result);

                _logger.LogInformation("Bulk insert completed for {EntityType}: {RecordsAffected} records in {Duration}ms ({RecordsPerSecond:F1} records/sec)", 
                    entityTypeName, result.RecordsAffected, result.TotalDuration.TotalMilliseconds, result.RecordsPerSecond);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.Success = false;
                result.TotalDuration = stopwatch.Elapsed;
                result.Errors.Add($"Bulk insert failed: {ex.Message}");
                
                _logger.LogError(ex, "Bulk insert failed for {EntityType}", entityTypeName);
                return result;
            }
        }

        public async Task<BulkOperationResult> BulkUpdateAsync<T>(IEnumerable<T> entities, int batchSize = 1000, CancellationToken cancellationToken = default) where T : class
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new BulkOperationResult();
            var entityType = typeof(T);
            var entityTypeName = entityType.Name;

            try
            {
                _logger.LogInformation("Starting bulk update for {EntityType} with batch size {BatchSize}", entityTypeName, batchSize);

                var entityList = entities.ToList();
                var totalRecords = entityList.Count;
                
                if (totalRecords == 0)
                {
                    result.Success = true;
                    return result;
                }

                var optimalBatchSize = GetOptimalBatchSize<T>();
                var actualBatchSize = Math.Min(batchSize, optimalBatchSize);

                var batches = entityList.Chunk(actualBatchSize);
                var batchesProcessed = 0;

                foreach (var batch in batches)
                {
                    try
                    {
                        _context.Set<T>().UpdateRange(batch);
                        await _context.SaveChangesAsync(cancellationToken);
                        
                        batchesProcessed++;
                        result.RecordsAffected += batch.Length;

                        _logger.LogDebug("Processed update batch {BatchNumber} with {RecordCount} records", batchesProcessed, batch.Length);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing update batch {BatchNumber} for {EntityType}", batchesProcessed + 1, entityTypeName);
                        result.Errors.Add($"Batch {batchesProcessed + 1}: {ex.Message}");
                        continue;
                    }
                }

                stopwatch.Stop();
                result.Success = result.Errors.Count == 0;
                result.BatchesProcessed = batchesProcessed;
                result.TotalDuration = stopwatch.Elapsed;
                result.RecordsPerSecond = result.TotalDuration.TotalSeconds > 0 ? result.RecordsAffected / result.TotalDuration.TotalSeconds : 0;

                // Update statistics
                UpdateStatistics(entityTypeName, result);

                _logger.LogInformation("Bulk update completed for {EntityType}: {RecordsAffected} records in {Duration}ms ({RecordsPerSecond:F1} records/sec)", 
                    entityTypeName, result.RecordsAffected, result.TotalDuration.TotalMilliseconds, result.RecordsPerSecond);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.Success = false;
                result.TotalDuration = stopwatch.Elapsed;
                result.Errors.Add($"Bulk update failed: {ex.Message}");
                
                _logger.LogError(ex, "Bulk update failed for {EntityType}", entityTypeName);
                return result;
            }
        }

        public async Task<BulkOperationResult> BulkDeleteAsync<T>(IEnumerable<object> ids, int batchSize = 1000, CancellationToken cancellationToken = default) where T : class
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new BulkOperationResult();
            var entityType = typeof(T);
            var entityTypeName = entityType.Name;

            try
            {
                _logger.LogInformation("Starting bulk delete for {EntityType} with batch size {BatchSize}", entityTypeName, batchSize);

                var idList = ids.ToList();
                var totalRecords = idList.Count;
                
                if (totalRecords == 0)
                {
                    result.Success = true;
                    return result;
                }

                var optimalBatchSize = GetOptimalBatchSize<T>();
                var actualBatchSize = Math.Min(batchSize, optimalBatchSize);

                var batches = idList.Chunk(actualBatchSize);
                var batchesProcessed = 0;

                foreach (var batch in batches)
                {
                    try
                    {
                        var entities = await _context.Set<T>()
                            .Where(e => batch.Contains(EF.Property<object>(e, "Id")))
                            .ToListAsync(cancellationToken);

                        if (entities.Any())
                        {
                            _context.Set<T>().RemoveRange(entities);
                            await _context.SaveChangesAsync(cancellationToken);
                            result.RecordsAffected += entities.Count;
                        }
                        
                        batchesProcessed++;

                        _logger.LogDebug("Processed delete batch {BatchNumber} with {RecordCount} records", batchesProcessed, batch.Length);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing delete batch {BatchNumber} for {EntityType}", batchesProcessed + 1, entityTypeName);
                        result.Errors.Add($"Batch {batchesProcessed + 1}: {ex.Message}");
                        continue;
                    }
                }

                stopwatch.Stop();
                result.Success = result.Errors.Count == 0;
                result.BatchesProcessed = batchesProcessed;
                result.TotalDuration = stopwatch.Elapsed;
                result.RecordsPerSecond = result.TotalDuration.TotalSeconds > 0 ? result.RecordsAffected / result.TotalDuration.TotalSeconds : 0;

                // Update statistics
                UpdateStatistics(entityTypeName, result);

                _logger.LogInformation("Bulk delete completed for {EntityType}: {RecordsAffected} records in {Duration}ms ({RecordsPerSecond:F1} records/sec)", 
                    entityTypeName, result.RecordsAffected, result.TotalDuration.TotalMilliseconds, result.RecordsPerSecond);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.Success = false;
                result.TotalDuration = stopwatch.Elapsed;
                result.Errors.Add($"Bulk delete failed: {ex.Message}");
                
                _logger.LogError(ex, "Bulk delete failed for {EntityType}", entityTypeName);
                return result;
            }
        }

        public async Task<BulkOperationResult> BulkUpsertAsync<T>(IEnumerable<T> entities, string[] conflictColumns, int batchSize = 1000, CancellationToken cancellationToken = default) where T : class
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new BulkOperationResult();
            var entityType = typeof(T);
            var entityTypeName = entityType.Name;

            try
            {
                _logger.LogInformation("Starting bulk upsert for {EntityType} with batch size {BatchSize}", entityTypeName, batchSize);

                var entityList = entities.ToList();
                var totalRecords = entityList.Count;
                
                if (totalRecords == 0)
                {
                    result.Success = true;
                    return result;
                }

                // For MySQL, we'll use INSERT ... ON DUPLICATE KEY UPDATE
                var sql = GenerateUpsertSql<T>(conflictColumns);
                var parameters = entityList.Select(e => ExtractEntityValues(e, conflictColumns)).ToList();

                var optimalBatchSize = GetOptimalBatchSize<T>();
                var actualBatchSize = Math.Min(batchSize, optimalBatchSize);

                var batches = parameters.Chunk(actualBatchSize);
                var batchesProcessed = 0;

                foreach (var batch in batches)
                {
                    try
                    {
                        foreach (var paramSet in batch)
                        {
                            await _context.Database.ExecuteSqlRawAsync(sql, paramSet);
                            result.RecordsAffected++;
                        }
                        
                        batchesProcessed++;
                        _logger.LogDebug("Processed upsert batch {BatchNumber} with {RecordCount} records", batchesProcessed, batch.Length);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing upsert batch {BatchNumber} for {EntityType}", batchesProcessed + 1, entityTypeName);
                        result.Errors.Add($"Batch {batchesProcessed + 1}: {ex.Message}");
                        continue;
                    }
                }

                stopwatch.Stop();
                result.Success = result.Errors.Count == 0;
                result.BatchesProcessed = batchesProcessed;
                result.TotalDuration = stopwatch.Elapsed;
                result.RecordsPerSecond = result.TotalDuration.TotalSeconds > 0 ? result.RecordsAffected / result.TotalDuration.TotalSeconds : 0;

                // Update statistics
                UpdateStatistics(entityTypeName, result);

                _logger.LogInformation("Bulk upsert completed for {EntityType}: {RecordsAffected} records in {Duration}ms ({RecordsPerSecond:F1} records/sec)", 
                    entityTypeName, result.RecordsAffected, result.TotalDuration.TotalMilliseconds, result.RecordsPerSecond);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.Success = false;
                result.TotalDuration = stopwatch.Elapsed;
                result.Errors.Add($"Bulk upsert failed: {ex.Message}");
                
                _logger.LogError(ex, "Bulk upsert failed for {EntityType}", entityTypeName);
                return result;
            }
        }

        public async Task<BulkOperationResult> ExecuteBulkSqlAsync(string sql, IEnumerable<object[]> parameters, int batchSize = 1000, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new BulkOperationResult();

            try
            {
                _logger.LogInformation("Starting bulk SQL execution with batch size {BatchSize}", batchSize);

                var parameterList = parameters.ToList();
                var totalRecords = parameterList.Count;
                
                if (totalRecords == 0)
                {
                    result.Success = true;
                    return result;
                }

                var batches = parameterList.Chunk(batchSize);
                var batchesProcessed = 0;

                foreach (var batch in batches)
                {
                    try
                    {
                        foreach (var paramSet in batch)
                        {
                            var rowsAffected = await _context.Database.ExecuteSqlRawAsync(sql, paramSet);
                            result.RecordsAffected += rowsAffected;
                        }
                        
                        batchesProcessed++;
                        _logger.LogDebug("Processed SQL batch {BatchNumber} with {RecordCount} records", batchesProcessed, batch.Length);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing SQL batch {BatchNumber}", batchesProcessed + 1);
                        result.Errors.Add($"Batch {batchesProcessed + 1}: {ex.Message}");
                        continue;
                    }
                }

                stopwatch.Stop();
                result.Success = result.Errors.Count == 0;
                result.BatchesProcessed = batchesProcessed;
                result.TotalDuration = stopwatch.Elapsed;
                result.RecordsPerSecond = result.TotalDuration.TotalSeconds > 0 ? result.RecordsAffected / result.TotalDuration.TotalSeconds : 0;

                _logger.LogInformation("Bulk SQL execution completed: {RecordsAffected} records in {Duration}ms ({RecordsPerSecond:F1} records/sec)", 
                    result.RecordsAffected, result.TotalDuration.TotalMilliseconds, result.RecordsPerSecond);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.Success = false;
                result.TotalDuration = stopwatch.Elapsed;
                result.Errors.Add($"Bulk SQL execution failed: {ex.Message}");
                
                _logger.LogError(ex, "Bulk SQL execution failed");
                return result;
            }
        }

        public int GetOptimalBatchSize<T>() where T : class
        {
            var entityType = typeof(T);
            
            if (_optimalBatchSizes.TryGetValue(entityType, out var batchSize))
            {
                return batchSize;
            }

            // Default batch size based on entity complexity
            return entityType.GetProperties().Length > 10 ? 200 : 1000;
        }

        public Task<BulkOperationStats> GetBulkOperationStatsAsync(CancellationToken cancellationToken = default)
        {
            var stats = new BulkOperationStats
            {
                TotalOperations = _totalOperations,
                TotalRecordsProcessed = _totalRecordsProcessed,
                StatsByEntityType = new Dictionary<string, EntityTypeStats>(_entityStats),
                LastUpdated = DateTime.UtcNow
            };

            if (_totalOperations > 0)
            {
                stats.AverageRecordsPerSecond = _totalRecordsProcessed / (double)_totalOperations;
            }

            return Task.FromResult(stats);
        }

        #region Private Methods

        private static void UpdateStatistics(string entityTypeName, BulkOperationResult result)
        {
            Interlocked.Increment(ref _totalOperations);
            Interlocked.Add(ref _totalRecordsProcessed, result.RecordsAffected);

            _entityStats.AddOrUpdate(entityTypeName, 
                new EntityTypeStats
                {
                    EntityType = entityTypeName,
                    OperationCount = 1,
                    RecordsProcessed = result.RecordsAffected,
                    AverageRecordsPerSecond = result.RecordsPerSecond,
                    AverageDuration = result.TotalDuration
                },
                (key, existing) =>
                {
                    lock (existing)
                    {
                        existing.OperationCount++;
                        existing.RecordsProcessed += result.RecordsAffected;
                        existing.AverageRecordsPerSecond = (existing.AverageRecordsPerSecond + result.RecordsPerSecond) / 2;
                        existing.AverageDuration = TimeSpan.FromMilliseconds(
                            (existing.AverageDuration.TotalMilliseconds + result.TotalDuration.TotalMilliseconds) / 2);
                    }
                    return existing;
                });
        }

        private static string GenerateUpsertSql<T>(string[] conflictColumns) where T : class
        {
            var entityType = typeof(T);
            var tableName = entityType.Name;
            var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            var columns = properties.Select(p => p.Name).ToArray();
            var values = columns.Select(c => $"@{c}").ToArray();
            var updateClause = columns.Select(c => $"{c} = @{c}").ToArray();
            var conflictClause = string.Join(", ", conflictColumns);

            return $@"
                INSERT INTO {tableName} ({string.Join(", ", columns)})
                VALUES ({string.Join(", ", values)})
                ON DUPLICATE KEY UPDATE {string.Join(", ", updateClause)}";
        }

        private static object[] ExtractEntityValues<T>(T entity, string[] conflictColumns) where T : class
        {
            var entityType = typeof(T);
            var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            return properties.Select(p => p.GetValue(entity) ?? DBNull.Value).ToArray();
        }

        #endregion
    }
}
