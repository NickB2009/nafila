using System.Text.Json.Serialization;

namespace Grande.Fila.API.Application.Public;

public class PublicSalonDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;
    
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }
    
    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }
    
    [JsonPropertyName("isOpen")]
    public bool IsOpen { get; set; }
    
    [JsonPropertyName("currentWaitTimeMinutes")]
    public int CurrentWaitTimeMinutes { get; set; }
    
    [JsonPropertyName("queueLength")]
    public int QueueLength { get; set; }
    
    [JsonPropertyName("isFast")]
    public bool IsFast { get; set; }
    
    [JsonPropertyName("isPopular")]
    public bool IsPopular { get; set; }
    
    [JsonPropertyName("rating")]
    public double Rating { get; set; }
    
    [JsonPropertyName("reviewCount")]
    public int ReviewCount { get; set; }
    
    [JsonPropertyName("services")]
    public List<string> Services { get; set; } = new();
    
    [JsonPropertyName("businessHours")]
    public Dictionary<string, string> BusinessHours { get; set; } = new();
}