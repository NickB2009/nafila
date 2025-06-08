using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Domain.Organizations;
using GrandeTech.QueueHub.API.Domain.Common.ValueObjects;
using GrandeTech.QueueHub.API.Domain.AuditLogs;
using GrandeTech.QueueHub.API.Domain.Subscriptions;

namespace GrandeTech.QueueHub.API.Application.Organizations
{    public class CreateOrganizationService
    {
        private readonly IOrganizationRepository _organizationRepo;
        private readonly IAuditLogRepository _auditLogRepo;
        private readonly ISubscriptionPlanRepository _subscriptionPlanRepo;

        public CreateOrganizationService(IOrganizationRepository organizationRepo, IAuditLogRepository auditLogRepo, ISubscriptionPlanRepository subscriptionPlanRepo)
        {
            _organizationRepo = organizationRepo;
            _auditLogRepo = auditLogRepo;
            _subscriptionPlanRepo = subscriptionPlanRepo;
        }

        public async Task<CreateOrganizationResult> CreateOrganizationAsync(CreateOrganizationRequest request, string userId, string userRole = "Admin", CancellationToken cancellationToken = default)
        {
            var result = new CreateOrganizationResult 
            { 
                Success = false, 
                FieldErrors = new Dictionary<string, string>(), 
                Errors = new List<string>(),
                OrganizationId = string.Empty,
                Status = "Pending",
                Slug = string.Empty
            };

            // Permissions check (platform-level admin only for creating organizations)
            if (userRole != "Admin")
            {
                result.Errors.Add("Forbidden: Only platform Admin can create organizations.");
                return result;
            }            // Validation
            if (string.IsNullOrWhiteSpace(request.Name))
                result.FieldErrors["Name"] = "This field is required.";
            if (string.IsNullOrWhiteSpace(request.Slug))
                result.FieldErrors["Slug"] = "This field is required.";
            if (string.IsNullOrWhiteSpace(request.ContactEmail))
                result.FieldErrors["ContactEmail"] = "This field is required.";
            if (string.IsNullOrWhiteSpace(request.SubscriptionPlanId))
                result.FieldErrors["SubscriptionPlanId"] = "This field is required.";

            // Email format validation
            if (!string.IsNullOrWhiteSpace(request.ContactEmail))
            {
                try { var _ = Email.Create(request.ContactEmail); }
                catch { result.FieldErrors["ContactEmail"] = "Enter a valid email address."; }
            }

            // Phone format validation
            if (!string.IsNullOrWhiteSpace(request.ContactPhone))
            {
                try { var _ = PhoneNumber.Create(request.ContactPhone); }
                catch { result.FieldErrors["ContactPhone"] = "Invalid phone number format. Use format: +5511999999999"; }
            }            // Subscription plan ID format validation
            if (!Guid.TryParse(request.SubscriptionPlanId, out var subscriptionPlanId))
            {
                result.FieldErrors["SubscriptionPlanId"] = "Invalid subscription plan ID format.";
            }
            else
            {
                // Check if subscription plan exists
                var subscriptionPlan = await _subscriptionPlanRepo.GetByIdAsync(subscriptionPlanId, cancellationToken);
                if (subscriptionPlan == null)
                {
                    result.FieldErrors["SubscriptionPlanId"] = "Subscription plan not found.";
                }
            }

            if (result.FieldErrors.Count > 0)
                return result;

            // Check slug uniqueness
            var slugToUse = request.Slug ?? request.Name;
            var isSlugUnique = await _organizationRepo.IsSlugUniqueAsync(slugToUse, cancellationToken);
            if (!isSlugUnique)
            {
                result.FieldErrors["Slug"] = $"Slug '{slugToUse}' is already taken.";
                return result;
            }

            // Create BrandingConfig if branding details provided
            BrandingConfig? brandingConfig = null;
            if (!string.IsNullOrWhiteSpace(request.PrimaryColor) || !string.IsNullOrWhiteSpace(request.SecondaryColor))
            {
                try
                {
                    brandingConfig = BrandingConfig.Create(
                        request.PrimaryColor ?? "#007bff",
                        request.SecondaryColor ?? "#6c757d",
                        request.LogoUrl ?? string.Empty,
                        request.FaviconUrl ?? string.Empty,
                        request.Name,
                        request.TagLine ?? string.Empty
                    );
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Invalid branding configuration: {ex.Message}");
                    return result;
                }
            }

            // Create Organization
            var organization = new Organization(
                request.Name,
                slugToUse,
                request.Description,
                request.ContactEmail,
                request.ContactPhone,
                request.WebsiteUrl,
                brandingConfig,
                subscriptionPlanId,
                userId
            );

            // Set analytics sharing preference
            if (request.SharesDataForAnalytics)
            {
                organization.SetAnalyticsSharing(true, userId);
            }

            try
            {
                await _organizationRepo.AddAsync(organization, cancellationToken);
            }
            catch (Exception)
            {
                result.Errors.Add("Unable to create organization. Please try again later.");
                return result;
            }

            result.Success = true;
            result.OrganizationId = organization.Id.ToString();
            result.Status = organization.IsActive ? "Active" : "Inactive";
            result.Slug = organization.Slug.Value;

            // Audit log
            await _auditLogRepo.LogAsync(new AuditLogEntry {
                UserId = userId,
                Action = "CreateOrganization",
                EntityId = organization.Id.ToString(),
                EntityType = "Organization",
                TimestampUtc = DateTime.UtcNow
            }, cancellationToken);

            return result;
        }
    }
}
