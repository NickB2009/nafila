using System.Text.Json.Serialization;

namespace Grande.Fila.API.Application.Public;

public class PublicQueueStatusDto
{
    [JsonPropertyName("salonId")]
    public string SalonId { get; set; } = string.Empty;
    
    [JsonPropertyName("salonName")]
    public string SalonName { get; set; } = string.Empty;
    
    [JsonPropertyName("queueLength")]
    public int QueueLength { get; set; }
    
    [JsonPropertyName("estimatedWaitTimeMinutes")]
    public int EstimatedWaitTimeMinutes { get; set; }
    
    [JsonPropertyName("isAcceptingCustomers")]
    public bool IsAcceptingCustomers { get; set; }
    
    [JsonPropertyName("lastUpdated")]
    public DateTime LastUpdated { get; set; }
    
    [JsonPropertyName("availableServices")]
    public List<string> AvailableServices { get; set; } = new();
}