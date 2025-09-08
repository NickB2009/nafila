using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Grande.Fila.API.Application.Auth;
using Grande.Fila.API.Infrastructure.Authorization;
using Grande.Fila.API.Domain.Users;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Grande.Fila.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AuthService authService, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _authService = authService;
            _configuration = configuration;
            _logger = logger;
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
            try
            {
                _logger.LogInformation("Registration request received for email: {Email}", request?.Email);
                
                if (request == null)
                {
                    _logger.LogWarning("Registration request is null");
                    return BadRequest(new RegisterResult
                    {
                        Success = false,
                        Error = "Invalid request data"
                    });
                }

                var result = await _authService.RegisterAsync(request, cancellationToken);
                
                if (!result.Success)
                {
                    _logger.LogWarning("Registration failed for email: {Email}. Errors: {Errors}", 
                        request.Email, 
                        result.FieldErrors?.Count > 0 ? string.Join(", ", result.FieldErrors.Select(x => $"{x.Key}: {x.Value}")) : result.Error);
                    
                    if (result.FieldErrors?.Count > 0)
                        return BadRequest(result);
                    if (!string.IsNullOrEmpty(result.Error))
                        return BadRequest(result);
                }

                _logger.LogInformation("Registration successful for email: {Email}", request.Email);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration for email: {Email}", request?.Email);
                return StatusCode(500, new RegisterResult
                {
                    Success = false,
                    Error = "An unexpected error occurred during registration. Please try again."
                });
            }
        }

        [HttpGet("profile")]
        [Authorize]
        public ActionResult<object> GetProfile()
        {
            var userId = User.FindFirst(TenantClaims.UserId)?.Value;
            var fullName = User.FindFirst(TenantClaims.Username)?.Value; // Username claim now contains FullName
            var email = User.FindFirst(TenantClaims.Email)?.Value;
            var phoneNumber = User.FindFirst("PhoneNumber")?.Value;
            var role = User.FindFirst(TenantClaims.Role)?.Value;
            var permissions = User.FindFirst(TenantClaims.Permissions)?.Value;

            string[]? deserializedPermissions = null;
            if (!string.IsNullOrEmpty(permissions))
            {
                try
                {
                    deserializedPermissions = System.Text.Json.JsonSerializer.Deserialize<string[]>(permissions);
                }
                catch (System.Text.Json.JsonException)
                {
                    // Handle malformed JSON by splitting on comma or returning raw value
                    deserializedPermissions = permissions.Contains(',') 
                        ? permissions.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        : new[] { permissions };
                }
            }

            return Ok(new
            {
                UserId = userId,
                FullName = fullName,
                Email = email,
                PhoneNumber = phoneNumber,
                Role = role,
                Permissions = deserializedPermissions
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

        [HttpPost("debug/test-registration")]
        public async Task<ActionResult> TestRegistration([FromBody] RegisterRequest request)
        {
            try
            {
                _logger.LogInformation("🔍 Debug: Testing registration process");
                _logger.LogInformation("📝 Request data: {@Request}", new { 
                    FullName = request?.FullName, 
                    Email = request?.Email, 
                    PhoneNumber = request?.PhoneNumber,
                    PasswordLength = request?.Password?.Length ?? 0
                });

                if (request == null)
                {
                    return BadRequest(new { error = "Request is null" });
                }

                // Test password validation
                var authService = new System.Reflection.FieldInfo[0];
                var validateMethod = typeof(AuthService).GetMethod("ValidatePasswordStrength", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                return Ok(new { 
                    message = "Debug endpoint working", 
                    requestReceived = true,
                    dataValid = !string.IsNullOrEmpty(request.FullName) && 
                               !string.IsNullOrEmpty(request.Email) && 
                               !string.IsNullOrEmpty(request.PhoneNumber) &&
                               !string.IsNullOrEmpty(request.Password)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Debug endpoint error");
                return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }
    }
} 