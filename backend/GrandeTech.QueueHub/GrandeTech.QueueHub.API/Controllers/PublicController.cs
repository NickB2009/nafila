using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Application.Public;
using Grande.Fila.API.Infrastructure.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grande.Fila.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowPublicAccess] // No authentication required for public endpoints
public class PublicController : ControllerBase
{
    private readonly GetPublicSalonsService _getPublicSalonsService;
    private readonly GetPublicSalonDetailService _getPublicSalonDetailService;
    private readonly GetPublicQueueStatusService _getPublicQueueStatusService;
    private readonly AnonymousJoinService _anonymousJoinService;
    private readonly GetQueueEntryStatusService _getQueueEntryStatusService;
    private readonly LeaveQueueService _leaveQueueService;
    private readonly UpdateQueueEntryService _updateQueueEntryService;
    private readonly ILogger<PublicController> _logger;

    public PublicController(
        GetPublicSalonsService getPublicSalonsService,
        GetPublicSalonDetailService getPublicSalonDetailService,
        GetPublicQueueStatusService getPublicQueueStatusService,
        AnonymousJoinService anonymousJoinService,
        GetQueueEntryStatusService getQueueEntryStatusService,
        LeaveQueueService leaveQueueService,
        UpdateQueueEntryService updateQueueEntryService,
        ILogger<PublicController> logger)
    {
        _getPublicSalonsService = getPublicSalonsService ?? throw new ArgumentNullException(nameof(getPublicSalonsService));
        _getPublicSalonDetailService = getPublicSalonDetailService ?? throw new ArgumentNullException(nameof(getPublicSalonDetailService));
        _getPublicQueueStatusService = getPublicQueueStatusService ?? throw new ArgumentNullException(nameof(getPublicQueueStatusService));
        _anonymousJoinService = anonymousJoinService ?? throw new ArgumentNullException(nameof(anonymousJoinService));
        _getQueueEntryStatusService = getQueueEntryStatusService ?? throw new ArgumentNullException(nameof(getQueueEntryStatusService));
        _leaveQueueService = leaveQueueService ?? throw new ArgumentNullException(nameof(leaveQueueService));
        _updateQueueEntryService = updateQueueEntryService ?? throw new ArgumentNullException(nameof(updateQueueEntryService));
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
    public async Task<IActionResult> GetSalonById([FromRoute] string salonId, CancellationToken cancellationToken = default)
    {
        // Parameter validation
        if (string.IsNullOrWhiteSpace(salonId))
        {
            _logger.LogWarning("Invalid salon ID provided: {SalonId}", salonId);
            return BadRequest(new { error = "Salon ID is required" });
        }

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
    public async Task<IActionResult> GetQueueStatus([FromRoute] string salonId, CancellationToken cancellationToken = default)
    {
        // Parameter validation
        if (string.IsNullOrWhiteSpace(salonId))
        {
            _logger.LogWarning("Invalid salon ID provided for queue status: {SalonId}", salonId);
            return BadRequest(new { error = "Salon ID is required" });
        }

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

    /// <summary>
    /// Allows anonymous users to join a queue without authentication
    /// </summary>
    /// <param name="request">Anonymous join request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Queue join result with position and estimated wait time</returns>
    [HttpPost("queue/join")]
    [ProducesResponseType(typeof(AnonymousJoinResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> JoinQueue([FromBody] AnonymousJoinRequest request, CancellationToken cancellationToken = default)
    {
        // Model validation
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? new string[0]
                );
            
            _logger.LogWarning("Model validation failed for anonymous join request: {Errors}", errors);
            return BadRequest(new { errors = errors });
        }

        _logger.LogInformation("Anonymous user joining queue for salon {SalonId}", request.SalonId);

        var result = await _anonymousJoinService.ExecuteAsync(request, cancellationToken);

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

            if (result.Errors.Contains("Salon not found or not accepting customers"))
            {
                return NotFound(new { message = "Salon not found or not accepting customers" });
            }

            if (result.Errors.Contains("User already in queue for this salon"))
            {
                return Conflict(new { message = "User already in queue for this salon" });
            }

            _logger.LogError("Failed to join queue for salon {SalonId}: {Errors}", request.SalonId, string.Join(", ", result.Errors));
            return StatusCode(StatusCodes.Status500InternalServerError, new { errors = result.Errors });
        }

        return Ok(result);
    }

    /// <summary>
    /// Gets the current status of a queue entry
    /// </summary>
    /// <param name="entryId">The queue entry ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Queue entry status information</returns>
    [HttpGet("queue/entry-status/{entryId}")]
    [AllowPublicAccess] // No authentication required for public queue status
    [ProducesResponseType(typeof(QueueEntryStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetQueueEntryStatus([FromRoute] string entryId, CancellationToken cancellationToken = default)
    {
        // Parameter validation
        if (string.IsNullOrWhiteSpace(entryId))
        {
            _logger.LogWarning("Invalid entry ID provided: {EntryId}", entryId);
            return BadRequest(new { error = "Entry ID is required" });
        }

        _logger.LogInformation("Getting queue status for entry {EntryId}", entryId);

        var result = await _getQueueEntryStatusService.ExecuteAsync(entryId, cancellationToken);

        if (!result.Success)
        {
            if (result.FieldErrors.Count > 0)
            {
                return BadRequest(new { errors = result.FieldErrors });
            }

            if (result.Errors.Contains("Queue entry not found"))
            {
                return NotFound(new { message = "Queue entry not found" });
            }

            _logger.LogError("Failed to retrieve queue entry status for entry {EntryId}: {Errors}", entryId, string.Join(", ", result.Errors));
            return StatusCode(StatusCodes.Status500InternalServerError, new { errors = result.Errors });
        }

        return Ok(result.EntryStatus);
    }

    /// <summary>
    /// Allows anonymous users to leave a queue
    /// </summary>
    /// <param name="entryId">The queue entry ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Leave queue result</returns>
    [HttpPost("queue/leave/{entryId}")]
    [AllowPublicAccess] // No authentication required for leaving queue
    [ProducesResponseType(typeof(LeaveQueueResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LeaveQueue([FromRoute] string entryId, CancellationToken cancellationToken = default)
    {
        // Parameter validation
        if (string.IsNullOrWhiteSpace(entryId))
        {
            _logger.LogWarning("Invalid entry ID provided for leave queue: {EntryId}", entryId);
            return BadRequest(new { error = "Entry ID is required" });
        }

        _logger.LogInformation("Anonymous user leaving queue entry {EntryId}", entryId);

        var result = await _leaveQueueService.ExecuteAsync(entryId, cancellationToken);

        if (!result.Success)
        {
            if (result.Errors.Contains("Queue entry not found"))
            {
                return NotFound(result);
            }

            _logger.LogError("Failed to leave queue for entry {EntryId}: {Errors}", entryId, string.Join(", ", result.Errors));
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Updates contact information for a queue entry
    /// </summary>
    /// <param name="entryId">The queue entry ID</param>
    /// <param name="request">Update request with new contact info</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Update result</returns>
    [HttpPut("queue/update/{entryId}")]
    [AllowPublicAccess] // No authentication required for updating contact info
    [ProducesResponseType(typeof(UpdateQueueEntryResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateQueueEntry(
        [FromRoute] string entryId,
        [FromBody] UpdateQueueEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        // Parameter validation
        if (string.IsNullOrWhiteSpace(entryId))
        {
            _logger.LogWarning("Invalid entry ID provided for update: {EntryId}", entryId);
            return BadRequest(new { error = "Entry ID is required" });
        }

        // Model validation
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? new string[0]
                );
            
            _logger.LogWarning("Model validation failed for update queue entry request: {Errors}", errors);
            return BadRequest(new { errors = errors });
        }

        _logger.LogInformation("Updating queue entry {EntryId}", entryId);

        var result = await _updateQueueEntryService.ExecuteAsync(entryId, request, cancellationToken);

        if (!result.Success)
        {
            if (result.Errors.Contains("Queue entry not found"))
            {
                return NotFound(result);
            }

            _logger.LogError("Failed to update queue entry {EntryId}: {Errors}", entryId, string.Join(", ", result.Errors));
            return BadRequest(result);
        }

        return Ok(result);
    }
}

// DTOs are now defined in their respective service files