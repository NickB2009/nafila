using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GrandeTech.QueueHub.API.Application.Locations;
using GrandeTech.QueueHub.API.Application.Locations.Requests;

namespace GrandeTech.QueueHub.API.Controllers;

[ApiController]
[Route("api/locations")]
[Authorize]
public class LocationsController : ControllerBase
{
    private readonly CreateLocationService _createLocationService;

    public LocationsController(CreateLocationService createLocationService)
    {
        _createLocationService = createLocationService ?? throw new ArgumentNullException(nameof(createLocationService));
    }

    [HttpPost]
    public async Task<IActionResult> CreateLocation([FromBody] CreateLocationRequest request)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
        
        var result = await _createLocationService.ExecuteAsync(request, currentUserId);

        if (!result.Success)
        {
            if (result.FieldErrors.Any())
            {
                return BadRequest(result);
            }
            
            return BadRequest(result);
        }

        return CreatedAtAction(
            nameof(GetLocation), 
            new { id = result.LocationId }, 
            result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetLocation(string id)
    {
        // Placeholder - will be implemented later
        return Ok(new { Id = id, Message = "Get location endpoint - to be implemented" });
    }
}
