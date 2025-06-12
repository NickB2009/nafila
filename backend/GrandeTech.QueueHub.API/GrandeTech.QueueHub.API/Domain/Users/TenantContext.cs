using System;
using System.Collections.Generic;
using System.Linq;

namespace GrandeTech.QueueHub.API.Domain.Users
{
    /// <summary>
    /// Represents tenant-specific claims for JWT tokens
    /// </summary>
    public static class TenantClaims
    {
        // Standard JWT claims
        public const string UserId = "sub";
        public const string Role = "role";
        public const string Email = "email";
        public const string Username = "preferred_username";
        
        // Tenant-specific claims
        public const string OrganizationId = "org_id";
        public const string LocationId = "loc_id";
        public const string TenantSlug = "tenant_slug";
        
        // Permission claims
        public const string Permissions = "permissions";
        
        // System claims
        public const string IsServiceAccount = "is_service_account";
    }

    /// <summary>
    /// Tenant context extracted from JWT claims
    /// </summary>
    public class TenantContext
    {
        public string UserId { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public Guid? OrganizationId { get; set; }
        public Guid? LocationId { get; set; }
        public string? TenantSlug { get; set; }
        public IReadOnlyList<string> Permissions { get; set; } = new List<string>();
        public bool IsServiceAccount { get; set; }

        /// <summary>
        /// Check if user can access organization
        /// </summary>
        public bool CanAccessOrganization(Guid organizationId)
        {
            // Platform admins can access any organization
            if (Role == UserRoles.PlatformAdmin)
                return true;
                
            // Users can only access their own organization
            return OrganizationId == organizationId;
        }

        /// <summary>
        /// Check if user can access location
        /// </summary>
        public bool CanAccessLocation(Guid locationId)
        {
            // Platform admins can access any location
            if (Role == UserRoles.PlatformAdmin)
                return true;
                
            // Location-specific roles must match location
            if (Role == UserRoles.Barber)
                return LocationId == locationId;
                
            // Organization-level roles can access any location in their org
            return true; // This will be validated against OrganizationId separately
        }

        /// <summary>
        /// Check if user has specific permission
        /// </summary>
        public bool HasPermission(string permission)
        {
            return Permissions.Contains(permission);
        }
    }
}
