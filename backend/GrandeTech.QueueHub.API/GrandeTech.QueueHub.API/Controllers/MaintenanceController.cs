using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Grande.Fila.API.Application.Locations;

namespace Grande.Fila.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaintenanceController : ControllerBase
    {
        private readonly ResetAverageService _resetAverageService;

        public MaintenanceController(ResetAverageService resetAverageService)
        {
            _resetAverageService = resetAverageService;
        }

        public class ResetAverageResponse
        {
            public bool Success { get; set; }
            public int ResetCount { get; set; }
            public string[] Errors { get; set; } = new string[0];
        }

        // UC-RESETAVG: Reset wait averages every 3 months
        [HttpPost("reset-averages")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> ResetAverages(CancellationToken cancellationToken)
        {
            var result = await _resetAverageService.ExecuteAsync(new ResetAverageRequest(), User.Identity?.Name ?? "system", cancellationToken);

            if (!result.Success)
            {
                return StatusCode(500, new ResetAverageResponse
                {
                    Success = false,
                    ResetCount = result.ResetCount,
                    Errors = result.Errors.ToArray()
                });
            }

            return Ok(new ResetAverageResponse
            {
                Success = true,
                ResetCount = result.ResetCount
            });
        }
    }
} 