using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Domain.Subscriptions;
using Grande.Fila.API.Domain.AuditLogs;
using Grande.Fila.API.Domain.Users;

namespace Grande.Fila.API.Application.SubscriptionPlans
{
    public class CreateSubscriptionPlanService
    {
        private readonly ISubscriptionPlanRepository _subscriptionPlanRepo;
        private readonly IAuditLogRepository _auditLogRepo;

        public CreateSubscriptionPlanService(ISubscriptionPlanRepository subscriptionPlanRepo, IAuditLogRepository auditLogRepo)
        {
            _subscriptionPlanRepo = subscriptionPlanRepo;
            _auditLogRepo = auditLogRepo;
        }

        public async Task<CreateSubscriptionPlanResult> CreateSubscriptionPlanAsync(CreateSubscriptionPlanRequest request, string userId, string userRole = "Admin", CancellationToken cancellationToken = default)
        {
            var result = new CreateSubscriptionPlanResult 
            { 
                Success = false, 
                FieldErrors = new Dictionary<string, string>(), 
                Errors = new List<string>(),
                SubscriptionPlanId = string.Empty,
                Name = string.Empty
            };

            // Permissions check (platform admin only)
            if (userRole != UserRoles.PlatformAdmin)
            {
                result.Errors.Add("Forbidden: Only platform Admin can create subscription plans.");
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

            // Check if subscription plan name already exists
            var existingPlan = await _subscriptionPlanRepo.GetByNameAsync(request.Name, cancellationToken);
            if (existingPlan != null)
            {
                result.FieldErrors["Name"] = "Subscription plan name already exists.";
                return result;
            }

            // Create SubscriptionPlan
            var subscriptionPlan = new SubscriptionPlan(
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

            try
            {
                await _subscriptionPlanRepo.AddAsync(subscriptionPlan, cancellationToken);
            }
            catch (Exception)
            {
                result.Errors.Add("Unable to create subscription plan. Please try again later.");
                return result;
            }

            result.Success = true;
            result.SubscriptionPlanId = subscriptionPlan.Id.ToString();
            result.Name = subscriptionPlan.Name;

            // Audit log
            await _auditLogRepo.LogAsync(new AuditLogEntry {
                UserId = userId,
                Action = "CreateSubscriptionPlan",
                EntityId = subscriptionPlan.Id.ToString(),
                EntityType = "SubscriptionPlan",
                TimestampUtc = DateTime.UtcNow
            }, cancellationToken);

            return result;
        }
    }
} 