using System.Collections.Generic;

namespace Grande.Fila.API.Application.Locations.Results;

public class UpdateWeeklyHoursResult
{
    public bool Success { get; set; }
    public string LocationId { get; set; } = string.Empty;
    public Dictionary<string, string> UpdatedBusinessHours { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public Dictionary<string, string> FieldErrors { get; set; } = new();
}