using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Grande.Fila.API.Infrastructure.Services
{
    /// <summary>
    /// Implementation of query cache service for optimizing database performance
    /// </summary>
    public class QueryCacheService : IQueryCacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<QueryCacheService> _logger;
        
        // Performance tracking
        private static readonly ConcurrentDictionary<string, CacheMetrics> _cacheMetrics = new();
        private static long _totalHits = 0;
        private static long _totalMisses = 0;
        private static readonly object _statsLock = new();

        // Cache configuration
        private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(5);
        private readonly TimeSpan _defaultSlidingExpiration = TimeSpan.FromMinutes(10);
        private readonly long _maxCacheSizeBytes = 100 * 1024 * 1024; // 100MB

        public QueryCacheService(IMemoryCache memoryCache, ILogger<QueryCacheService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public async Task<T> GetOrSetAsync<T>(string cacheKey, Func<Task<T>> query, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            var effectiveExpiration = expiration ?? _defaultExpiration;
            var metrics = GetOrCreateMetrics(cacheKey);

            try
            {
                // Try to get from cache
                if (_memoryCache.TryGetValue(cacheKey, out object? cachedValue) && cachedValue is T typedValue)
                {
                    Interlocked.Increment(ref _totalHits);
                    lock (metrics) { metrics.Hits++; }
                    
                    _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
                    return typedValue;
                }

                // Cache miss - execute query
                Interlocked.Increment(ref _totalMisses);
                lock (metrics) { metrics.Misses++; }
                
                _logger.LogDebug("Cache miss for key: {CacheKey}, executing query", cacheKey);

                var result = await query();
                
                // Cache the result
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = effectiveExpiration,
                    Size = EstimateSize(result),
                    Priority = CacheItemPriority.Normal
                };

                cacheOptions.RegisterPostEvictionCallback((key, value, reason, state) =>
                {
                    _logger.LogDebug("Cache entry evicted: {CacheKey}, Reason: {Reason}", key, reason);
                });

                _memoryCache.Set(cacheKey, result, cacheOptions);
                
                _logger.LogDebug("Cached result for key: {CacheKey} with expiration: {Expiration}", 
                    cacheKey, effectiveExpiration);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetOrSetAsync for key: {CacheKey}", cacheKey);
                throw;
            }
        }

        public async Task<T> GetOrSetSlidingAsync<T>(string cacheKey, Func<Task<T>> query, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default)
        {
            var effectiveSlidingExpiration = slidingExpiration ?? _defaultSlidingExpiration;
            var metrics = GetOrCreateMetrics(cacheKey);

            try
            {
                // Try to get from cache
                if (_memoryCache.TryGetValue(cacheKey, out object? cachedValue) && cachedValue is T typedValue)
                {
                    Interlocked.Increment(ref _totalHits);
                    lock (metrics) { metrics.Hits++; }
                    
                    _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
                    return typedValue;
                }

                // Cache miss - execute query
                Interlocked.Increment(ref _totalMisses);
                lock (metrics) { metrics.Misses++; }
                
                _logger.LogDebug("Cache miss for key: {CacheKey}, executing query", cacheKey);

                var result = await query();
                
                // Cache the result with sliding expiration
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    SlidingExpiration = effectiveSlidingExpiration,
                    Size = EstimateSize(result),
                    Priority = CacheItemPriority.Normal
                };

                cacheOptions.RegisterPostEvictionCallback((key, value, reason, state) =>
                {
                    _logger.LogDebug("Cache entry evicted: {CacheKey}, Reason: {Reason}", key, reason);
                });

                _memoryCache.Set(cacheKey, result, cacheOptions);
                
                _logger.LogDebug("Cached result for key: {CacheKey} with sliding expiration: {SlidingExpiration}", 
                    cacheKey, effectiveSlidingExpiration);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetOrSetSlidingAsync for key: {CacheKey}", cacheKey);
                throw;
            }
        }

        public Task RemoveAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            _memoryCache.Remove(cacheKey);
            _logger.LogDebug("Removed cache entry: {CacheKey}", cacheKey);
            return Task.CompletedTask;
        }

        public Task RemovePatternAsync(string pattern, CancellationToken cancellationToken = default)
        {
            // Note: IMemoryCache doesn't support pattern-based removal natively
            // This is a simplified implementation that would need to be enhanced
            // with a custom cache wrapper for production use
            
            _logger.LogWarning("Pattern-based cache removal not fully supported with IMemoryCache. Pattern: {Pattern}", pattern);
            return Task.CompletedTask;
        }

        public Task ClearAllAsync(CancellationToken cancellationToken = default)
        {
            // Note: IMemoryCache doesn't support clearing all entries natively
            // This would need to be implemented with a custom cache wrapper
            
            _logger.LogWarning("Clear all cache not fully supported with IMemoryCache");
            return Task.CompletedTask;
        }

        public Task<CacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
        {
            var stats = new CacheStatistics
            {
                CacheHits = _totalHits,
                CacheMisses = _totalMisses,
                HitRatio = _totalHits + _totalMisses > 0 ? (double)_totalHits / (_totalHits + _totalMisses) : 0,
                TotalCacheEntries = _cacheMetrics.Count,
                LastUpdated = DateTime.UtcNow
            };

            // Calculate statistics by type
            var typeGroups = _cacheMetrics.GroupBy(kvp => GetTypeFromCacheKey(kvp.Key));
            foreach (var group in typeGroups)
            {
                var typeStats = new CacheTypeStatistics
                {
                    TypeName = group.Key,
                    EntryCount = group.Count(),
                    HitCount = group.Sum(m => m.Value.Hits),
                    MissCount = group.Sum(m => m.Value.Misses)
                };
                
                typeStats.HitRatio = typeStats.HitCount + typeStats.MissCount > 0 
                    ? (double)typeStats.HitCount / (typeStats.HitCount + typeStats.MissCount) 
                    : 0;

                stats.StatisticsByType[group.Key] = typeStats;
            }

            // Get most accessed keys
            stats.MostAccessedKeys = _cacheMetrics
                .OrderByDescending(kvp => kvp.Value.Hits + kvp.Value.Misses)
                .Take(10)
                .Select(kvp => kvp.Key)
                .ToList();

            return Task.FromResult(stats);
        }

        public Task InvalidateEntityCacheAsync<T>(Guid entityId, CancellationToken cancellationToken = default)
        {
            var entityType = typeof(T).Name;
            var patterns = new[]
            {
                $"{entityType}_*_{entityId}",
                $"*_{entityType}_{entityId}_*",
                $"*_{entityId}_*"
            };

            foreach (var pattern in patterns)
            {
                RemovePatternAsync(pattern, cancellationToken);
            }

            _logger.LogDebug("Invalidated cache for {EntityType} with ID: {EntityId}", entityType, entityId);
            return Task.CompletedTask;
        }

        public Task InvalidateEntityTypeCacheAsync<T>(CancellationToken cancellationToken = default)
        {
            var entityType = typeof(T).Name;
            var pattern = $"{entityType}_*";

            RemovePatternAsync(pattern, cancellationToken);
            
            _logger.LogDebug("Invalidated all cache entries for entity type: {EntityType}", entityType);
            return Task.CompletedTask;
        }

        #region Private Methods

        private static CacheMetrics GetOrCreateMetrics(string cacheKey)
        {
            return _cacheMetrics.GetOrAdd(cacheKey, _ => new CacheMetrics());
        }

        private static string GetTypeFromCacheKey(string cacheKey)
        {
            // Extract entity type from cache key
            // Assumes format like "EntityType_Operation_Id" or similar
            var parts = cacheKey.Split('_');
            return parts.Length > 0 ? parts[0] : "Unknown";
        }

        private static long EstimateSize<T>(T value)
        {
            if (value == null) return 1;

            try
            {
                // Rough estimation based on object type
                if (value is string str)
                {
                    return str.Length * 2; // Unicode characters
                }
                
                if (value is IEnumerable<object> enumerable)
                {
                    return enumerable.Count() * 100; // Rough estimate per item
                }

                // Use reflection to estimate size (simplified)
                var type = value.GetType();
                if (type.IsPrimitive)
                {
                    return 8; // Most primitives are 8 bytes or less
                }

                // Default estimation
                return 100;
            }
            catch
            {
                return 100; // Default fallback
            }
        }

        #endregion

        #region Nested Classes

        private class CacheMetrics
        {
            public long Hits { get; set; }
            public long Misses { get; set; }
            public DateTime FirstAccessed { get; set; } = DateTime.UtcNow;
            public DateTime LastAccessed { get; set; } = DateTime.UtcNow;
        }

        #endregion
    }
}
