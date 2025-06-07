namespace GrandeTech.QueueHub.API.Application.ServiceProviders.Results;

public class CreateServiceProviderResult
{
    public bool Success { get; set; }
    public string? ServiceProviderId { get; set; }
    public string? LocationSlug { get; set; }
    public string? BusinessName { get; set; }
    public List<string> Errors { get; set; } = new();
    public Dictionary<string, string> FieldErrors { get; set; } = new();
}
