using System.Collections.Generic;

namespace Grande.Fila.API.Application.Staff
{
    public class AddBarberResult
    {
        public bool Success { get; set; }
        public required string BarberId { get; set; }
        public required string Status { get; set; }
        public required Dictionary<string, string> FieldErrors { get; set; }
        public required List<string> Errors { get; set; }
    }
} 