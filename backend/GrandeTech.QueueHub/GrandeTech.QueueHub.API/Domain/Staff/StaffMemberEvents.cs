using System;
using Grande.Fila.API.Domain.Common;

namespace Grande.Fila.API.Domain.Staff
{
    public class StaffMemberCreatedEvent : DomainEvent
    {
        public Guid StaffMemberId { get; }
        public string StaffMemberName { get; }
        public Guid LocationId { get; }        public StaffMemberCreatedEvent(Guid staffMemberId, string staffMemberName, Guid locationId)
        {
            StaffMemberId = staffMemberId;
            StaffMemberName = staffMemberName;
            LocationId = locationId;
        }
    }

    public class StaffMemberUpdatedEvent : DomainEvent
    {
        public Guid StaffMemberId { get; }

        public StaffMemberUpdatedEvent(Guid staffMemberId)
        {
            StaffMemberId = staffMemberId;
        }
    }

    public class StaffMemberActivatedEvent : DomainEvent
    {
        public Guid StaffMemberId { get; }

        public StaffMemberActivatedEvent(Guid staffMemberId)
        {
            StaffMemberId = staffMemberId;
        }
    }

    public class StaffMemberDeactivatedEvent : DomainEvent
    {
        public Guid StaffMemberId { get; }

        public StaffMemberDeactivatedEvent(Guid staffMemberId)
        {
            StaffMemberId = staffMemberId;
        }
    }

    public class StaffMemberStatusChangedEvent : DomainEvent
    {
        public Guid StaffMemberId { get; }
        public string Status { get; }

        public StaffMemberStatusChangedEvent(Guid staffMemberId, string status)
        {
            StaffMemberId = staffMemberId;
            Status = status;
        }
    }

    public class StaffMemberStartedBreakEvent : DomainEvent
    {
        public Guid StaffMemberId { get; }
        public Guid BreakId { get; }
        public TimeSpan Duration { get; }
        public string? Reason { get; }

        public StaffMemberStartedBreakEvent(Guid staffMemberId, Guid breakId, TimeSpan duration, string? reason)
        {
            StaffMemberId = staffMemberId;
            BreakId = breakId;
            Duration = duration;
            Reason = reason;
        }
    }

    public class StaffMemberEndedBreakEvent : DomainEvent
    {
        public Guid StaffMemberId { get; }
        public Guid BreakId { get; }

        public StaffMemberEndedBreakEvent(Guid staffMemberId, Guid breakId)
        {
            StaffMemberId = staffMemberId;
            BreakId = breakId;
        }
    }

    public class StaffMemberSpecialtyAddedEvent : DomainEvent
    {
        public Guid StaffMemberId { get; }
        public Guid ServiceTypeId { get; }

        public StaffMemberSpecialtyAddedEvent(Guid staffMemberId, Guid serviceTypeId)
        {
            StaffMemberId = staffMemberId;
            ServiceTypeId = serviceTypeId;
        }
    }

    public class StaffMemberSpecialtyRemovedEvent : DomainEvent
    {
        public Guid StaffMemberId { get; }
        public Guid ServiceTypeId { get; }

        public StaffMemberSpecialtyRemovedEvent(Guid staffMemberId, Guid serviceTypeId)
        {
            StaffMemberId = staffMemberId;
            ServiceTypeId = serviceTypeId;
        }
    }

    public class StaffMemberConnectedToUserEvent : DomainEvent
    {
        public Guid StaffMemberId { get; }
        public string UserId { get; }

        public StaffMemberConnectedToUserEvent(Guid staffMemberId, string userId)
        {
            StaffMemberId = staffMemberId;
            UserId = userId;
        }
    }

    public class StaffMemberServiceStatsUpdatedEvent : DomainEvent
    {
        public Guid StaffMemberId { get; }
        public double AverageServiceTimeInMinutes { get; }
        public int CompletedServicesCount { get; }

        public StaffMemberServiceStatsUpdatedEvent(
            Guid staffMemberId,
            double averageServiceTimeInMinutes,
            int completedServicesCount)
        {
            StaffMemberId = staffMemberId;
            AverageServiceTimeInMinutes = averageServiceTimeInMinutes;
            CompletedServicesCount = completedServicesCount;
        }
    }
}
