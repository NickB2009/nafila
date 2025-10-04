using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic; // Added missing import

namespace Grande.Fila.API.Infrastructure.Middleware
{
    /// <summary>
    /// Rate limiting middleware for queue operations
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly RateLimitingOptions _options;
        private readonly ConcurrentDictionary<string, RateLimitInfo> _rateLimitStore;

        public RateLimitingMiddleware(
            RequestDelegate next,
            ILogger<RateLimitingMiddleware> logger,
            IOptions<RateLimitingOptions> options)
        {
            _next = next;
            _logger = logger;
            _options = options.Value;
            _rateLimitStore = new ConcurrentDictionary<string, RateLimitInfo>();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientId = GetClientIdentifier(context);
            var endpoint = context.Request.Path.Value;

            // Check if this endpoint should be rate limited
            if (ShouldRateLimit(endpoint))
            {
                if (!await CheckRateLimit(clientId, endpoint))
                {
                    _logger.LogWarning("Rate limit exceeded for client {ClientId} on endpoint {Endpoint}", 
                        clientId, endpoint);
                    
                    context.Response.StatusCode = 429; // Too Many Requests
                    context.Response.ContentType = "application/json";
                    
                    var response = new
                    {
                        Error = "Rate limit exceeded",
                        Message = "Too many requests. Please try again later.",
                        RetryAfter = _options.WindowMinutes * 60, // seconds
                        Timestamp = DateTime.UtcNow
                    };
                    
                    await context.Response.WriteAsJsonAsync(response);
                    return;
                }
            }

            await _next(context);
        }

        private string GetClientIdentifier(HttpContext context)
        {
            // Try to get client IP address
            var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            
            // If behind a proxy, try to get the real IP
            if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
            {
                clientIp = forwardedFor.ToString().Split(',')[0].Trim();
            }
            else if (context.Request.Headers.TryGetValue("X-Real-IP", out var realIp))
            {
                clientIp = realIp.ToString();
            }

            return clientIp;
        }

        private bool ShouldRateLimit(string endpoint)
        {
            if (string.IsNullOrEmpty(endpoint)) return false;

            // Rate limit different endpoint types with different limits
            return endpoint.Contains("/api/Public/queue/join") ||
                   endpoint.Contains("/api/Public/queue/leave") ||
                   endpoint.Contains("/api/Public/queue/update") ||
                   endpoint.Contains("/api/Public/queue/entry-status") ||
                   endpoint.Contains("/api/Public/salons/") ||
                   endpoint.Contains("/api/Public/queue-status/");
        }

        private async Task<bool> CheckRateLimit(string clientId, string endpoint)
        {
            var key = $"{clientId}:{endpoint}";
            var now = DateTime.UtcNow;

            // Clean up expired entries
            CleanupExpiredEntries();

            // Get endpoint-specific rate limits
            var (maxRequests, windowMinutes) = GetEndpointRateLimit(endpoint);

            // Get or create rate limit info for this client/endpoint
            var rateLimitInfo = _rateLimitStore.GetOrAdd(key, _ => new RateLimitInfo
            {
                FirstRequest = now,
                RequestCount = 0
            });

            // Check if we're in a new time window
            if (now.Subtract(rateLimitInfo.FirstRequest).TotalMinutes >= windowMinutes)
            {
                rateLimitInfo.FirstRequest = now;
                rateLimitInfo.RequestCount = 0;
            }

            // Increment request count
            rateLimitInfo.RequestCount++;

            // Check if limit exceeded
            if (rateLimitInfo.RequestCount > maxRequests)
            {
                return false;
            }

            return true;
        }

        private (int maxRequests, int windowMinutes) GetEndpointRateLimit(string endpoint)
        {
            // Different rate limits for different endpoint types
            if (endpoint.Contains("/api/Public/queue/join"))
            {
                return (5, 1); // 5 join requests per minute
            }
            else if (endpoint.Contains("/api/Public/queue/leave") || endpoint.Contains("/api/Public/queue/update"))
            {
                return (10, 1); // 10 leave/update requests per minute
            }
            else if (endpoint.Contains("/api/Public/queue/entry-status"))
            {
                return (30, 1); // 30 status checks per minute
            }
            else if (endpoint.Contains("/api/Public/salons/") || endpoint.Contains("/api/Public/queue-status/"))
            {
                return (60, 1); // 60 salon/queue status requests per minute
            }
            else
            {
                return (_options.MaxRequestsPerWindow, _options.WindowMinutes); // Default limits
            }
        }

        private void CleanupExpiredEntries()
        {
            var cutoff = DateTime.UtcNow.AddMinutes(-_options.WindowMinutes);
            var expiredKeys = new List<string>();

            foreach (var kvp in _rateLimitStore)
            {
                if (kvp.Value.FirstRequest < cutoff)
                {
                    expiredKeys.Add(kvp.Key);
                }
            }

            foreach (var key in expiredKeys)
            {
                _rateLimitStore.TryRemove(key, out _);
            }
        }
    }

    /// <summary>
    /// Rate limiting configuration options
    /// </summary>
    public class RateLimitingOptions
    {
        public int MaxRequestsPerWindow { get; set; } = 10; // 10 requests per window
        public int WindowMinutes { get; set; } = 1; // 1 minute window
    }

    /// <summary>
    /// Rate limit information for a client/endpoint combination
    /// </summary>
    public class RateLimitInfo
    {
        public DateTime FirstRequest { get; set; }
        public int RequestCount { get; set; }
    }
}
