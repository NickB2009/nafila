using System;
using System.Collections.Generic;
using GrandeTech.QueueHub.API.Domain.Common;

namespace GrandeTech.QueueHub.API.Domain.Advertising
{
    /// <summary>
    /// Represents an advertisement that can be displayed on kiosks or queue screens
    /// </summary>
    public class Advertisement : BaseEntity, IAggregateRoot
    {
        public string Title { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public string MediaUrl { get; private set; } = string.Empty;
        public string MediaType { get; private set; } = string.Empty; // image, video
        public Guid LocationId { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime? StartDate { get; private set; }
        public DateTime? EndDate { get; private set; }
        public int DisplayDurationSeconds { get; private set; }
        public int DisplayPriority { get; private set; }
        public bool IsGlobal { get; private set; }
        
        // For EF Core
        private Advertisement() { }

        public Advertisement(
            string title,
            string? description,
            string mediaUrl,
            string mediaType,
            Guid locationId,
            DateTime? startDate,
            DateTime? endDate,
            int displayDurationSeconds,
            int displayPriority,
            bool isGlobal,
            string createdBy)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Advertisement title is required", nameof(title));

            if (string.IsNullOrWhiteSpace(mediaUrl))
                throw new ArgumentException("Media URL is required", nameof(mediaUrl));

            if (string.IsNullOrWhiteSpace(mediaType))
                throw new ArgumentException("Media type is required", nameof(mediaType));

            if (!IsValidMediaType(mediaType))
                throw new ArgumentException("Invalid media type. Must be 'image' or 'video'", nameof(mediaType));

            if (displayDurationSeconds <= 0)
                throw new ArgumentException("Display duration must be positive", nameof(displayDurationSeconds));

            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
                throw new ArgumentException("Start date cannot be after end date", nameof(startDate));

            Title = title;
            Description = description;
            MediaUrl = mediaUrl;
            MediaType = mediaType.ToLower();
            LocationId = locationId;
            StartDate = startDate;
            EndDate = endDate;
            DisplayDurationSeconds = displayDurationSeconds;
            DisplayPriority = displayPriority;
            IsGlobal = isGlobal;
            IsActive = true;
            CreatedBy = createdBy;

            AddDomainEvent(new AdvertisementCreatedEvent(Id, Title, LocationId));
        }

        // Domain behavior methods
        public void UpdateDetails(
            string title,
            string? description,
            string mediaUrl,
            string mediaType,
            DateTime? startDate,
            DateTime? endDate,
            int displayDurationSeconds,
            int displayPriority,
            string updatedBy)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Advertisement title is required", nameof(title));

            if (string.IsNullOrWhiteSpace(mediaUrl))
                throw new ArgumentException("Media URL is required", nameof(mediaUrl));

            if (string.IsNullOrWhiteSpace(mediaType))
                throw new ArgumentException("Media type is required", nameof(mediaType));

            if (!IsValidMediaType(mediaType))
                throw new ArgumentException("Invalid media type. Must be 'image' or 'video'", nameof(mediaType));

            if (displayDurationSeconds <= 0)
                throw new ArgumentException("Display duration must be positive", nameof(displayDurationSeconds));

            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
                throw new ArgumentException("Start date cannot be after end date", nameof(startDate));

            Title = title;
            Description = description;
            MediaUrl = mediaUrl;
            MediaType = mediaType.ToLower();
            StartDate = startDate;
            EndDate = endDate;
            DisplayDurationSeconds = displayDurationSeconds;
            DisplayPriority = displayPriority;
            
            MarkAsModified(updatedBy);
            AddDomainEvent(new AdvertisementUpdatedEvent(Id));
        }

        public void Activate(string updatedBy)
        {
            if (!IsActive)
            {
                IsActive = true;
                MarkAsModified(updatedBy);
                AddDomainEvent(new AdvertisementActivatedEvent(Id));
            }
        }

        public void Deactivate(string updatedBy)
        {
            if (IsActive)
            {
                IsActive = false;
                MarkAsModified(updatedBy);
                AddDomainEvent(new AdvertisementDeactivatedEvent(Id));
            }
        }

        public void SetGlobalVisibility(bool isGlobal, string updatedBy)
        {
            if (IsGlobal != isGlobal)
            {
                IsGlobal = isGlobal;
                MarkAsModified(updatedBy);
                AddDomainEvent(new AdvertisementVisibilityChangedEvent(Id, isGlobal));
            }
        }

        public bool IsValid()
        {
            var now = DateTime.UtcNow;
            
            if (!IsActive)
                return false;
            
            if (StartDate.HasValue && now < StartDate.Value)
                return false;
            
            if (EndDate.HasValue && now > EndDate.Value)
                return false;
            
            return true;
        }

        private bool IsValidMediaType(string mediaType)
        {
            var normalizedType = mediaType.ToLowerInvariant();
            return normalizedType == "image" || normalizedType == "video";
        }
    }
}
