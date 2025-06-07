namespace GrandeTech.QueueHub.API.Application.ServiceProviders.Requests;

public class CreateServiceProviderRequest
{
    public string BusinessName { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public ServiceProviderAddressRequest Address { get; set; } = new();
    public Dictionary<string, string> BusinessHours { get; set; } = new();
    public int MaxQueueCapacity { get; set; } = 10;
    public string? Description { get; set; }
    public string? Website { get; set; }
}

public class ServiceProviderAddressRequest
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}
