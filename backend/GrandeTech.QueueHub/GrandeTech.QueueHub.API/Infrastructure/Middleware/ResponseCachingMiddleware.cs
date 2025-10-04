using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Grande.Fila.API.Infrastructure.Middleware
{
    /// <summary>
    /// Middleware to add response caching for GET endpoints
    /// </summary>
    public class ResponseCachingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ResponseCachingMiddleware> _logger;

        public ResponseCachingMiddleware(RequestDelegate next, IMemoryCache cache, ILogger<ResponseCachingMiddleware> logger)
        {
            _next = next;
            _cache = cache;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only cache GET requests to public endpoints
            if (context.Request.Method != "GET" || !IsCacheableEndpoint(context.Request.Path))
            {
                await _next(context);
                return;
            }

            var cacheKey = GenerateCacheKey(context);
            
            // Try to get from cache first
            if (_cache.TryGetValue(cacheKey, out var cachedResponse))
            {
                _logger.LogDebug("Cache hit for {CacheKey}", cacheKey);
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(cachedResponse.ToString() ?? "");
                return;
            }

            // Capture the response
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            // Cache successful responses
            if (context.Response.StatusCode == 200)
            {
                responseBody.Seek(0, SeekOrigin.Begin);
                var responseText = await new StreamReader(responseBody).ReadToEndAsync();
                
                // Cache for different durations based on endpoint type
                var cacheDuration = GetCacheDuration(context.Request.Path);
                _cache.Set(cacheKey, responseText, TimeSpan.FromSeconds(cacheDuration));
                
                _logger.LogDebug("Cached response for {CacheKey} for {Duration}s", cacheKey, cacheDuration);
            }

            // Copy response back to original stream
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
            context.Response.Body = originalBodyStream;
        }

        private static bool IsCacheableEndpoint(PathString path)
        {
            var pathValue = path.Value?.ToLowerInvariant() ?? "";
            return pathValue.StartsWith("/api/public/salons") ||
                   pathValue.StartsWith("/api/public/queue-status");
        }

        private static string GenerateCacheKey(HttpContext context)
        {
            var path = context.Request.Path.Value ?? "";
            var queryString = context.Request.QueryString.Value ?? "";
            return $"cache:{path}:{queryString}";
        }

        private static int GetCacheDuration(PathString path)
        {
            var pathValue = path.Value?.ToLowerInvariant() ?? "";
            
            // Different cache durations for different endpoints
            if (pathValue.Contains("/api/public/salons") && !pathValue.Contains("/queue-status"))
            {
                return 300; // 5 minutes for salon list/details
            }
            else if (pathValue.Contains("/queue-status"))
            {
                return 30; // 30 seconds for queue status (more dynamic)
            }
            
            return 60; // Default 1 minute
        }
    }
}
