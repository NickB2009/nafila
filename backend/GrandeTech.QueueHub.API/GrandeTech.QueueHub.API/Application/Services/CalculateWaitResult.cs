using System.Collections.Generic;

namespace Grande.Fila.API.Application.Services
{
    public class CalculateWaitResult
    {
        public bool Success { get; set; }
        public int EstimatedWaitMinutes { get; set; }
        public Dictionary<string, string> FieldErrors { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }
} 