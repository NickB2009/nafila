using System;
using System.Collections.Generic;

namespace Grande.Fila.API.Application.Staff
{
    public class AddBarberRequest
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }        public required string Username { get; set; }
        public required string LocationId { get; set; }
        public required List<string> ServiceTypeIds { get; set; }
        public bool DeactivateOnCreation { get; set; }
        public string? Address { get; set; }
        public string? Notes { get; set; }
    }
} 