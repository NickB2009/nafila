using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Domain.Staff;

namespace Grande.Fila.API.Application.Staff
{
    /// <summary>
    /// Service for handling UC-STAFFSTATUS: Barber changes status use case
    /// Implements TDD-driven business logic for updating staff member status
    /// </summary>
    public class UpdateStaffStatusService
    {
        private readonly IStaffMemberRepository _staffRepository;

        public UpdateStaffStatusService(IStaffMemberRepository staffRepository)
        {
            _staffRepository = staffRepository ?? throw new ArgumentNullException(nameof(staffRepository));
        }

        /// <summary>
        /// Updates a staff member's status (available, busy, break, offline)
        /// </summary>
        public async Task<UpdateStaffStatusResult> UpdateStaffStatusAsync(
            UpdateStaffStatusRequest request, 
            string userId, 
            CancellationToken cancellationToken = default)
        {
            var result = new UpdateStaffStatusResult
            {
                Success = false,
                StaffMemberId = request.StaffMemberId,
                NewStatus = request.NewStatus,
                PreviousStatus = null,
                FieldErrors = new Dictionary<string, string>(),
                Errors = new List<string>()
            };

            // Input validation
            if (string.IsNullOrWhiteSpace(request.StaffMemberId))
            {
                result.FieldErrors["StaffMemberId"] = "Staff member ID is required.";
                return result;
            }

            if (!Guid.TryParse(request.StaffMemberId, out var staffMemberId))
            {
                result.FieldErrors["StaffMemberId"] = "Invalid staff member ID format.";
                return result;
            }

            if (string.IsNullOrWhiteSpace(request.NewStatus))
            {
                result.FieldErrors["NewStatus"] = "Status is required.";
            }

            // Validate status values
            var validStatuses = new[] { "available", "busy", "away", "offline" };
            if (!string.IsNullOrWhiteSpace(request.NewStatus) && !Array.Exists(validStatuses, s => s.Equals(request.NewStatus, StringComparison.OrdinalIgnoreCase)))
            {
                result.FieldErrors["NewStatus"] = "Invalid status. Must be one of: available, busy, away, offline";
            }

            if (result.FieldErrors.Count > 0)
                return result;

            try
            {
                // Get staff member
                var staffMember = await _staffRepository.GetByIdAsync(staffMemberId, cancellationToken);
                if (staffMember == null)
                {
                    result.Errors.Add("Staff member not found.");
                    return result;
                }

                // Check if staff member is active
                if (!staffMember.IsActive)
                {
                    result.Errors.Add("Cannot update status for inactive staff member.");
                    return result;
                }

                // Store previous status
                result.PreviousStatus = staffMember.StaffStatus;

                // Update status
                var normalizedStatus = request.NewStatus.ToLowerInvariant();
                staffMember.UpdateStatus(normalizedStatus, userId);

                // Update in repository
                await _staffRepository.UpdateAsync(staffMember, cancellationToken);

                // Return success
                result.Success = true;
                result.NewStatus = normalizedStatus;
                
                return result;
            }
            catch (InvalidOperationException ex)
            {
                result.Errors.Add(ex.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                result.Errors.Add(ex.Message);
                return result;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"An error occurred while updating staff status: {ex.Message}");
                return result;
            }
        }
    }
} 