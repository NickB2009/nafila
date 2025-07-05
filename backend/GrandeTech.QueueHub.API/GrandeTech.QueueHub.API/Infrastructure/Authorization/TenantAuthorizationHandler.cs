using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Grande.Fila.API.Domain.Users;

namespace Grande.Fila.API.Infrastructure.Authorization
{
    /// <summary>
    /// Custom authorization requirements for tenant-specific access control
    /// </summary>
    public class TenantRequirement : IAuthorizationRequirement
    {
        public string RequiredRole { get; }
        public bool RequireOrganizationContext { get; }
        public bool RequireLocationContext { get; }

        public TenantRequirement(string requiredRole, bool requireOrganizationContext = true, bool requireLocationContext = false)
        {
            RequiredRole = requiredRole;
            RequireOrganizationContext = requireOrganizationContext;
            RequireLocationContext = requireLocationContext;
        }
    }

    /// <summary>
    /// Authorization handler for tenant-specific requirements
    /// </summary>
    public class TenantAuthorizationHandler : AuthorizationHandler<TenantRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            TenantRequirement requirement)
        {
            var user = context.User;
            
            // Check if user is authenticated
            if (!user.Identity?.IsAuthenticated ?? false)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            // Get tenant context from claims
            var tenantContext = GetTenantContext(user);
            
            // Validate role
            if (!IsValidRole(tenantContext.Role, requirement.RequiredRole))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            // Check organization context if required
            if (requirement.RequireOrganizationContext && 
                tenantContext.Role != UserRoles.PlatformAdmin && 
                !tenantContext.OrganizationId.HasValue)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            // Check location context if required
            if (requirement.RequireLocationContext && 
                tenantContext.Role == UserRoles.Barber && 
                !tenantContext.LocationId.HasValue)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        private static TenantContext GetTenantContext(ClaimsPrincipal user)
        {
            return new TenantContext
            {
                UserId = user.FindFirst(TenantClaims.UserId)?.Value ?? string.Empty,
                Role = user.FindFirst(TenantClaims.Role)?.Value ?? string.Empty,
                Email = user.FindFirst(TenantClaims.Email)?.Value ?? string.Empty,
                Username = user.FindFirst(TenantClaims.Username)?.Value ?? string.Empty,
                OrganizationId = Guid.TryParse(user.FindFirst(TenantClaims.OrganizationId)?.Value, out var orgId) ? orgId : null,
                LocationId = Guid.TryParse(user.FindFirst(TenantClaims.LocationId)?.Value, out var locId) ? locId : null,
                TenantSlug = user.FindFirst(TenantClaims.TenantSlug)?.Value,
                IsServiceAccount = bool.Parse(user.FindFirst(TenantClaims.IsServiceAccount)?.Value ?? "false")
            };
        }        private static bool IsValidRole(string userRole, string requiredRole)
        {
            // Platform admin can do anything
            if (userRole == UserRoles.PlatformAdmin)
                return true;

            // Check specific role requirements based on the role hierarchy
            return requiredRole switch
            {
                // Admin requirement: only Admin role
                UserRoles.Admin => userRole == UserRoles.Admin,
                // Barber requirement: Admin or Barber can access
                UserRoles.Barber => userRole == UserRoles.Admin || userRole == UserRoles.Barber,
                // Client requirement: Any authenticated user can access
                UserRoles.Client => userRole == UserRoles.Admin || userRole == UserRoles.Barber || userRole == UserRoles.Client,
                // Service account requirement: only service accounts
                UserRoles.ServiceAccount => userRole == UserRoles.ServiceAccount,
                _ => userRole == requiredRole
            };
        }
    }
}
