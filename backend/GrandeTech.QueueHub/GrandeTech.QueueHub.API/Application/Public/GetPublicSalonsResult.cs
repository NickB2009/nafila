namespace Grande.Fila.API.Application.Public;

public class GetPublicSalonsResult
{
    public bool Success { get; set; }
    public List<PublicSalonDto> Salons { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public Dictionary<string, string> FieldErrors { get; set; } = new();
}