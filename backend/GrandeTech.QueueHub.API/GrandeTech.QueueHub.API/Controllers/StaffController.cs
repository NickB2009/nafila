using System.Threading;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrandeTech.QueueHub.API.Application.Staff;

namespace GrandeTech.QueueHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StaffController : ControllerBase
    {
        private readonly AddBarberService _addBarberService;

        public StaffController(AddBarberService addBarberService)
        {
            _addBarberService = addBarberService;
        }
        [HttpPost("barbers")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<ActionResult<AddBarberResult>> AddBarber(
            [FromBody] AddBarberRequest request,
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("User ID not found in claims");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";

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
    }
} 