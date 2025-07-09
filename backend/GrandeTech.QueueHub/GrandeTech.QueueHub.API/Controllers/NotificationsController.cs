using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Grande.Fila.API.Application.Notifications;

namespace Grande.Fila.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly SmsNotificationService _smsNotificationService;

        public NotificationsController(SmsNotificationService smsNotificationService)
        {
            _smsNotificationService = smsNotificationService;
        }

        public class SendSmsRequestDto
        {
            public string QueueEntryId { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public string NotificationType { get; set; } = string.Empty;
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
            var request = new SmsNotificationRequest
            {
                QueueEntryId = dto.QueueEntryId,
                Message = dto.Message,
                NotificationType = dto.NotificationType
            };

            var result = await _smsNotificationService.ExecuteAsync(request, User.Identity?.Name ?? "system", cancellationToken);

            if (!result.Success)
            {
                return BadRequest(new SendSmsResponseDto
                {
                    Success = false,
                    Errors = result.Errors.Count > 0 ? result.Errors.ToArray() : new[] { "Validation errors" }
                });
            }

            return Ok(new SendSmsResponseDto
            {
                Success = true,
                MessageId = result.MessageId
            });
        }
    }
} 