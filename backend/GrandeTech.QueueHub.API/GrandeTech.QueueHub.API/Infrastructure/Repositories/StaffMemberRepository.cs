using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Domain.Staff;
using GrandeTech.QueueHub.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GrandeTech.QueueHub.API.Infrastructure.Repositories
{
    /// <summary>
    /// Implementation of the StaffMember repository
    /// </summary>
    public class StaffMemberRepository : BaseRepository<StaffMember>, IStaffMemberRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Application database context</param>
        public StaffMemberRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Gets all staff members for a service provider
        /// </summary>
        public async Task<IReadOnlyList<StaffMember>> GetByServiceProviderAsync(Guid serviceProviderId, CancellationToken cancellationToken = default)
        {
            return await FindAsync(s => s.ServiceProviderId == serviceProviderId, cancellationToken);
        }
        
        /// <summary>
        /// Gets all active staff members for a service provider
        /// </summary>
        public async Task<IReadOnlyList<StaffMember>> GetActiveStaffMembersAsync(Guid serviceProviderId, CancellationToken cancellationToken = default)
        {
            return await FindAsync(s => s.ServiceProviderId == serviceProviderId && s.IsActive, cancellationToken);
        }
        
        /// <summary>
        /// Gets staff members who are currently available (on duty and not on break)
        /// </summary>
        public async Task<IReadOnlyList<StaffMember>> GetAvailableStaffAsync(Guid serviceProviderId, CancellationToken cancellationToken = default)
        {
            var staffMembers = await FindAsync(s => 
                s.ServiceProviderId == serviceProviderId && 
                s.IsActive && 
                s.IsOnDuty, 
                cancellationToken);

            return staffMembers.Where(s => !s.IsOnBreak()).ToList();
        }
        
        /// <summary>
        /// Gets a staff member by their employee code
        /// </summary>
        public async Task<StaffMember?> GetByEmployeeCodeAsync(Guid serviceProviderId, string employeeCode, CancellationToken cancellationToken = default)
        {
            return await FindAsync(s => 
                s.ServiceProviderId == serviceProviderId && 
                s.EmployeeCode == employeeCode, 
                cancellationToken)
                .ContinueWith(t => t.Result.FirstOrDefault());
        }

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await ExistsAsync(s => s.Email == email, cancellationToken);
        }

        public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            return await ExistsAsync(s => s.Username == username, cancellationToken);
        }
    }
} 