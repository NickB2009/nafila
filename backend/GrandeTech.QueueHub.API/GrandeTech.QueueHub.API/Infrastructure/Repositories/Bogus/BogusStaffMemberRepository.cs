using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Domain.Common;
using GrandeTech.QueueHub.API.Domain.Staff;

namespace GrandeTech.QueueHub.API.Infrastructure.Repositories.Bogus
{
    public class BogusStaffMemberRepository : BogusBaseRepository<StaffMember>, IStaffMemberRepository
    {
        public override async Task<StaffMember?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await base.GetByIdAsync(id, cancellationToken);
        }

        public override async Task<IReadOnlyList<StaffMember>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await base.GetAllAsync(cancellationToken);
        }

        public override async Task<IReadOnlyList<StaffMember>> FindAsync(System.Linq.Expressions.Expression<Func<StaffMember, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await base.FindAsync(predicate, cancellationToken);
        }

        public override async Task<StaffMember> AddAsync(StaffMember entity, CancellationToken cancellationToken = default)
        {
            return await base.AddAsync(entity, cancellationToken);
        }

        public override async Task<StaffMember> UpdateAsync(StaffMember entity, CancellationToken cancellationToken = default)
        {
            return await base.UpdateAsync(entity, cancellationToken);
        }

        public override async Task<bool> DeleteAsync(StaffMember entity, CancellationToken cancellationToken = default)
        {
            return await base.DeleteAsync(entity, cancellationToken);
        }

        public override async Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await base.DeleteByIdAsync(id, cancellationToken);
        }

        public override async Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<StaffMember, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await base.ExistsAsync(predicate, cancellationToken);
        }

        protected override StaffMember CreateNewEntityWithId(StaffMember entity, Guid id)
        {
            var staffMember = new StaffMember(
                entity.Name,
                entity.ServicesProviderId,
                entity.Email?.Value,
                entity.PhoneNumber?.Value,
                entity.ProfilePictureUrl,
                entity.Role,
                entity.Username,
                entity.UserId,
                entity.CreatedBy);
            
            // Set the ID using reflection since it's protected
            var idProperty = typeof(BaseEntity).GetProperty("Id");
            idProperty?.SetValue(staffMember, id);
            
            return staffMember;
        }

        public async Task<IReadOnlyList<StaffMember>> GetByServicesProviderAsync(Guid ServicesProviderId, CancellationToken cancellationToken = default)
        {
            var staffMembers = await GetAllAsync(cancellationToken);
            return staffMembers.Where(sm => sm.ServicesProviderId == ServicesProviderId).ToList();
        }

        public async Task<IReadOnlyList<StaffMember>> GetActiveStaffMembersAsync(Guid ServicesProviderId, CancellationToken cancellationToken = default)
        {
            var staffMembers = await GetAllAsync(cancellationToken);
            return staffMembers.Where(sm => sm.ServicesProviderId == ServicesProviderId && sm.IsActive).ToList();
        }

        public async Task<IReadOnlyList<StaffMember>> GetAvailableStaffAsync(Guid ServicesProviderId, CancellationToken cancellationToken = default)
        {
            var staffMembers = await GetAllAsync(cancellationToken);
            return staffMembers
                .Where(sm => sm.ServicesProviderId == ServicesProviderId && 
                    sm.IsActive && 
                    sm.IsOnDuty && 
                    !sm.IsOnBreak())
                .ToList();
        }

        public async Task<StaffMember?> GetByEmployeeCodeAsync(Guid ServicesProviderId, string employeeCode, CancellationToken cancellationToken = default)
        {
            var staffMembers = await GetAllAsync(cancellationToken);
            return staffMembers.FirstOrDefault(sm => 
                sm.ServicesProviderId == ServicesProviderId && 
                sm.EmployeeCode == employeeCode);
        }

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var staffMembers = await GetAllAsync(cancellationToken);
            return staffMembers.Any(sm => sm.Email?.Value == email);
        }

        public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            var staffMembers = await GetAllAsync(cancellationToken);
            return staffMembers.Any(sm => sm.Username == username);
        }
    }
} 