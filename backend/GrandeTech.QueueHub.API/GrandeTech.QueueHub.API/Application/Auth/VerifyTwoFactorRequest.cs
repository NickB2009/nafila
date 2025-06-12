namespace GrandeTech.QueueHub.API.Application.Auth
{
    public class VerifyTwoFactorRequest
    {
        public required string Username { get; set; }
        public required string TwoFactorCode { get; set; }
        public required string TwoFactorToken { get; set; }
    }
} 