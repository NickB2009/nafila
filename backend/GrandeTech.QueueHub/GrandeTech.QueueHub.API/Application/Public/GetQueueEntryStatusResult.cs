namespace Grande.Fila.API.Application.Public;

public class GetQueueEntryStatusResult
{
    public bool Success { get; set; }
    public QueueEntryStatusDto? EntryStatus { get; set; }
    public List<string> Errors { get; set; } = new();
    public Dictionary<string, string> FieldErrors { get; set; } = new();
}

public class QueueEntryStatusDto
{
    public string? Id { get; set; }
    public int Position { get; set; }
    public int EstimatedWaitMinutes { get; set; }
    public string? Status { get; set; }
    public DateTime? JoinedAt { get; set; }
    public string? ServiceRequested { get; set; }
    public string? CustomerName { get; set; }
}
