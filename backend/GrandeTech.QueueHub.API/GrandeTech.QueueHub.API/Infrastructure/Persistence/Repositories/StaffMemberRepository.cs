using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Domain.Staff;
using Microsoft.EntityFrameworkCore;

namespace GrandeTech.QueueHub.API.Infrastructure.Persistence.Repositories
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
            return await _dbSet
                .Include(s => s.Breaks.Where(b => b.EndTime > DateTime.UtcNow.AddDays(-1)))
                .Where(s => s.ServiceProviderId == serviceProviderId)
                .ToListAsync(cancellationToken);
        }
        
        /// <summary>
        /// Gets all active staff members for a service provider
        /// </summary>
        public async Task<IReadOnlyList<StaffMember>> GetActiveStaffMembersAsync(Guid serviceProviderId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(s => s.Breaks.Where(b => b.EndTime > DateTime.UtcNow.AddDays(-1)))
                .Where(s => 
                    s.ServiceProviderId == serviceProviderId && 
                    s.IsActive)
                .ToListAsync(cancellationToken);
        }
        
        /// <summary>
        /// Gets staff members who are currently available (on duty and not on break)
        /// </summary>
        public async Task<IReadOnlyList<StaffMember>> GetAvailableStaffAsync(Guid serviceProviderId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            
            return await _dbSet                .Include(s => s.Breaks.Where(b => b.EndTime > DateTime.UtcNow.AddHours(-1)))
                .Where(s => 
                    s.ServiceProviderId == serviceProviderId && 
                    s.IsActive &&
                    s.IsOnDuty &&
                    !s.Breaks.Any(b => b.StartedAt <= now && b.EndTime >= now))
                .ToListAsync(cancellationToken);
        }
        
        /// <summary>
        /// Gets a staff member by their employee code
        /// </summary>
        public async Task<StaffMember?> GetByEmployeeCodeAsync(Guid serviceProviderId, string employeeCode, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(s => s.Breaks.Where(b => b.EndTime > DateTime.UtcNow.AddDays(-1)))
                .FirstOrDefaultAsync(s => 
                    s.ServiceProviderId == serviceProviderId &&
                    s.EmployeeCode == employeeCode, 
                    cancellationToken);
        }
    }
}
