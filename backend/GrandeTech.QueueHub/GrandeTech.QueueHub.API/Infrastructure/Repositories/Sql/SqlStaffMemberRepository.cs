using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Grande.Fila.API.Domain.Staff;
using Grande.Fila.API.Domain.Common.ValueObjects;
using Grande.Fila.API.Infrastructure.Data;

namespace Grande.Fila.API.Infrastructure.Repositories.Sql
{
    public class SqlStaffMemberRepository : SqlBaseRepository<StaffMember>, IStaffMemberRepository
    {
        public SqlStaffMemberRepository(QueueHubDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<StaffMember>> GetByLocationAsync(Guid locationId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(s => s.LocationId == locationId)
                .OrderBy(s => s.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<StaffMember>> GetActiveStaffMembersAsync(Guid locationId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(s => s.LocationId == locationId && s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<StaffMember>> GetAvailableStaffAsync(Guid locationId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(s => s.LocationId == locationId && 
                          s.IsActive && 
                          s.StaffStatus == "available")
                .OrderBy(s => s.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<StaffMember?> GetByEmployeeCodeAsync(Guid locationId, string employeeCode, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(employeeCode))
                throw new ArgumentException("Employee code cannot be null or whitespace", nameof(employeeCode));

            return await _dbSet
                .Where(s => s.LocationId == locationId && s.EmployeeCode == employeeCode)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            // Create Email value object for comparison
            var emailValueObject = Email.Create(email);
            return await _dbSet
                .AnyAsync(s => s.Email != null && s.Email == emailValueObject, cancellationToken);
        }

        public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            return await _dbSet
                .AnyAsync(s => s.Username == username, cancellationToken);
        }

        public async Task<IReadOnlyList<StaffMember>> GetStaffMembersByLocationIdsAsync(List<Guid> locationIds, CancellationToken cancellationToken = default)
        {
            if (locationIds == null || !locationIds.Any())
                return new List<StaffMember>();

            return await _dbSet
                .Where(s => locationIds.Contains(s.LocationId))
                .OrderBy(s => s.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<StaffMember>> GetStaffMembersByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default)
        {
            // This would require joining with Location table or having OrganizationId on StaffMember
            // For now, this is a placeholder implementation
            // In a real scenario, you'd need navigation properties or a join query
            return await _dbSet
                .Where(s => s.IsActive) // Simplified for now
                .OrderBy(s => s.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<StaffMember>> GetStaffMembersByStatusAsync(
            Guid locationId, 
            string status, 
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(s => s.LocationId == locationId && s.StaffStatus == status)
                .OrderBy(s => s.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<StaffMember>> GetStaffMembersOnBreakAsync(Guid locationId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(s => s.LocationId == locationId && s.StaffStatus == "on-break")
                .OrderBy(s => s.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> IsEmployeeCodeUniqueAsync(string employeeCode, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(employeeCode))
                return false;

            return !await _dbSet
                .AnyAsync(s => s.EmployeeCode == employeeCode, cancellationToken);
        }

        public async Task<bool> IsEmployeeCodeUniqueForLocationAsync(
            string employeeCode, 
            Guid locationId, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(employeeCode))
                return false;

            return !await _dbSet
                .AnyAsync(s => s.EmployeeCode == employeeCode && s.LocationId == locationId, cancellationToken);
        }

        public async Task<int> GetActiveStaffCountAsync(Guid locationId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .CountAsync(s => s.LocationId == locationId && s.IsActive, cancellationToken);
        }
    }
} 