using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Grande.Fila.API.Application.Queues;
using Grande.Fila.API.Application.Queues.Models;
using Grande.Fila.API.Infrastructure.Authorization;

namespace Grande.Fila.API.Controllers
{
    /// <summary>
    /// Controller for queue transfer operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class QueueTransferController : ControllerBase
    {
        private readonly IQueueTransferService _queueTransferService;

        public QueueTransferController(IQueueTransferService queueTransferService)
        {
            _queueTransferService = queueTransferService;
        }

        /// <summary>
        /// Transfer a queue entry to another salon, service, or time slot
        /// </summary>
        [HttpPost("transfer")]
        [AllowPublicAccess] // Allow anonymous transfers
        public async Task<IActionResult> TransferQueueEntry(
            [FromBody] QueueTransferRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _queueTransferService.TransferQueueEntryAsync(request, cancellationToken);
            
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Get transfer suggestions for a queue entry
        /// </summary>
        [HttpPost("suggestions")]
        [AllowPublicAccess] // Allow anonymous access
        public async Task<IActionResult> GetTransferSuggestions(
            [FromBody] TransferSuggestionsRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _queueTransferService.GetTransferSuggestionsAsync(request, cancellationToken);
            
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Check if a queue entry is eligible for transfer
        /// </summary>
        [HttpPost("eligibility")]
        [AllowPublicAccess] // Allow anonymous access
        public async Task<IActionResult> CheckTransferEligibility(
            [FromBody] TransferEligibilityRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _queueTransferService.CheckTransferEligibilityAsync(request, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Perform bulk transfer of multiple queue entries (staff only)
        /// </summary>
        [HttpPost("bulk-transfer")]
        [Authorize(Policy = "RequireStaff")]
        public async Task<IActionResult> BulkTransfer(
            [FromBody] BulkTransferRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _queueTransferService.BulkTransferAsync(request, cancellationToken);
            
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Get transfer analytics for a specific transfer
        /// </summary>
        [HttpGet("analytics/{transferId}")]
        [Authorize(Policy = "RequireStaff")]
        public async Task<IActionResult> GetTransferAnalytics(
            string transferId,
            CancellationToken cancellationToken)
        {
            var result = await _queueTransferService.GetTransferAnalyticsAsync(transferId, cancellationToken);
            
            if (result != null)
            {
                return Ok(result);
            }

            return NotFound();
        }

        /// <summary>
        /// Get transfer history for a customer
        /// </summary>
        [HttpGet("history/{customerId}")]
        [AllowPublicAccess] // Allow customers to see their own history
        public async Task<IActionResult> GetCustomerTransferHistory(
            string customerId,
            CancellationToken cancellationToken)
        {
            var result = await _queueTransferService.GetCustomerTransferHistoryAsync(customerId, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Cancel a pending transfer
        /// </summary>
        [HttpPost("cancel/{transferId}")]
        [AllowPublicAccess] // Allow customers to cancel their own transfers
        public async Task<IActionResult> CancelTransfer(
            string transferId,
            CancellationToken cancellationToken)
        {
            var result = await _queueTransferService.CancelTransferAsync(transferId, cancellationToken);
            
            if (result)
            {
                return Ok(new { success = true, message = "Transfer cancelled successfully" });
            }

            return BadRequest(new { success = false, message = "Failed to cancel transfer" });
        }

        /// <summary>
        /// Get transfer statistics for a salon (staff only)
        /// </summary>
        [HttpGet("stats/{salonId}")]
        [Authorize(Policy = "RequireStaff")]
        public async Task<IActionResult> GetSalonTransferStats(
            string salonId,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            CancellationToken cancellationToken = default)
        {
            var result = await _queueTransferService.GetSalonTransferStatsAsync(
                salonId, fromDate, toDate, cancellationToken);
            
            return Ok(result);
        }

        /// <summary>
        /// Get nearby salons for transfer suggestions
        /// </summary>
        [HttpGet("nearby-salons/{salonId}")]
        [AllowPublicAccess]
        public async Task<IActionResult> GetNearbySalonsForTransfer(
            string salonId,
            [FromQuery] double maxDistanceKm = 5.0,
            [FromQuery] int maxResults = 10,
            CancellationToken cancellationToken = default)
        {
            // TODO: Implement nearby salons lookup
            var mockSalons = new[]
            {
                new
                {
                    id = Guid.NewGuid().ToString(),
                    name = "Barbearia Moderna",
                    address = "Rua das Flores, 123",
                    distanceKm = 0.8,
                    currentWaitMinutes = 15,
                    queueLength = 2,
                    isOpen = true
                },
                new
                {
                    id = Guid.NewGuid().ToString(),
                    name = "Studio Hair",
                    address = "Av. Paulista, 456",
                    distanceKm = 1.2,
                    currentWaitMinutes = 25,
                    queueLength = 4,
                    isOpen = true
                }
            };

            return Ok(new { salons = mockSalons });
        }
    }
}
