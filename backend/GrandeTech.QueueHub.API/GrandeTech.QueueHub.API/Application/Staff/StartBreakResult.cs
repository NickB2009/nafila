using System.Collections.Generic;

namespace Grande.Fila.API.Application.Staff
{
    public class StartBreakResult
    {
        public bool Success { get; set; }
        public required string StaffMemberId { get; set; }
        public string? BreakId { get; set; }
        public string? NewStatus { get; set; }
        public required Dictionary<string, string> FieldErrors { get; set; }
        public required List<string> Errors { get; set; }
    }
} 