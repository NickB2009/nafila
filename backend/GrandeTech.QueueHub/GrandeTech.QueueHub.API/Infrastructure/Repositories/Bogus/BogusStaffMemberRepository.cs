using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Domain.Common;
using Grande.Fila.API.Domain.Staff;

namespace Grande.Fila.API.Infrastructure.Repositories.Bogus
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
                entity.LocationId,
                entity.Email?.Value,
                entity.PhoneNumber?.Value,
                entity.ProfilePictureUrl,
                entity.Role,
                entity.Username,
                entity.UserId,
                entity.CreatedBy ?? "system");
            
            // Set the ID using reflection since it's protected
            var idProperty = typeof(BaseEntity).GetProperty("Id");
            idProperty?.SetValue(staffMember, id);
            
            return staffMember;
        }

        public async Task<IReadOnlyList<StaffMember>> GetByLocationAsync(Guid LocationId, CancellationToken cancellationToken = default)
        {
            var staffMembers = await GetAllAsync(cancellationToken);
            return staffMembers.Where(sm => sm.LocationId == LocationId).ToList();
        }

        public async Task<IReadOnlyList<StaffMember>> GetActiveStaffMembersAsync(Guid LocationId, CancellationToken cancellationToken = default)
        {
            var staffMembers = await GetAllAsync(cancellationToken);
            return staffMembers.Where(sm => sm.LocationId == LocationId && sm.IsActive).ToList();
        }

        public async Task<IReadOnlyList<StaffMember>> GetAvailableStaffAsync(Guid LocationId, CancellationToken cancellationToken = default)
        {
            var staffMembers = await GetAllAsync(cancellationToken);
            return staffMembers
                .Where(sm => sm.LocationId == LocationId && 
                    sm.IsActive && 
                    sm.IsOnDuty && 
                    !sm.IsOnBreak())
                .ToList();
        }

        public async Task<StaffMember?> GetByEmployeeCodeAsync(Guid LocationId, string employeeCode, CancellationToken cancellationToken = default)
        {
            var staffMembers = await GetAllAsync(cancellationToken);
            return staffMembers.FirstOrDefault(sm => 
                sm.LocationId == LocationId && 
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

        public async Task<IReadOnlyList<StaffMember>> GetStaffMembersByLocationIdsAsync(List<Guid> locationIds, CancellationToken cancellationToken = default)
        {
            var staffMembers = await GetAllAsync(cancellationToken);
            return staffMembers.Where(sm => locationIds.Contains(sm.LocationId)).ToList();
        }

        public async Task<IReadOnlyList<StaffMember>> GetStaffMembersByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default)
        {
            // For the bogus repository, we can't directly filter by organization ID since staff only have locationId
            // This would normally be done via a join in a real database implementation
            var staffMembers = await GetAllAsync(cancellationToken);
            return staffMembers.ToList();
        }
    }
} 