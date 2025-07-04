using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Grande.Fila.API.Application.QrCode;

namespace Grande.Fila.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QrCodeController : ControllerBase
    {
        private readonly QrJoinService _qrJoinService;

        public QrCodeController(QrJoinService qrJoinService)
        {
            _qrJoinService = qrJoinService;
        }

        public class GenerateQrRequestDto
        {
            public string LocationId { get; set; } = string.Empty;
            public string? ServiceTypeId { get; set; }
            public string? ExpiryMinutes { get; set; }
        }

        public class GenerateQrResponseDto
        {
            public bool Success { get; set; }
            public string? QrCodeBase64 { get; set; }
            public string? JoinUrl { get; set; }
            public string? ExpiresAt { get; set; }
            public string[] Errors { get; set; } = new string[0];
        }

        // UC-QRJOIN: Generate QR code for queue joining
        [HttpPost("generate")]
        [Authorize(Policy = "RequireStaff")]
        public async Task<IActionResult> GenerateQrCode([FromBody] GenerateQrRequestDto dto, CancellationToken cancellationToken)
        {
            var request = new QrJoinRequest
            {
                LocationId = dto.LocationId,
                ServiceTypeId = dto.ServiceTypeId,
                ExpiryMinutes = dto.ExpiryMinutes
            };

            var result = await _qrJoinService.ExecuteAsync(request, User.Identity?.Name ?? "system", cancellationToken);

            if (!result.Success)
            {
                return BadRequest(new GenerateQrResponseDto
                {
                    Success = false,
                    Errors = result.Errors.Count > 0 ? result.Errors.ToArray() : new[] { "Validation errors" }
                });
            }

            return Ok(new GenerateQrResponseDto
            {
                Success = true,
                QrCodeBase64 = result.QrCodeBase64,
                JoinUrl = result.JoinUrl,
                ExpiresAt = result.ExpiresAt?.ToString("yyyy-MM-ddTHH:mm:ssZ")
            });
        }
    }
} 