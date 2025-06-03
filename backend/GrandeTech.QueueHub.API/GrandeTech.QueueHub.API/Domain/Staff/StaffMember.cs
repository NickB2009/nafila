using System;
using System.Collections.Generic;
using GrandeTech.QueueHub.API.Domain.Common;
using GrandeTech.QueueHub.API.Domain.Common.ValueObjects;

namespace GrandeTech.QueueHub.API.Domain.Staff
{
    /// <summary>
    /// Represents a staff member (barber) who serves customers
    /// </summary>
    public class StaffMember : BaseEntity, IAggregateRoot
    {        public string Name { get; private set; } = string.Empty;
        public Guid ServiceProviderId { get; private set; }
        public Email? Email { get; private set; }
        public PhoneNumber? PhoneNumber { get; private set; }
        public string? ProfilePictureUrl { get; private set; }
        public string Role { get; private set; } = string.Empty;
        public bool IsActive { get; private set; }
        public bool IsOnDuty { get; private set; }
        public string StaffStatus { get; private set; } = "available";
        public string? UserId { get; private set; }
        public double AverageServiceTimeInMinutes { get; private set; }
        public int CompletedServicesCount { get; private set; }
        public string EmployeeCode { get; private set; } = string.Empty;
        
        // Navigation properties
        private readonly List<StaffBreak> _breaks = new();
        public IReadOnlyCollection<StaffBreak> Breaks => _breaks.AsReadOnly();

        private readonly List<Guid> _specialtyServiceTypeIds = new();
        public IReadOnlyCollection<Guid> SpecialtyServiceTypeIds => _specialtyServiceTypeIds.AsReadOnly();
        
        // For EF Core
        private StaffMember() { }

        public StaffMember(
            string name,
            Guid serviceProviderId,
            string? email,
            string? phoneNumber,
            string? profilePictureUrl,
            string role,
            string? userId,
            string createdBy)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Staff member name is required", nameof(name));

            if (serviceProviderId == Guid.Empty)
                throw new ArgumentException("Service provider ID is required", nameof(serviceProviderId));

            if (string.IsNullOrWhiteSpace(role))
                throw new ArgumentException("Role is required", nameof(role));

            Name = name;
            ServiceProviderId = serviceProviderId;
            Email = email != null ? Email.Create(email) : null;
            PhoneNumber = phoneNumber != null ? PhoneNumber.Create(phoneNumber) : null;
            ProfilePictureUrl = profilePictureUrl;
            Role = role;
            IsActive = true;
            StaffStatus = "available";
            UserId = userId;
            AverageServiceTimeInMinutes = 30; // Default average time
            CompletedServicesCount = 0;
            CreatedBy = createdBy;

            AddDomainEvent(new StaffMemberCreatedEvent(Id, Name, ServiceProviderId));
        }

        // Domain behavior methods
        public void UpdateDetails(
            string name,
            string? email,
            string? phoneNumber,
            string? profilePictureUrl,
            string role,
            string updatedBy)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Staff member name is required", nameof(name));

            if (string.IsNullOrWhiteSpace(role))
                throw new ArgumentException("Role is required", nameof(role));

            Name = name;
            Email = email != null ? Email.Create(email) : null;
            PhoneNumber = phoneNumber != null ? PhoneNumber.Create(phoneNumber) : null;
            ProfilePictureUrl = profilePictureUrl;
            Role = role;
            
            MarkAsModified(updatedBy);
            AddDomainEvent(new StaffMemberUpdatedEvent(Id));
        }

        public void Activate(string updatedBy)
        {
            if (!IsActive)
            {
                IsActive = true;
                MarkAsModified(updatedBy);
                AddDomainEvent(new StaffMemberActivatedEvent(Id));
            }
        }

        public void Deactivate(string updatedBy)
        {
            if (IsActive)
            {
                IsActive = false;
                MarkAsModified(updatedBy);
                AddDomainEvent(new StaffMemberDeactivatedEvent(Id));
            }
        }

        public void UpdateStatus(string status, string updatedBy)
        {
            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentException("Status cannot be empty", nameof(status));

            var validStatuses = new[] { "available", "busy", "away", "offline" };
            if (!Array.Exists(validStatuses, s => s == status))
                throw new ArgumentException($"Invalid status. Must be one of: {string.Join(", ", validStatuses)}", nameof(status));

            if (StaffStatus != status)
            {
                StaffStatus = status;
                MarkAsModified(updatedBy);
                AddDomainEvent(new StaffMemberStatusChangedEvent(Id, status));
            }
        }

        public StaffBreak StartBreak(TimeSpan duration, string reason, string updatedBy)
        {
            if (StaffStatus == "on-break")
                throw new InvalidOperationException("Staff member is already on break");

            var staffBreak = new StaffBreak(Id, duration, reason);
            _breaks.Add(staffBreak);
            
            StaffStatus = "on-break";
            MarkAsModified(updatedBy);
            
            AddDomainEvent(new StaffMemberStartedBreakEvent(Id, staffBreak.Id, duration, reason));
            return staffBreak;
        }

        public void EndBreak(Guid breakId, string updatedBy)
        {
            var staffBreak = _breaks.Find(b => b.Id == breakId && !b.EndedAt.HasValue);
            
            if (staffBreak == null)
                throw new InvalidOperationException("No active break found with the specified ID");

            staffBreak.End();
            StaffStatus = "available";
            MarkAsModified(updatedBy);
            
            AddDomainEvent(new StaffMemberEndedBreakEvent(Id, staffBreak.Id));
        }

        public void AddSpecialty(Guid serviceTypeId)
        {
            if (!_specialtyServiceTypeIds.Contains(serviceTypeId))
            {
                _specialtyServiceTypeIds.Add(serviceTypeId);
                AddDomainEvent(new StaffMemberSpecialtyAddedEvent(Id, serviceTypeId));
            }
        }

        public void RemoveSpecialty(Guid serviceTypeId)
        {
            if (_specialtyServiceTypeIds.Contains(serviceTypeId))
            {
                _specialtyServiceTypeIds.Remove(serviceTypeId);
                AddDomainEvent(new StaffMemberSpecialtyRemovedEvent(Id, serviceTypeId));
            }
        }

        public void ConnectUserAccount(string userId, string updatedBy)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            UserId = userId;
            MarkAsModified(updatedBy);
            AddDomainEvent(new StaffMemberConnectedToUserEvent(Id, userId));
        }

        public void UpdateServiceStats(int serviceDurationMinutes, string updatedBy)
        {
            if (serviceDurationMinutes <= 0)
                throw new ArgumentException("Service duration must be positive", nameof(serviceDurationMinutes));

            // Update average service time using a weighted average
            var totalMinutes = AverageServiceTimeInMinutes * CompletedServicesCount;
            CompletedServicesCount++;
            AverageServiceTimeInMinutes = (totalMinutes + serviceDurationMinutes) / CompletedServicesCount;
            
            MarkAsModified(updatedBy);
            AddDomainEvent(new StaffMemberServiceStatsUpdatedEvent(Id, AverageServiceTimeInMinutes, CompletedServicesCount));
        }

        public bool IsOnBreak()
        {
            return StaffStatus == "on-break";
        }

        public bool IsAvailableForService()
        {
            return IsActive && StaffStatus == "available";
        }

        public StaffBreak? GetCurrentBreak()
        {
            return _breaks.Find(b => !b.EndedAt.HasValue);
        }
    }
}
