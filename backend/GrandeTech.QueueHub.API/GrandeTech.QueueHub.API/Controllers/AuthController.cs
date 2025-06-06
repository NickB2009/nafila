using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GrandeTech.QueueHub.API.Application.Auth;

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
    }
} 