using System;
using System.Collections.Generic;
using GrandeTech.QueueHub.API.Domain.Common;
using GrandeTech.QueueHub.API.Domain.Common.ValueObjects;

namespace GrandeTech.QueueHub.API.Domain.Customers
{
    /// <summary>
    /// Represents a customer (client) who can join queues
    /// </summary>
    public class Customer : BaseEntity, IAggregateRoot
    {
        public string Name { get; private set; } = string.Empty;
        public PhoneNumber? PhoneNumber { get; private set; }
        public Email? Email { get; private set; }
        public bool IsAnonymous { get; private set; }
        public bool NotificationsEnabled { get; private set; }
        public string? PreferredNotificationChannel { get; private set; }
        public string? UserId { get; private set; }
        
        // Service history tracking
        private readonly List<ServiceHistoryItem> _serviceHistory = new();
        public IReadOnlyCollection<ServiceHistoryItem> ServiceHistory => _serviceHistory.AsReadOnly();

        // Favorite service providers
        private readonly List<Guid> _favoriteLocationIds = new();
        public IReadOnlyCollection<Guid> FavoriteLocationIds => _favoriteLocationIds.AsReadOnly();

        // For EF Core
        private Customer() { }

        public Customer(
            string name,
            string? phoneNumber,
            string? email,
            bool isAnonymous,
            string? userId = null)
        {
            if (isAnonymous && string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Even anonymous customers must have a name", nameof(name));

            if (!isAnonymous && (string.IsNullOrWhiteSpace(phoneNumber) && string.IsNullOrWhiteSpace(email)))
                throw new ArgumentException("Non-anonymous customers must provide either phone number or email");

            Name = name;
            PhoneNumber = phoneNumber != null ? PhoneNumber.Create(phoneNumber) : null;
            Email = email != null ? Email.Create(email) : null;
            IsAnonymous = isAnonymous;
            NotificationsEnabled = !isAnonymous;
            PreferredNotificationChannel = DeterminePreferredNotificationChannel(phoneNumber, email);
            UserId = userId;

            AddDomainEvent(new CustomerCreatedEvent(Id, Name, IsAnonymous));
        }

        // Domain behavior methods
        public void UpdateProfile(
            string name,
            string? phoneNumber,
            string? email,
            string updatedBy)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required", nameof(name));

            Name = name;
            PhoneNumber = phoneNumber != null ? PhoneNumber.Create(phoneNumber) : null;
            Email = email != null ? Email.Create(email) : null;
            
            // If both phone and email are provided, and this was previously anonymous, it's no longer anonymous
            if (IsAnonymous && !string.IsNullOrWhiteSpace(phoneNumber) && !string.IsNullOrWhiteSpace(email))
            {
                IsAnonymous = false;
                AddDomainEvent(new CustomerConvertedToRegisteredEvent(Id));
            }

            PreferredNotificationChannel = DeterminePreferredNotificationChannel(phoneNumber, email);
            MarkAsModified(updatedBy);
            AddDomainEvent(new CustomerProfileUpdatedEvent(Id));
        }

        public void EnableNotifications(string updatedBy)
        {
            if (!NotificationsEnabled)
            {
                NotificationsEnabled = true;
                MarkAsModified(updatedBy);
                AddDomainEvent(new CustomerNotificationsEnabledEvent(Id));
            }
        }

        public void DisableNotifications(string updatedBy)
        {
            if (NotificationsEnabled)
            {
                NotificationsEnabled = false;
                MarkAsModified(updatedBy);
                AddDomainEvent(new CustomerNotificationsDisabledEvent(Id));
            }
        }

        public void SetPreferredNotificationChannel(string channel, string updatedBy)
        {
            if (string.IsNullOrWhiteSpace(channel))
                throw new ArgumentException("Notification channel cannot be empty", nameof(channel));

            if (channel != "sms" && channel != "email" && channel != "push" && channel != "whatsapp")
                throw new ArgumentException("Invalid notification channel. Supported values: sms, email, push, whatsapp", nameof(channel));

            PreferredNotificationChannel = channel;
            MarkAsModified(updatedBy);
            AddDomainEvent(new CustomerNotificationChannelChangedEvent(Id, channel));
        }

        public void AddServiceHistoryItem(
            Guid LocationId,
            Guid staffMemberId,
            Guid serviceTypeId,
            DateTime serviceDate,
            string? notes = null)
        {
            var historyItem = new ServiceHistoryItem(
                LocationId,
                staffMemberId,
                serviceTypeId,
                serviceDate,
                notes);

            _serviceHistory.Add(historyItem);
            AddDomainEvent(new CustomerServiceHistoryAddedEvent(Id, historyItem.Id));
        }

        public void AddFavoriteLocation(Guid LocationId)
        {
            if (!_favoriteLocationIds.Contains(LocationId))
            {
                _favoriteLocationIds.Add(LocationId);
                AddDomainEvent(new CustomerFavoriteLocationAddedEvent(Id, LocationId));
            }
        }

        public void RemoveFavoriteLocation(Guid LocationId)
        {
            if (_favoriteLocationIds.Contains(LocationId))
            {
                _favoriteLocationIds.Remove(LocationId);
                AddDomainEvent(new CustomerFavoriteLocationRemovedEvent(Id, LocationId));
            }
        }

        public void ConnectUserAccount(string userId, string updatedBy)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            if (IsAnonymous)
            {
                IsAnonymous = false;
                AddDomainEvent(new CustomerConvertedToRegisteredEvent(Id));
            }

            UserId = userId;
            MarkAsModified(updatedBy);
            AddDomainEvent(new CustomerConnectedToUserEvent(Id, userId));
        }

        private string DeterminePreferredNotificationChannel(string? phoneNumber, string? email)
        {
            if (!string.IsNullOrWhiteSpace(phoneNumber))
                return "whatsapp"; // Default to WhatsApp if phone is available

            if (!string.IsNullOrWhiteSpace(email))
                return "email";

            return "none";
        }
    }    public class ServiceHistoryItem
    {
        public Guid Id { get; private set; }
        public Guid LocationId { get; private set; }
        public Guid StaffMemberId { get; private set; }
        public Guid ServiceTypeId { get; private set; }
        public DateTime ServiceDate { get; private set; }
        public string? Notes { get; private set; }
        public int? Rating { get; private set; }
        public string? Feedback { get; private set; }

        // For EF Core
        private ServiceHistoryItem() { }        public ServiceHistoryItem(
            Guid locationId,
            Guid staffMemberId,
            Guid serviceTypeId,
            DateTime serviceDate,
            string? notes = null,
            int? rating = null,
            string? feedback = null)
        {
            Id = Guid.NewGuid();
            LocationId = locationId;
            StaffMemberId = staffMemberId;
            ServiceTypeId = serviceTypeId;
            ServiceDate = serviceDate;
            Notes = notes;
            Rating = rating;
            Feedback = feedback;
        }

        public void SetRating(int rating)
        {
            if (rating < 1 || rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5", nameof(rating));
            
            Rating = rating;
        }

        public void SetFeedback(string? feedback)
        {
            Feedback = feedback;
        }
    }
}
