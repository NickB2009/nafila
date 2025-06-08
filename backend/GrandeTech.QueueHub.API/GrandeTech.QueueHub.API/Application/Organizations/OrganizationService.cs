using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Domain.Organizations;
using GrandeTech.QueueHub.API.Domain.Common.ValueObjects;
using GrandeTech.QueueHub.API.Domain.AuditLogs;

namespace GrandeTech.QueueHub.API.Application.Organizations
{
    public class OrganizationService
    {
        private readonly IOrganizationRepository _organizationRepo;
        private readonly IAuditLogRepository _auditLogRepo;

        public OrganizationService(IOrganizationRepository organizationRepo, IAuditLogRepository auditLogRepo)
        {
            _organizationRepo = organizationRepo;
            _auditLogRepo = auditLogRepo;
        }

        public async Task<OrganizationOperationResult> UpdateOrganizationAsync(UpdateOrganizationRequest request, string userId, string userRole = "Admin", CancellationToken cancellationToken = default)
        {
            var result = new OrganizationOperationResult 
            { 
                Success = false, 
                FieldErrors = new Dictionary<string, string>(), 
                Errors = new List<string>()
            };

            // Permissions check
            if (userRole != "Admin" && userRole != "Owner")
            {
                result.Errors.Add("Forbidden: Only Admin/Owner can update organizations.");
                return result;
            }

            // Validation
            if (string.IsNullOrWhiteSpace(request.Name))
                result.FieldErrors["Name"] = "This field is required.";

            if (!Guid.TryParse(request.OrganizationId, out var organizationId))
            {
                result.FieldErrors["OrganizationId"] = "Invalid organization ID format.";
                return result;
            }

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
                catch { result.FieldErrors["ContactPhone"] = "Enter a valid phone number."; }
            }

            if (result.FieldErrors.Count > 0)
                return result;

            // Get organization
            var organization = await _organizationRepo.GetByIdAsync(organizationId, cancellationToken);
            if (organization == null)
            {
                result.Errors.Add("Organization not found.");
                return result;
            }

            try
            {
                organization.UpdateDetails(
                    request.Name,
                    request.Description,
                    request.ContactEmail,
                    request.ContactPhone,
                    request.WebsiteUrl,
                    userId
                );

                await _organizationRepo.UpdateAsync(organization, cancellationToken);
                result.Success = true;

                // Audit log
                await _auditLogRepo.LogAsync(new AuditLogEntry {
                    UserId = userId,
                    Action = "UpdateOrganization",
                    EntityId = organization.Id.ToString(),
                    EntityType = "Organization",
                    TimestampUtc = DateTime.UtcNow
                }, cancellationToken);
            }
            catch (Exception)
            {
                result.Errors.Add("Unable to update organization. Please try again later.");
            }

            return result;
        }

        public async Task<OrganizationOperationResult> UpdateBrandingAsync(UpdateBrandingRequest request, string userId, string userRole = "Admin", CancellationToken cancellationToken = default)
        {
            var result = new OrganizationOperationResult 
            { 
                Success = false, 
                FieldErrors = new Dictionary<string, string>(), 
                Errors = new List<string>()
            };

            // Permissions check
            if (userRole != "Admin" && userRole != "Owner")
            {
                result.Errors.Add("Forbidden: Only Admin/Owner can update organization branding.");
                return result;
            }

            // Validation
            if (string.IsNullOrWhiteSpace(request.PrimaryColor))
                result.FieldErrors["PrimaryColor"] = "This field is required.";
            if (string.IsNullOrWhiteSpace(request.SecondaryColor))
                result.FieldErrors["SecondaryColor"] = "This field is required.";

            if (!Guid.TryParse(request.OrganizationId, out var organizationId))
            {
                result.FieldErrors["OrganizationId"] = "Invalid organization ID format.";
                return result;
            }

            if (result.FieldErrors.Count > 0)
                return result;

            // Get organization
            var organization = await _organizationRepo.GetByIdAsync(organizationId, cancellationToken);
            if (organization == null)
            {
                result.Errors.Add("Organization not found.");
                return result;
            }

            try
            {
                organization.UpdateBranding(
                    request.PrimaryColor,
                    request.SecondaryColor,
                    request.LogoUrl,
                    request.FaviconUrl,
                    request.TagLine,
                    userId
                );

                await _organizationRepo.UpdateAsync(organization, cancellationToken);
                result.Success = true;

                // Audit log
                await _auditLogRepo.LogAsync(new AuditLogEntry {
                    UserId = userId,
                    Action = "UpdateOrganizationBranding",
                    EntityId = organization.Id.ToString(),
                    EntityType = "Organization",
                    TimestampUtc = DateTime.UtcNow
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Unable to update organization branding: {ex.Message}");
            }

            return result;
        }

        public async Task<OrganizationOperationResult> ChangeSubscriptionPlanAsync(string organizationId, string subscriptionPlanId, string userId, string userRole = "Admin", CancellationToken cancellationToken = default)
        {
            var result = new OrganizationOperationResult 
            { 
                Success = false, 
                FieldErrors = new Dictionary<string, string>(), 
                Errors = new List<string>()
            };

            // Permissions check (only platform admin can change subscription plans)
            if (userRole != "Admin")
            {
                result.Errors.Add("Forbidden: Only platform Admin can change subscription plans.");
                return result;
            }

            if (!Guid.TryParse(organizationId, out var orgId))
            {
                result.FieldErrors["OrganizationId"] = "Invalid organization ID format.";
                return result;
            }

            if (!Guid.TryParse(subscriptionPlanId, out var planId))
            {
                result.FieldErrors["SubscriptionPlanId"] = "Invalid subscription plan ID format.";
                return result;
            }

            // Get organization
            var organization = await _organizationRepo.GetByIdAsync(orgId, cancellationToken);
            if (organization == null)
            {
                result.Errors.Add("Organization not found.");
                return result;
            }

            try
            {
                organization.ChangeSubscriptionPlan(planId, userId);
                await _organizationRepo.UpdateAsync(organization, cancellationToken);
                result.Success = true;

                // Audit log
                await _auditLogRepo.LogAsync(new AuditLogEntry {
                    UserId = userId,
                    Action = "ChangeOrganizationSubscriptionPlan",
                    EntityId = organization.Id.ToString(),
                    EntityType = "Organization",
                    TimestampUtc = DateTime.UtcNow
                }, cancellationToken);
            }
            catch (Exception)
            {
                result.Errors.Add("Unable to change subscription plan. Please try again later.");
            }

            return result;
        }

        public async Task<OrganizationOperationResult> SetAnalyticsSharingAsync(string organizationId, bool sharesData, string userId, string userRole = "Admin", CancellationToken cancellationToken = default)
        {
            var result = new OrganizationOperationResult 
            { 
                Success = false, 
                FieldErrors = new Dictionary<string, string>(), 
                Errors = new List<string>()
            };

            // Permissions check
            if (userRole != "Admin" && userRole != "Owner")
            {
                result.Errors.Add("Forbidden: Only Admin/Owner can change analytics sharing settings.");
                return result;
            }

            if (!Guid.TryParse(organizationId, out var orgId))
            {
                result.FieldErrors["OrganizationId"] = "Invalid organization ID format.";
                return result;
            }

            // Get organization
            var organization = await _organizationRepo.GetByIdAsync(orgId, cancellationToken);
            if (organization == null)
            {
                result.Errors.Add("Organization not found.");
                return result;
            }

            try
            {
                organization.SetAnalyticsSharing(sharesData, userId);
                await _organizationRepo.UpdateAsync(organization, cancellationToken);
                result.Success = true;

                // Audit log
                await _auditLogRepo.LogAsync(new AuditLogEntry {
                    UserId = userId,
                    Action = "SetOrganizationAnalyticsSharing",
                    EntityId = organization.Id.ToString(),
                    EntityType = "Organization",
                    TimestampUtc = DateTime.UtcNow
                }, cancellationToken);
            }
            catch (Exception)
            {
                result.Errors.Add("Unable to update analytics sharing. Please try again later.");
            }

            return result;
        }

        public async Task<OrganizationOperationResult> ActivateOrganizationAsync(string organizationId, string userId, string userRole = "Admin", CancellationToken cancellationToken = default)
        {
            var result = new OrganizationOperationResult 
            { 
                Success = false, 
                FieldErrors = new Dictionary<string, string>(), 
                Errors = new List<string>()
            };

            // Permissions check
            if (userRole != "Admin")
            {
                result.Errors.Add("Forbidden: Only platform Admin can activate organizations.");
                return result;
            }

            if (!Guid.TryParse(organizationId, out var orgId))
            {
                result.FieldErrors["OrganizationId"] = "Invalid organization ID format.";
                return result;
            }

            // Get organization
            var organization = await _organizationRepo.GetByIdAsync(orgId, cancellationToken);
            if (organization == null)
            {
                result.Errors.Add("Organization not found.");
                return result;
            }

            try
            {
                organization.Activate(userId);
                await _organizationRepo.UpdateAsync(organization, cancellationToken);
                result.Success = true;

                // Audit log
                await _auditLogRepo.LogAsync(new AuditLogEntry {
                    UserId = userId,
                    Action = "ActivateOrganization",
                    EntityId = organization.Id.ToString(),
                    EntityType = "Organization",
                    TimestampUtc = DateTime.UtcNow
                }, cancellationToken);
            }
            catch (Exception)
            {
                result.Errors.Add("Unable to activate organization. Please try again later.");
            }

            return result;
        }

        public async Task<OrganizationOperationResult> DeactivateOrganizationAsync(string organizationId, string userId, string userRole = "Admin", CancellationToken cancellationToken = default)
        {
            var result = new OrganizationOperationResult 
            { 
                Success = false, 
                FieldErrors = new Dictionary<string, string>(), 
                Errors = new List<string>()
            };

            // Permissions check
            if (userRole != "Admin")
            {
                result.Errors.Add("Forbidden: Only platform Admin can deactivate organizations.");
                return result;
            }

            if (!Guid.TryParse(organizationId, out var orgId))
            {
                result.FieldErrors["OrganizationId"] = "Invalid organization ID format.";
                return result;
            }

            // Get organization
            var organization = await _organizationRepo.GetByIdAsync(orgId, cancellationToken);
            if (organization == null)
            {
                result.Errors.Add("Organization not found.");
                return result;
            }

            try
            {
                organization.Deactivate(userId);
                await _organizationRepo.UpdateAsync(organization, cancellationToken);
                result.Success = true;

                // Audit log
                await _auditLogRepo.LogAsync(new AuditLogEntry {
                    UserId = userId,
                    Action = "DeactivateOrganization",
                    EntityId = organization.Id.ToString(),
                    EntityType = "Organization",
                    TimestampUtc = DateTime.UtcNow
                }, cancellationToken);
            }
            catch (Exception)
            {
                result.Errors.Add("Unable to deactivate organization. Please try again later.");
            }

            return result;
        }
    }
}
