namespace Grande.Fila.API.Application.Staff
{
    /// <summary>
    /// Request to edit an existing barber's details
    /// </summary>
    public class EditBarberRequest
    {
        public string StaffMemberId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string Role { get; set; } = "Barber";
    }
}