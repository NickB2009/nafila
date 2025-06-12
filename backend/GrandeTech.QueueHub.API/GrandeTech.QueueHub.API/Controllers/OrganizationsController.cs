using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrandeTech.QueueHub.API.Application.Organizations;
using GrandeTech.QueueHub.API.Domain.Organizations;
using GrandeTech.QueueHub.API.Infrastructure.Authorization;
using GrandeTech.QueueHub.API.Domain.Users;

namespace GrandeTech.QueueHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrganizationsController : ControllerBase
    {
        private readonly CreateOrganizationService _createOrganizationService;
        private readonly OrganizationService _organizationService;
        private readonly IOrganizationRepository _organizationRepository;

        public OrganizationsController(
            CreateOrganizationService createOrganizationService,
            OrganizationService organizationService,
            IOrganizationRepository organizationRepository)
        {
            _createOrganizationService = createOrganizationService;
            _organizationService = organizationService;
            _organizationRepository = organizationRepository;
        }

        /// <summary>
        /// Creates a new organization (UC-CREATEBARBER related)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CreateOrganizationResult>> CreateOrganization(
            [FromBody] CreateOrganizationRequest request,
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(TenantClaims.UserId)?.Value ?? throw new UnauthorizedAccessException("User ID not found in claims");
            var userRole = User.FindFirst(TenantClaims.Role)?.Value ?? "User";

            var result = await _createOrganizationService.CreateOrganizationAsync(request, userId, userRole, cancellationToken);

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
        /// Updates organization details
        /// </summary>
        [HttpPut("{organizationId}")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<ActionResult<OrganizationOperationResult>> UpdateOrganization(
            string organizationId,
            [FromBody] UpdateOrganizationRequest request,
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(TenantClaims.UserId)?.Value ?? throw new UnauthorizedAccessException("User ID not found in claims");
            var userRole = User.FindFirst(TenantClaims.Role)?.Value ?? "User";

            request.OrganizationId = organizationId;
            var result = await _organizationService.UpdateOrganizationAsync(request, userId, userRole, cancellationToken);

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
        /// Updates organization branding (UC-BRANDING)
        /// </summary>
        [HttpPut("{organizationId}/branding")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<ActionResult<OrganizationOperationResult>> UpdateBranding(
            string organizationId,
            [FromBody] UpdateBrandingRequest request,
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(TenantClaims.UserId)?.Value ?? throw new UnauthorizedAccessException("User ID not found in claims");
            var userRole = User.FindFirst(TenantClaims.Role)?.Value ?? "User";

            request.OrganizationId = organizationId;
            var result = await _organizationService.UpdateBrandingAsync(request, userId, userRole, cancellationToken);

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
        /// Changes organization subscription plan (UC-SUBPLAN)
        /// </summary>
        [HttpPut("{organizationId}/subscription/{subscriptionPlanId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<OrganizationOperationResult>> ChangeSubscriptionPlan(
            string organizationId,
            string subscriptionPlanId,
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(TenantClaims.UserId)?.Value ?? throw new UnauthorizedAccessException("User ID not found in claims");
            var userRole = User.FindFirst(TenantClaims.Role)?.Value ?? "User";

            var result = await _organizationService.ChangeSubscriptionPlanAsync(organizationId, subscriptionPlanId, userId, userRole, cancellationToken);

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
        /// Sets analytics sharing preference (UC-ANALYTICS)
        /// </summary>
        [HttpPut("{organizationId}/analytics-sharing")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<ActionResult<OrganizationOperationResult>> SetAnalyticsSharing(
            string organizationId,
            [FromBody] bool sharesData,
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(TenantClaims.UserId)?.Value ?? throw new UnauthorizedAccessException("User ID not found in claims");
            var userRole = User.FindFirst(TenantClaims.Role)?.Value ?? "User";

            var result = await _organizationService.SetAnalyticsSharingAsync(organizationId, sharesData, userId, userRole, cancellationToken);

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
        /// Activates an organization
        /// </summary>
        [HttpPut("{organizationId}/activate")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<OrganizationOperationResult>> ActivateOrganization(
            string organizationId,
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(TenantClaims.UserId)?.Value ?? throw new UnauthorizedAccessException("User ID not found in claims");
            var userRole = User.FindFirst(TenantClaims.Role)?.Value ?? "User";

            var result = await _organizationService.ActivateOrganizationAsync(organizationId, userId, userRole, cancellationToken);

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
        /// Deactivates an organization
        /// </summary>
        [HttpPut("{organizationId}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<OrganizationOperationResult>> DeactivateOrganization(
            string organizationId,
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(TenantClaims.UserId)?.Value ?? throw new UnauthorizedAccessException("User ID not found in claims");
            var userRole = User.FindFirst(TenantClaims.Role)?.Value ?? "User";

            var result = await _organizationService.DeactivateOrganizationAsync(organizationId, userId, userRole, cancellationToken);

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
        /// Gets all organizations (UC-MULTILOC, UC-ANALYTICS)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<object>>> GetOrganizations(CancellationToken cancellationToken)
        {
            var organizations = await _organizationRepository.GetActiveOrganizationsAsync(cancellationToken);

            var result = organizations.Select(org => new
            {
                Id = org.Id,
                Name = org.Name,
                Slug = org.Slug.Value,
                Description = org.Description,
                ContactEmail = org.ContactEmail?.Value,
                ContactPhone = org.ContactPhone?.Value,
                WebsiteUrl = org.WebsiteUrl,
                IsActive = org.IsActive,
                SharesDataForAnalytics = org.SharesDataForAnalytics,
                SubscriptionPlanId = org.SubscriptionPlanId,
                BrandingConfig = new
                {
                    PrimaryColor = org.BrandingConfig.PrimaryColor,
                    SecondaryColor = org.BrandingConfig.SecondaryColor,
                    LogoUrl = org.BrandingConfig.LogoUrl,
                    FaviconUrl = org.BrandingConfig.FaviconUrl,
                    CompanyName = org.BrandingConfig.CompanyName,
                    TagLine = org.BrandingConfig.TagLine
                },                ServiceProviderCount = org.LocationIds.Count,
                CreatedAt = org.CreatedAt,
                UpdatedAt = org.LastModifiedAt
            });

            return Ok(result);
        }

        /// <summary>
        /// Gets a specific organization by ID
        /// </summary>
        [HttpGet("{organizationId}")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<ActionResult<object>> GetOrganization(string organizationId, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(organizationId, out var orgId))
            {
                return BadRequest("Invalid organization ID format.");
            }

            var organization = await _organizationRepository.GetByIdAsync(orgId, cancellationToken);
            if (organization == null)
            {
                return NotFound("Organization not found.");
            }

            var result = new
            {
                Id = organization.Id,
                Name = organization.Name,
                Slug = organization.Slug.Value,
                Description = organization.Description,
                ContactEmail = organization.ContactEmail?.Value,
                ContactPhone = organization.ContactPhone?.Value,
                WebsiteUrl = organization.WebsiteUrl,
                IsActive = organization.IsActive,
                SharesDataForAnalytics = organization.SharesDataForAnalytics,
                SubscriptionPlanId = organization.SubscriptionPlanId,
                BrandingConfig = new
                {
                    PrimaryColor = organization.BrandingConfig.PrimaryColor,
                    SecondaryColor = organization.BrandingConfig.SecondaryColor,
                    LogoUrl = organization.BrandingConfig.LogoUrl,
                    FaviconUrl = organization.BrandingConfig.FaviconUrl,
                    CompanyName = organization.BrandingConfig.CompanyName,
                    TagLine = organization.BrandingConfig.TagLine
                },                ServiceProviderIds = organization.LocationIds,
                CreatedAt = organization.CreatedAt,
                UpdatedAt = organization.LastModifiedAt
            };

            return Ok(result);
        }

        /// <summary>
        /// Gets organization by slug
        /// </summary>
        [HttpGet("by-slug/{slug}")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetOrganizationBySlug(string slug, CancellationToken cancellationToken)
        {
            var organization = await _organizationRepository.GetBySlugAsync(slug, cancellationToken);
            if (organization == null)
            {
                return NotFound("Organization not found.");
            }

            // Return limited public information
            var result = new
            {
                Id = organization.Id,
                Name = organization.Name,
                Slug = organization.Slug.Value,
                Description = organization.Description,
                WebsiteUrl = organization.WebsiteUrl,
                BrandingConfig = new
                {
                    PrimaryColor = organization.BrandingConfig.PrimaryColor,
                    SecondaryColor = organization.BrandingConfig.SecondaryColor,
                    LogoUrl = organization.BrandingConfig.LogoUrl,
                    FaviconUrl = organization.BrandingConfig.FaviconUrl,
                    CompanyName = organization.BrandingConfig.CompanyName,
                    TagLine = organization.BrandingConfig.TagLine
                }
            };

            return Ok(result);
        }
    }
}
