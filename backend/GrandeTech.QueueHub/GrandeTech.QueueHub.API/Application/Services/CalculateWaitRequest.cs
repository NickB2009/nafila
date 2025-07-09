namespace Grande.Fila.API.Application.Services
{
    public class CalculateWaitRequest
    {
        public required string LocationId { get; set; }
        public required string QueueId { get; set; }
        public required string EntryId { get; set; }
    }
} 