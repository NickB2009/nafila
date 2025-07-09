using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Domain.Common;
using Grande.Fila.API.Domain.Staff;

namespace Grande.Fila.API.Domain.Staff
{
    /// <summary>
    /// Repository interface for StaffMember aggregate root
    /// </summary>
    public interface IStaffMemberRepository : IRepository<StaffMember>
    {
        /// <summary>
        /// Gets all staff members for a service provider
        /// </summary>
        /// <param name="LocationId">The service provider ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of staff members for the specified service provider</returns>
        Task<IReadOnlyList<StaffMember>> GetByLocationAsync(Guid LocationId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets all active staff members for a service provider
        /// </summary>
        /// <param name="LocationId">The service provider ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of active staff members</returns>
        Task<IReadOnlyList<StaffMember>> GetActiveStaffMembersAsync(Guid LocationId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets staff members who are currently available (on duty and not on break)
        /// </summary>
        /// <param name="LocationId">The service provider ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of available staff members</returns>
        Task<IReadOnlyList<StaffMember>> GetAvailableStaffAsync(Guid LocationId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a staff member by their employee code
        /// </summary>
        /// <param name="LocationId">The service provider ID</param>
        /// <param name="employeeCode">The employee code</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The staff member or null if not found</returns>
        Task<StaffMember?> GetByEmployeeCodeAsync(Guid LocationId, string employeeCode, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Checks if a staff member exists by email
        /// </summary>
        /// <param name="email">The email address</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if a staff member with the email exists, false otherwise</returns>
        Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Checks if a staff member exists by username
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if a staff member with the username exists, false otherwise</returns>
        Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets staff members by location IDs
        /// </summary>
        /// <param name="locationIds">List of location IDs</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of staff members for the specified locations</returns>
        Task<IReadOnlyList<StaffMember>> GetStaffMembersByLocationIdsAsync(List<Guid> locationIds, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets staff members by organization ID
        /// </summary>
        /// <param name="organizationId">Organization ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of staff members for the specified organization</returns>
        Task<IReadOnlyList<StaffMember>> GetStaffMembersByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default);
    }
}
