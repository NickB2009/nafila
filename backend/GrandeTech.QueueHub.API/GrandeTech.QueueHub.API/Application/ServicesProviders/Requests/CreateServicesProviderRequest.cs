namespace GrandeTech.QueueHub.API.Application.ServicesProviders.Requests;

public class CreateServicesProviderRequest
{
    public string BusinessName { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public ServicesProviderAddressRequest Address { get; set; } = new();
    public Dictionary<string, string> BusinessHours { get; set; } = new();
    public int MaxQueueCapacity { get; set; } = 10;
    public string? Description { get; set; }
    public string? Website { get; set; }
}

public class ServicesProviderAddressRequest
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}
