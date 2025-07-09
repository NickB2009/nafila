using System.Threading;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Grande.Fila.API.Application.Staff;
using Grande.Fila.API.Infrastructure.Authorization;
using Grande.Fila.API.Domain.Users;

namespace Grande.Fila.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StaffController : ControllerBase
    {
        private readonly AddBarberService _addBarberService;
        private readonly EditBarberService _editBarberService;
        private readonly UpdateStaffStatusService _updateStaffStatusService;
        private readonly StartBreakService _startBreakService;
        private readonly EndBreakService _endBreakService;

        public StaffController(
            AddBarberService addBarberService, 
            EditBarberService editBarberService,
            UpdateStaffStatusService updateStaffStatusService, 
            StartBreakService startBreakService, 
            EndBreakService endBreakService)
        {
            _addBarberService = addBarberService;
            _editBarberService = editBarberService;
            _updateStaffStatusService = updateStaffStatusService;
            _startBreakService = startBreakService;
            _endBreakService = endBreakService;
        }

        [HttpPost("barbers")]
        [RequireOwner]
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

        [HttpPut("barbers/{staffMemberId}")]
        [RequireOwner] // UC-EDITBARBER: Admin/Owner can edit barbers
        public async Task<ActionResult<EditBarberResult>> EditBarber(
            string staffMemberId,
            [FromBody] EditBarberRequest request,
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(TenantClaims.UserId)?.Value ?? throw new UnauthorizedAccessException("User ID not found in claims");
            var userRole = User.FindFirst(TenantClaims.Role)?.Value ?? "User";

            // Ensure the request uses the path parameter
            request.StaffMemberId = staffMemberId;

            var result = await _editBarberService.EditBarberAsync(request, userId, userRole, cancellationToken);

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

        [HttpPut("barbers/{staffMemberId}/start-break")]
        [RequireBarber] // UC-STARTBREAK: Barber can start a break
        public async Task<ActionResult<StartBreakResult>> StartBreak(
            string staffMemberId,
            [FromBody] StartBreakRequest request,
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(TenantClaims.UserId)?.Value ?? throw new UnauthorizedAccessException("User ID not found in claims");
            request.StaffMemberId = staffMemberId;
            var result = await _startBreakService.StartBreakAsync(request, userId, cancellationToken);
            if (!result.Success)
            {
                if (result.FieldErrors.Count > 0)
                    return BadRequest(result);
                if (result.Errors.Count > 0)
                    return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPut("barbers/{staffMemberId}/end-break")]
        [RequireBarber] // UC-ENDBREAK: Barber can end a break
        public async Task<ActionResult<EndBreakResult>> EndBreak(
            string staffMemberId,
            [FromBody] EndBreakRequest request,
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(TenantClaims.UserId)?.Value ?? throw new UnauthorizedAccessException("User ID not found in claims");
            request.StaffMemberId = staffMemberId;
            var result = await _endBreakService.EndBreakAsync(request, userId, cancellationToken);
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