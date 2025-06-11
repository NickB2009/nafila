using GrandeTech.QueueHub.API.Application.ServicesOffered;
using GrandeTech.QueueHub.API.Domain.ServicesOffered;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GrandeTech.QueueHub.API.Controllers
{
    [ApiController]
    [Route("api/servicetypes")]
    [Authorize]
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
        public async Task<IActionResult> AddServiceType([FromBody] AddServiceOfferedRequest request, CancellationToken cancellationToken)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";

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

                return BadRequest(new { errors = result.Errors });
            }

            return CreatedAtAction(
                nameof(GetServiceType),
                new { id = result.ServiceTypeId },
                result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetServiceType(string id, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(id, out var guid))
                return BadRequest("Invalid service type ID format.");

            var serviceType = await _serviceTypeRepository.GetByIdAsync(guid, cancellationToken); if (serviceType == null)
                return NotFound();

            var dto = new ServicesOfferedDto
            {
                Id = serviceType.Id,
                Name = serviceType.Name,
                Description = serviceType.Description,
                LocationId = serviceType.LocationId,
                EstimatedDurationMinutes = serviceType.EstimatedDurationMinutes,
                Price = serviceType.Price?.Amount,
                ImageUrl = serviceType.ImageUrl,
                IsActive = serviceType.IsActive
            };
            return Ok(dto);
        }

        [HttpGet]
        public async Task<IActionResult> GetServiceTypes([FromQuery] Guid? locationId, CancellationToken cancellationToken)
        {
            var serviceTypes = await _serviceTypeRepository.GetAllAsync(cancellationToken); if (locationId.HasValue)
                serviceTypes = serviceTypes.Where(st => st.LocationId == locationId.Value).ToList();
            var dtos = serviceTypes.Select(st => new ServicesOfferedDto
            {
                Id = st.Id,
                Name = st.Name,
                Description = st.Description,
                LocationId = st.LocationId,
                EstimatedDurationMinutes = st.EstimatedDurationMinutes,
                Price = st.Price?.Amount,
                ImageUrl = st.ImageUrl,
                IsActive = st.IsActive
            }).ToList();
            return Ok(dtos);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateServiceType(string id, [FromBody] UpdateServicesOfferedRequest request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(id, out var guid))
                return BadRequest("Invalid service type ID format.");

            var serviceType = await _serviceTypeRepository.GetByIdAsync(guid, cancellationToken);
            if (serviceType == null)
                return NotFound(); serviceType.Update(request.Name, request.Description, request.LocationId, request.EstimatedDurationMinutes, request.Price, request.ImageUrl, request.IsActive);
            await _serviceTypeRepository.UpdateAsync(serviceType, cancellationToken);

            var dto = new ServicesOfferedDto
            {
                Id = serviceType.Id,
                Name = serviceType.Name,
                Description = serviceType.Description,
                LocationId = serviceType.LocationId,
                EstimatedDurationMinutes = serviceType.EstimatedDurationMinutes,
                Price = serviceType.Price?.Amount,
                ImageUrl = serviceType.ImageUrl,
                IsActive = serviceType.IsActive
            };
            return Ok(dto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServiceType(string id, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(id, out var guid))
                return BadRequest("Invalid service type ID format.");

            var serviceType = await _serviceTypeRepository.GetByIdAsync(guid, cancellationToken);
            if (serviceType == null)
                return NotFound();

            await _serviceTypeRepository.DeleteAsync(serviceType, cancellationToken);
            return NoContent();
        }

        [HttpPost("{id}/activate")]
        public async Task<IActionResult> ActivateServiceType(string id, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(id, out var guid))
                return BadRequest("Invalid service type ID format.");

            var serviceType = await _serviceTypeRepository.GetByIdAsync(guid, cancellationToken);
            if (serviceType == null)
                return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(); serviceType.Activate(userId);
            await _serviceTypeRepository.UpdateAsync(serviceType, cancellationToken);

            var dto = new ServicesOfferedDto
            {
                Id = serviceType.Id,
                Name = serviceType.Name,
                Description = serviceType.Description,
                LocationId = serviceType.LocationId,
                EstimatedDurationMinutes = serviceType.EstimatedDurationMinutes,
                Price = serviceType.Price?.Amount,
                ImageUrl = serviceType.ImageUrl,
                IsActive = serviceType.IsActive
            };
            return Ok(dto);
        }
    }
}