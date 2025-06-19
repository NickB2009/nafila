namespace GrandeTech.QueueHub.API.Application.Auth
{
    public class LoginResult
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? Error { get; set; }
        public string? Username { get; set; }
        public string? Role { get; set; }
        public string[]? Permissions { get; set; }
        public bool RequiresTwoFactor { get; set; }
        public string? TwoFactorToken { get; set; }
    }
} 