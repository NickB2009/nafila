namespace Grande.Fila.API.Application.Queues
{
    /// <summary>
    /// Request to save haircut details when completing a service
    /// </summary>
    public class SaveHaircutDetailsRequest
    {
        public string QueueEntryId { get; set; } = string.Empty;
        public string HaircutDetails { get; set; } = string.Empty;
        public string? AdditionalNotes { get; set; }
        public string? PhotoUrl { get; set; }
    }
}