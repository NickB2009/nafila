using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Grande.Fila.API.Application.SubscriptionPlans;
using Grande.Fila.API.Domain.Subscriptions;
using Grande.Fila.API.Domain.Users;
using Grande.Fila.API.Infrastructure.Authorization;

namespace Grande.Fila.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SubscriptionPlansController : ControllerBase
    {
        private readonly CreateSubscriptionPlanService _createSubscriptionPlanService;
        private readonly SubscriptionPlanService _subscriptionPlanService;
        private readonly ISubscriptionPlanRepository _subscriptionPlanRepository;

        public SubscriptionPlansController(
            CreateSubscriptionPlanService createSubscriptionPlanService,
            SubscriptionPlanService subscriptionPlanService,
            ISubscriptionPlanRepository subscriptionPlanRepository)
        {
            _createSubscriptionPlanService = createSubscriptionPlanService;
            _subscriptionPlanService = subscriptionPlanService;
            _subscriptionPlanRepository = subscriptionPlanRepository;
        }

        /// <summary>
        /// Creates a new subscription plan (UC-SUBPLAN)
        /// </summary>
        [HttpPost]
        [RequireAdmin]
        public async Task<ActionResult<CreateSubscriptionPlanResult>> CreateSubscriptionPlan(
            [FromBody] CreateSubscriptionPlanRequest request,
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(TenantClaims.UserId)?.Value ?? throw new UnauthorizedAccessException("User ID not found in claims");
            var userRole = User.FindFirst(TenantClaims.Role)?.Value ?? "User";

            var result = await _createSubscriptionPlanService.CreateSubscriptionPlanAsync(request, userId, userRole, cancellationToken);

            if (!result.Success)
            {
                if (result.FieldErrors.Count > 0)
                    return BadRequest(result);
                if (result.Errors.Count > 0)
                    return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Updates subscription plan details (UC-SUBPLAN)
        /// </summary>
        [HttpPut("{subscriptionPlanId}")]
        [RequireAdmin]
        public async Task<ActionResult<SubscriptionPlanOperationResult>> UpdateSubscriptionPlan(
            string subscriptionPlanId,
            [FromBody] UpdateSubscriptionPlanRequest request,
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(TenantClaims.UserId)?.Value ?? throw new UnauthorizedAccessException("User ID not found in claims");
            var userRole = User.FindFirst(TenantClaims.Role)?.Value ?? "User";

            request.SubscriptionPlanId = subscriptionPlanId;
            var result = await _subscriptionPlanService.UpdateSubscriptionPlanAsync(request, userId, userRole, cancellationToken);

            if (!result.Success)
            {
                if (result.FieldErrors.Count > 0)
                    return BadRequest(result);
                if (result.Errors.Count > 0)
                    return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Activates a subscription plan (UC-SUBPLAN)
        /// </summary>
        [HttpPut("{subscriptionPlanId}/activate")]
        [RequireAdmin]
        public async Task<ActionResult<SubscriptionPlanOperationResult>> ActivateSubscriptionPlan(
            string subscriptionPlanId,
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(TenantClaims.UserId)?.Value ?? throw new UnauthorizedAccessException("User ID not found in claims");
            var userRole = User.FindFirst(TenantClaims.Role)?.Value ?? "User";

            var result = await _subscriptionPlanService.ActivateSubscriptionPlanAsync(subscriptionPlanId, userId, userRole, cancellationToken);

            if (!result.Success)
            {
                if (result.FieldErrors.Count > 0)
                    return BadRequest(result);
                if (result.Errors.Count > 0)
                    return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Deactivates a subscription plan (UC-SUBPLAN)
        /// </summary>
        [HttpPut("{subscriptionPlanId}/deactivate")]
        [RequireAdmin]
        public async Task<ActionResult<SubscriptionPlanOperationResult>> DeactivateSubscriptionPlan(
            string subscriptionPlanId,
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(TenantClaims.UserId)?.Value ?? throw new UnauthorizedAccessException("User ID not found in claims");
            var userRole = User.FindFirst(TenantClaims.Role)?.Value ?? "User";

            var result = await _subscriptionPlanService.DeactivateSubscriptionPlanAsync(subscriptionPlanId, userId, userRole, cancellationToken);

            if (!result.Success)
            {
                if (result.FieldErrors.Count > 0)
                    return BadRequest(result);
                if (result.Errors.Count > 0)
                    return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Gets all active subscription plans (UC-SUBPLAN)
        /// </summary>
        [HttpGet]
        [RequireAdmin]
        public async Task<ActionResult<IEnumerable<object>>> GetSubscriptionPlans(CancellationToken cancellationToken)
        {
            var subscriptionPlans = await _subscriptionPlanRepository.GetActiveSubscriptionPlansAsync(cancellationToken);

            var result = subscriptionPlans.Select(plan => new
            {
                Id = plan.Id,
                Name = plan.Name,
                Description = plan.Description,
                MonthlyPrice = plan.MonthlyPriceAmount,
                MonthlyPriceCurrency = plan.MonthlyPriceCurrency,
                YearlyPrice = plan.YearlyPriceAmount,
                YearlyPriceCurrency = plan.YearlyPriceCurrency,
                IsActive = plan.IsActive,
                IsDefault = plan.IsDefault,
                MaxLocations = plan.MaxLocations,
                MaxStaffPerLocation = plan.MaxStaffPerLocation,
                IncludesAnalytics = plan.IncludesAnalytics,
                IncludesAdvancedReporting = plan.IncludesAdvancedReporting,
                IncludesCustomBranding = plan.IncludesCustomBranding,
                IncludesAdvertising = plan.IncludesAdvertising,
                IncludesMultipleLocations = plan.IncludesMultipleLocations,
                MaxQueueEntriesPerDay = plan.MaxQueueEntriesPerDay,
                IsFeatured = plan.IsFeatured,
                CreatedAt = plan.CreatedAt,
                UpdatedAt = plan.LastModifiedAt
            });

            return Ok(result);
        }

        /// <summary>
        /// Gets a specific subscription plan by ID (UC-SUBPLAN)
        /// </summary>
        [HttpGet("{subscriptionPlanId}")]
        [RequireAdmin]
        public async Task<ActionResult<object>> GetSubscriptionPlan(string subscriptionPlanId, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(subscriptionPlanId, out var planId))
            {
                return BadRequest("Invalid subscription plan ID format.");
            }

            var subscriptionPlan = await _subscriptionPlanRepository.GetByIdAsync(planId, cancellationToken);
            if (subscriptionPlan == null)
            {
                return NotFound("Subscription plan not found.");
            }

            var result = new
            {
                Id = subscriptionPlan.Id,
                Name = subscriptionPlan.Name,
                Description = subscriptionPlan.Description,
                MonthlyPrice = subscriptionPlan.MonthlyPriceAmount,
                MonthlyPriceCurrency = subscriptionPlan.MonthlyPriceCurrency,
                YearlyPrice = subscriptionPlan.YearlyPriceAmount,
                YearlyPriceCurrency = subscriptionPlan.YearlyPriceCurrency,
                IsActive = subscriptionPlan.IsActive,
                IsDefault = subscriptionPlan.IsDefault,
                MaxLocations = subscriptionPlan.MaxLocations,
                MaxStaffPerLocation = subscriptionPlan.MaxStaffPerLocation,
                IncludesAnalytics = subscriptionPlan.IncludesAnalytics,
                IncludesAdvancedReporting = subscriptionPlan.IncludesAdvancedReporting,
                IncludesCustomBranding = subscriptionPlan.IncludesCustomBranding,
                IncludesAdvertising = subscriptionPlan.IncludesAdvertising,
                IncludesMultipleLocations = subscriptionPlan.IncludesMultipleLocations,
                MaxQueueEntriesPerDay = subscriptionPlan.MaxQueueEntriesPerDay,
                IsFeatured = subscriptionPlan.IsFeatured,
                CreatedAt = subscriptionPlan.CreatedAt,
                UpdatedAt = subscriptionPlan.LastModifiedAt
            };

            return Ok(result);
        }

        /// <summary>
        /// Gets the default subscription plan (public endpoint for organization creation)
        /// </summary>
        [HttpGet("default")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetDefaultSubscriptionPlan(CancellationToken cancellationToken)
        {
            var subscriptionPlan = await _subscriptionPlanRepository.GetDefaultSubscriptionPlanAsync(cancellationToken);
            if (subscriptionPlan == null)
            {
                return NotFound("No default subscription plan found.");
            }

            var result = new
            {
                Id = subscriptionPlan.Id,
                Name = subscriptionPlan.Name,
                Description = subscriptionPlan.Description,
                MonthlyPrice = subscriptionPlan.MonthlyPriceAmount,
                MonthlyPriceCurrency = subscriptionPlan.MonthlyPriceCurrency,
                YearlyPrice = subscriptionPlan.YearlyPriceAmount,
                YearlyPriceCurrency = subscriptionPlan.YearlyPriceCurrency,
                MaxLocations = subscriptionPlan.MaxLocations,
                MaxStaffPerLocation = subscriptionPlan.MaxStaffPerLocation,
                IncludesAnalytics = subscriptionPlan.IncludesAnalytics,
                IncludesAdvancedReporting = subscriptionPlan.IncludesAdvancedReporting,
                IncludesCustomBranding = subscriptionPlan.IncludesCustomBranding,
                IncludesAdvertising = subscriptionPlan.IncludesAdvertising,
                IncludesMultipleLocations = subscriptionPlan.IncludesMultipleLocations,
                MaxQueueEntriesPerDay = subscriptionPlan.MaxQueueEntriesPerDay,
                IsFeatured = subscriptionPlan.IsFeatured
            };

            return Ok(result);
        }
    }
} 