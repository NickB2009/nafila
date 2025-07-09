using System;
using System.Collections.Generic;

namespace Grande.Fila.API.Application.QrCode
{
    public class QrJoinResult
    {
        public bool Success { get; set; }
        public string? QrCodeBase64 { get; set; }
        public string? JoinUrl { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public Dictionary<string, string> FieldErrors { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }
} 