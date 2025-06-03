using System;
using System.Collections.Generic;
using System.Linq;

namespace GrandeTech.QueueHub.API.Domain.Common
{
    /// <summary>
    /// Base class for all domain entities providing common attributes and behavior
    /// </summary>
    public abstract class BaseEntity
    {
        private readonly List<IDomainEvent> _domainEvents = new();        public Guid Id { get; protected set; }
        public DateTime CreatedAt { get; set; } // Allow EF Core to set this
        public string? CreatedBy { get; protected set; }
        public DateTime? LastModifiedAt { get; set; } // Allow EF Core to set this
        public string? LastModifiedBy { get; protected set; }
        public bool IsDeleted { get; protected set; }
        public DateTime? DeletedAt { get; protected set; }
        public string? DeletedBy { get; protected set; }

        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        protected BaseEntity()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
        }

        public void MarkAsModified(string modifiedBy)
        {
            LastModifiedAt = DateTime.UtcNow;
            LastModifiedBy = modifiedBy;
        }

        public void MarkAsDeleted(string deletedBy)
        {
            if (!IsDeleted)
            {
                IsDeleted = true;
                DeletedAt = DateTime.UtcNow;
                DeletedBy = deletedBy;
            }
        }

        public void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void RemoveDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Remove(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}
