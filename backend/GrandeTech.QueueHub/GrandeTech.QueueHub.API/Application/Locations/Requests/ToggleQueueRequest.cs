namespace Grande.Fila.API.Application.Locations.Requests
{
    /// <summary>
    /// Request to enable or disable queue for a location
    /// </summary>
    public class ToggleQueueRequest
    {
        public string LocationId { get; set; } = string.Empty;
        public bool EnableQueue { get; set; }
    }
}