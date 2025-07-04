using System.Collections.Generic;

namespace Grande.Fila.API.Application.Staff
{
    /// <summary>
    /// Result of editing a barber's details
    /// </summary>
    public class EditBarberResult
    {
        public bool Success { get; set; }
        public string? StaffMemberId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? Role { get; set; }
        public List<string> Errors { get; set; } = new();
        public Dictionary<string, string> FieldErrors { get; set; } = new();
    }
}