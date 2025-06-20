namespace GrandeTech.QueueHub.API.Application.Staff
{
    /// <summary>
    /// Request DTO for UC-STAFFSTATUS: Barber changes status use case
    /// </summary>
    public class UpdateStaffStatusRequest
    {
        /// <summary>
        /// The ID of the staff member whose status is being updated
        /// </summary>
        public required string StaffMemberId { get; set; }

        /// <summary>
        /// The new status to set (available, busy, break, offline)
        /// </summary>
        public required string NewStatus { get; set; }

        /// <summary>
        /// Optional notes about the status change
        /// </summary>
        public string? Notes { get; set; }
    }
} 