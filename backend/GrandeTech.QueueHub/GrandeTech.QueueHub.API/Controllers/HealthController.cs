using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Grande.Fila.API.Infrastructure.Data;

namespace Grande.Fila.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly QueueHubDbContext _context;
        private readonly ILogger<HealthController> _logger;

        public HealthController(QueueHubDbContext context, ILogger<HealthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                // Check database connectivity
                var canConnect = await _context.Database.CanConnectAsync();
                
                if (!canConnect)
                {
                    _logger.LogWarning("Database connection failed");
                    return StatusCode(503, new { status = "unhealthy", database = "disconnected" });
                }

                // Check if we can query a simple table
                var userCount = await _context.Users.CountAsync();
                
                return Ok(new 
                { 
                    status = "healthy", 
                    database = "connected",
                    timestamp = DateTime.UtcNow,
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                    userCount = userCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(503, new { status = "unhealthy", error = ex.Message });
            }
        }

        [HttpGet("ready")]
        public async Task<IActionResult> Ready()
        {
            try
            {
                // More comprehensive readiness check
                var canConnect = await _context.Database.CanConnectAsync();
                var migrations = await _context.Database.GetAppliedMigrationsAsync();
                
                return Ok(new 
                { 
                    status = "ready", 
                    database = canConnect ? "connected" : "disconnected",
                    migrationsApplied = migrations.Count(),
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Readiness check failed");
                return StatusCode(503, new { status = "not ready", error = ex.Message });
            }
        }
    }
} 