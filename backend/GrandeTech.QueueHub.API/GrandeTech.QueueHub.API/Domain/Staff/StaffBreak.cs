using System;
using GrandeTech.QueueHub.API.Domain.Common;

namespace GrandeTech.QueueHub.API.Domain.Staff
{
    /// <summary>
    /// Represents a break taken by a staff member
    /// </summary>
    public class StaffBreak : BaseEntity
    {
        public Guid StaffMemberId { get; private set; }
        public DateTime StartedAt { get; private set; }
        public DateTime? EndedAt { get; private set; }
        public TimeSpan Duration { get; private set; }
        public string? Reason { get; private set; }
        public DateTime ScheduledEndTime { get; private set; }
        public DateTime EndTime { get; private set; } // Simplified end time property

        // For EF Core
        private StaffBreak() { }

        public StaffBreak(
            Guid staffMemberId,
            TimeSpan duration,
            string? reason = null)
        {
            if (staffMemberId == Guid.Empty)
                throw new ArgumentException("Staff member ID is required", nameof(staffMemberId));

            if (duration <= TimeSpan.Zero)
                throw new ArgumentException("Duration must be positive", nameof(duration));            StaffMemberId = staffMemberId;
            StartedAt = DateTime.UtcNow;
            Duration = duration;
            Reason = reason;
            ScheduledEndTime = StartedAt.Add(duration);
            EndTime = ScheduledEndTime;
        }

        // Domain behavior methods
        public void End()
        {
            if (EndedAt.HasValue)
                throw new InvalidOperationException("Break is already ended");

            EndedAt = DateTime.UtcNow;
        }

        public bool IsActive()
        {
            return !EndedAt.HasValue;
        }

        public bool IsOverdue()
        {
            return IsActive() && DateTime.UtcNow > ScheduledEndTime;
        }

        public TimeSpan GetActualDuration()
        {
            if (!EndedAt.HasValue)
                return DateTime.UtcNow - StartedAt;

            return EndedAt.Value - StartedAt;
        }

        public TimeSpan GetRemainingTime()
        {
            if (EndedAt.HasValue)
                return TimeSpan.Zero;

            var remaining = ScheduledEndTime - DateTime.UtcNow;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }
    }
}
