using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace Grande.Fila.API.Infrastructure.Middleware
{
    /// <summary>
    /// Middleware to add correlation IDs for request tracking
    /// </summary>
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationIdHeaderName = "X-Correlation-ID";

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Get correlation ID from request header or generate a new one
            var correlationId = context.Request.Headers[CorrelationIdHeaderName].FirstOrDefault() 
                               ?? Guid.NewGuid().ToString();

            // Add correlation ID to response headers
            context.Response.Headers[CorrelationIdHeaderName] = correlationId;

            // Add correlation ID to the current activity for distributed tracing
            Activity.Current?.SetTag("correlation.id", correlationId);

            // Store correlation ID in HttpContext for use by other middleware/services
            context.Items["CorrelationId"] = correlationId;

            await _next(context);
        }
    }
}
