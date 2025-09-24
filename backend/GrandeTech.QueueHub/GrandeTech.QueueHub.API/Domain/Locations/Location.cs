using System;
using System.Collections.Generic;
using Grande.Fila.API.Domain.Common;
using Grande.Fila.API.Domain.Common.ValueObjects;

namespace Grande.Fila.API.Domain.Locations
{
    /// <summary>
    /// Represents a location (barbershop) that belongs to an organization
    /// </summary>
    public class Location : BaseEntity, IAggregateRoot
    {
        public string Name { get; private set; } = string.Empty;
        public Slug Slug { get; private set; } = null!;
        public string? Description { get; private set; }
        public Guid OrganizationId { get; private set; }
        public Address Address { get; private set; } = null!;
        public PhoneNumber? ContactPhone { get; private set; }
        public Email? ContactEmail { get; private set; }
        // Flattened branding properties (replaces CustomBranding value object)
        public string? PrimaryColor { get; private set; }
        public string? SecondaryColor { get; private set; }
        public string? LogoUrl { get; private set; }
        public string? FaviconUrl { get; private set; }
        public string? CompanyName { get; private set; }
        public string? TagLine { get; private set; }
        public string? FontFamily { get; private set; }

        // Flattened weekly hours properties (replaces WeeklyBusinessHours value object)
        public TimeSpan? MondayOpenTime { get; private set; }
        public TimeSpan? MondayCloseTime { get; private set; }
        public bool MondayIsClosed { get; private set; }
        public TimeSpan? TuesdayOpenTime { get; private set; }
        public TimeSpan? TuesdayCloseTime { get; private set; }
        public bool TuesdayIsClosed { get; private set; }
        public TimeSpan? WednesdayOpenTime { get; private set; }
        public TimeSpan? WednesdayCloseTime { get; private set; }
        public bool WednesdayIsClosed { get; private set; }
        public TimeSpan? ThursdayOpenTime { get; private set; }
        public TimeSpan? ThursdayCloseTime { get; private set; }
        public bool ThursdayIsClosed { get; private set; }
        public TimeSpan? FridayOpenTime { get; private set; }
        public TimeSpan? FridayCloseTime { get; private set; }
        public bool FridayIsClosed { get; private set; }
        public TimeSpan? SaturdayOpenTime { get; private set; }
        public TimeSpan? SaturdayCloseTime { get; private set; }
        public bool SaturdayIsClosed { get; private set; }
        public TimeSpan? SundayOpenTime { get; private set; }
        public TimeSpan? SundayCloseTime { get; private set; }
        public bool SundayIsClosed { get; private set; }
        public bool IsQueueEnabled { get; private set; }
        public int MaxQueueSize { get; private set; }
        public int LateClientCapTimeInMinutes { get; private set; }
        public bool IsActive { get; private set; }
        public double AverageServiceTimeInMinutes { get; private set; }
        public DateTime LastAverageTimeReset { get; private set; }

        // Navigation properties
        private readonly List<Guid> _staffMemberIds = new();
        public IReadOnlyCollection<Guid> StaffMemberIds => _staffMemberIds.AsReadOnly();

        private readonly List<Guid> _serviceTypeIds = new();
        public IReadOnlyCollection<Guid> ServiceTypeIds => _serviceTypeIds.AsReadOnly();

        private readonly List<Guid> _advertisementIds = new();
        public IReadOnlyCollection<Guid> AdvertisementIds => _advertisementIds.AsReadOnly();

        // For EF Core
        private Location() { }

        public Location(
            string name,
            string slug,
            string description,
            Guid organizationId,
            Address address,
            string? contactPhone,
            string? contactEmail,
            TimeSpan openingTime,
            TimeSpan closingTime,
            int maxQueueSize,
            int lateClientCapTimeInMinutes,
            string createdBy)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Location name is required", nameof(name));

            if (organizationId == Guid.Empty)
                throw new ArgumentException("Organization ID is required", nameof(organizationId));

            Name = name;
            Slug = Slug.Create(slug ?? name);
            Description = description;
            OrganizationId = organizationId;
            Address = address;
            ContactPhone = contactPhone != null ? PhoneNumber.Create(contactPhone) : null;
            ContactEmail = contactEmail != null ? Email.Create(contactEmail) : null;
            // Set default business hours (Monday-Saturday same hours, Sunday closed)
            SetBusinessHours(openingTime, closingTime);
            IsQueueEnabled = true;
            MaxQueueSize = maxQueueSize > 0 ? maxQueueSize : 100;
            LateClientCapTimeInMinutes = lateClientCapTimeInMinutes >= 0 ? lateClientCapTimeInMinutes : 15;
            IsActive = true;
            AverageServiceTimeInMinutes = 30; // Default average time
            LastAverageTimeReset = DateTime.UtcNow;
            CreatedBy = createdBy;

            AddDomainEvent(new LocationCreatedEvent(Id, Name, OrganizationId));
        }

        // Domain behavior methods
        public void UpdateDetails(
            string name,
            string description,
            Address address,
            string? contactPhone,
            string? contactEmail,
            TimeSpan openingTime,
            TimeSpan closingTime,
            string updatedBy)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Location name is required", nameof(name));

            Name = name;
            Description = description;
            Address = address;
            ContactPhone = contactPhone != null ? PhoneNumber.Create(contactPhone) : null;
            ContactEmail = contactEmail != null ? Email.Create(contactEmail) : null;
            SetBusinessHours(openingTime, closingTime);

            MarkAsModified(updatedBy);
            AddDomainEvent(new LocationUpdatedEvent(Id));
        }

        public void SetCustomBranding(
            string primaryColor,
            string secondaryColor,
            string? logoUrl,
            string? faviconUrl,
            string? tagLine,
            string updatedBy)
        {
            PrimaryColor = ValidateHexColor(primaryColor);
            SecondaryColor = ValidateHexColor(secondaryColor ?? "#FFFFFF");
            LogoUrl = logoUrl?.Trim() ?? string.Empty;
            FaviconUrl = faviconUrl?.Trim() ?? string.Empty;
            CompanyName = Name;
            TagLine = tagLine?.Trim() ?? string.Empty;
            FontFamily = "Arial, sans-serif";

            MarkAsModified(updatedBy);
            AddDomainEvent(new LocationBrandingUpdatedEvent(Id));
        }

        public void ClearCustomBranding(string updatedBy)
        {
            PrimaryColor = null;
            SecondaryColor = null;
            LogoUrl = null;
            FaviconUrl = null;
            CompanyName = null;
            TagLine = null;
            FontFamily = null;
            
            MarkAsModified(updatedBy);
            AddDomainEvent(new LocationBrandingUpdatedEvent(Id));
        }

        public void EnableQueue(string updatedBy)
        {
            if (!IsQueueEnabled)
            {
                IsQueueEnabled = true;
                MarkAsModified(updatedBy);
                AddDomainEvent(new LocationQueueEnabledEvent(Id));
            }
        }

        public void DisableQueue(string updatedBy)
        {
            if (IsQueueEnabled)
            {
                IsQueueEnabled = false;
                MarkAsModified(updatedBy);
                AddDomainEvent(new LocationQueueDisabledEvent(Id));
            }
        }

        public void UpdateQueueSettings(int maxQueueSize, int lateClientCapTimeInMinutes, string updatedBy)
        {
            if (maxQueueSize <= 0)
                throw new ArgumentException("Maximum queue size must be positive", nameof(maxQueueSize));

            if (lateClientCapTimeInMinutes < 0)
                throw new ArgumentException("Late client cap time cannot be negative", nameof(lateClientCapTimeInMinutes));

            MaxQueueSize = maxQueueSize;
            LateClientCapTimeInMinutes = lateClientCapTimeInMinutes;
            MarkAsModified(updatedBy);
            AddDomainEvent(new LocationQueueSettingsUpdatedEvent(Id));
        }

        public void Activate(string updatedBy)
        {
            if (!IsActive)
            {
                IsActive = true;
                MarkAsModified(updatedBy);
                AddDomainEvent(new LocationActivatedEvent(Id));
            }
        }

        public void Deactivate(string updatedBy)
        {
            if (IsActive)
            {
                IsActive = false;
                MarkAsModified(updatedBy);
                AddDomainEvent(new LocationDeactivatedEvent(Id));
            }
        }

        public void AddStaffMember(Guid staffMemberId)
        {
            if (!_staffMemberIds.Contains(staffMemberId))
            {
                _staffMemberIds.Add(staffMemberId);
                AddDomainEvent(new StaffMemberAddedToLocationEvent(Id, staffMemberId));
            }
        }

        public void RemoveStaffMember(Guid staffMemberId)
        {
            if (_staffMemberIds.Contains(staffMemberId))
            {
                _staffMemberIds.Remove(staffMemberId);
                AddDomainEvent(new StaffMemberRemovedFromLocationEvent(Id, staffMemberId));
            }
        }

        public void AddServiceType(Guid serviceTypeId)
        {
            if (!_serviceTypeIds.Contains(serviceTypeId))
            {
                _serviceTypeIds.Add(serviceTypeId);
                AddDomainEvent(new ServiceTypeAddedToLocationEvent(Id, serviceTypeId));
            }
        }

        public void RemoveServiceType(Guid serviceTypeId)
        {
            if (_serviceTypeIds.Contains(serviceTypeId))
            {
                _serviceTypeIds.Remove(serviceTypeId);
                AddDomainEvent(new ServiceTypeRemovedFromLocationEvent(Id, serviceTypeId));
            }
        }

        public void AddAdvertisement(Guid advertisementId)
        {
            if (!_advertisementIds.Contains(advertisementId))
            {
                _advertisementIds.Add(advertisementId);
                AddDomainEvent(new AdvertisementAddedToLocationEvent(Id, advertisementId));
            }
        }

        public void RemoveAdvertisement(Guid advertisementId)
        {
            if (_advertisementIds.Contains(advertisementId))
            {
                _advertisementIds.Remove(advertisementId);
                AddDomainEvent(new AdvertisementRemovedFromLocationEvent(Id, advertisementId));
            }
        }

        public void UpdateAverageServiceTime(double newAverageTimeInMinutes, string updatedBy)
        {
            if (newAverageTimeInMinutes < 0)
                throw new ArgumentException("Average service time cannot be negative", nameof(newAverageTimeInMinutes));

            AverageServiceTimeInMinutes = newAverageTimeInMinutes;
            MarkAsModified(updatedBy);
            AddDomainEvent(new LocationAverageTimeUpdatedEvent(Id, newAverageTimeInMinutes));
        }

        public void ResetAverageServiceTime(string updatedBy)
        {
            LastAverageTimeReset = DateTime.UtcNow;
            MarkAsModified(updatedBy);
            AddDomainEvent(new LocationAverageTimeResetEvent(Id));
        }

        public bool IsOpen()
        {
            if (!IsActive) return false;

            try
            {
                // Use Brazil timezone (UTC-3) since all locations are in Brazil
                var brazilTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, brazilTimeZone);
                return IsOpenAt(now);
            }
            catch (TimeZoneNotFoundException)
            {
                // Fallback to UTC if Brazil timezone is not available (e.g., in test environments)
                var now = DateTime.UtcNow;
                return IsOpenAt(now);
            }
        }

        public bool CanAcceptQueueEntries()
        {
            return IsActive && IsQueueEnabled && IsOpen();
        }

        public void UpdateWeeklyHours(WeeklyBusinessHours weeklyHours, string updatedBy)
        {
            if (weeklyHours == null)
                throw new ArgumentNullException(nameof(weeklyHours));

            // Copy hours from the value object to flattened properties
            MondayOpenTime = weeklyHours.Monday.OpenTime;
            MondayCloseTime = weeklyHours.Monday.CloseTime;
            MondayIsClosed = !weeklyHours.Monday.IsOpen;
            TuesdayOpenTime = weeklyHours.Tuesday.OpenTime;
            TuesdayCloseTime = weeklyHours.Tuesday.CloseTime;
            TuesdayIsClosed = !weeklyHours.Tuesday.IsOpen;
            WednesdayOpenTime = weeklyHours.Wednesday.OpenTime;
            WednesdayCloseTime = weeklyHours.Wednesday.CloseTime;
            WednesdayIsClosed = !weeklyHours.Wednesday.IsOpen;
            ThursdayOpenTime = weeklyHours.Thursday.OpenTime;
            ThursdayCloseTime = weeklyHours.Thursday.CloseTime;
            ThursdayIsClosed = !weeklyHours.Thursday.IsOpen;
            FridayOpenTime = weeklyHours.Friday.OpenTime;
            FridayCloseTime = weeklyHours.Friday.CloseTime;
            FridayIsClosed = !weeklyHours.Friday.IsOpen;
            SaturdayOpenTime = weeklyHours.Saturday.OpenTime;
            SaturdayCloseTime = weeklyHours.Saturday.CloseTime;
            SaturdayIsClosed = !weeklyHours.Saturday.IsOpen;
            SundayOpenTime = weeklyHours.Sunday.OpenTime;
            SundayCloseTime = weeklyHours.Sunday.CloseTime;
            SundayIsClosed = !weeklyHours.Sunday.IsOpen;

            MarkAsModified(updatedBy);
            AddDomainEvent(new LocationUpdatedEvent(Id));
        }

        // Helper methods for working with flattened properties

        /// <summary>
        /// Sets business hours for Monday-Saturday with same hours, Sunday closed
        /// </summary>
        private void SetBusinessHours(TimeSpan openingTime, TimeSpan closingTime)
        {
            // Monday-Saturday: same hours
            MondayOpenTime = openingTime;
            MondayCloseTime = closingTime;
            MondayIsClosed = false;
            TuesdayOpenTime = openingTime;
            TuesdayCloseTime = closingTime;
            TuesdayIsClosed = false;
            WednesdayOpenTime = openingTime;
            WednesdayCloseTime = closingTime;
            WednesdayIsClosed = false;
            ThursdayOpenTime = openingTime;
            ThursdayCloseTime = closingTime;
            ThursdayIsClosed = false;
            FridayOpenTime = openingTime;
            FridayCloseTime = closingTime;
            FridayIsClosed = false;
            SaturdayOpenTime = openingTime;
            SaturdayCloseTime = closingTime;
            SaturdayIsClosed = false;
            
            // Sunday: closed
            SundayOpenTime = null;
            SundayCloseTime = null;
            SundayIsClosed = true;
        }

        /// <summary>
        /// Checks if the location is open at a specific date/time
        /// </summary>
        private bool IsOpenAt(DateTime dateTime)
        {
            var dayOfWeek = dateTime.DayOfWeek;
            var timeOfDay = dateTime.TimeOfDay;

            return dayOfWeek switch
            {
                DayOfWeek.Monday => !MondayIsClosed && IsTimeWithinHours(timeOfDay, MondayOpenTime, MondayCloseTime),
                DayOfWeek.Tuesday => !TuesdayIsClosed && IsTimeWithinHours(timeOfDay, TuesdayOpenTime, TuesdayCloseTime),
                DayOfWeek.Wednesday => !WednesdayIsClosed && IsTimeWithinHours(timeOfDay, WednesdayOpenTime, WednesdayCloseTime),
                DayOfWeek.Thursday => !ThursdayIsClosed && IsTimeWithinHours(timeOfDay, ThursdayOpenTime, ThursdayCloseTime),
                DayOfWeek.Friday => !FridayIsClosed && IsTimeWithinHours(timeOfDay, FridayOpenTime, FridayCloseTime),
                DayOfWeek.Saturday => !SaturdayIsClosed && IsTimeWithinHours(timeOfDay, SaturdayOpenTime, SaturdayCloseTime),
                DayOfWeek.Sunday => !SundayIsClosed && IsTimeWithinHours(timeOfDay, SundayOpenTime, SundayCloseTime),
                _ => false
            };
        }

        /// <summary>
        /// Checks if a time is within the specified open/close hours
        /// </summary>
        private static bool IsTimeWithinHours(TimeSpan time, TimeSpan? openTime, TimeSpan? closeTime)
        {
            if (!openTime.HasValue || !closeTime.HasValue)
                return false;

            // Handle cases where close time is after midnight (e.g., 22:00 - 02:00)
            if (closeTime.Value < openTime.Value)
            {
                return time >= openTime.Value || time <= closeTime.Value;
            }

            return time >= openTime.Value && time <= closeTime.Value;
        }

        /// <summary>
        /// Validates hex color format
        /// </summary>
        private static string ValidateHexColor(string color)
        {
            if (string.IsNullOrWhiteSpace(color))
                return "#000000";

            color = color.Trim();

            // If it doesn't start with #, add it
            if (!color.StartsWith("#"))
                color = "#" + color;

            // If it's a short form hex color (#RGB), convert to long form (#RRGGBB)
            if (color.Length == 4)
            {
                color = "#" +
                       color[1] + color[1] +
                       color[2] + color[2] +
                       color[3] + color[3];
            }

            // Check if it's a valid hex color
            if (color.Length != 7 || !System.Text.RegularExpressions.Regex.IsMatch(color, "^#[0-9A-Fa-f]{6}$"))
                throw new ArgumentException("Invalid hex color format. Must be #RRGGBB", nameof(color));

            return color.ToUpperInvariant();
        }

        /// <summary>
        /// Gets business hours as a dictionary for API responses
        /// This replaces the hardcoded logic in service classes
        /// </summary>
        public Dictionary<string, string> GetBusinessHoursDictionary()
        {
            return new Dictionary<string, string>
            {
                ["monday"] = FormatDayHours(MondayOpenTime, MondayCloseTime, MondayIsClosed),
                ["tuesday"] = FormatDayHours(TuesdayOpenTime, TuesdayCloseTime, TuesdayIsClosed),
                ["wednesday"] = FormatDayHours(WednesdayOpenTime, WednesdayCloseTime, WednesdayIsClosed),
                ["thursday"] = FormatDayHours(ThursdayOpenTime, ThursdayCloseTime, ThursdayIsClosed),
                ["friday"] = FormatDayHours(FridayOpenTime, FridayCloseTime, FridayIsClosed),
                ["saturday"] = FormatDayHours(SaturdayOpenTime, SaturdayCloseTime, SaturdayIsClosed),
                ["sunday"] = FormatDayHours(SundayOpenTime, SundayCloseTime, SundayIsClosed)
            };
        }

        /// <summary>
        /// Formats a day's hours for display
        /// </summary>
        private static string FormatDayHours(TimeSpan? openTime, TimeSpan? closeTime, bool isClosed)
        {
            if (isClosed)
                return "Closed";

            if (!openTime.HasValue || !closeTime.HasValue)
                return "Closed";

            return $"{openTime.Value:hh\\:mm} - {closeTime.Value:hh\\:mm}";
        }

        public int CalculateEstimatedWaitTime(int positionInQueue, double? overrideAverageTime = null)
        {
            var averageTime = overrideAverageTime ?? AverageServiceTimeInMinutes;
            var activeBarbers = _staffMemberIds.Count > 0 ? _staffMemberIds.Count : 1;
            
            // Simple calculation for now: position / active barbers * average time
            return (int)Math.Ceiling(positionInQueue / (double)activeBarbers * averageTime);
        }
    }
}
