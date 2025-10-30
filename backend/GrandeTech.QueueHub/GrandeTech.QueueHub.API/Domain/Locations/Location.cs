using System;
using System.Collections.Generic;
using Grande.Fila.API.Domain.Common;
using Grande.Fila.API.Domain.Common.ValueObjects;

namespace Grande.Fila.API.Domain.Locations
{
    /// <summary>
    /// Represents a location (barbershop) that belongs to an organization
    /// </summary>
    public class Location : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public Slug Slug { get; private set; } = null!;
        public string? Description { get; private set; }
        public Guid OrganizationId { get; private set; }
        public Address Address { get; private set; } = null!;
        public PhoneNumber? ContactPhone { get; private set; }
        public Email? ContactEmail { get; private set; }
        // Flattened branding properties (replacing BrandingConfig value object)
        public string? PrimaryColor { get; private set; }
        public string? SecondaryColor { get; private set; }
        public string? LogoUrl { get; private set; }
        public string? FaviconUrl { get; private set; }
        public string? CompanyName { get; private set; }
        public string? TagLine { get; private set; }
        public string? FontFamily { get; private set; }
        
        // Flattened weekly hours properties (replacing WeeklyBusinessHours value object)
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
            // Initialize flattened weekly hours (Monday-Saturday same hours, Sunday closed)
            SetWeeklyHoursForDays(openingTime, closingTime);
            IsQueueEnabled = true;
            MaxQueueSize = maxQueueSize > 0 ? maxQueueSize : 100;
            LateClientCapTimeInMinutes = lateClientCapTimeInMinutes >= 0 ? lateClientCapTimeInMinutes : 15;
            IsActive = true;
            AverageServiceTimeInMinutes = 30; // Default average time
            LastAverageTimeReset = DateTime.UtcNow;
            // AddDomainEvent(new LocationCreatedEvent(Id, Name, OrganizationId));
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
            // Update flattened weekly hours
            SetWeeklyHoursForDays(openingTime, closingTime);

            // MarkAsModified(updatedBy);
            // AddDomainEvent(new LocationUpdatedEvent(Id));
        }

        public void SetCustomBranding(
            string primaryColor,
            string secondaryColor,
            string? logoUrl,
            string? faviconUrl,
            string? tagLine,
            string updatedBy)
        {
            // Validate and set flattened branding properties
            PrimaryColor = ValidateHexColor(primaryColor);
            SecondaryColor = ValidateHexColor(secondaryColor ?? "#FFFFFF");
            LogoUrl = logoUrl?.Trim() ?? string.Empty;
            FaviconUrl = faviconUrl?.Trim() ?? string.Empty;
            CompanyName = Name.Trim();
            TagLine = tagLine?.Trim() ?? string.Empty;
            FontFamily = "Arial, sans-serif"; // Default font

            // MarkAsModified(updatedBy);
            // AddDomainEvent(new LocationBrandingUpdatedEvent(Id));
        }

        public void ClearCustomBranding(string updatedBy)
        {
            // Clear all flattened branding properties
            PrimaryColor = null;
            SecondaryColor = null;
            LogoUrl = null;
            FaviconUrl = null;
            CompanyName = null;
            TagLine = null;
            FontFamily = null;
            
            // MarkAsModified(updatedBy);
            // AddDomainEvent(new LocationBrandingUpdatedEvent(Id));
        }

        public void EnableQueue(string updatedBy)
        {
            if (!IsQueueEnabled)
            {
                IsQueueEnabled = true;
                // MarkAsModified(updatedBy);
                // AddDomainEvent(new LocationQueueEnabledEvent(Id));
            }
        }

        public void DisableQueue(string updatedBy)
        {
            if (IsQueueEnabled)
            {
                IsQueueEnabled = false;
                // MarkAsModified(updatedBy);
                // AddDomainEvent(new LocationQueueDisabledEvent(Id));
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
            // MarkAsModified(updatedBy);
            // AddDomainEvent(new LocationQueueSettingsUpdatedEvent(Id));
        }

        public void Activate(string updatedBy)
        {
            if (!IsActive)
            {
                IsActive = true;
                // MarkAsModified(updatedBy);
                // AddDomainEvent(new LocationActivatedEvent(Id));
            }
        }

        public void Deactivate(string updatedBy)
        {
            if (IsActive)
            {
                IsActive = false;
                // MarkAsModified(updatedBy);
                // AddDomainEvent(new LocationDeactivatedEvent(Id));
            }
        }

        public void AddStaffMember(Guid staffMemberId)
        {
            if (!_staffMemberIds.Contains(staffMemberId))
            {
                _staffMemberIds.Add(staffMemberId);
                // AddDomainEvent(new StaffMemberAddedToLocationEvent(Id, staffMemberId));
            }
        }

        public void RemoveStaffMember(Guid staffMemberId)
        {
            if (_staffMemberIds.Contains(staffMemberId))
            {
                _staffMemberIds.Remove(staffMemberId);
                // AddDomainEvent(new StaffMemberRemovedFromLocationEvent(Id, staffMemberId));
            }
        }

        public void AddServiceType(Guid serviceTypeId)
        {
            if (!_serviceTypeIds.Contains(serviceTypeId))
            {
                _serviceTypeIds.Add(serviceTypeId);
                // AddDomainEvent(new ServiceTypeAddedToLocationEvent(Id, serviceTypeId));
            }
        }

        public void RemoveServiceType(Guid serviceTypeId)
        {
            if (_serviceTypeIds.Contains(serviceTypeId))
            {
                _serviceTypeIds.Remove(serviceTypeId);
                // AddDomainEvent(new ServiceTypeRemovedFromLocationEvent(Id, serviceTypeId));
            }
        }

        public void AddAdvertisement(Guid advertisementId)
        {
            if (!_advertisementIds.Contains(advertisementId))
            {
                _advertisementIds.Add(advertisementId);
                // AddDomainEvent(new AdvertisementAddedToLocationEvent(Id, advertisementId));
            }
        }

        public void RemoveAdvertisement(Guid advertisementId)
        {
            if (_advertisementIds.Contains(advertisementId))
            {
                _advertisementIds.Remove(advertisementId);
                // AddDomainEvent(new AdvertisementRemovedFromLocationEvent(Id, advertisementId));
            }
        }

        public void UpdateAverageServiceTime(double newAverageTimeInMinutes, string updatedBy)
        {
            if (newAverageTimeInMinutes < 0)
                throw new ArgumentException("Average service time cannot be negative", nameof(newAverageTimeInMinutes));

            AverageServiceTimeInMinutes = newAverageTimeInMinutes;
            // MarkAsModified(updatedBy);
            // AddDomainEvent(new LocationAverageTimeUpdatedEvent(Id, newAverageTimeInMinutes));
        }

        public void ResetAverageServiceTime(string updatedBy)
        {
            LastAverageTimeReset = DateTime.UtcNow;
            // MarkAsModified(updatedBy);
            // AddDomainEvent(new LocationAverageTimeResetEvent(Id));
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
            if (weeklyHours == null) throw new ArgumentNullException(nameof(weeklyHours));
            
            // Update flattened weekly hours properties
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
            
            // MarkAsModified(updatedBy);
            // AddDomainEvent(new LocationUpdatedEvent(Id));
        }

        /// <summary>
        /// Gets business hours as a dictionary for API responses
        /// This replaces the hardcoded logic in service classes
        /// </summary>
        public Dictionary<string, string> GetBusinessHoursDictionary()
        {
            return new Dictionary<string, string>
            {
                ["monday"] = GetDayHoursDisplayString(MondayOpenTime, MondayCloseTime, MondayIsClosed),
                ["tuesday"] = GetDayHoursDisplayString(TuesdayOpenTime, TuesdayCloseTime, TuesdayIsClosed),
                ["wednesday"] = GetDayHoursDisplayString(WednesdayOpenTime, WednesdayCloseTime, WednesdayIsClosed),
                ["thursday"] = GetDayHoursDisplayString(ThursdayOpenTime, ThursdayCloseTime, ThursdayIsClosed),
                ["friday"] = GetDayHoursDisplayString(FridayOpenTime, FridayCloseTime, FridayIsClosed),
                ["saturday"] = GetDayHoursDisplayString(SaturdayOpenTime, SaturdayCloseTime, SaturdayIsClosed),
                ["sunday"] = GetDayHoursDisplayString(SundayOpenTime, SundayCloseTime, SundayIsClosed)
            };
        }

        public int CalculateEstimatedWaitTime(int positionInQueue, double? overrideAverageTime = null)
        {
            var averageTime = overrideAverageTime ?? AverageServiceTimeInMinutes;
            var activeBarbers = _staffMemberIds.Count > 0 ? _staffMemberIds.Count : 1;
            
            // Simple calculation for now: position / active barbers * average time
            return (int)Math.Ceiling(positionInQueue / (double)activeBarbers * averageTime);
        }

        // Helper methods for flattened properties

        /// <summary>
        /// Checks if the location is open at a specific date/time using flattened properties
        /// </summary>
        private bool IsOpenAt(DateTime dateTime)
        {
            var dayOfWeek = dateTime.DayOfWeek;
            var timeOfDay = dateTime.TimeOfDay;

            return dayOfWeek switch
            {
                DayOfWeek.Monday => IsDayOpen(timeOfDay, MondayOpenTime, MondayCloseTime, MondayIsClosed),
                DayOfWeek.Tuesday => IsDayOpen(timeOfDay, TuesdayOpenTime, TuesdayCloseTime, TuesdayIsClosed),
                DayOfWeek.Wednesday => IsDayOpen(timeOfDay, WednesdayOpenTime, WednesdayCloseTime, WednesdayIsClosed),
                DayOfWeek.Thursday => IsDayOpen(timeOfDay, ThursdayOpenTime, ThursdayCloseTime, ThursdayIsClosed),
                DayOfWeek.Friday => IsDayOpen(timeOfDay, FridayOpenTime, FridayCloseTime, FridayIsClosed),
                DayOfWeek.Saturday => IsDayOpen(timeOfDay, SaturdayOpenTime, SaturdayCloseTime, SaturdayIsClosed),
                DayOfWeek.Sunday => IsDayOpen(timeOfDay, SundayOpenTime, SundayCloseTime, SundayIsClosed),
                _ => false
            };
        }

        /// <summary>
        /// Checks if a specific day is open at a given time
        /// </summary>
        private static bool IsDayOpen(TimeSpan time, TimeSpan? openTime, TimeSpan? closeTime, bool isClosed)
        {
            if (isClosed || !openTime.HasValue || !closeTime.HasValue)
                return false;

            var open = openTime.Value;
            var close = closeTime.Value;

            // Handle midnight closing (00:00:00) as end of day
            if (close == TimeSpan.Zero)
            {
                return time >= open;
            }

            // Handle closing after midnight (e.g., 02:00 for 2 AM)
            if (close < open)
            {
                // Business spans midnight (e.g., 22:00-02:00)
                return time >= open || time <= close;
            }

            // Normal case: open and close within same day
            return time >= open && time <= close;
        }

        /// <summary>
        /// Sets weekly hours for Monday-Saturday with same hours, Sunday closed
        /// </summary>
        private void SetWeeklyHoursForDays(TimeSpan openingTime, TimeSpan closingTime)
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
        /// Gets display string for a day's hours
        /// </summary>
        private static string GetDayHoursDisplayString(TimeSpan? openTime, TimeSpan? closeTime, bool isClosed)
        {
            if (isClosed)
                return "closed";

            if (!openTime.HasValue || !closeTime.HasValue)
                return "closed";

            return $"{openTime:hh\\:mm}-{closeTime:hh\\:mm}";
        }

        /// <summary>
        /// Validates and formats hex color (moved from BrandingConfig)
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
                throw new ArgumentException("Invalid hex color format. Must be #RRGGBB");

            return color.ToUpperInvariant();
        }
    }
}
