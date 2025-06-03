using System;
using System.Collections.Generic;

namespace GrandeTech.QueueHub.API.Application.Staff
{
    public class AddBarberRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public Guid ServiceProviderId { get; set; }
        public List<Guid> ServiceTypeIds { get; set; } = new();
        public string? Address { get; set; }
        public string? Notes { get; set; }
        public bool DeactivateOnCreation { get; set; } = false;
    }
} 