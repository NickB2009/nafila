using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GrandeTech.QueueHub.API.Application.Auth;
using GrandeTech.QueueHub.API.Infrastructure.Authorization;
using GrandeTech.QueueHub.API.Domain.Users;

namespace GrandeTech.QueueHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResult>> Login(
            [FromBody] LoginRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _authService.LoginAsync(request, cancellationToken);
            
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("verify-2fa")]
        public async Task<ActionResult<LoginResult>> VerifyTwoFactor(
            [FromBody] VerifyTwoFactorRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _authService.VerifyTwoFactorAsync(request, cancellationToken);
            
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<ActionResult<RegisterResult>> Register(
            [FromBody] RegisterRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _authService.RegisterAsync(request, cancellationToken);
            
            if (!result.Success)
            {
                if (result.FieldErrors?.Count > 0)
                    return BadRequest(result);
                if (!string.IsNullOrEmpty(result.Error))
                    return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("profile")]
        [Authorize]
        public ActionResult<object> GetProfile()
        {
            var userId = User.FindFirst(TenantClaims.UserId)?.Value;
            var username = User.FindFirst(TenantClaims.Username)?.Value;
            var email = User.FindFirst(TenantClaims.Email)?.Value;
            var role = User.FindFirst(TenantClaims.Role)?.Value;
            var permissions = User.FindFirst(TenantClaims.Permissions)?.Value;

            return Ok(new
            {
                UserId = userId,
                Username = username,
                Email = email,
                Role = role,
                Permissions = permissions != null ? System.Text.Json.JsonSerializer.Deserialize<string[]>(permissions) : null
            });
        }

        [HttpPost("admin/verify")]
        [RequirePlatformAdmin]
        public async Task<ActionResult<AdminVerificationResult>> VerifyAdmin(
            [FromBody] AdminVerificationRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _authService.VerifyAdminAsync(request, cancellationToken);
            
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
} 