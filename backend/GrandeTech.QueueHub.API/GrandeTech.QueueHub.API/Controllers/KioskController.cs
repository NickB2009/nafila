using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Grande.Fila.API.Application.Queues;
using Grande.Fila.API.Infrastructure.Authorization;

namespace Grande.Fila.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KioskController : ControllerBase
    {
        private readonly JoinQueueService _joinQueueService;
        private readonly CancelQueueService _cancelQueueService;

        public KioskController(
            JoinQueueService joinQueueService,
            CancelQueueService cancelQueueService)
        {
            _joinQueueService = joinQueueService;
            _cancelQueueService = cancelQueueService;
        }

        public class KioskJoinRequest
        {
            public string QueueId { get; set; } = string.Empty;
            public string CustomerName { get; set; } = string.Empty;
            public string? PhoneNumber { get; set; }
        }

        public class KioskJoinResult
        {
            public bool Success { get; set; }
            public string? QueueEntryId { get; set; }
            public string? CustomerName { get; set; }
            public int Position { get; set; }
            public int EstimatedWaitTimeMinutes { get; set; }
            public string[] Errors { get; set; } = new string[0];
        }

        public class KioskCancelRequest
        {
            public string QueueEntryId { get; set; } = string.Empty;
        }

        public class KioskCancelResult
        {
            public bool Success { get; set; }
            public string? CustomerName { get; set; }
            public string[] Errors { get; set; } = new string[0];
        }

        [HttpPost("join")]
        [AllowPublicAccess] // UC-INPUTDATA: Kiosk allows anonymous entry with basic data
        public async Task<IActionResult> JoinQueue(
            [FromBody] KioskJoinRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.CustomerName))
            {
                return BadRequest(new KioskJoinResult 
                { 
                    Success = false, 
                    Errors = new[] { "Customer name is required." } 
                });
            }

            if (string.IsNullOrWhiteSpace(request.QueueId))
            {
                return BadRequest(new KioskJoinResult 
                { 
                    Success = false, 
                    Errors = new[] { "Queue ID is required." } 
                });
            }

            // Convert to JoinQueueRequest
            var joinRequest = new JoinQueueRequest
            {
                QueueId = request.QueueId,
                CustomerName = request.CustomerName,
                PhoneNumber = request.PhoneNumber,
                IsAnonymous = true // Kiosk entries are always anonymous
            };

            var result = await _joinQueueService.JoinQueueAsync(joinRequest, "kiosk-user", cancellationToken);

            if (!result.Success)
            {
                var errors = new List<string>();
                errors.AddRange(result.Errors);
                foreach (var fieldError in result.FieldErrors)
                {
                    errors.Add($"{fieldError.Key}: {fieldError.Value}");
                }

                return BadRequest(new KioskJoinResult 
                { 
                    Success = false, 
                    Errors = errors.ToArray() 
                });
            }

            return Ok(new KioskJoinResult
            {
                Success = true,
                QueueEntryId = result.QueueEntryId,
                CustomerName = request.CustomerName,
                Position = result.Position,
                EstimatedWaitTimeMinutes = result.EstimatedWaitTimeMinutes
            });
        }

        [HttpPost("cancel")]
        [AllowPublicAccess] // UC-KIOSKCANCEL: Kiosk allows cancellation
        public async Task<IActionResult> CancelQueue(
            [FromBody] KioskCancelRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.QueueEntryId))
            {
                return BadRequest(new KioskCancelResult 
                { 
                    Success = false, 
                    Errors = new[] { "Queue entry ID is required." } 
                });
            }

            var cancelRequest = new CancelQueueRequest
            {
                QueueEntryId = request.QueueEntryId
            };

            var result = await _cancelQueueService.CancelQueueAsync(cancelRequest, "kiosk-user", cancellationToken);

            if (!result.Success)
            {
                var errors = new List<string>();
                errors.AddRange(result.Errors);
                foreach (var fieldError in result.FieldErrors)
                {
                    errors.Add($"{fieldError.Key}: {fieldError.Value}");
                }

                return BadRequest(new KioskCancelResult 
                { 
                    Success = false, 
                    Errors = errors.ToArray() 
                });
            }

            return Ok(new KioskCancelResult
            {
                Success = true,
                CustomerName = result.CustomerName
            });
        }
    }
}