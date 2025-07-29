using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Application.Public;
using Grande.Fila.API.Infrastructure.Authorization;
using Microsoft.AspNetCore.Http;

namespace Grande.Fila.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowPublicAccess] // No authentication required for public endpoints
public class PublicController : ControllerBase
{
    private readonly GetPublicSalonsService _getPublicSalonsService;
    private readonly GetPublicSalonDetailService _getPublicSalonDetailService;
    private readonly GetPublicQueueStatusService _getPublicQueueStatusService;
    private readonly ILogger<PublicController> _logger;

    public PublicController(
        GetPublicSalonsService getPublicSalonsService,
        GetPublicSalonDetailService getPublicSalonDetailService,
        GetPublicQueueStatusService getPublicQueueStatusService,
        ILogger<PublicController> logger)
    {
        _getPublicSalonsService = getPublicSalonsService ?? throw new ArgumentNullException(nameof(getPublicSalonsService));
        _getPublicSalonDetailService = getPublicSalonDetailService ?? throw new ArgumentNullException(nameof(getPublicSalonDetailService));
        _getPublicQueueStatusService = getPublicQueueStatusService ?? throw new ArgumentNullException(nameof(getPublicQueueStatusService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all available salons with their current status
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of public salon information</returns>
    [HttpGet("salons")]
    [ProducesResponseType(typeof(PublicSalonDto[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSalons(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all public salons");

        var result = await _getPublicSalonsService.ExecuteAsync(cancellationToken);

        if (!result.Success)
        {
            _logger.LogError("Failed to retrieve salons: {Errors}", string.Join(", ", result.Errors));
            return StatusCode(StatusCodes.Status500InternalServerError, new { errors = result.Errors });
        }

        return Ok(result.Salons);
    }

    /// <summary>
    /// Gets detailed information for a specific salon
    /// </summary>
    /// <param name="salonId">The salon ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Detailed salon information</returns>
    [HttpGet("salons/{salonId}")]
    [ProducesResponseType(typeof(PublicSalonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSalonById(string salonId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting salon details for {SalonId}", salonId);

        var result = await _getPublicSalonDetailService.ExecuteAsync(salonId, cancellationToken);

        if (!result.Success)
        {
            if (result.FieldErrors.Count > 0)
            {
                return BadRequest(new { errors = result.FieldErrors });
            }

            if (result.Errors.Contains("Salon not found"))
            {
                return NotFound(new { message = "Salon not found" });
            }

            _logger.LogError("Failed to retrieve salon {SalonId}: {Errors}", salonId, string.Join(", ", result.Errors));
            return StatusCode(StatusCodes.Status500InternalServerError, new { errors = result.Errors });
        }

        return Ok(result.Salon);
    }

    /// <summary>
    /// Gets the current queue status for a specific salon
    /// </summary>
    /// <param name="salonId">The salon ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Queue status information</returns>
    [HttpGet("queue-status/{salonId}")]
    [ProducesResponseType(typeof(PublicQueueStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetQueueStatus(string salonId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting queue status for salon {SalonId}", salonId);

        var result = await _getPublicQueueStatusService.ExecuteAsync(salonId, cancellationToken);

        if (!result.Success)
        {
            if (result.FieldErrors.Count > 0)
            {
                return BadRequest(new { errors = result.FieldErrors });
            }

            if (result.Errors.Contains("Salon not found"))
            {
                return NotFound(new { message = "Salon not found" });
            }

            _logger.LogError("Failed to retrieve queue status for salon {SalonId}: {Errors}", salonId, string.Join(", ", result.Errors));
            return StatusCode(StatusCodes.Status500InternalServerError, new { errors = result.Errors });
        }

        return Ok(result.QueueStatus);
    }
}