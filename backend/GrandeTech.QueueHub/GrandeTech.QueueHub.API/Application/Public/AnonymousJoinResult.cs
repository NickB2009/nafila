using System;
using System.Collections.Generic;

namespace Grande.Fila.API.Application.Public
{
    public class AnonymousJoinResult
    {
        public bool Success { get; set; }
        public string? Id { get; set; }
        public int Position { get; set; }
        public int EstimatedWaitMinutes { get; set; }
        public DateTime? JoinedAt { get; set; }
        public string Status { get; set; } = "waiting";
        public Dictionary<string, string> FieldErrors { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }
}