using System.Security.Claims;
using GrandeTech.QueueHub.API.Domain.Users;

namespace GrandeTech.QueueHub.API.Infrastructure.Authorization
{
    /// <summary>
    /// Service to provide tenant context throughout the application
    /// </summary>
    public interface ITenantContextService
    {
        TenantContext? GetCurrentTenant();
        bool IsAuthenticated();
        bool HasRole(string role);
        bool CanAccessOrganization(Guid organizationId);
        bool CanAccessLocation(Guid locationId);
    }

    public class TenantContextService : ITenantContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public TenantContext? GetCurrentTenant()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
                return null;

            return new TenantContext
            {
                UserId = user.FindFirst(TenantClaims.UserId)?.Value ?? string.Empty,
                Role = user.FindFirst(TenantClaims.Role)?.Value ?? string.Empty,
                Email = user.FindFirst(TenantClaims.Email)?.Value ?? string.Empty,
                Username = user.FindFirst(TenantClaims.Username)?.Value ?? string.Empty,
                OrganizationId = GetGuidClaim(user, TenantClaims.OrganizationId),
                LocationId = GetGuidClaim(user, TenantClaims.LocationId),
                TenantSlug = user.FindFirst(TenantClaims.TenantSlug)?.Value,
                IsServiceAccount = bool.Parse(user.FindFirst(TenantClaims.IsServiceAccount)?.Value ?? "false"),
                Permissions = GetPermissions(user)
            };
        }

        public bool IsAuthenticated()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
        }

        public bool HasRole(string role)
        {
            var tenant = GetCurrentTenant();
            return tenant?.Role == role || 
                   (tenant?.Role == UserRoles.PlatformAdmin && role != UserRoles.ServiceAccount);
        }

        public bool CanAccessOrganization(Guid organizationId)
        {
            var tenant = GetCurrentTenant();
            return tenant?.CanAccessOrganization(organizationId) == true;
        }

        public bool CanAccessLocation(Guid locationId)
        {
            var tenant = GetCurrentTenant();
            return tenant?.CanAccessLocation(locationId) == true;
        }

        private static Guid? GetGuidClaim(ClaimsPrincipal user, string claimType)
        {
            var claimValue = user.FindFirst(claimType)?.Value;
            return Guid.TryParse(claimValue, out var guid) ? guid : null;
        }

        private static IReadOnlyList<string> GetPermissions(ClaimsPrincipal user)
        {
            var permissionsClaim = user.FindFirst(TenantClaims.Permissions)?.Value;
            if (string.IsNullOrEmpty(permissionsClaim))
                return new List<string>();

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<string[]>(permissionsClaim) ?? new string[0];
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}
