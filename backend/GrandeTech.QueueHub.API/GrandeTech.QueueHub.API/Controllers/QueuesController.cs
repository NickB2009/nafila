using System;
using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Application.Queues;
using GrandeTech.QueueHub.API.Domain.Queues;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrandeTech.QueueHub.API.Infrastructure.Authorization;

namespace GrandeTech.QueueHub.API.Controllers
{    [ApiController]
    [Route("api/[controller]")]
    [RequireOwner] // Default: Admin/Owner can manage queues
    public class QueuesController : ControllerBase
    {
        private readonly AddQueueService _addQueueService;
        private readonly IQueueRepository _queueRepository;

        public QueuesController(
            AddQueueService addQueueService,
            IQueueRepository queueRepository)
        {
            _addQueueService = addQueueService;
            _queueRepository = queueRepository;
        }

        public class QueueDto
        {
            public Guid Id { get; set; }
            public Guid LocationId { get; set; }
            public DateTime QueueDate { get; set; }
            public bool IsActive { get; set; }
            public int MaxSize { get; set; }
            public int LateClientCapTimeInMinutes { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> AddQueue([FromBody] AddQueueRequest request, CancellationToken cancellationToken)
        {
            if (request.LocationId == Guid.Empty)
            {
                return BadRequest("LocationId is required.");
            }
            if (request.MaxSize <= 0)
            {
                return BadRequest("MaxSize must be positive.");
            }
            if (request.LateClientCapTimeInMinutes < 0)
            {
                return BadRequest("LateClientCapTimeInMinutes cannot be negative.");
            }

            var result = await _addQueueService.AddQueueAsync(request, cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result.ErrorMessage);
            }

            return CreatedAtAction(nameof(GetQueue), new { id = result.QueueId }, result);
        }        [HttpGet("{id}")]
        [RequireClient] // UC-QUEUELISTCLI: Any authenticated user can view queue status
        public async Task<IActionResult> GetQueue(Guid id, CancellationToken cancellationToken)
        {
            var queue = await _queueRepository.GetByIdAsync(id, cancellationToken);
            if (queue == null)
            {
                return NotFound();
            }

            var dto = new QueueDto
            {
                Id = queue.Id,
                LocationId = queue.LocationId,
                QueueDate = queue.QueueDate,
                IsActive = queue.IsActive,
                MaxSize = queue.MaxSize,
                LateClientCapTimeInMinutes = queue.LateClientCapTimeInMinutes
            };

            return Ok(dto);
        }

        [HttpPost("{id}/join")]
        [RequireClient] // Allow any authenticated user to join queues
        public async Task<IActionResult> JoinQueue(
            string id,
            [FromBody] JoinQueueRequest request,
            CancellationToken cancellationToken)
        {
            // Validate queue id
            if (!Guid.TryParse(id, out var queueId))
                return BadRequest("Invalid queue id.");

            // Ensure request.QueueId matches route id
            if (request.QueueId != id)
                request.QueueId = id;

            // Get user id (or anonymous)
            var userId = User?.Identity?.IsAuthenticated == true
                ? User.FindFirst(GrandeTech.QueueHub.API.Domain.Users.TenantClaims.UserId)?.Value ?? "anonymous"
                : "anonymous";

            // Get queue
            var queue = await _queueRepository.GetByIdAsync(queueId, cancellationToken);
            if (queue == null)
                return NotFound();

            // Use JoinQueueService (resolve from DI if needed)
            var joinQueueService = HttpContext.RequestServices.GetService(typeof(JoinQueueService)) as JoinQueueService;
            if (joinQueueService == null)
                return StatusCode(500, "JoinQueueService not available");

            var result = await joinQueueService.JoinQueueAsync(request, userId, cancellationToken);

            if (!result.Success)
            {
                if (result.FieldErrors.Count > 0)
                    return BadRequest(result);
                if (result.Errors.Any(e => e.Contains("maximum size")))
                    return BadRequest(result);
                if (result.Errors.Any())
                    return BadRequest(result);
            }

            return Ok(result);
        }
    }
} 