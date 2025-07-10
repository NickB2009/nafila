using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Domain.Staff;

namespace Grande.Fila.API.Application.Staff
{
    public class EndBreakService
    {
        private readonly IStaffMemberRepository _staffRepository;

        public EndBreakService(IStaffMemberRepository staffRepository)
        {
            _staffRepository = staffRepository;
        }

        public async Task<EndBreakResult> EndBreakAsync(EndBreakRequest request, string userId, CancellationToken cancellationToken)
        {
            var fieldErrors = new Dictionary<string, string>();
            var errors = new List<string>();

            // Validate StaffMemberId
            if (string.IsNullOrWhiteSpace(request.StaffMemberId))
            {
                fieldErrors["StaffMemberId"] = "Staff member ID is required.";
            }
            else if (!Guid.TryParse(request.StaffMemberId, out _))
            {
                fieldErrors["StaffMemberId"] = "Invalid staff member ID format.";
            }

            // Validate BreakId
            if (string.IsNullOrWhiteSpace(request.BreakId))
            {
                fieldErrors["BreakId"] = "Break ID is required.";
            }
            else if (!Guid.TryParse(request.BreakId, out _))
            {
                fieldErrors["BreakId"] = "Invalid break ID format.";
            }

            if (fieldErrors.Count > 0)
            {
                return new EndBreakResult
                {
                    Success = false,
                    StaffMemberId = request.StaffMemberId,
                    BreakId = request.BreakId,
                    NewStatus = string.Empty,
                    FieldErrors = fieldErrors,
                    Errors = errors
                };
            }

            var staffMemberId = Guid.Parse(request.StaffMemberId);
            var breakId = Guid.Parse(request.BreakId);

            // Get staff member by ID
            var staffMember = await _staffRepository.GetByIdAsync(staffMemberId, cancellationToken);
            if (staffMember == null)
            {
                errors.Add("Staff member not found.");
                return new EndBreakResult
                {
                    Success = false,
                    StaffMemberId = request.StaffMemberId,
                    BreakId = request.BreakId,
                    NewStatus = string.Empty,
                    FieldErrors = fieldErrors,
                    Errors = errors
                };
            }

            // Check if staff member is active
            if (!staffMember.IsActive)
            {
                errors.Add("Cannot end break for inactive staff member.");
                return new EndBreakResult
                {
                    Success = false,
                    StaffMemberId = request.StaffMemberId,
                    BreakId = request.BreakId,
                    NewStatus = string.Empty,
                    FieldErrors = fieldErrors,
                    Errors = errors
                };
            }

            // Check if staff member is currently on break
            var currentBreak = staffMember.GetCurrentBreak();
            if (currentBreak == null)
            {
                errors.Add("No active break found for this staff member.");
                return new EndBreakResult
                {
                    Success = false,
                    StaffMemberId = request.StaffMemberId,
                    BreakId = request.BreakId,
                    NewStatus = string.Empty,
                    FieldErrors = fieldErrors,
                    Errors = errors
                };
            }

            // Check if the break ID matches
            if (currentBreak.Id != breakId)
            {
                errors.Add("No active break found with the specified break ID.");
                return new EndBreakResult
                {
                    Success = false,
                    StaffMemberId = request.StaffMemberId,
                    BreakId = request.BreakId,
                    NewStatus = string.Empty,
                    FieldErrors = fieldErrors,
                    Errors = errors
                };
            }

            // End the break
            staffMember.EndBreak(breakId, userId);

            // Update staff member in repository
            await _staffRepository.UpdateAsync(staffMember, cancellationToken);

            return new EndBreakResult
            {
                Success = true,
                StaffMemberId = request.StaffMemberId,
                BreakId = request.BreakId,
                NewStatus = "available",
                FieldErrors = fieldErrors,
                Errors = errors
            };
        }
    }
} 