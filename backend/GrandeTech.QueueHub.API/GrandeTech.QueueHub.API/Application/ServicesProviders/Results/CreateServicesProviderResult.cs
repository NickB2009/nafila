namespace GrandeTech.QueueHub.API.Application.ServicesProviders.Results;

public class CreateServicesProviderResult
{
    public bool Success { get; set; }
    public string? ServicesProviderId { get; set; }
    public string? LocationSlug { get; set; }
    public string? BusinessName { get; set; }
    public List<string> Errors { get; set; } = new();
    public Dictionary<string, string> FieldErrors { get; set; } = new();
}
