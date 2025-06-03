using System;
using System.Collections.Generic;

namespace GrandeTech.QueueHub.API.Application.Staff
{
    public class AddBarberResult
    {
        public bool Success { get; set; }
        public Guid? BarberId { get; set; }
        public string? Status { get; set; }
        public List<string>? Errors { get; set; }
        public Dictionary<string, string>? FieldErrors { get; set; }
    }
} 