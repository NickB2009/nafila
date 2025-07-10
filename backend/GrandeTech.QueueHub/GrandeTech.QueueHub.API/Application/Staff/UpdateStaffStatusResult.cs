using System.Collections.Generic;

namespace Grande.Fila.API.Application.Staff
{
    /// <summary>
    /// Result DTO for UC-STAFFSTATUS: Barber changes status use case
    /// </summary>
    public class UpdateStaffStatusResult
    {
        public bool Success { get; set; }
        public required string StaffMemberId { get; set; }
        public required string NewStatus { get; set; }
        public string? PreviousStatus { get; set; }
        public required Dictionary<string, string> FieldErrors { get; set; }
        public required List<string> Errors { get; set; }
    }
} 