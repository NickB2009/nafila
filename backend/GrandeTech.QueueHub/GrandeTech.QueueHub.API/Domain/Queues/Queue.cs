using System;
using System.Collections.Generic;
using System.Linq;
using Grande.Fila.API.Domain.Common;
using System.Text.Json.Serialization;

namespace Grande.Fila.API.Domain.Queues
{
    /// <summary>
    /// Represents a queue for a service provider
    /// </summary>
    public class Queue : BaseEntity, IAggregateRoot
    {
        [JsonPropertyName("locationId")]
        public Guid LocationId { get; private set; }
        public DateTime QueueDate { get; private set; }
        public bool IsActive { get; private set; }
        public int MaxSize { get; private set; }
        public int LateClientCapTimeInMinutes { get; private set; }

        // Navigation properties
        private readonly List<QueueEntry> _entries = new();
        public IReadOnlyCollection<QueueEntry> Entries => _entries.AsReadOnly();

        // For EF Core and deserialization
        private Queue() { }

        public Queue(
            Guid locationId,
            int maxSize,
            int lateClientCapTimeInMinutes,
            string createdBy)
        {
            if (locationId == Guid.Empty)
                throw new ArgumentException("Service provider ID is required", nameof(locationId));

            this.LocationId = locationId;
            QueueDate = DateTime.UtcNow.Date;
            IsActive = true;
            MaxSize = maxSize > 0 ? maxSize : 100;
            LateClientCapTimeInMinutes = lateClientCapTimeInMinutes >= 0 ? lateClientCapTimeInMinutes : 15;
            CreatedBy = createdBy;

            AddDomainEvent(new QueueCreatedEvent(Id, LocationId, QueueDate));
        }

        // Domain behavior methods
        public QueueEntry AddCustomerToQueue(
            Guid customerId,
            string customerName,
            Guid? staffMemberId = null,
            Guid? serviceTypeId = null,
            string? notes = null)
        {
            if (customerId == Guid.Empty)
                throw new ArgumentException("Customer ID is required", nameof(customerId));

            if (!IsActive)
                throw new InvalidOperationException("Cannot add customers to an inactive queue");

            if (GetActiveEntries().Count() >= MaxSize)
                throw new InvalidOperationException("Queue has reached its maximum size");

            var position = CalculateNextPosition();
            var queueEntry = new QueueEntry(
                Id,
                customerId,
                customerName,
                position,
                staffMemberId,
                serviceTypeId,
                notes);

            _entries.Add(queueEntry);
            
            // Mark the queue as modified so EF Core knows it has been updated
            MarkAsModified("System"); // TODO: Pass actual user context when available
            
            AddDomainEvent(new CustomerAddedToQueueEvent(Id, queueEntry.Id, customerId, position));
            
            return queueEntry;
        }

        public QueueEntry CallNextCustomer(Guid staffMemberId)
        {
            if (staffMemberId == Guid.Empty)
                throw new ArgumentException("Staff member ID is required", nameof(staffMemberId));

            var nextEntry = GetActiveEntries()
                .OrderBy(e => e.Position)
                .FirstOrDefault();

            if (nextEntry == null)
                throw new InvalidOperationException("No customers in queue");

            nextEntry.Call(staffMemberId);
            
            // Mark the queue as modified since it affects queue state
            MarkAsModified("System"); // TODO: Pass actual user context when available
            
            AddDomainEvent(new CustomerCalledFromQueueEvent(Id, nextEntry.Id, nextEntry.CustomerId, staffMemberId));
            
            return nextEntry;
        }

        public void UpdateSettings(int maxSize, int lateClientCapTimeInMinutes, string updatedBy)
        {
            if (maxSize <= 0)
                throw new ArgumentException("Maximum queue size must be positive", nameof(maxSize));

            if (lateClientCapTimeInMinutes < 0)
                throw new ArgumentException("Late client cap time cannot be negative", nameof(lateClientCapTimeInMinutes));

            MaxSize = maxSize;
            LateClientCapTimeInMinutes = lateClientCapTimeInMinutes;
            MarkAsModified(updatedBy);
            AddDomainEvent(new QueueSettingsUpdatedEvent(Id));
        }

        public void Activate(string updatedBy)
        {
            if (!IsActive)
            {
                IsActive = true;
                MarkAsModified(updatedBy);
                AddDomainEvent(new QueueActivatedEvent(Id));
            }
        }

        public void Deactivate(string updatedBy)
        {
            if (IsActive)
            {
                IsActive = false;
                MarkAsModified(updatedBy);
                AddDomainEvent(new QueueDeactivatedEvent(Id));
            }
        }

        public void RemoveLateCustomers()
        {
            var now = DateTime.UtcNow;
            var lateEntries = _entries.Where(e => 
                e.Status == QueueEntryStatus.Called && 
                e.CalledAt.HasValue &&
                now.Subtract(e.CalledAt.Value).TotalMinutes > LateClientCapTimeInMinutes).ToList();

            foreach (var entry in lateEntries)
            {
                entry.MarkAsNoShow();
                AddDomainEvent(new CustomerMarkedAsNoShowEvent(Id, entry.Id, entry.CustomerId));
            }

            if (lateEntries.Any())
            {
                // Mark the queue as modified since entries were changed
                MarkAsModified("System"); // TODO: Pass actual user context when available
                
                AddDomainEvent(new LateCustomersRemovedEvent(Id, lateEntries.Count));
            }
        }

        public int GetPosition(Guid queueEntryId)
        {
            var entry = _entries.FirstOrDefault(e => e.Id == queueEntryId);
            if (entry == null)
                throw new InvalidOperationException($"Queue entry with ID {queueEntryId} not found");

            if (entry.Status != QueueEntryStatus.Waiting)
                return 0; // Not in line anymore

            return GetActiveEntries().Count(e => e.Position < entry.Position) + 1;
        }

        public int CalculateEstimatedWaitTimeInMinutes(
            Guid queueEntryId,
            double averageServiceTimeInMinutes,
            int activeStaffCount)
        {
            var position = GetPosition(queueEntryId);
            if (position == 0)
                return 0; // Not waiting

            // Simple calculation: position / active staff * average time
            var effectiveStaffCount = Math.Max(1, activeStaffCount);
            return (int)Math.Ceiling(position / (double)effectiveStaffCount * averageServiceTimeInMinutes);
        }

        private int CalculateNextPosition()
        {
            if (!_entries.Any())
                return 1;

            return _entries.Max(e => e.Position) + 1;
        }

        private IEnumerable<QueueEntry> GetActiveEntries()
        {
            return _entries.Where(e => e.Status == QueueEntryStatus.Waiting || e.Status == QueueEntryStatus.Called);
        }
    }
}
