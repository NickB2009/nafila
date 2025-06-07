using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GrandeTech.QueueHub.API.Application.ServiceProviders;
using GrandeTech.QueueHub.API.Application.ServiceProviders.Requests;

namespace GrandeTech.QueueHub.API.Controllers;

[ApiController]
[Route("api/service-providers")]
[Authorize]
public class ServiceProvidersController : ControllerBase
{
    private readonly CreateServiceProviderService _createServiceProviderService;

    public ServiceProvidersController(CreateServiceProviderService createServiceProviderService)
    {
        _createServiceProviderService = createServiceProviderService ?? throw new ArgumentNullException(nameof(createServiceProviderService));
    }

    [HttpPost]
    public async Task<IActionResult> CreateServiceProvider([FromBody] CreateServiceProviderRequest request)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
        
        var result = await _createServiceProviderService.ExecuteAsync(request, currentUserId);

        if (!result.Success)
        {
            if (result.FieldErrors.Any())
            {
                return BadRequest(result);
            }
            
            return BadRequest(result);
        }

        return CreatedAtAction(
            nameof(GetServiceProvider), 
            new { id = result.ServiceProviderId }, 
            result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetServiceProvider(string id)
    {
        // Placeholder - will be implemented later
        return Ok(new { Id = id, Message = "Get service provider endpoint - to be implemented" });
    }
}
