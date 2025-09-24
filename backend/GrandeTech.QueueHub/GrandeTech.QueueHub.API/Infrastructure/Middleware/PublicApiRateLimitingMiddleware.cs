using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Grande.Fila.API.Infrastructure.Middleware
{
    /// <summary>
    /// Rate limiting middleware specifically for public APIs to prevent abuse
    /// </summary>
    public class PublicApiRateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PublicApiRateLimitingMiddleware> _logger;
        private readonly int _requestsPerMinute;
        private readonly ConcurrentDictionary<string, ClientRequestInfo> _clientRequests = new();

        public PublicApiRateLimitingMiddleware(
            RequestDelegate next,
            ILogger<PublicApiRateLimitingMiddleware> logger,
            IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _requestsPerMinute = configuration.GetValue<int>("RateLimiting:PublicApiRequestsPerMinute", 60);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant();
            
            // Only apply rate limiting to public API endpoints
            if (path != null && (path.StartsWith("/api/public") || path.StartsWith("/api/kiosk")))
            {
                var clientId = GetClientIdentifier(context);
                var now = DateTime.UtcNow;

                if (!IsRequestAllowed(clientId, now))
                {
                    _logger.LogWarning("Rate limit exceeded for client {ClientId} on path {Path}", clientId, path);
                    context.Response.StatusCode = 429; // Too Many Requests
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"error\":\"Rate limit exceeded. Please try again later.\"}");
                    return;
                }
            }

            await _next(context);
        }

        private string GetClientIdentifier(HttpContext context)
        {
            // Use IP address as primary identifier
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            
            // For authenticated requests, also consider user ID
            var userId = context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(userId))
            {
                return $"{ipAddress}:{userId}";
            }

            return ipAddress;
        }

        private bool IsRequestAllowed(string clientId, DateTime now)
        {
            var clientInfo = _clientRequests.GetOrAdd(clientId, _ => new ClientRequestInfo());

            lock (clientInfo)
            {
                // Remove old requests (older than 1 minute)
                clientInfo.Requests.RemoveAll(r => r < now.AddMinutes(-1));

                // Check if under limit
                if (clientInfo.Requests.Count >= _requestsPerMinute)
                {
                    return false;
                }

                // Add current request
                clientInfo.Requests.Add(now);
                return true;
            }
        }

        private class ClientRequestInfo
        {
            public List<DateTime> Requests { get; } = new();
        }
    }
}


