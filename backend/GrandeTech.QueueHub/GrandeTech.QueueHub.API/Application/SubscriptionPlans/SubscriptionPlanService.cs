using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Domain.Subscriptions;
using Grande.Fila.API.Domain.AuditLogs;
using Grande.Fila.API.Domain.Users;

namespace Grande.Fila.API.Application.SubscriptionPlans
{
    public class SubscriptionPlanService
    {
        private readonly ISubscriptionPlanRepository _subscriptionPlanRepo;
        private readonly IAuditLogRepository _auditLogRepo;

        public SubscriptionPlanService(ISubscriptionPlanRepository subscriptionPlanRepo, IAuditLogRepository auditLogRepo)
        {
            _subscriptionPlanRepo = subscriptionPlanRepo;
            _auditLogRepo = auditLogRepo;
        }

        public async Task<SubscriptionPlanOperationResult> UpdateSubscriptionPlanAsync(UpdateSubscriptionPlanRequest request, string userId, string userRole = "Owner", CancellationToken cancellationToken = default)
        {
            var result = new SubscriptionPlanOperationResult 
            { 
                Success = false, 
                FieldErrors = new Dictionary<string, string>(), 
                Errors = new List<string>()
            };

            // Permissions check (platform admin only)
            if (userRole != UserRoles.PlatformAdmin)
            {
                result.Errors.Add("Forbidden: Only platform Admin can manage subscription plans.");
                return result;
            }

            if (!Guid.TryParse(request.SubscriptionPlanId, out var subscriptionPlanId))
            {
                result.FieldErrors["SubscriptionPlanId"] = "Invalid subscription plan ID format.";
                return result;
            }

            // Validation
            if (string.IsNullOrWhiteSpace(request.Name))
                result.FieldErrors["Name"] = "This field is required.";
            if (request.MonthlyPrice <= 0)
                result.FieldErrors["MonthlyPrice"] = "Monthly price must be greater than zero.";
            if (request.YearlyPrice <= 0)
                result.FieldErrors["YearlyPrice"] = "Yearly price must be greater than zero.";
            if (request.MaxLocations <= 0)
                result.FieldErrors["MaxLocations"] = "Maximum locations must be greater than zero.";
            if (request.MaxStaffPerLocation <= 0)
                result.FieldErrors["MaxStaffPerLocation"] = "Maximum staff per location must be greater than zero.";
            if (request.MaxQueueEntriesPerDay <= 0)
                result.FieldErrors["MaxQueueEntriesPerDay"] = "Maximum queue entries per day must be greater than zero.";

            if (result.FieldErrors.Count > 0)
                return result;

            // Get subscription plan
            var subscriptionPlan = await _subscriptionPlanRepo.GetByIdAsync(subscriptionPlanId, cancellationToken);
            if (subscriptionPlan == null)
            {
                result.Errors.Add("Subscription plan not found.");
                return result;
            }

            // Check if new name already exists (excluding current plan)
            var existingPlan = await _subscriptionPlanRepo.GetByNameAsync(request.Name, cancellationToken);
            if (existingPlan != null && existingPlan.Id != subscriptionPlanId)
            {
                result.FieldErrors["Name"] = "Subscription plan name already exists.";
                return result;
            }

            try
            {
                subscriptionPlan.UpdateDetails(
                    request.Name,
                    request.Description ?? string.Empty,
                    request.MonthlyPrice,
                    request.YearlyPrice,
                    request.MaxLocations,
                    request.MaxStaffPerLocation,
                    request.IncludesAnalytics,
                    request.IncludesAdvancedReporting,
                    request.IncludesCustomBranding,
                    request.IncludesAdvertising,
                    request.IncludesMultipleLocations,
                    request.MaxQueueEntriesPerDay,
                    request.IsFeatured,
                    userId
                );

                await _subscriptionPlanRepo.UpdateAsync(subscriptionPlan, cancellationToken);
                result.Success = true;

                // Audit log
                await _auditLogRepo.LogAsync(new AuditLogEntry {
                    UserId = userId,
                    Action = "UpdateSubscriptionPlan",
                    EntityId = subscriptionPlan.Id.ToString(),
                    EntityType = "SubscriptionPlan",
                    TimestampUtc = DateTime.UtcNow
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Unable to update subscription plan: {ex.Message}");
            }

            return result;
        }

        public async Task<SubscriptionPlanOperationResult> ActivateSubscriptionPlanAsync(string subscriptionPlanId, string userId, string userRole = "Owner", CancellationToken cancellationToken = default)
        {
            var result = new SubscriptionPlanOperationResult 
            { 
                Success = false, 
                FieldErrors = new Dictionary<string, string>(), 
                Errors = new List<string>()
            };

            // Permissions check (platform admin only)
            if (userRole != UserRoles.PlatformAdmin)
            {
                result.Errors.Add("Forbidden: Only platform Admin can manage subscription plans.");
                return result;
            }

            if (!Guid.TryParse(subscriptionPlanId, out var planId))
            {
                result.FieldErrors["SubscriptionPlanId"] = "Invalid subscription plan ID format.";
                return result;
            }

            // Get subscription plan
            var subscriptionPlan = await _subscriptionPlanRepo.GetByIdAsync(planId, cancellationToken);
            if (subscriptionPlan == null)
            {
                result.Errors.Add("Subscription plan not found.");
                return result;
            }

            try
            {
                subscriptionPlan.Activate(userId);
                await _subscriptionPlanRepo.UpdateAsync(subscriptionPlan, cancellationToken);
                result.Success = true;

                // Audit log
                await _auditLogRepo.LogAsync(new AuditLogEntry {
                    UserId = userId,
                    Action = "ActivateSubscriptionPlan",
                    EntityId = subscriptionPlan.Id.ToString(),
                    EntityType = "SubscriptionPlan",
                    TimestampUtc = DateTime.UtcNow
                }, cancellationToken);
            }
            catch (Exception)
            {
                result.Errors.Add("Unable to activate subscription plan. Please try again later.");
            }

            return result;
        }

        public async Task<SubscriptionPlanOperationResult> DeactivateSubscriptionPlanAsync(string subscriptionPlanId, string userId, string userRole = "Owner", CancellationToken cancellationToken = default)
        {
            var result = new SubscriptionPlanOperationResult 
            { 
                Success = false, 
                FieldErrors = new Dictionary<string, string>(), 
                Errors = new List<string>()
            };

            // Permissions check (platform admin only)
            if (userRole != UserRoles.PlatformAdmin)
            {
                result.Errors.Add("Forbidden: Only platform Admin can manage subscription plans.");
                return result;
            }

            if (!Guid.TryParse(subscriptionPlanId, out var planId))
            {
                result.FieldErrors["SubscriptionPlanId"] = "Invalid subscription plan ID format.";
                return result;
            }

            // Get subscription plan
            var subscriptionPlan = await _subscriptionPlanRepo.GetByIdAsync(planId, cancellationToken);
            if (subscriptionPlan == null)
            {
                result.Errors.Add("Subscription plan not found.");
                return result;
            }

            try
            {
                subscriptionPlan.Deactivate(userId);
                await _subscriptionPlanRepo.UpdateAsync(subscriptionPlan, cancellationToken);
                result.Success = true;

                // Audit log
                await _auditLogRepo.LogAsync(new AuditLogEntry {
                    UserId = userId,
                    Action = "DeactivateSubscriptionPlan",
                    EntityId = subscriptionPlan.Id.ToString(),
                    EntityType = "SubscriptionPlan",
                    TimestampUtc = DateTime.UtcNow
                }, cancellationToken);
            }
            catch (Exception)
            {
                result.Errors.Add("Unable to deactivate subscription plan. Please try again later.");
            }

            return result;
        }
    }
} 