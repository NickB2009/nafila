using System.Threading;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrandeTech.QueueHub.API.Application.Staff;
using GrandeTech.QueueHub.API.Infrastructure.Authorization;
using GrandeTech.QueueHub.API.Domain.Users;

namespace GrandeTech.QueueHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StaffController : ControllerBase
    {
        private readonly AddBarberService _addBarberService;
        private readonly UpdateStaffStatusService _updateStaffStatusService;

        public StaffController(AddBarberService addBarberService, UpdateStaffStatusService updateStaffStatusService)
        {
            _addBarberService = addBarberService;
            _updateStaffStatusService = updateStaffStatusService;
        }

        [HttpPost("barbers")]
        [RequireOwner] // UC-ADDBARBER: Admin/Owner can add barbers
        public async Task<ActionResult<AddBarberResult>> AddBarber(
            [FromBody] AddBarberRequest request,
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(TenantClaims.UserId)?.Value ?? throw new UnauthorizedAccessException("User ID not found in claims");
            var userRole = User.FindFirst(TenantClaims.Role)?.Value ?? "User";

            var result = await _addBarberService.AddBarberAsync(request, userId, userRole, cancellationToken);

            if (!result.Success)
            {
                if (result.FieldErrors.Count > 0)
                    return BadRequest(result);
                if (result.Errors.Count > 0)
                    return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPut("barbers/{staffMemberId}/status")]
        [RequireBarber] // UC-STAFFSTATUS: Barber can change their own status
        public async Task<ActionResult<UpdateStaffStatusResult>> UpdateStaffStatus(
            string staffMemberId,
            [FromBody] UpdateStaffStatusRequest request,
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(TenantClaims.UserId)?.Value ?? throw new UnauthorizedAccessException("User ID not found in claims");
            var userRole = User.FindFirst(TenantClaims.Role)?.Value ?? "User";

            // Ensure the request uses the path parameter
            request.StaffMemberId = staffMemberId;

            var result = await _updateStaffStatusService.UpdateStaffStatusAsync(request, userId, cancellationToken);

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