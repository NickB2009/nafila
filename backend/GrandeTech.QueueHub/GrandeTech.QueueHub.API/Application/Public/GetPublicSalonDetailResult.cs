namespace Grande.Fila.API.Application.Public;

public class GetPublicSalonDetailResult
{
    public bool Success { get; set; }
    public PublicSalonDto? Salon { get; set; }
    public List<string> Errors { get; set; } = new();
    public Dictionary<string, string> FieldErrors { get; set; } = new();
}