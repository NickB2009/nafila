using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Domain.Staff;

namespace Grande.Fila.API.Application.Staff
{
    public class StartBreakService
    {
        private readonly IStaffMemberRepository _staffRepository;

        public StartBreakService(IStaffMemberRepository staffRepository)
        {
            _staffRepository = staffRepository ?? throw new ArgumentNullException(nameof(staffRepository));
        }

        public async Task<StartBreakResult> StartBreakAsync(StartBreakRequest request, string userId, CancellationToken cancellationToken = default)
        {
            var result = new StartBreakResult
            {
                Success = false,
                StaffMemberId = request.StaffMemberId,
                BreakId = null,
                NewStatus = null,
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

            if (request.DurationMinutes <= 0)
            {
                result.FieldErrors["DurationMinutes"] = "Duration must be greater than 0.";
                return result;
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
                    result.Errors.Add("Cannot start break for inactive staff member.");
                    return result;
                }

                // Check if already on break
                if (staffMember.IsOnBreak())
                {
                    result.Errors.Add("Staff member is already on break.");
                    return result;
                }

                // Start break
                var breakDuration = TimeSpan.FromMinutes(request.DurationMinutes);
                var staffBreak = staffMember.StartBreak(breakDuration, request.Reason ?? "Break", userId);

                // Update in repository
                await _staffRepository.UpdateAsync(staffMember, cancellationToken);

                // Return success
                result.Success = true;
                result.BreakId = staffBreak.Id.ToString();
                result.NewStatus = "on-break";
                
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
                result.Errors.Add($"An error occurred while starting break: {ex.Message}");
                return result;
            }
        }
    }
} 