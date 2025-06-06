namespace GrandeTech.QueueHub.API.Application.Auth
{
    public class RegisterResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public Dictionary<string, string>? FieldErrors { get; set; }
    }
} 