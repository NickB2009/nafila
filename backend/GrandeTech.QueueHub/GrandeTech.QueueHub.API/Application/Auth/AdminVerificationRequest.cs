namespace Grande.Fila.API.Application.Auth
{
    public class AdminVerificationRequest
    {
        public required string Username { get; set; }
        public required string TwoFactorCode { get; set; }
        public required string TwoFactorToken { get; set; }
    }
} 