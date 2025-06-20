using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Grande.Fila.API.Application.Locations;
using Grande.Fila.API.Application.Locations.Requests;
using Grande.Fila.API.Infrastructure.Authorization;
using Grande.Fila.API.Domain.Users;
using Grande.Fila.API.Domain.Common.ValueObjects;
using System.Collections.Generic;
using System.Threading;
using Grande.Fila.API.Domain.Locations;
using Microsoft.AspNetCore.Http;

namespace Grande.Fila.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[RequireAdmin] // Admin/Owner can create locations
public class LocationsController : ControllerBase
{
    private readonly CreateLocationService _createLocationService;
    private readonly ILocationRepository _locationRepository;

    public LocationsController(
        CreateLocationService createLocationService,
        ILocationRepository locationRepository)
    {
        _createLocationService = createLocationService ?? throw new ArgumentNullException(nameof(createLocationService));
        _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
    }

    [HttpPost]
    public async Task<IActionResult> CreateLocation([FromBody] CreateLocationRequest request)
    {
        var currentUserId = User.FindFirst(TenantClaims.UserId)?.Value ?? "system";
        
        var result = await _createLocationService.ExecuteAsync(request, currentUserId);

        if (!result.Success)
        {
            if (result.FieldErrors.Any())
            {
                return BadRequest(result);
            }
            
            return BadRequest(result);
        }

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet("{id}")]
    [AllowPublicAccess] // Public can view location info by slug/QR codes
    public async Task<IActionResult> GetLocation(string id, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var guid))
            return BadRequest("Invalid location id.");

        var location = await _locationRepository.GetByIdAsync(guid, cancellationToken);
        if (location == null)
            return NotFound();

        var dto = new
        {
            Id = location.Id,
            BusinessName = location.Name,
            Address = new LocationAddressRequest
            {
                Street = location.Address.Street,
                City = location.Address.City,
                State = location.Address.State,
                PostalCode = location.Address.PostalCode,
                Country = location.Address.Country
            },
            BusinessHours = new Dictionary<string, string>
            {
                ["Monday"] = $"{location.BusinessHours.Start:hh\\:mm}-{location.BusinessHours.End:hh\\:mm}"
            },
            MaxQueueCapacity = location.MaxQueueSize,
            Description = location.Description ?? string.Empty
        };

        return Ok(dto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLocation(string id, [FromBody] UpdateLocationRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var guid))
            return BadRequest("Invalid location id.");

        var location = await _locationRepository.GetByIdAsync(guid, cancellationToken);
        if (location == null)
            return NotFound();

        // Update details
        var address = Address.Create(
            street: request.Address.Street,
            number: "",
            complement: "",
            neighborhood: "",
            city: request.Address.City,
            state: request.Address.State,
            country: request.Address.Country,
            postalCode: request.Address.PostalCode);

        // Parse business hours (we only support Monday for now)
        var openingTime = TimeSpan.Parse("08:00");
        var closingTime = TimeSpan.Parse("18:00");
        if (request.BusinessHours.TryGetValue("Monday", out var mondayHours))
        {
            var parts = mondayHours.Split('-');
            if (parts.Length == 2 &&
                TimeSpan.TryParse(parts[0], out var openTs) &&
                TimeSpan.TryParse(parts[1], out var closeTs))
            {
                openingTime = openTs;
                closingTime = closeTs;
            }
        }

        location.UpdateDetails(
            name: request.BusinessName,
            description: request.Description,
            address: address,
            contactPhone: location.ContactPhone?.Value,
            contactEmail: location.ContactEmail?.Value,
            openingTime: openingTime,
            closingTime: closingTime,
            updatedBy: User.FindFirst(TenantClaims.UserId)?.Value ?? "system");

        location.UpdateQueueSettings(request.MaxQueueCapacity, location.LateClientCapTimeInMinutes, User.FindFirst(TenantClaims.UserId)?.Value ?? "system");

        await _locationRepository.UpdateAsync(location, cancellationToken);

        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLocation(string id, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var guid))
            return BadRequest("Invalid location id.");

        var deleted = await _locationRepository.DeleteByIdAsync(guid, cancellationToken);
        if (!deleted)
            return NotFound();

        return Ok();
    }
}
