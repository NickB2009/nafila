using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Grande.Fila.API.Infrastructure.Services;
using Grande.Fila.API.Infrastructure.Authorization;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.Locations;
using Grande.Fila.API.Domain.Organizations;
using Grande.Fila.API.Domain.Staff;
using Grande.Fila.API.Domain.Customers;
using Grande.Fila.API.Domain.Users;
using Grande.Fila.API.Domain.ServicesOffered;

namespace Grande.Fila.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BulkOperationsController : ControllerBase
    {
        private readonly IBulkOperationsService _bulkOperationsService;
        private readonly ILogger<BulkOperationsController> _logger;

        public BulkOperationsController(
            IBulkOperationsService bulkOperationsService,
            ILogger<BulkOperationsController> logger)
        {
            _bulkOperationsService = bulkOperationsService;
            _logger = logger;
        }

        /// <summary>
        /// Bulk insert queue entries
        /// </summary>
        /// <param name="queueEntries">Queue entries to insert</param>
        /// <param name="batchSize">Batch size for processing (default: optimal)</param>
        /// <returns>Bulk operation result</returns>
        [HttpPost("queue-entries/insert")]
        [Authorize(Policy = "AdminOrBarber")]
        [ProducesResponseType(typeof(BulkOperationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> BulkInsertQueueEntries(
            [FromBody] IEnumerable<QueueEntry> queueEntries,
            [FromQuery] int batchSize = 0,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var effectiveBatchSize = batchSize > 0 ? batchSize : _bulkOperationsService.GetOptimalBatchSize<QueueEntry>();
                
                _logger.LogInformation("Starting bulk insert of {Count} queue entries with batch size {BatchSize}", 
                    queueEntries.Count(), effectiveBatchSize);

                var result = await _bulkOperationsService.BulkInsertAsync(queueEntries, effectiveBatchSize, cancellationToken);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to bulk insert queue entries");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Bulk insert failed" });
            }
        }

        /// <summary>
        /// Bulk update queue entries
        /// </summary>
        /// <param name="queueEntries">Queue entries to update</param>
        /// <param name="batchSize">Batch size for processing (default: optimal)</param>
        /// <returns>Bulk operation result</returns>
        [HttpPost("queue-entries/update")]
        [Authorize(Policy = "AdminOrBarber")]
        [ProducesResponseType(typeof(BulkOperationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> BulkUpdateQueueEntries(
            [FromBody] IEnumerable<QueueEntry> queueEntries,
            [FromQuery] int batchSize = 0,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var effectiveBatchSize = batchSize > 0 ? batchSize : _bulkOperationsService.GetOptimalBatchSize<QueueEntry>();
                
                _logger.LogInformation("Starting bulk update of {Count} queue entries with batch size {BatchSize}", 
                    queueEntries.Count(), effectiveBatchSize);

                var result = await _bulkOperationsService.BulkUpdateAsync(queueEntries, effectiveBatchSize, cancellationToken);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to bulk update queue entries");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Bulk update failed" });
            }
        }

        /// <summary>
        /// Bulk delete queue entries by IDs
        /// </summary>
        /// <param name="queueEntryIds">Queue entry IDs to delete</param>
        /// <param name="batchSize">Batch size for processing (default: optimal)</param>
        /// <returns>Bulk operation result</returns>
        [HttpPost("queue-entries/delete")]
        [Authorize(Policy = "AdminOrBarber")]
        [ProducesResponseType(typeof(BulkOperationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> BulkDeleteQueueEntries(
            [FromBody] IEnumerable<Guid> queueEntryIds,
            [FromQuery] int batchSize = 0,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var effectiveBatchSize = batchSize > 0 ? batchSize : _bulkOperationsService.GetOptimalBatchSize<QueueEntry>();
                
                _logger.LogInformation("Starting bulk delete of {Count} queue entries with batch size {BatchSize}", 
                    queueEntryIds.Count(), effectiveBatchSize);

                var result = await _bulkOperationsService.BulkDeleteAsync<QueueEntry>(queueEntryIds.Cast<object>(), effectiveBatchSize, cancellationToken);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to bulk delete queue entries");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Bulk delete failed" });
            }
        }

        /// <summary>
        /// Bulk upsert customers (insert or update based on email)
        /// </summary>
        /// <param name="customers">Customers to upsert</param>
        /// <param name="batchSize">Batch size for processing (default: optimal)</param>
        /// <returns>Bulk operation result</returns>
        [HttpPost("customers/upsert")]
        [Authorize(Policy = "AdminOrBarber")]
        [ProducesResponseType(typeof(BulkOperationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> BulkUpsertCustomers(
            [FromBody] IEnumerable<Customer> customers,
            [FromQuery] int batchSize = 0,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var effectiveBatchSize = batchSize > 0 ? batchSize : _bulkOperationsService.GetOptimalBatchSize<Customer>();
                
                _logger.LogInformation("Starting bulk upsert of {Count} customers with batch size {BatchSize}", 
                    customers.Count(), effectiveBatchSize);

                var result = await _bulkOperationsService.BulkUpsertAsync(customers, new[] { "Email" }, effectiveBatchSize, cancellationToken);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to bulk upsert customers");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Bulk upsert failed" });
            }
        }

        /// <summary>
        /// Bulk insert staff members
        /// </summary>
        /// <param name="staffMembers">Staff members to insert</param>
        /// <param name="batchSize">Batch size for processing (default: optimal)</param>
        /// <returns>Bulk operation result</returns>
        [HttpPost("staff-members/insert")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(BulkOperationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> BulkInsertStaffMembers(
            [FromBody] IEnumerable<StaffMember> staffMembers,
            [FromQuery] int batchSize = 0,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var effectiveBatchSize = batchSize > 0 ? batchSize : _bulkOperationsService.GetOptimalBatchSize<StaffMember>();
                
                _logger.LogInformation("Starting bulk insert of {Count} staff members with batch size {BatchSize}", 
                    staffMembers.Count(), effectiveBatchSize);

                var result = await _bulkOperationsService.BulkInsertAsync(staffMembers, effectiveBatchSize, cancellationToken);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to bulk insert staff members");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Bulk insert failed" });
            }
        }

        /// <summary>
        /// Bulk insert services offered
        /// </summary>
        /// <param name="servicesOffered">Services to insert</param>
        /// <param name="batchSize">Batch size for processing (default: optimal)</param>
        /// <returns>Bulk operation result</returns>
        [HttpPost("services-offered/insert")]
        [Authorize(Policy = "AdminOrBarber")]
        [ProducesResponseType(typeof(BulkOperationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> BulkInsertServicesOffered(
            [FromBody] IEnumerable<ServiceOffered> servicesOffered,
            [FromQuery] int batchSize = 0,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var effectiveBatchSize = batchSize > 0 ? batchSize : _bulkOperationsService.GetOptimalBatchSize<ServiceOffered>();
                
                _logger.LogInformation("Starting bulk insert of {Count} services with batch size {BatchSize}", 
                    servicesOffered.Count(), effectiveBatchSize);

                var result = await _bulkOperationsService.BulkInsertAsync(servicesOffered, effectiveBatchSize, cancellationToken);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to bulk insert services offered");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Bulk insert failed" });
            }
        }

        /// <summary>
        /// Execute custom bulk SQL operation
        /// </summary>
        /// <param name="request">Bulk SQL execution request</param>
        /// <returns>Bulk operation result</returns>
        [HttpPost("execute-sql")]
        [Authorize(Policy = "PlatformAdminOnly")]
        [ProducesResponseType(typeof(BulkOperationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ExecuteBulkSql(
            [FromBody] BulkSqlRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Starting bulk SQL execution with {ParameterCount} parameter sets", request.Parameters.Count());

                var result = await _bulkOperationsService.ExecuteBulkSqlAsync(
                    request.Sql, 
                    request.Parameters, 
                    request.BatchSize, 
                    cancellationToken);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute bulk SQL");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Bulk SQL execution failed" });
            }
        }

        /// <summary>
        /// Get bulk operation statistics
        /// </summary>
        /// <returns>Bulk operation statistics</returns>
        [HttpGet("stats")]
        [Authorize(Policy = "PlatformAdminOnly")]
        [ProducesResponseType(typeof(BulkOperationStats), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetBulkOperationStats(CancellationToken cancellationToken = default)
        {
            try
            {
                var stats = await _bulkOperationsService.GetBulkOperationStatsAsync(cancellationToken);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get bulk operation statistics");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Failed to get statistics" });
            }
        }

        /// <summary>
        /// Get optimal batch size for a specific entity type
        /// </summary>
        /// <param name="entityType">Entity type name</param>
        /// <returns>Optimal batch size</returns>
        [HttpGet("batch-size/{entityType}")]
        [Authorize(Policy = "AdminOrBarber")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult GetOptimalBatchSize(string entityType)
        {
            try
            {
                var batchSize = entityType.ToLower() switch
                {
                    "queueentry" => _bulkOperationsService.GetOptimalBatchSize<QueueEntry>(),
                    "queue" => _bulkOperationsService.GetOptimalBatchSize<Queue>(),
                    "location" => _bulkOperationsService.GetOptimalBatchSize<Location>(),
                    "organization" => _bulkOperationsService.GetOptimalBatchSize<Organization>(),
                    "staffmember" => _bulkOperationsService.GetOptimalBatchSize<StaffMember>(),
                    "customer" => _bulkOperationsService.GetOptimalBatchSize<Customer>(),
                    "user" => _bulkOperationsService.GetOptimalBatchSize<User>(),
                    "serviceoffered" => _bulkOperationsService.GetOptimalBatchSize<ServiceOffered>(),
                    _ => 1000 // Default
                };

                return Ok(new { EntityType = entityType, OptimalBatchSize = batchSize });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get optimal batch size for entity type {EntityType}", entityType);
                return BadRequest(new { error = "Invalid entity type" });
            }
        }
    }

    /// <summary>
    /// Request model for bulk SQL execution
    /// </summary>
    public class BulkSqlRequest
    {
        public string Sql { get; set; } = string.Empty;
        public IEnumerable<object[]> Parameters { get; set; } = new List<object[]>();
        public int BatchSize { get; set; } = 1000;
    }
}
