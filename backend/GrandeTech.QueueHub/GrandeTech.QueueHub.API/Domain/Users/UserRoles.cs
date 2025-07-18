using System;
using System.Collections.Generic;
using System.Linq;

namespace Grande.Fila.API.Domain.Users
{
    /// <summary>
    /// Defines the user roles in the multi-tenant queue hub system
    /// Simplified 5-role structure aligned with business model
    /// </summary>
    public static class UserRoles
    {
        // Platform-level roles
        public const string PlatformAdmin = "PlatformAdmin";
        
        // Organization-level roles  
        public const string Owner = "Owner";
        
        // Location-level roles
        public const string Staff = "Staff";
        
        // Public roles
        public const string Customer = "Customer";
        
        // Service roles (for background processes)
        public const string ServiceAccount = "ServiceAccount";

        /// <summary>
        /// All valid user roles
        /// </summary>
        public static readonly IReadOnlyList<string> AllRoles = new[]
        {
            PlatformAdmin,
            Owner,
            Staff,
            Customer,
            ServiceAccount
        };

        /// <summary>
        /// Roles that can manage organizations (platform level)
        /// </summary>
        public static readonly IReadOnlyList<string> OrganizationManagers = new[]
        {
            PlatformAdmin
        };

        /// <summary>
        /// Roles that can manage locations within an organization
        /// </summary>
        public static readonly IReadOnlyList<string> LocationManagers = new[]
        {
            Owner
        };

        /// <summary>
        /// Roles that can manage staff members
        /// </summary>
        public static readonly IReadOnlyList<string> StaffManagers = new[]
        {
            Owner
        };

        /// <summary>
        /// Roles that can operate queues
        /// </summary>
        public static readonly IReadOnlyList<string> QueueOperators = new[]
        {
            Staff
        };

        /// <summary>
        /// Check if a role is valid
        /// </summary>
        public static bool IsValidRole(string role)
        {
            return AllRoles.Contains(role);
        }

        /// <summary>
        /// Check if a role can manage organizations
        /// </summary>
        public static bool CanManageOrganizations(string role)
        {
            return OrganizationManagers.Contains(role);
        }

        /// <summary>
        /// Check if a role can manage locations
        /// </summary>
        public static bool CanManageLocations(string role)
        {
            return LocationManagers.Contains(role) || role == PlatformAdmin;
        }

        /// <summary>
        /// Check if a role can manage staff
        /// </summary>
        public static bool CanManageStaff(string role)
        {
            return StaffManagers.Contains(role) || role == PlatformAdmin;
        }

        /// <summary>
        /// Check if a role can operate queues
        /// </summary>
        public static bool CanOperateQueues(string role)
        {
            return QueueOperators.Contains(role) || role == Owner || role == PlatformAdmin;
        }
    }
}
