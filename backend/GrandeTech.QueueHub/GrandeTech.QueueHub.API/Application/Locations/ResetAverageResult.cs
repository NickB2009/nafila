using System.Collections.Generic;

namespace Grande.Fila.API.Application.Locations
{
    public class ResetAverageResult
    {
        public bool Success { get; set; }
        public int ResetCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }
} 