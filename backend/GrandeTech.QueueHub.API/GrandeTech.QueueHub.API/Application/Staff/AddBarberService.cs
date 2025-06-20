using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Domain.Staff;
using Grande.Fila.API.Domain.Locations;
using Grande.Fila.API.Domain.Common.ValueObjects;
using Grande.Fila.API.Domain.AuditLogs;

namespace Grande.Fila.API.Application.Staff
{
    public class AddBarberService
    {
        private readonly IStaffMemberRepository _staffRepo;
        private readonly ILocationRepository _locationRepo;
        private readonly IAuditLogRepository _auditLogRepo;

        public AddBarberService(IStaffMemberRepository staffRepo, ILocationRepository locationRepo, IAuditLogRepository auditLogRepo)
        {
            _staffRepo = staffRepo;
            _locationRepo = locationRepo;
            _auditLogRepo = auditLogRepo;
        }

        public async Task<AddBarberResult> AddBarberAsync(AddBarberRequest request, string userId, string userRole = "Admin", CancellationToken cancellationToken = default)
        {
            var result = new AddBarberResult 
            { 
                Success = false, 
                FieldErrors = new Dictionary<string, string>(), 
                Errors = new List<string>(),
                BarberId = string.Empty,
                Status = "Pending"
            };

            // Permissions check (to be replaced by a proper user context in the future)
            if (userRole != "Admin" && userRole != "Owner")
            {
                result.Errors.Add("Forbidden: Only Admin/Owner can add barbers.");
                return result;
            }

            // Validation
            if (string.IsNullOrWhiteSpace(request.FirstName))
                result.FieldErrors["FirstName"] = "This field is required.";
            if (string.IsNullOrWhiteSpace(request.LastName))
                result.FieldErrors["LastName"] = "This field is required.";
            if (string.IsNullOrWhiteSpace(request.Email))
                result.FieldErrors["Email"] = "This field is required.";
            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                result.FieldErrors["PhoneNumber"] = "This field is required.";
            if (request.ServiceTypeIds == null || request.ServiceTypeIds.Count == 0)
                result.FieldErrors["ServiceTypeIds"] = "At least one service type is required.";
            if (string.IsNullOrWhiteSpace(request.Username))
                result.FieldErrors["Username"] = "This field is required.";

            // Email format
            if (!result.FieldErrors.ContainsKey("Email"))
            {
                try { var _ = Email.Create(request.Email); }
                catch { result.FieldErrors["Email"] = "Enter a valid email address."; }
            }
            // Phone format
            if (!result.FieldErrors.ContainsKey("PhoneNumber"))
            {
                try { var _ = PhoneNumber.Create(request.PhoneNumber); }
                catch { result.FieldErrors["PhoneNumber"] = "Enter a valid phone number."; }
            }

            if (result.FieldErrors.Count > 0)
                return result;            // Location existence
            var locationId = Guid.Parse(request.LocationId);
            var locationExists = await _locationRepo.ExistsAsync(location => location.Id == locationId, cancellationToken);
            if (!locationExists)
            {
                result.Errors.Add("Location not found.");
                return result;
            }

            // Uniqueness: Email
            var emailExists = await _staffRepo.ExistsByEmailAsync(request.Email, cancellationToken);
            if (emailExists)
            {
                result.FieldErrors["Email"] = "A barber with this email already exists.";
                return result;
            }

            // Uniqueness: Username
            var usernameExists = await _staffRepo.ExistsByUsernameAsync(request.Username, cancellationToken);
            if (usernameExists)
            {
                result.FieldErrors["Username"] = "A barber with this username already exists.";
                return result;
            }

            // Create StaffMember
            var fullName = request.FirstName.Trim() + " " + request.LastName.Trim();            var staff = new StaffMember(
                fullName,
                locationId,
                request.Email,
                request.PhoneNumber,
                null, // ProfilePictureUrl
                "Barber", // Role
                request.Username,
                null, // UserId
                userId
            );

            if (request.DeactivateOnCreation)
                staff.Deactivate(userId);

            // Add specialties
            if (request.ServiceTypeIds != null)
            {
                foreach (var serviceTypeId in request.ServiceTypeIds)
                    staff.AddSpecialty(Guid.Parse(serviceTypeId));
            }

            try
            {
                await _staffRepo.AddAsync(staff, cancellationToken);
            }
            catch (Exception)
            {
                result.Errors.Add("Unable to create barber. Please try again later.");
                return result;
            }

            result.Success = true;
            result.BarberId = staff.Id.ToString();
            result.Status = staff.IsActive ? "Active" : "Inactive";

            // Audit log
            await _auditLogRepo.LogAsync(new AuditLogEntry {
                UserId = userId,
                Action = "CreateBarber",
                EntityId = staff.Id.ToString(),
                EntityType = "Barber",
                TimestampUtc = DateTime.UtcNow
            }, cancellationToken);

            return result;
        }
    }
}
