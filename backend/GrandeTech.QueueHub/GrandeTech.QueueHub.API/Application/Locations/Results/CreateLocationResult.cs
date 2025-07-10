using System.Text.Json.Serialization;

namespace Grande.Fila.API.Application.Locations.Results;

public class CreateLocationResult
{
    public bool Success { get; set; }
    [JsonPropertyName("locationId")]
    public string? LocationId { get; set; }
    public string? LocationSlug { get; set; }
    public string? BusinessName { get; set; }
    public List<string> Errors { get; set; } = new();
    public Dictionary<string, string> FieldErrors { get; set; } = new();
}
