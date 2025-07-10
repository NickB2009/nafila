using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Grande.Fila.API.Application.Promotions;

namespace Grande.Fila.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PromotionsController : ControllerBase
    {
        private readonly CouponNotificationService _couponNotificationService;

        public PromotionsController(CouponNotificationService couponNotificationService)
        {
            _couponNotificationService = couponNotificationService;
        }

        public class SendCouponNotificationRequestDto
        {
            public string CustomerId { get; set; } = string.Empty;
            public string CouponId { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public string NotificationChannel { get; set; } = "SMS";
        }

        public class SendCouponNotificationResponseDto
        {
            public bool Success { get; set; }
            public string? NotificationId { get; set; }
            public string[] Errors { get; set; } = new string[0];
        }

        // UC-COUPONNOTIF: Send coupon notification to customer
        [HttpPost("notify-coupon")]
        [Authorize(Policy = "RequireServiceAccount")]
        public async Task<IActionResult> SendCouponNotification([FromBody] SendCouponNotificationRequestDto dto, CancellationToken cancellationToken)
        {
            var request = new CouponNotificationRequest
            {
                CustomerId = dto.CustomerId,
                CouponId = dto.CouponId,
                Message = dto.Message,
                NotificationChannel = dto.NotificationChannel
            };

            var result = await _couponNotificationService.ExecuteAsync(request, User.Identity?.Name ?? "system", cancellationToken);

            if (!result.Success)
            {
                return BadRequest(new SendCouponNotificationResponseDto
                {
                    Success = false,
                    Errors = result.Errors.Count > 0 ? result.Errors.ToArray() : new[] { "Validation errors" }
                });
            }

            return Ok(new SendCouponNotificationResponseDto
            {
                Success = true,
                NotificationId = result.NotificationId
            });
        }
    }
} 