using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;


namespace Grande.Fila.API.Infrastructure.Middleware
{
    /// <summary>
    /// Global exception handler middleware for consistent error handling across the API
    /// </summary>
    public class GlobalExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            var (statusCode, errorResponse) = GetErrorResponse(exception);
            context.Response.StatusCode = statusCode;

            var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            await context.Response.WriteAsync(jsonResponse);
        }

        private (int statusCode, ErrorResponse errorResponse) GetErrorResponse(Exception exception)
        {
            return exception switch
            {
                ArgumentException argEx => (
                    (int)HttpStatusCode.BadRequest,
                    new ErrorResponse
                    {
                        Error = "Validation Error",
                        Message = argEx.Message,
                        Details = new[] { argEx.ParamName ?? "Parameter" },
                        Timestamp = DateTime.UtcNow,
                        TraceId = Guid.NewGuid().ToString()
                    }
                ),
                
                InvalidOperationException invOpEx => (
                    (int)HttpStatusCode.BadRequest,
                    new ErrorResponse
                    {
                        Error = "Invalid Operation",
                        Message = invOpEx.Message,
                        Details = new[] { "The requested operation cannot be performed" },
                        Timestamp = DateTime.UtcNow,
                        TraceId = Guid.NewGuid().ToString()
                    }
                ),
                
                UnauthorizedAccessException unauthEx => (
                    (int)HttpStatusCode.Unauthorized,
                    new ErrorResponse
                    {
                        Error = "Unauthorized",
                        Message = "Access denied",
                        Details = new[] { unauthEx.Message },
                        Timestamp = DateTime.UtcNow,
                        TraceId = Guid.NewGuid().ToString()
                    }
                ),
                
                KeyNotFoundException keyNotFoundEx => (
                    (int)HttpStatusCode.NotFound,
                    new ErrorResponse
                    {
                        Error = "Not Found",
                        Message = keyNotFoundEx.Message,
                        Details = new[] { "The requested resource was not found" },
                        Timestamp = DateTime.UtcNow,
                        TraceId = Guid.NewGuid().ToString()
                    }
                ),
                
                TimeoutException timeoutEx => (
                    (int)HttpStatusCode.RequestTimeout,
                    new ErrorResponse
                    {
                        Error = "Request Timeout",
                        Message = "The operation timed out",
                        Details = new[] { timeoutEx.Message },
                        Timestamp = DateTime.UtcNow,
                        TraceId = Guid.NewGuid().ToString()
                    }
                ),
                
                _ => (
                    (int)HttpStatusCode.InternalServerError,
                    new ErrorResponse
                    {
                        Error = "Internal Server Error",
                        Message = "An unexpected error occurred",
                        Details = new[] { "Please try again later or contact support" },
                        Timestamp = DateTime.UtcNow,
                        TraceId = Guid.NewGuid().ToString()
                    }
                )
            };
        }
    }

    /// <summary>
    /// Standardized error response model
    /// </summary>
    public class ErrorResponse
    {
        public string Error { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string[] Details { get; set; } = Array.Empty<string>();
        public DateTime Timestamp { get; set; }
        public string TraceId { get; set; } = string.Empty;
    }
}
