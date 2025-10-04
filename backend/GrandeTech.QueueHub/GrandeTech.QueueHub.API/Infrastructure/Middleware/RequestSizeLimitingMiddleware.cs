using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Grande.Fila.API.Infrastructure.Middleware
{
    /// <summary>
    /// Middleware to limit request size for public endpoints
    /// </summary>
    public class RequestSizeLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestSizeLimitingMiddleware> _logger;
        private const int MaxRequestSizeBytes = 1024 * 1024; // 1MB limit for public endpoints

        public RequestSizeLimitingMiddleware(RequestDelegate next, ILogger<RequestSizeLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only apply size limits to public endpoints
            if (!IsPublicEndpoint(context.Request.Path))
            {
                await _next(context);
                return;
            }

            // Check content length
            if (context.Request.ContentLength.HasValue && context.Request.ContentLength > MaxRequestSizeBytes)
            {
                _logger.LogWarning("Request size limit exceeded. ContentLength: {ContentLength}, Limit: {Limit}", 
                    context.Request.ContentLength, MaxRequestSizeBytes);
                
                context.Response.StatusCode = 413; // Payload Too Large
                context.Response.ContentType = "application/json";
                
                var response = new
                {
                    error = "Request too large",
                    message = $"Request size exceeds the maximum allowed size of {MaxRequestSizeBytes} bytes",
                    maxSizeBytes = MaxRequestSizeBytes
                };
                
                await context.Response.WriteAsJsonAsync(response);
                return;
            }

            // For requests without Content-Length header, we need to check the body
            if (context.Request.ContentLength == null && context.Request.Method != "GET")
            {
                var originalBodyStream = context.Response.Body;
                using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;

                // Read request body to check size
                using var memoryStream = new MemoryStream();
                await context.Request.Body.CopyToAsync(memoryStream);
                var requestBody = memoryStream.ToArray();

                if (requestBody.Length > MaxRequestSizeBytes)
                {
                    _logger.LogWarning("Request body size limit exceeded. BodyLength: {BodyLength}, Limit: {Limit}", 
                        requestBody.Length, MaxRequestSizeBytes);
                    
                    context.Response.StatusCode = 413; // Payload Too Large
                    context.Response.ContentType = "application/json";
                    
                    var response = new
                    {
                        error = "Request too large",
                        message = $"Request body size exceeds the maximum allowed size of {MaxRequestSizeBytes} bytes",
                        maxSizeBytes = MaxRequestSizeBytes
                    };
                    
                    await context.Response.WriteAsJsonAsync(response);
                    return;
                }

                // Reset request body stream for next middleware
                context.Request.Body = new MemoryStream(requestBody);

                // Continue with the request
                await _next(context);

                // Copy response back to original stream
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
                context.Response.Body = originalBodyStream;
            }
            else
            {
                await _next(context);
            }
        }

        private static bool IsPublicEndpoint(PathString path)
        {
            var pathValue = path.Value?.ToLowerInvariant() ?? "";
            return pathValue.StartsWith("/api/public/");
        }
    }
}
