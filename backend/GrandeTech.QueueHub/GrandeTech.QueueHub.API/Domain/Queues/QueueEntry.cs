using System;
using Grande.Fila.API.Domain.Common;

namespace Grande.Fila.API.Domain.Queues
{
    public enum QueueEntryStatus
    {
        Waiting,
        Called,
        CheckedIn,
        Completed,
        Cancelled,
        NoShow
    }    /// <summary>
    /// Represents a single entry in a queue
    /// </summary>
    public class QueueEntry : BaseEntity
    {
        public Guid QueueId { get; private set; }
        public Guid CustomerId { get; private set; }
        public string CustomerName { get; private set; } = string.Empty;
        public int Position { get; private set; }
        public QueueEntryStatus Status { get; private set; }
        public Guid? StaffMemberId { get; private set; }
        public Guid? ServiceTypeId { get; private set; }
        public string? Notes { get; private set; }
        public DateTime EnteredAt { get; private set; }
        public DateTime? CalledAt { get; private set; }
        public DateTime? CheckedInAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }
        public DateTime? CancelledAt { get; private set; }        public int? ServiceDurationMinutes { get; private set; }
        public int? EstimatedDurationMinutes { get; private set; }
        public DateTime? EstimatedStartTime { get; private set; }
        public DateTime? ActualStartTime { get; private set; }
        public DateTime? CompletionTime { get; private set; }
        public string? CustomerNotes { get; private set; }
        public string? StaffNotes { get; private set; }
        public string? TokenNumber { get; private set; }
        public bool NotificationSent { get; private set; }
        public DateTime? NotificationSentAt { get; private set; }
        public string? NotificationChannel { get; private set; }

        // For EF Core
        private QueueEntry() { }

        public QueueEntry(
            Guid queueId,
            Guid customerId,
            string customerName,
            int position,
            Guid? staffMemberId = null,
            Guid? serviceTypeId = null,
            string? notes = null)
        {
            if (queueId == Guid.Empty)
                throw new ArgumentException("Queue ID is required", nameof(queueId));
            if (customerId == Guid.Empty)
                throw new ArgumentException("Customer ID is required", nameof(customerId));
            if (string.IsNullOrWhiteSpace(customerName))
                throw new ArgumentException("Customer name is required", nameof(customerName));
            if (position <= 0)
                throw new ArgumentException("Position must be positive", nameof(position));

            QueueId = queueId;
            CustomerId = customerId;
            CustomerName = customerName;
            Position = position;
            Status = QueueEntryStatus.Waiting;
            StaffMemberId = staffMemberId;
            ServiceTypeId = serviceTypeId;
            Notes = notes;
            EnteredAt = DateTime.UtcNow;
            NotificationSent = false;
        }

        // Domain behavior methods
        public void Call(Guid staffMemberId)
        {
            if (Status != QueueEntryStatus.Waiting)
                throw new InvalidOperationException($"Cannot call a customer with status {Status}");

            Status = QueueEntryStatus.Called;
            StaffMemberId = staffMemberId;
            CalledAt = DateTime.UtcNow;
        }

        public void CheckIn()
        {
            if (Status != QueueEntryStatus.Called)
                throw new InvalidOperationException($"Cannot check in a customer with status {Status}");

            Status = QueueEntryStatus.CheckedIn;
            CheckedInAt = DateTime.UtcNow;
        }

        public void Complete(int serviceDurationMinutes)
        {
            if (Status != QueueEntryStatus.CheckedIn)
                throw new InvalidOperationException($"Cannot complete service for a customer with status {Status}");

            if (serviceDurationMinutes <= 0)
                throw new ArgumentException("Service duration must be positive", nameof(serviceDurationMinutes));

            Status = QueueEntryStatus.Completed;
            CompletedAt = DateTime.UtcNow;
            ServiceDurationMinutes = serviceDurationMinutes;
        }

        public void Cancel()
        {
            if (Status != QueueEntryStatus.Waiting)
                throw new InvalidOperationException($"Cannot cancel a customer with status {Status}. Only waiting customers can be cancelled.");

            Status = QueueEntryStatus.Cancelled;
            CancelledAt = DateTime.UtcNow;
        }

        public void MarkAsNoShow()
        {
            if (Status != QueueEntryStatus.Called)
                throw new InvalidOperationException($"Cannot mark as no-show a customer with status {Status}");

            Status = QueueEntryStatus.NoShow;
        }

        public void UpdateServiceType(Guid serviceTypeId)
        {
            ServiceTypeId = serviceTypeId;
        }

        public void UpdateNotes(string notes)
        {
            Notes = notes;
        }

        public void MarkNotificationSent(string channel)
        {
            if (string.IsNullOrWhiteSpace(channel))
                throw new ArgumentException("Notification channel cannot be empty", nameof(channel));

            NotificationSent = true;
            NotificationSentAt = DateTime.UtcNow;
            NotificationChannel = channel;
        }

        public bool IsActive()
        {
            return Status == QueueEntryStatus.Waiting || Status == QueueEntryStatus.Called;
        }

        public int CalculateWaitTimeInMinutes()
        {
            if (CalledAt.HasValue)
            {
                return (int)Math.Ceiling((CalledAt.Value - EnteredAt).TotalMinutes);
            }

            return (int)Math.Ceiling((DateTime.UtcNow - EnteredAt).TotalMinutes);
        }

        public int? CalculateServiceTimeInMinutes()
        {
            if (!CompletedAt.HasValue || !CheckedInAt.HasValue)
                return null;

            return (int)Math.Ceiling((CompletedAt.Value - CheckedInAt.Value).TotalMinutes);
        }

        public int? CalculateTotalTimeInMinutes()
        {
            if (!CompletedAt.HasValue)
                return null;

            return (int)Math.Ceiling((CompletedAt.Value - EnteredAt).TotalMinutes);
        }

        // Add a method to set status for testing
        public void SetStatusForTest(QueueEntryStatus status)
        {
            Status = status;
        }
    }
}
