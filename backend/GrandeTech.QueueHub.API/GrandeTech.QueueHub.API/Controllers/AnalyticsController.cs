using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Grande.Fila.API.Application.Services;
using Grande.Fila.API.Application.Analytics;
using Grande.Fila.API.Infrastructure.Authorization;
using Grande.Fila.API.Domain.Users;

namespace Grande.Fila.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly CalculateWaitService _calculateWaitService;
        private readonly AnalyticsService _analyticsService;

        public AnalyticsController(CalculateWaitService calculateWaitService, AnalyticsService analyticsService)
        {
            _calculateWaitService = calculateWaitService;
            _analyticsService = analyticsService;
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

        // UC-ANALYTICS: Cross-barbershop analytics for platform administrators
        [HttpPost("cross-barbershop")]
        [RequireAdmin] // Platform administrators only
        public async Task<IActionResult> GetCrossBarbershopAnalytics([FromBody] CrossBarbershopAnalyticsRequest request, CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(TenantClaims.UserId)?.Value ?? "anonymous";
            var userRole = User.FindFirst(TenantClaims.Role)?.Value ?? "Client";

            var result = await _analyticsService.GetCrossBarbershopAnalyticsAsync(request, userId, userRole, cancellationToken);

            if (!result.Success)
            {
                if (result.FieldErrors.Count > 0)
                    return BadRequest(result);
                if (result.Errors.Count > 0)
                    return BadRequest(result);
            }

            return Ok(result);
        }

        // UC-ANALYTICS: Organization-specific analytics for owners/admins
        [HttpPost("organization")]
        [RequireOwner] // Organization owners/admins can view their own analytics
        public async Task<IActionResult> GetOrganizationAnalytics([FromBody] OrganizationAnalyticsRequest request, CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(TenantClaims.UserId)?.Value ?? "anonymous";
            var userRole = User.FindFirst(TenantClaims.Role)?.Value ?? "Client";

            var result = await _analyticsService.GetOrganizationAnalyticsAsync(request, userId, userRole, cancellationToken);

            if (!result.Success)
            {
                if (result.FieldErrors.Count > 0)
                    return BadRequest(result);
                if (result.Errors.Count > 0)
                    return BadRequest(result);
            }

            return Ok(result);
        }

        // UC-ANALYTICS: Top performing organizations for platform administrators
        [HttpPost("top-organizations")]
        [RequireAdmin] // Platform administrators only
        public async Task<IActionResult> GetTopPerformingOrganizations([FromBody] TopPerformingOrganizationsRequest request, CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(TenantClaims.UserId)?.Value ?? "anonymous";
            var userRole = User.FindFirst(TenantClaims.Role)?.Value ?? "Client";

            var result = await _analyticsService.GetTopPerformingOrganizationsAsync(request, userId, userRole, cancellationToken);

            if (!result.Success)
            {
                if (result.FieldErrors.Count > 0)
                    return BadRequest(result);
                if (result.Errors.Count > 0)
                    return BadRequest(result);
            }

            return Ok(result);
        }
    }
} 