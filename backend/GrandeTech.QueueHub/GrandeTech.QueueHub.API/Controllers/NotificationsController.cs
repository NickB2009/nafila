using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Grande.Fila.API.Application.Notifications;
using Grande.Fila.API.Application.Queues;

namespace Grande.Fila.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly IQueueService _queueService;

        public NotificationsController(IQueueService queueService)
        {
            _queueService = queueService;
        }

        public class SendSmsRequestDto
        {
            public string QueueEntryId { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public string NotificationType { get; set; } = string.Empty;
            public string PhoneNumber { get; set; } = string.Empty;
            public string CustomerName { get; set; } = string.Empty;
        }

        public class SendSmsResponseDto
        {
            public bool Success { get; set; }
            public string? MessageId { get; set; }
            public string[] Errors { get; set; } = new string[0];
        }

        // UC-SMSNOTIF: Send SMS notification to client
        [HttpPost("sms")]
        [Authorize(Policy = "RequireServiceAccount")]
        public async Task<IActionResult> SendSmsNotification([FromBody] SendSmsRequestDto dto, CancellationToken cancellationToken)
        {
            // Validate request
            if (string.IsNullOrWhiteSpace(dto.QueueEntryId))
            {
                return BadRequest(new SendSmsResponseDto
                {
                    Success = false,
                    Errors = new[] { "QueueEntryId is required" }
                });
            }

            if (string.IsNullOrWhiteSpace(dto.Message))
            {
                return BadRequest(new SendSmsResponseDto
                {
                    Success = false,
                    Errors = new[] { "Message is required" }
                });
            }

            // Create SMS notification message for queue processing
            var smsMessage = new SmsNotificationMessage
            {
                QueueEntryId = dto.QueueEntryId,
                Message = dto.Message,
                NotificationType = dto.NotificationType,
                PhoneNumber = dto.PhoneNumber,
                CustomerName = dto.CustomerName,
                CreatedBy = User.Identity?.Name ?? "system",
                Priority = MessagePriority.High // SMS notifications should be high priority
            };

            // Enqueue the message for background processing
            var enqueued = await _queueService.EnqueueAsync(smsMessage, cancellationToken);

            if (!enqueued)
            {
                return BadRequest(new SendSmsResponseDto
                {
                    Success = false,
                    Errors = new[] { "Failed to queue SMS notification for processing" }
                });
            }

            // Return success immediately - the SMS will be processed in the background
            return Ok(new SendSmsResponseDto
            {
                Success = true,
                MessageId = smsMessage.Id.ToString() // Return the message ID for tracking
            });
        }

        // New endpoint to get queue health status
        [HttpGet("queue/health")]
        [Authorize(Policy = "RequireServiceAccount")]
        public async Task<IActionResult> GetQueueHealth(CancellationToken cancellationToken)
        {
            var healthStatus = await _queueService.GetHealthStatusAsync(cancellationToken);
            return Ok(healthStatus);
        }

        // New endpoint to get queue length
        [HttpGet("queue/length")]
        [Authorize(Policy = "RequireServiceAccount")]
        public async Task<IActionResult> GetQueueLength(CancellationToken cancellationToken)
        {
            var length = await _queueService.GetQueueLengthAsync(cancellationToken);
            return Ok(new { queueLength = length });
        }
    }
} 