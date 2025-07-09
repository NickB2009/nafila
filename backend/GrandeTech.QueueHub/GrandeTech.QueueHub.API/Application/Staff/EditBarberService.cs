using System;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Domain.Staff;
using Microsoft.Extensions.Logging;

namespace Grande.Fila.API.Application.Staff
{
    /// <summary>
    /// Service to handle editing barber details
    /// </summary>
    public class EditBarberService
    {
        private readonly IStaffMemberRepository _staffRepository;
        private readonly ILogger<EditBarberService> _logger;

        public EditBarberService(
            IStaffMemberRepository staffRepository,
            ILogger<EditBarberService> logger)
        {
            _staffRepository = staffRepository ?? throw new ArgumentNullException(nameof(staffRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<EditBarberResult> EditBarberAsync(
            EditBarberRequest request,
            string currentUserId,
            string currentUserRole,
            CancellationToken cancellationToken = default)
        {
            var result = new EditBarberResult();

            try
            {
                // Validate request
                if (!Guid.TryParse(request.StaffMemberId, out var staffId))
                {
                    result.FieldErrors["StaffMemberId"] = "Invalid staff member ID format";
                    return result;
                }

                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    result.FieldErrors["Name"] = "Staff member name is required";
                    return result;
                }

                // Get the staff member
                var staffMember = await _staffRepository.GetByIdAsync(staffId, cancellationToken);
                if (staffMember == null)
                {
                    result.Errors.Add("Staff member not found");
                    return result;
                }

                // Update staff member details
                staffMember.UpdateDetails(
                    name: request.Name,
                    email: request.Email,
                    phoneNumber: request.PhoneNumber,
                    profilePictureUrl: request.ProfilePictureUrl,
                    role: request.Role,
                    updatedBy: currentUserId
                );

                // Save changes
                await _staffRepository.UpdateAsync(staffMember, cancellationToken);

                // Return success result
                result.Success = true;
                result.StaffMemberId = staffMember.Id.ToString();
                result.Name = staffMember.Name;
                result.Email = staffMember.Email?.Value;
                result.PhoneNumber = staffMember.PhoneNumber?.Value;
                result.ProfilePictureUrl = staffMember.ProfilePictureUrl;
                result.Role = staffMember.Role;

                _logger.LogInformation(
                    "Staff member {StaffMemberId} updated by user {UserId}",
                    staffMember.Id,
                    currentUserId
                );

                return result;
            }
            catch (ArgumentException ex)
            {
                result.Errors.Add(ex.Message);
                _logger.LogWarning(ex, "Validation error in EditBarberService");
                return result;
            }
            catch (Exception ex)
            {
                result.Errors.Add("An unexpected error occurred while updating the staff member");
                _logger.LogError(ex, "Error updating staff member {StaffMemberId}", request.StaffMemberId);
                return result;
            }
        }
    }
}