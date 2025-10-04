using Microsoft.AspNetCore.Http;

namespace Grande.Fila.API.Infrastructure.Middleware
{
    /// <summary>
    /// Middleware to add security headers to all responses
    /// </summary>
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Add security headers
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Response.Headers["X-Frame-Options"] = "DENY";
            context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";

            // Add Content Security Policy for public endpoints
            if (IsPublicEndpoint(context.Request.Path))
            {
                context.Response.Headers["Content-Security-Policy"] = 
                    "default-src 'self'; " +
                    "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                    "style-src 'self' 'unsafe-inline'; " +
                    "img-src 'self' data: https:; " +
                    "font-src 'self' data:; " +
                    "connect-src 'self' wss: ws:; " +
                    "frame-ancestors 'none';";
            }

            await _next(context);
        }

        private static bool IsPublicEndpoint(PathString path)
        {
            var pathValue = path.Value?.ToLowerInvariant() ?? "";
            return pathValue.StartsWith("/api/public/") || 
                   pathValue.StartsWith("/swagger") ||
                   pathValue.StartsWith("/health");
        }
    }
}
