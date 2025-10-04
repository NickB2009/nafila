using Grande.Fila.API.Application.Logging;
using System.Diagnostics;

namespace Grande.Fila.API.Infrastructure.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILoggingService _loggingService;

    public RequestLoggingMiddleware(RequestDelegate next, ILoggingService loggingService)
    {
        _next = next;
        _loggingService = loggingService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;

        try
        {
            // Log the incoming request
            var userId = context.User?.Identity?.IsAuthenticated == true 
                ? context.User.FindFirst("sub")?.Value 
                : null;
            
            var organizationId = context.User?.FindFirst("org")?.Value;
            var locationId = context.User?.FindFirst("loc")?.Value;

            var correlationId = context.Items["CorrelationId"]?.ToString();

            _loggingService.LogApiRequest(
                context.Request.Method,
                context.Request.Path,
                userId,
                organizationId,
                locationId,
                correlationId
            );

            // Process the request
            await _next(context);

            // Log the response
            stopwatch.Stop();

            _loggingService.LogApiResponse(
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                userId,
                correlationId
            );
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            // Log the error
            _loggingService.LogError(
                "Unhandled exception in request pipeline",
                ex,
                context.Request.Method,
                context.Request.Path
            );

            // Set error response
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            
            var errorResponse = new
            {
                error = "Internal Server Error",
                message = "An unexpected error occurred",
                timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsJsonAsync(errorResponse);
        }
    }
}

// Extension method for easy registration
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
} 