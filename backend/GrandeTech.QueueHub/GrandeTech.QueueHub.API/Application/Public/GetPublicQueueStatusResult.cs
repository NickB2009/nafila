namespace Grande.Fila.API.Application.Public;

public class GetPublicQueueStatusResult
{
    public bool Success { get; set; }
    public PublicQueueStatusDto? QueueStatus { get; set; }
    public List<string> Errors { get; set; } = new();
    public Dictionary<string, string> FieldErrors { get; set; } = new();
}