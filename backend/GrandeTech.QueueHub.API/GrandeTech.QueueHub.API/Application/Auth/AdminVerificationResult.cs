namespace GrandeTech.QueueHub.API.Application.Auth
{
    public class AdminVerificationResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string? Token { get; set; }
        public string[]? Permissions { get; set; }
    }
} 