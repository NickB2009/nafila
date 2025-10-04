using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Grande.Fila.API.Application.Queues;
using Grande.Fila.API.Infrastructure.Authorization;
using Grande.Fila.API.Application.Kiosk;

namespace Grande.Fila.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KioskController : ControllerBase
    {
        private readonly JoinQueueService _joinQueueService;
        private readonly CancelQueueService _cancelQueueService;
        private readonly KioskDisplayService _kioskDisplayService;
        private readonly Grande.Fila.API.Infrastructure.Services.IKioskNotificationService _kioskNotificationService;

        public KioskController(
            JoinQueueService joinQueueService,
            CancelQueueService cancelQueueService,
            KioskDisplayService kioskDisplayService,
            Grande.Fila.API.Infrastructure.Services.IKioskNotificationService kioskNotificationService)
        {
            _joinQueueService = joinQueueService;
            _cancelQueueService = cancelQueueService;
            _kioskDisplayService = kioskDisplayService;
            _kioskNotificationService = kioskNotificationService;
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

        /// <summary>
        /// Gets real-time queue display data for kiosk screens
        /// </summary>
        /// <param name="locationId">The location ID</param>
        /// <param name="includeCompleted">Whether to include completed entries</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Kiosk display data with queue information</returns>
        [HttpGet("display/{locationId}")]
        [AllowPublicAccess] // UC-KIOSKCALL: Public display of queue
        [ProducesResponseType(typeof(KioskDisplayResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetKioskDisplay(
            [FromRoute] string locationId,
            [FromQuery] bool includeCompleted = false,
            CancellationToken cancellationToken = default)
        {
            // Parameter validation
            if (string.IsNullOrWhiteSpace(locationId))
            {
                return BadRequest(new KioskDisplayResult 
                { 
                    Success = false, 
                    Errors = { "Location ID is required" } 
                });
            }

            // Validate GUID format
            if (!Guid.TryParse(locationId, out var parsedLocationId))
            {
                return BadRequest(new KioskDisplayResult 
                { 
                    Success = false, 
                    Errors = { "Invalid location ID format" } 
                });
            }

            var request = new KioskDisplayRequest
            {
                LocationId = locationId,
                IncludeCompletedEntries = includeCompleted
            };

            var result = await _kioskDisplayService.ExecuteAsync(request, "kiosk-display", cancellationToken);

            if (!result.Success)
            {
                if (result.Errors.Contains("Location not found"))
                {
                    return NotFound(result);
                }
                
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Gets statistics about active kiosk connections
        /// </summary>
        /// <returns>Connection statistics</returns>
        [HttpGet("stats")]
        [AllowPublicAccess]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public IActionResult GetKioskStats()
        {
            // This would typically require authentication in production
            var stats = new
            {
                activeConnections = _kioskNotificationService.GetConnectionStatistics(),
                timestamp = DateTime.UtcNow
            };

            return Ok(stats);
        }
    }
}