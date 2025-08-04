using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Grande.Fila.API.Application.Locations.Requests;
using Grande.Fila.API.Application.Locations.Results;
using Grande.Fila.API.Domain.Locations;
using Grande.Fila.API.Domain.Common.ValueObjects;

namespace Grande.Fila.API.Application.Locations;

public class UpdateWeeklyHoursService
{
    private readonly ILocationRepository _locationRepository;
    private readonly ILogger<UpdateWeeklyHoursService> _logger;

    public UpdateWeeklyHoursService(
        ILocationRepository locationRepository,
        ILogger<UpdateWeeklyHoursService> logger)
    {
        _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UpdateWeeklyHoursResult> UpdateWeeklyHoursAsync(
        UpdateWeeklyHoursRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = new UpdateWeeklyHoursResult();

        try
        {
            // Validate request
            var validationErrors = ValidateRequest(request);
            if (validationErrors.Count > 0)
            {
                result.FieldErrors = validationErrors;
                return result;
            }

            // Parse location ID
            if (!Guid.TryParse(request.LocationId, out var locationId))
            {
                result.FieldErrors["LocationId"] = "Invalid location ID format.";
                return result;
            }

            // Get location
            var location = await _locationRepository.GetByIdAsync(locationId, cancellationToken);
            if (location == null)
            {
                result.Errors.Add("Location not found.");
                return result;
            }

            // Convert DTO to domain value object
            var weeklyHours = ConvertToWeeklyBusinessHours(request.WeeklyHours);
            if (weeklyHours == null)
            {
                result.Errors.Add("Invalid business hours data.");
                return result;
            }

            // Update location
            var currentUserId = "system"; // TODO: Get from authenticated user context
            location.UpdateWeeklyHours(weeklyHours, currentUserId);

            // Save changes
            await _locationRepository.UpdateAsync(location, cancellationToken);

            // Return success result
            result.Success = true;
            result.LocationId = location.Id.ToString();
            result.UpdatedBusinessHours = location.GetBusinessHoursDictionary();

            _logger.LogInformation(
                "Weekly business hours updated for location {LocationId} by user {UserId}",
                locationId,
                currentUserId);

            return result;
        }
        catch (Exception ex)
        {
            result.Errors.Add("An unexpected error occurred while updating business hours.");
            _logger.LogError(ex, "Error updating weekly hours for location {LocationId}", request.LocationId);
            return result;
        }
    }

    private Dictionary<string, string> ValidateRequest(UpdateWeeklyHoursRequest request)
    {
        var errors = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(request.LocationId))
            errors["LocationId"] = "Location ID is required.";

        if (request.WeeklyHours == null)
            errors["WeeklyHours"] = "Weekly hours data is required.";

        return errors;
    }

    private WeeklyBusinessHours? ConvertToWeeklyBusinessHours(WeeklyHoursDto dto)
    {
        try
        {
            var monday = ConvertToDayBusinessHours(dto.Monday);
            var tuesday = ConvertToDayBusinessHours(dto.Tuesday);
            var wednesday = ConvertToDayBusinessHours(dto.Wednesday);
            var thursday = ConvertToDayBusinessHours(dto.Thursday);
            var friday = ConvertToDayBusinessHours(dto.Friday);
            var saturday = ConvertToDayBusinessHours(dto.Saturday);
            var sunday = ConvertToDayBusinessHours(dto.Sunday);

            return WeeklyBusinessHours.Create(
                monday, tuesday, wednesday, thursday, friday, saturday, sunday);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting weekly hours DTO to domain object");
            return null;
        }
    }

    private DayBusinessHours ConvertToDayBusinessHours(DayHoursDto dto)
    {
        if (!dto.IsOpen)
            return DayBusinessHours.Closed();

        if (string.IsNullOrWhiteSpace(dto.OpenTime) || string.IsNullOrWhiteSpace(dto.CloseTime))
            throw new ArgumentException("Open and close times are required when day is marked as open");

        if (!TimeSpan.TryParse(dto.OpenTime, out var openTime))
            throw new ArgumentException($"Invalid open time format: {dto.OpenTime}. Use HH:mm format.");

        if (!TimeSpan.TryParse(dto.CloseTime, out var closeTime))
            throw new ArgumentException($"Invalid close time format: {dto.CloseTime}. Use HH:mm format.");

        return DayBusinessHours.Create(openTime, closeTime);
    }
}