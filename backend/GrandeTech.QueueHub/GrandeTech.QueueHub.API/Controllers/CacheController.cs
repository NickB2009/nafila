using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Grande.Fila.API.Application.Services;

namespace Grande.Fila.API.Controllers
{
    [ApiController]
    [Route("api/cache")] // e.g., PUT /api/cache/wait-time/locations/{id}
    public class CacheController : ControllerBase
    {
        private readonly UpdateCacheService _updateCacheService;

        public CacheController(UpdateCacheService updateCacheService)
        {
            _updateCacheService = updateCacheService;
        }

        public class UpdateAverageRequestDto
        {
            public double AverageServiceTimeMinutes { get; set; }
        }

        public class UpdateAverageResponseDto
        {
            public bool Success { get; set; }
            public string[] Errors { get; set; } = new string[0];
        }

        // UC-UPDATECACHE
        [HttpPut("wait-time/locations/{locationId}")]
        [Authorize(Policy = "RequireAdmin")] // could also allow ServiceAccount
        public async Task<IActionResult> UpdateWaitAverage(string locationId, [FromBody] UpdateAverageRequestDto dto, CancellationToken cancellationToken)
        {
            var request = new UpdateCacheRequest
            {
                LocationId = locationId,
                AverageServiceTimeMinutes = dto.AverageServiceTimeMinutes
            };

            var result = await _updateCacheService.ExecuteAsync(request, User.Identity?.Name ?? "system", cancellationToken);

            if (!result.Success)
            {
                return BadRequest(new UpdateAverageResponseDto
                {
                    Success = false,
                    Errors = result.Errors.Count > 0 ? result.Errors.ToArray() : new[] { "Validation errors" }
                });
            }

            return Ok(new UpdateAverageResponseDto { Success = true });
        }
    }
} 