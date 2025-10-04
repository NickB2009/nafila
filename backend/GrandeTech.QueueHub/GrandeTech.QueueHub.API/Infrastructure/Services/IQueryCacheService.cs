using Microsoft.Extensions.Caching.Memory;

namespace Grande.Fila.API.Infrastructure.Services
{
    /// <summary>
    /// Service for caching frequently accessed database query results
    /// </summary>
    public interface IQueryCacheService
    {
        /// <summary>
        /// Get cached result or execute query and cache the result
        /// </summary>
        Task<T> GetOrSetAsync<T>(string cacheKey, Func<Task<T>> query, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get cached result or execute query and cache the result with sliding expiration
        /// </summary>
        Task<T> GetOrSetSlidingAsync<T>(string cacheKey, Func<Task<T>> query, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Remove specific cache entries
        /// </summary>
        Task RemoveAsync(string cacheKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// Remove cache entries matching a pattern
        /// </summary>
        Task RemovePatternAsync(string pattern, CancellationToken cancellationToken = default);

        /// <summary>
        /// Clear all cache entries
        /// </summary>
        Task ClearAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get cache statistics
        /// </summary>
        Task<CacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Invalidate cache for specific entities
        /// </summary>
        Task InvalidateEntityCacheAsync<T>(Guid entityId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Invalidate cache for all entities of a specific type
        /// </summary>
        Task InvalidateEntityTypeCacheAsync<T>(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Cache statistics
    /// </summary>
    public class CacheStatistics
    {
        public long TotalCacheEntries { get; set; }
        public long CacheHits { get; set; }
        public long CacheMisses { get; set; }
        public double HitRatio { get; set; }
        public long TotalMemoryUsageBytes { get; set; }
        public Dictionary<string, CacheTypeStatistics> StatisticsByType { get; set; } = new();
        public List<string> MostAccessedKeys { get; set; } = new();
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// Cache statistics for a specific type
    /// </summary>
    public class CacheTypeStatistics
    {
        public string TypeName { get; set; } = string.Empty;
        public long EntryCount { get; set; }
        public long HitCount { get; set; }
        public long MissCount { get; set; }
        public double HitRatio { get; set; }
        public TimeSpan AverageExpiration { get; set; }
    }
}
