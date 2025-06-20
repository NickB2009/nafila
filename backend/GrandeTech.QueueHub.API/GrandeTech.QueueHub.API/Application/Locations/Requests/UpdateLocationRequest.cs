using System.Collections.Generic;

namespace Grande.Fila.API.Application.Locations.Requests;

public class UpdateLocationRequest
{
    public string BusinessName { get; set; } = string.Empty;
    public LocationAddressRequest Address { get; set; } = new LocationAddressRequest();
    public Dictionary<string, string> BusinessHours { get; set; } = new Dictionary<string, string>();
    public int MaxQueueCapacity { get; set; }
    public string Description { get; set; } = string.Empty;
} 