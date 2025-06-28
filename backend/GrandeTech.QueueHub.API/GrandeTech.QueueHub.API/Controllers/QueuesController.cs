using System;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Application.Queues;
using Grande.Fila.API.Domain.Queues;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Grande.Fila.API.Infrastructure.Authorization;
using System.Collections.Generic;
using System.Linq;
using Grande.Fila.API.Application.Services;

namespace Grande.Fila.API.Controllers
{    [ApiController]
    [Route("api/[controller]")]
    public class QueuesController : ControllerBase
    {
        private readonly AddQueueService _addQueueService;
        private readonly IQueueRepository _queueRepository;
        private readonly EstimatedWaitTimeService _estimatedWaitTimeService;
        private readonly FinishService _finishService;

        public QueuesController(
            AddQueueService addQueueService,
            IQueueRepository queueRepository,
            EstimatedWaitTimeService estimatedWaitTimeService,
            FinishService finishService)
        {
            _addQueueService = addQueueService;
            _queueRepository = queueRepository;
            _estimatedWaitTimeService = estimatedWaitTimeService;
            _finishService = finishService;
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
        [RequireOwner] // Admin/Owner can manage queues
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
        [AllowPublicAccess] // Allow anonymous users to join queues
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
                ? User.FindFirst(Grande.Fila.API.Domain.Users.TenantClaims.UserId)?.Value ?? "anonymous"
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

        [HttpPost("{id}/call-next")]
        [RequireBarber] // Only barbers can call next customer
        public async Task<IActionResult> CallNext(
            string id,
            [FromBody] CallNextRequest request,
            CancellationToken cancellationToken)
        {
            // Validate queue id
            if (!Guid.TryParse(id, out var queueId))
                return BadRequest("Invalid queue id.");

            // Validate staff member id
            if (string.IsNullOrWhiteSpace(request.StaffMemberId))
                return BadRequest("Staff member ID is required.");

            if (!Guid.TryParse(request.StaffMemberId, out var staffMemberId))
                return BadRequest("Invalid staff member ID format.");

            // Get user id
            var userId = User?.Identity?.IsAuthenticated == true
                ? User.FindFirst(Grande.Fila.API.Domain.Users.TenantClaims.UserId)?.Value ?? "anonymous"
                : "anonymous";

            // Get queue
            var queue = await _queueRepository.GetByIdAsync(queueId, cancellationToken);
            if (queue == null)
                return NotFound();

            try
            {
                // Call next customer using domain logic
                var queueEntry = queue.CallNextCustomer(staffMemberId);

                // Update queue in repository
                await _queueRepository.UpdateAsync(queue, cancellationToken);

                // Return success result
                var result = new CallNextResult
                {
                    Success = true,
                    QueueEntryId = queueEntry.Id.ToString(),
                    CustomerName = queueEntry.CustomerName,
                    Position = queueEntry.Position,
                    StaffMemberId = queueEntry.StaffMemberId?.ToString(),
                    CalledAt = queueEntry.CalledAt
                };

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                var result = new CallNextResult
                {
                    Success = false,
                    Errors = new List<string> { ex.Message }
                };
                return BadRequest(result);
            }
            catch (ArgumentException ex)
            {
                var result = new CallNextResult
                {
                    Success = false,
                    FieldErrors = new Dictionary<string, string> { { "StaffMemberId", ex.Message } }
                };
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                var result = new CallNextResult
                {
                    Success = false,
                    Errors = new List<string> { $"An error occurred while calling next customer: {ex.Message}" }
                };
                return BadRequest(result);
            }
        }

        [HttpPost("{id}/check-in")]
        [RequireBarber] // Only barbers can check in customers
        public async Task<IActionResult> CheckIn(
            string id,
            [FromBody] CheckInRequest request,
            CancellationToken cancellationToken)
        {
            // Validate queue id
            if (!Guid.TryParse(id, out var queueId))
                return BadRequest("Invalid queue id.");

            // Validate queue entry id
            if (string.IsNullOrWhiteSpace(request.QueueEntryId))
                return BadRequest("Queue entry ID is required.");

            if (!Guid.TryParse(request.QueueEntryId, out var queueEntryId))
                return BadRequest("Invalid queue entry ID format.");

            // Get user id
            var userId = User?.Identity?.IsAuthenticated == true
                ? User.FindFirst(Grande.Fila.API.Domain.Users.TenantClaims.UserId)?.Value ?? "anonymous"
                : "anonymous";

            // Get queue
            var queue = await _queueRepository.GetByIdAsync(queueId, cancellationToken);
            if (queue == null)
                return NotFound();

            try
            {
                // Find the queue entry
                var queueEntry = queue.Entries.FirstOrDefault(e => e.Id == queueEntryId);
                if (queueEntry == null)
                    return NotFound();

                // Check in the customer using domain logic
                queueEntry.CheckIn();

                // Update queue in repository
                await _queueRepository.UpdateAsync(queue, cancellationToken);

                // Return success result
                var result = new CheckInResult
                {
                    Success = true,
                    QueueEntryId = queueEntry.Id.ToString(),
                    CustomerName = queueEntry.CustomerName,
                    Position = queueEntry.Position,
                    CheckedInAt = queueEntry.CheckedInAt
                };

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                var result = new CheckInResult
                {
                    Success = false,
                    Errors = new List<string> { ex.Message }
                };
                return BadRequest(result);
            }
            catch (ArgumentException ex)
            {
                var result = new CheckInResult
                {
                    Success = false,
                    FieldErrors = new Dictionary<string, string> { { "QueueEntryId", ex.Message } }
                };
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                var result = new CheckInResult
                {
                    Success = false,
                    Errors = new List<string> { $"An error occurred while checking in customer: {ex.Message}" }
                };
                return BadRequest(result);
            }
        }

        [HttpPost("{id}/finish")]
        [RequireBarber] // Only barbers can complete services
        public async Task<IActionResult> Finish(
            string id,
            [FromBody] FinishRequest request,
            CancellationToken cancellationToken)
        {
            // Validate queue id
            if (!Guid.TryParse(id, out var queueId))
                return BadRequest("Invalid queue id.");

            // Get user id
            var userId = User?.Identity?.IsAuthenticated == true
                ? User.FindFirst(Grande.Fila.API.Domain.Users.TenantClaims.UserId)?.Value ?? "anonymous"
                : "anonymous";

            // Use the FinishService to handle the business logic
            var result = await _finishService.FinishAsync(request, userId, cancellationToken);

            if (!result.Success)
            {
                if (result.FieldErrors.Count > 0)
                    return BadRequest(result);
                if (result.Errors.Count > 0)
                    return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("{id}/cancel")]
        [RequireClient] // Clients can cancel their own queue entries
        public async Task<IActionResult> Cancel(
            string id,
            [FromBody] CancelQueueRequest request,
            CancellationToken cancellationToken)
        {
            var result = new CancelQueueResult();

            // Validate queue id
            if (!Guid.TryParse(id, out var queueId))
            {
                result.Success = false;
                result.FieldErrors.Add("QueueId", "Invalid queue id.");
                return BadRequest(result);
            }

            // Validate queue entry id
            if (string.IsNullOrWhiteSpace(request.QueueEntryId))
            {
                result.Success = false;
                result.FieldErrors.Add("QueueEntryId", "Queue entry ID is required.");
                return BadRequest(result);
            }

            if (!Guid.TryParse(request.QueueEntryId, out var queueEntryId))
            {
                result.Success = false;
                result.FieldErrors.Add("QueueEntryId", "Invalid queue entry ID format.");
                return BadRequest(result);
            }

            // Get user id
            var userId = User?.Identity?.IsAuthenticated == true
                ? User.FindFirst(Grande.Fila.API.Domain.Users.TenantClaims.UserId)?.Value ?? "anonymous"
                : "anonymous";

            // Get queue
            var queue = await _queueRepository.GetByIdAsync(queueId, cancellationToken);
            if (queue == null)
                return NotFound();

            try
            {
                // Find the queue entry
                var queueEntry = queue.Entries.FirstOrDefault(e => e.Id == queueEntryId);
                if (queueEntry == null)
                    return NotFound();

                // Cancel the queue entry using domain logic
                queueEntry.Cancel();

                // Update queue in repository
                await _queueRepository.UpdateAsync(queue, cancellationToken);

                // Return success result
                result.Success = true;
                result.QueueEntryId = queueEntry.Id.ToString();
                result.CustomerName = queueEntry.CustomerName;
                result.CancelledAt = queueEntry.CancelledAt;

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                result.Success = false;
                result.Errors.Add(ex.Message);
                return BadRequest(result);
            }
            catch (ArgumentException ex)
            {
                result.Success = false;
                result.FieldErrors.Add("QueueEntryId", ex.Message);
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"An error occurred while canceling queue entry: {ex.Message}");
                return BadRequest(result);
            }
        }

        [HttpGet("{id}/entries/{entryId}/wait-time")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEstimatedWaitTime(Guid id, Guid entryId, CancellationToken cancellationToken)
        {
            var waitTime = await _estimatedWaitTimeService.CalculateAsync(id, entryId, cancellationToken);
            if (waitTime < 0)
            {
                return NotFound("Could not calculate wait time. Queue or queue entry not found, or no staff available.");
            }
            return Ok(new { estimatedWaitTimeInMinutes = waitTime });
        }
    }
} 