using Microsoft.AspNetCore.Mvc;
using System;

namespace GrandeTech.QueueHub.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Version = "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
            });
        }

        [HttpGet("ready")]
        public IActionResult Ready()
        {
            // Add any readiness checks here (database connectivity, external services, etc.)
            return Ok(new
            {
                Status = "Ready",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("live")]
        public IActionResult Live()
        {
            // Simple liveness check
            return Ok(new
            {
                Status = "Alive",
                Timestamp = DateTime.UtcNow
            });
        }
    }
} 