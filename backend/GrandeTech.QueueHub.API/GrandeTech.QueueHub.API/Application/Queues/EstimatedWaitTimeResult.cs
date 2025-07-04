namespace Grande.Fila.API.Application.Queues
{
    public class EstimatedWaitTimeResult
    {
        public bool Success { get; set; }
        public int EstimatedWaitMinutes { get; set; }
    }
} 