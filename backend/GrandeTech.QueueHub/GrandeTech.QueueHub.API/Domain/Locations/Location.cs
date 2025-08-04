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
        public BrandingConfig? CustomBranding { get; private set; }
        public WeeklyBusinessHours WeeklyHours { get; private set; } = null!;
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
            WeeklyHours = WeeklyBusinessHours.CreateMondayToSaturday(openingTime, closingTime); // Default: Monday-Saturday same hours, Sunday closed
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
            WeeklyHours = WeeklyBusinessHours.CreateMondayToSaturday(openingTime, closingTime); // Maintain backward compatibility

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
            CustomBranding = BrandingConfig.Create(
                primaryColor,
                secondaryColor,
                logoUrl ?? string.Empty,
                faviconUrl ?? string.Empty,
                Name,
                tagLine ?? string.Empty
            );

            MarkAsModified(updatedBy);
            AddDomainEvent(new LocationBrandingUpdatedEvent(Id));
        }

        public void ClearCustomBranding(string updatedBy)
        {
            if (CustomBranding != null)
            {
                CustomBranding = null;
                MarkAsModified(updatedBy);
                AddDomainEvent(new LocationBrandingUpdatedEvent(Id));
            }
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
            
            // Use Brazil timezone (UTC-3) since all locations are in Brazil
            var brazilTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, brazilTimeZone);
            return WeeklyHours.IsOpenAt(now);
        }

        public bool CanAcceptQueueEntries()
        {
            return IsActive && IsQueueEnabled && IsOpen();
        }

        public void UpdateWeeklyHours(WeeklyBusinessHours weeklyHours, string updatedBy)
        {
            WeeklyHours = weeklyHours ?? throw new ArgumentNullException(nameof(weeklyHours));
            MarkAsModified(updatedBy);
            AddDomainEvent(new LocationUpdatedEvent(Id));
        }

        /// <summary>
        /// Gets business hours as a dictionary for API responses
        /// This replaces the hardcoded logic in service classes
        /// </summary>
        public Dictionary<string, string> GetBusinessHoursDictionary()
        {
            return WeeklyHours.ToDictionary();
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
