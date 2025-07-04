namespace Grande.Fila.API.Application.Kiosk
{
    public class KioskDisplayRequest
    {
        public required string LocationId { get; set; }
        public bool IncludeCompletedEntries { get; set; } = false;
    }
} 