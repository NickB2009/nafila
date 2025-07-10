namespace Grande.Fila.API.Application.Organizations
{
    /// <summary>
    /// Request DTO for UC-TRACKQ: Admin/Owner track live activity
    /// </summary>
    public class TrackLiveActivityRequest
    {
        public string OrganizationId { get; set; } = string.Empty;
    }
}