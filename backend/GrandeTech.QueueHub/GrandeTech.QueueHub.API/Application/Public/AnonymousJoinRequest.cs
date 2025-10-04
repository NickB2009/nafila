using System.ComponentModel.DataAnnotations;

namespace Grande.Fila.API.Application.Public
{
    public class AnonymousJoinRequest
    {
        [Required(ErrorMessage = "Salon ID is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Salon ID must be between 1 and 100 characters")]
        public string SalonId { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-'\.]+$", ErrorMessage = "Name can only contain letters, spaces, hyphens, apostrophes, and periods")]
        public string Name { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(320, ErrorMessage = "Email cannot exceed 320 characters")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Anonymous user ID is required")]
        [RegularExpression(@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$", 
            ErrorMessage = "Anonymous user ID must be a valid GUID")]
        public string AnonymousUserId { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Service requested is required")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Service requested must be between 1 and 200 characters")]
        public string ServiceRequested { get; set; } = string.Empty;
        
        public bool EmailNotifications { get; set; } = false;
        public bool BrowserNotifications { get; set; } = false;
    }
}