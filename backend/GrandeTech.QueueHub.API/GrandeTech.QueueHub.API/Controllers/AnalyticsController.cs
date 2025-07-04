using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Grande.Fila.API.Application.Services;

namespace Grande.Fila.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly CalculateWaitService _calculateWaitService;

        public AnalyticsController(CalculateWaitService calculateWaitService)
        {
            _calculateWaitService = calculateWaitService;
        }

        public class CalculateWaitRequestDto
        {
            public string LocationId { get; set; } = string.Empty;
            public string QueueId { get; set; } = string.Empty;
            public string EntryId { get; set; } = string.Empty;
        }

        public class CalculateWaitResponseDto
        {
            public bool Success { get; set; }
            public int EstimatedWaitMinutes { get; set; }
            public string[] Errors { get; set; } = new string[0];
        }

        // UC-CALCWAIT: Calculate estimated wait time
        [HttpPost("calculate-wait")]
        [Authorize(Policy = "RequireServiceAccount")]
        public async Task<IActionResult> CalculateWaitTime([FromBody] CalculateWaitRequestDto dto, CancellationToken cancellationToken)
        {
            var request = new CalculateWaitRequest
            {
                LocationId = dto.LocationId,
                QueueId = dto.QueueId,
                EntryId = dto.EntryId
            };

            var result = await _calculateWaitService.ExecuteAsync(request, User.Identity?.Name ?? "system", cancellationToken);

            if (!result.Success)
            {
                return BadRequest(new CalculateWaitResponseDto
                {
                    Success = false,
                    EstimatedWaitMinutes = -1,
                    Errors = result.Errors.Count > 0 ? result.Errors.ToArray() : new[] { "Validation errors" }
                });
            }

            return Ok(new CalculateWaitResponseDto
            {
                Success = true,
                EstimatedWaitMinutes = result.EstimatedWaitMinutes
            });
        }
    }
} 