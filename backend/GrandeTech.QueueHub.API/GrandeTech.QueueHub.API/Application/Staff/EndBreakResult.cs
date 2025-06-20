using System.Collections.Generic;

namespace GrandeTech.QueueHub.API.Application.Staff
{
    public class EndBreakResult
    {
        public bool Success { get; set; }
        public required string StaffMemberId { get; set; }
        public required string BreakId { get; set; }
        public required string NewStatus { get; set; }
        public required Dictionary<string, string> FieldErrors { get; set; }
        public required List<string> Errors { get; set; }
    }
} 