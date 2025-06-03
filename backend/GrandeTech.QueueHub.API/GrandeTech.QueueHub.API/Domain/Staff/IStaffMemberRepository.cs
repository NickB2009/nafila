using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Domain.Common;
using GrandeTech.QueueHub.API.Domain.Staff;

namespace GrandeTech.QueueHub.API.Domain.Staff
{
    /// <summary>
    /// Repository interface for StaffMember aggregate root
    /// </summary>
    public interface IStaffMemberRepository : IRepository<StaffMember>
    {
        /// <summary>
        /// Gets all staff members for a service provider
        /// </summary>
        /// <param name="serviceProviderId">The service provider ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of staff members for the specified service provider</returns>
        Task<IReadOnlyList<StaffMember>> GetByServiceProviderAsync(Guid serviceProviderId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets all active staff members for a service provider
        /// </summary>
        /// <param name="serviceProviderId">The service provider ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of active staff members</returns>
        Task<IReadOnlyList<StaffMember>> GetActiveStaffMembersAsync(Guid serviceProviderId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets staff members who are currently available (on duty and not on break)
        /// </summary>
        /// <param name="serviceProviderId">The service provider ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of available staff members</returns>
        Task<IReadOnlyList<StaffMember>> GetAvailableStaffAsync(Guid serviceProviderId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a staff member by their employee code
        /// </summary>
        /// <param name="serviceProviderId">The service provider ID</param>
        /// <param name="employeeCode">The employee code</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The staff member or null if not found</returns>
        Task<StaffMember?> GetByEmployeeCodeAsync(Guid serviceProviderId, string employeeCode, CancellationToken cancellationToken = default);
    }
}
