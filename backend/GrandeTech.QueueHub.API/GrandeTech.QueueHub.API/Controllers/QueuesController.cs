using System;
using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Application.Queues;
using GrandeTech.QueueHub.API.Domain.Queues;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrandeTech.QueueHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
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
        }

        [HttpGet("{id}")]
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
    }
} 