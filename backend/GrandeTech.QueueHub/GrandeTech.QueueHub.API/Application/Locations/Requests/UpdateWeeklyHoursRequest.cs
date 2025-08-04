using System.ComponentModel.DataAnnotations;

namespace Grande.Fila.API.Application.Locations.Requests;

public class UpdateWeeklyHoursRequest
{
    [Required]
    public string LocationId { get; set; } = string.Empty;
    
    [Required]
    public WeeklyHoursDto WeeklyHours { get; set; } = new();
}

public class WeeklyHoursDto
{
    public DayHoursDto Monday { get; set; } = new();
    public DayHoursDto Tuesday { get; set; } = new();
    public DayHoursDto Wednesday { get; set; } = new();
    public DayHoursDto Thursday { get; set; } = new();
    public DayHoursDto Friday { get; set; } = new();
    public DayHoursDto Saturday { get; set; } = new();
    public DayHoursDto Sunday { get; set; } = new();
}

public class DayHoursDto
{
    public bool IsOpen { get; set; } = true;
    
    /// <summary>
    /// Opening time in HH:mm format (e.g., "09:00")
    /// </summary>
    public string? OpenTime { get; set; }
    
    /// <summary>
    /// Closing time in HH:mm format (e.g., "17:00")
    /// </summary>
    public string? CloseTime { get; set; }
}