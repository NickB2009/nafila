namespace Grande.Fila.API.Application.Auth
{
    public class AdminVerificationRequest
    {
        public required string PhoneNumber { get; set; }
        public required string TwoFactorCode { get; set; }
        public required string TwoFactorToken { get; set; }
    }
} 