using System.ComponentModel.DataAnnotations;

namespace Grande.Fila.API.Application.Public
{
    public class UpdateQueueEntryRequest
    {
        [Required(ErrorMessage = "Queue entry ID is required")]
        [RegularExpression(@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$", 
            ErrorMessage = "Queue entry ID must be a valid GUID")]
        public string EntryId { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
        
        [StringLength(50, ErrorMessage = "Service type cannot exceed 50 characters")]
        public string? ServiceType { get; set; }
        
        public bool EmailNotifications { get; set; } = false;
        public bool BrowserNotifications { get; set; } = false;
    }
}
