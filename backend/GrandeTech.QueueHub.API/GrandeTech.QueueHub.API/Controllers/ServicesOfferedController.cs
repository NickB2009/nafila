using GrandeTech.QueueHub.API.Application.ServicesOffered;
using GrandeTech.QueueHub.API.Domain.ServicesOffered;
using GrandeTech.QueueHub.API.Domain.Users;
using GrandeTech.QueueHub.API.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrandeTech.QueueHub.API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    [Authorize] // Base authorization requirement
    public class ServicesOfferedController : ControllerBase
    {
        private readonly AddServiceOfferedService _addServiceTypeService;
        private readonly IServicesOfferedRepository _serviceTypeRepository;

        public ServicesOfferedController(AddServiceOfferedService addServiceTypeService, IServicesOfferedRepository serviceTypeRepository)
        {
            _addServiceTypeService = addServiceTypeService ?? throw new ArgumentNullException(nameof(addServiceTypeService));
            _serviceTypeRepository = serviceTypeRepository ?? throw new ArgumentNullException(nameof(serviceTypeRepository));
        }

        [HttpPost]
        [RequireAdmin] // Only admins can add services
        public async Task<IActionResult> AddServiceType([FromBody] AddServiceOfferedRequest request, CancellationToken cancellationToken)
        {
            var currentUserId = User.FindFirst(TenantClaims.UserId)?.Value ?? throw new UnauthorizedAccessException("User ID not found in claims");

            var result = await _addServiceTypeService.AddServiceTypeAsync(request, currentUserId, cancellationToken);

            if (!result.Success)
            {
                if (result.FieldErrors.Count > 0)
                {
                    foreach (var fieldError in result.FieldErrors)
                    {
                        ModelState.AddModelError(fieldError.Key, fieldError.Value);
                    }
                    return BadRequest(ModelState);
                }
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetServiceType), new { id = result.ServiceTypeId }, result);
        }

        [HttpGet("{id}")]
        [RequireClient] // Allow clients to view services
        public async Task<IActionResult> GetServiceType(string id, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(id, out var serviceTypeId))
            {
                return BadRequest("Invalid service type ID format");
            }

            var serviceType = await _serviceTypeRepository.GetByIdAsync(serviceTypeId, cancellationToken);
            if (serviceType == null)
            {
                return NotFound();
            }

            return Ok(serviceType);
        }

        [HttpGet]
        [RequireClient] // Allow clients to view services
        public async Task<IActionResult> GetServiceTypes(CancellationToken cancellationToken)
        {
            var serviceTypes = await _serviceTypeRepository.GetAllAsync(cancellationToken);
            return Ok(serviceTypes);
        }

        [HttpPut("{id}")]
        [RequireAdmin] // Only admins can update services
        public async Task<IActionResult> UpdateServiceType(string id, [FromBody] UpdateServiceOfferedRequest request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(id, out var serviceTypeId))
            {
                return BadRequest("Invalid service type ID format");
            }

            var currentUserId = User.FindFirst(TenantClaims.UserId)?.Value ?? throw new UnauthorizedAccessException("User ID not found in claims");

            var result = await _addServiceTypeService.UpdateServiceTypeAsync(serviceTypeId, request, currentUserId, cancellationToken);

            if (!result.Success)
            {
                if (result.FieldErrors.Count > 0)
                {
                    foreach (var fieldError in result.FieldErrors)
                    {
                        ModelState.AddModelError(fieldError.Key, fieldError.Value);
                    }
                    return BadRequest(ModelState);
                }
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [RequireAdmin] // Only admins can delete services
        public async Task<IActionResult> DeleteServiceType(string id, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(id, out var serviceTypeId))
            {
                return BadRequest("Invalid service type ID format");
            }

            var currentUserId = User.FindFirst(TenantClaims.UserId)?.Value ?? throw new UnauthorizedAccessException("User ID not found in claims");

            var result = await _addServiceTypeService.DeleteServiceTypeAsync(serviceTypeId, currentUserId, cancellationToken);

            if (!result.Success)
            {
                if (result.FieldErrors.Count > 0)
                {
                    foreach (var fieldError in result.FieldErrors)
                    {
                        ModelState.AddModelError(fieldError.Key, fieldError.Value);
                    }
                    return BadRequest(ModelState);
                }
                return BadRequest(result);
            }

            return NoContent();
        }

        [HttpPut("{id}/activate")]
        [RequireAdmin] // Only admins can activate services
        public async Task<IActionResult> ActivateServiceType(string id, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(id, out var serviceTypeId))
            {
                return BadRequest("Invalid service type ID format");
            }

            var currentUserId = User.FindFirst(TenantClaims.UserId)?.Value ?? throw new UnauthorizedAccessException("User ID not found in claims");

            var result = await _addServiceTypeService.ActivateServiceTypeAsync(serviceTypeId, currentUserId, cancellationToken);

            if (!result.Success)
            {
                if (result.FieldErrors.Count > 0)
                {
                    foreach (var fieldError in result.FieldErrors)
                    {
                        ModelState.AddModelError(fieldError.Key, fieldError.Value);
                    }
                    return BadRequest(ModelState);
                }
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
