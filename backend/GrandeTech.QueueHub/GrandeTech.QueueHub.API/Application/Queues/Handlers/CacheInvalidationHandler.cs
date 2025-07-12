using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace Grande.Fila.API.Application.Queues.Handlers
{
    public class CacheInvalidationHandler : IQueueMessageHandler<CacheInvalidationMessage>
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<CacheInvalidationHandler> _logger;

        public CacheInvalidationHandler(
            IMemoryCache memoryCache,
            ILogger<CacheInvalidationHandler> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public async Task<bool> HandleAsync(CacheInvalidationMessage message, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Processing cache invalidation of type {InvalidationType}", message.InvalidationType);

                switch (message.InvalidationType.ToLower())
                {
                    case "single":
                        return await InvalidateSingleKey(message.CacheKey);
                    
                    case "multiple":
                        return await InvalidateMultipleKeys(message.CacheKeys);
                    
                    case "pattern":
                        return await InvalidateByPattern(message.CachePattern);
                    
                    default:
                        _logger.LogError("Unknown cache invalidation type: {InvalidationType}", message.InvalidationType);
                        return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing cache invalidation of type {InvalidationType}", message.InvalidationType);
                return false;
            }
        }

        private Task<bool> InvalidateSingleKey(string cacheKey)
        {
            if (string.IsNullOrWhiteSpace(cacheKey))
            {
                _logger.LogError("Cache key is null or empty for single invalidation");
                return Task.FromResult(false);
            }

            try
            {
                _memoryCache.Remove(cacheKey);
                _logger.LogDebug("Successfully invalidated cache key: {CacheKey}", cacheKey);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating cache key: {CacheKey}", cacheKey);
                return Task.FromResult(false);
            }
        }

        private Task<bool> InvalidateMultipleKeys(string[] cacheKeys)
        {
            if (cacheKeys == null || cacheKeys.Length == 0)
            {
                _logger.LogError("Cache keys array is null or empty for multiple invalidation");
                return Task.FromResult(false);
            }

            var successCount = 0;
            foreach (var cacheKey in cacheKeys)
            {
                if (string.IsNullOrWhiteSpace(cacheKey))
                {
                    _logger.LogWarning("Skipping null or empty cache key in multiple invalidation");
                    continue;
                }

                try
                {
                    _memoryCache.Remove(cacheKey);
                    successCount++;
                    _logger.LogDebug("Successfully invalidated cache key: {CacheKey}", cacheKey);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error invalidating cache key: {CacheKey}", cacheKey);
                }
            }

            var success = successCount > 0;
            _logger.LogDebug("Invalidated {SuccessCount} out of {TotalCount} cache keys", successCount, cacheKeys.Length);
            return Task.FromResult(success);
        }

        private Task<bool> InvalidateByPattern(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
            {
                _logger.LogError("Cache pattern is null or empty for pattern invalidation");
                return Task.FromResult(false);
            }

            try
            {
                // Note: IMemoryCache doesn't have a built-in way to get all keys or invalidate by pattern
                // In a real implementation, you might need to:
                // 1. Use a distributed cache like Redis that supports pattern matching
                // 2. Maintain a separate index of cache keys
                // 3. Use a cache implementation that supports pattern invalidation
                
                // For now, we'll log a warning and return true to indicate the message was processed
                _logger.LogWarning("Pattern-based cache invalidation is not fully implemented for IMemoryCache. Pattern: {Pattern}", pattern);
                
                // If you're using Redis or another cache that supports pattern matching:
                // await _distributedCache.RemoveByPatternAsync(pattern, cancellationToken);
                
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating cache by pattern: {Pattern}", pattern);
                return Task.FromResult(false);
            }
        }
    }
} 