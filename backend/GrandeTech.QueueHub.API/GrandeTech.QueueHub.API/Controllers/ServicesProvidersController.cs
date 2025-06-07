using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GrandeTech.QueueHub.API.Application.ServicesProviders;
using GrandeTech.QueueHub.API.Application.ServicesProviders.Requests;

namespace GrandeTech.QueueHub.API.Controllers;

[ApiController]
[Route("api/service-providers")]
[Authorize]
public class ServicesProvidersController : ControllerBase
{
    private readonly CreateServicesProviderService _createServicesProviderService;

    public ServicesProvidersController(CreateServicesProviderService createServicesProviderService)
    {
        _createServicesProviderService = createServicesProviderService ?? throw new ArgumentNullException(nameof(createServicesProviderService));
    }

    [HttpPost]
    public async Task<IActionResult> CreateServicesProvider([FromBody] CreateServicesProviderRequest request)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
        
        var result = await _createServicesProviderService.ExecuteAsync(request, currentUserId);

        if (!result.Success)
        {
            if (result.FieldErrors.Any())
            {
                return BadRequest(result);
            }
            
            return BadRequest(result);
        }

        return CreatedAtAction(
            nameof(GetServicesProvider), 
            new { id = result.ServicesProviderId }, 
            result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetServicesProvider(string id)
    {
        // Placeholder - will be implemented later
        return Ok(new { Id = id, Message = "Get service provider endpoint - to be implemented" });
    }
}
