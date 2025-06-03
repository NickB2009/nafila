using System;

namespace GrandeTech.QueueHub.API.Domain.Common
{
    /// <summary>
    /// Simplified version of BaseEntity for building purposes
    /// </summary>
    public abstract class SimplifiedBaseEntity
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; } 
        public DateTime? LastModifiedAt { get; set; }
        public string? LastModifiedBy { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

        protected SimplifiedBaseEntity()
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
    }
}
