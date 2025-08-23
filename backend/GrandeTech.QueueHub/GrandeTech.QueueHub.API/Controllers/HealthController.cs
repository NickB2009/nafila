using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Grande.Fila.API.Infrastructure.Data;

namespace Grande.Fila.API.Controllers
{
    /// <summary>
    /// Health check controller for monitoring system health and dependencies
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;
        private readonly QueueHubDbContext _dbContext;
        private readonly HealthCheckService _healthCheckService;

        public HealthController(
            ILogger<HealthController> logger,
            QueueHubDbContext dbContext,
            HealthCheckService healthCheckService)
        {
            _logger = logger;
            _dbContext = dbContext;
            _healthCheckService = healthCheckService;
        }

        /// <summary>
        /// Basic health check endpoint
        /// </summary>
        [HttpGet]
        public IActionResult Get()
        {
            var healthStatus = new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Version = GetType().Assembly.GetName().Version?.ToString() ?? "Unknown",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
                MachineName = Environment.MachineName,
                ProcessId = Environment.ProcessId,
                WorkingSet = GC.GetTotalMemory(false) / 1024 / 1024, // MB
                Uptime = Process.GetCurrentProcess().TotalProcessorTime.TotalSeconds
            };

            return Ok(healthStatus);
        }

        /// <summary>
        /// Detailed health check with all registered health checks
        /// </summary>
        [HttpGet("detailed")]
        public async Task<IActionResult> GetDetailed(CancellationToken cancellationToken = default)
        {
            try
            {
                var report = await _healthCheckService.CheckHealthAsync(cancellationToken: cancellationToken);
                
                var healthStatus = new
                {
                    Status = report.Status.ToString(),
                    Timestamp = DateTime.UtcNow,
                    TotalChecks = report.Entries.Count,
                    HealthyChecks = report.Entries.Count(e => e.Value.Status == HealthStatus.Healthy),
                    DegradedChecks = report.Entries.Count(e => e.Value.Status == HealthStatus.Degraded),
                    UnhealthyChecks = report.Entries.Count(e => e.Value.Status == HealthStatus.Unhealthy),
                    Checks = report.Entries.Select(e => new
                    {
                        Name = e.Key,
                        Status = e.Value.Status.ToString(),
                        Description = e.Value.Description,
                        Duration = e.Value.Duration.TotalMilliseconds,
                        Tags = e.Value.Tags,
                        Data = e.Value.Data
                    }).ToList()
                };

                var statusCode = report.Status == HealthStatus.Healthy ? 200 : 
                               report.Status == HealthStatus.Degraded ? 200 : 503;

                return StatusCode(statusCode, healthStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during detailed health check");
                return StatusCode(503, new
                {
                    Status = "Unhealthy",
                    Timestamp = DateTime.UtcNow,
                    Error = "Health check service unavailable",
                    Details = ex.Message
                });
            }
        }

        /// <summary>
        /// Database connectivity health check
        /// </summary>
        [HttpGet("database")]
        public async Task<IActionResult> GetDatabaseHealth(CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                // Test database connectivity
                var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
                stopwatch.Stop();

                if (canConnect)
                {
                    // Test a simple query
                    var queueCount = await _dbContext.Queues.CountAsync(cancellationToken);
                    
                    var healthStatus = new
                    {
                        Status = "Healthy",
                        Timestamp = DateTime.UtcNow,
                        Database = "Connected",
                        ResponseTime = stopwatch.ElapsedMilliseconds,
                        QueueCount = queueCount,
                        ConnectionString = _dbContext.Database.GetConnectionString()?.Substring(0, Math.Min(50, _dbContext.Database.GetConnectionString()?.Length ?? 0)) + "..."
                    };

                    return Ok(healthStatus);
                }
                else
                {
                    return StatusCode(503, new
                    {
                        Status = "Unhealthy",
                        Timestamp = DateTime.UtcNow,
                        Database = "Cannot connect",
                        ResponseTime = stopwatch.ElapsedMilliseconds,
                        Error = "Database connection failed"
                    });
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Database health check failed");
                
                return StatusCode(503, new
                {
                    Status = "Unhealthy",
                    Timestamp = DateTime.UtcNow,
                    Database = "Error",
                    ResponseTime = stopwatch.ElapsedMilliseconds,
                    Error = ex.Message,
                    ExceptionType = ex.GetType().Name
                });
            }
        }

        /// <summary>
        /// System resources health check
        /// </summary>
        [HttpGet("system")]
        public IActionResult GetSystemHealth()
        {
            try
            {
                var process = Process.GetCurrentProcess();
                var gcInfo = GC.GetGCMemoryInfo();
                
                var systemHealth = new
                {
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow,
                    Process = new
                    {
                        Id = process.Id,
                        Name = process.ProcessName,
                        StartTime = process.StartTime,
                        TotalProcessorTime = process.TotalProcessorTime.TotalSeconds,
                        UserProcessorTime = process.UserProcessorTime.TotalSeconds
                    },
                    Memory = new
                    {
                        WorkingSet = process.WorkingSet64 / 1024 / 1024, // MB
                        PrivateMemory = process.PrivateMemorySize64 / 1024 / 1024, // MB
                        VirtualMemory = process.VirtualMemorySize64 / 1024 / 1024, // MB
                        GCTotalMemory = GC.GetTotalMemory(false) / 1024 / 1024, // MB
                        GCMaxGeneration = GC.MaxGeneration,
                        GCMemoryInfo = new
                        {
                            HeapSizeBytes = gcInfo.HeapSizeBytes / 1024 / 1024, // MB
                            FragmentedBytes = gcInfo.FragmentedBytes / 1024 / 1024, // MB
                                                    GenerationInfo = new[]
                        {
                            new { Generation = 0, SizeBytes = 0, FragmentationBytes = 0 },
                            new { Generation = 1, SizeBytes = 0, FragmentationBytes = 0 },
                            new { Generation = 2, SizeBytes = 0, FragmentationBytes = 0 }
                        }
                        }
                    },
                    Threads = new
                    {
                        Count = process.Threads.Count,
                        MaxWorkerThreads = 0,
                        MaxCompletionPortThreads = 0,
                        AvailableWorkerThreads = 0,
                        AvailableCompletionPortThreads = 0
                    },
                    Environment = new
                    {
                        ProcessorCount = Environment.ProcessorCount,
                        Is64BitProcess = Environment.Is64BitProcess,
                        Is64BitOperatingSystem = Environment.Is64BitOperatingSystem,
                        OSVersion = Environment.OSVersion.ToString(),
                        FrameworkDescription = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
                        RuntimeIdentifier = System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier
                    }
                };

                return Ok(systemHealth);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "System health check failed");
                
                return StatusCode(503, new
                {
                    Status = "Unhealthy",
                    Timestamp = DateTime.UtcNow,
                    Error = "System health check failed",
                    Details = ex.Message
                });
            }
        }

        /// <summary>
        /// Queue system specific health check
        /// </summary>
        [HttpGet("queues")]
        public async Task<IActionResult> GetQueueSystemHealth(CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                // Check queue system health
                var activeQueues = await _dbContext.Queues
                    .Where(q => q.IsActive)
                    .CountAsync(cancellationToken);

                var totalQueueEntries = await _dbContext.QueueEntries
                    .Where(e => e.Status == Domain.Queues.QueueEntryStatus.Waiting || 
                               e.Status == Domain.Queues.QueueEntryStatus.Called)
                    .CountAsync(cancellationToken);

                var staffMembers = await _dbContext.StaffMembers
                    .Where(s => s.IsActive)
                    .CountAsync(cancellationToken);

                stopwatch.Stop();

                var queueHealth = new
                {
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow,
                    ResponseTime = stopwatch.ElapsedMilliseconds,
                    Metrics = new
                    {
                        ActiveQueues = activeQueues,
                        TotalQueueEntries = totalQueueEntries,
                        ActiveStaffMembers = staffMembers,
                        AverageQueueLength = activeQueues > 0 ? totalQueueEntries / activeQueues : 0
                    },
                    SystemStatus = new
                    {
                        QueuesAvailable = activeQueues > 0,
                        StaffAvailable = staffMembers > 0,
                        SystemResponsive = stopwatch.ElapsedMilliseconds < 1000 // Less than 1 second
                    }
                };

                var isHealthy = activeQueues > 0 && staffMembers > 0 && stopwatch.ElapsedMilliseconds < 1000;
                var statusCode = isHealthy ? 200 : 503;

                return StatusCode(statusCode, queueHealth);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Queue system health check failed");
                
                return StatusCode(503, new
                {
                    Status = "Unhealthy",
                    Timestamp = DateTime.UtcNow,
                    ResponseTime = stopwatch.ElapsedMilliseconds,
                    Error = "Queue system health check failed",
                    Details = ex.Message
                });
            }
        }

        /// <summary>
        /// Ping endpoint for basic connectivity testing
        /// </summary>
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new
            {
                Message = "Pong",
                Timestamp = DateTime.UtcNow,
                ServerTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
            });
        }

        /// <summary>
        /// Readiness probe for Kubernetes/container orchestration
        /// </summary>
        [HttpGet("ready")]
        public async Task<IActionResult> Ready(CancellationToken cancellationToken = default)
        {
            try
            {
                // Check if the application is ready to receive traffic
                var dbReady = await _dbContext.Database.CanConnectAsync(cancellationToken);
                
                if (dbReady)
                {
                    return Ok(new
                    {
                        Status = "Ready",
                        Timestamp = DateTime.UtcNow,
                        Database = "Connected",
                        Application = "Running"
                    });
                }
                else
                {
                    return StatusCode(503, new
                    {
                        Status = "Not Ready",
                        Timestamp = DateTime.UtcNow,
                        Database = "Not Connected",
                        Application = "Running"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Readiness check failed");
                
                return StatusCode(503, new
                {
                    Status = "Not Ready",
                    Timestamp = DateTime.UtcNow,
                    Error = "Readiness check failed",
                    Details = ex.Message
                });
            }
        }

        /// <summary>
        /// Liveness probe for Kubernetes/container orchestration
        /// </summary>
        [HttpGet("live")]
        public IActionResult Live()
        {
            // Simple check if the application is alive
            return Ok(new
            {
                Status = "Alive",
                Timestamp = DateTime.UtcNow,
                ProcessId = Environment.ProcessId,
                Uptime = Process.GetCurrentProcess().TotalProcessorTime.TotalSeconds
            });
        }
    }
} 