using GrandeTech.QueueHub.API.Domain.Queues;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GrandeTech.QueueHub.API.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Entity Framework configuration for QueueEntry entity
    /// </summary>
    public class QueueEntryConfiguration : IEntityTypeConfiguration<QueueEntry>
    {
        public void Configure(EntityTypeBuilder<QueueEntry> builder)
        {
            // Table name
            builder.ToTable("QueueEntries");
            
            // Key
            builder.HasKey(qe => qe.Id);
            
            // Properties
            builder.Property(qe => qe.CustomerId)
                .IsRequired();
                
            builder.Property<Guid>("QueueId")
                .IsRequired();
                
            builder.Property(qe => qe.ServiceTypeId)
                .IsRequired();
                
            builder.Property(qe => qe.StaffMemberId);
            
            builder.Property(qe => qe.Status)
                .IsRequired()
                .HasConversion<string>();
                  // Optional estimated durations
            builder.Property(qe => qe.EstimatedDurationMinutes);
                
            builder.Property(qe => qe.EstimatedStartTime);
                
            builder.Property(qe => qe.ActualStartTime);
            
            builder.Property(qe => qe.CompletionTime);
            
            builder.Property(qe => qe.CustomerNotes)
                .HasMaxLength(500);
                
            builder.Property(qe => qe.StaffNotes)
                .HasMaxLength(500);
                
            builder.Property(qe => qe.Position)
                .IsRequired();
                
            builder.Property(qe => qe.TokenNumber)
                .HasMaxLength(10);
                
            builder.Property(qe => qe.CreatedBy)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(qe => qe.LastModifiedBy)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(qe => qe.CreatedAt)
                .IsRequired();
                
            builder.Property(qe => qe.LastModifiedAt)
                .IsRequired();
                
            // Relationships
            // The relationship to Queue is defined in the Queue configuration
            
            builder.HasOne<Domain.Customers.Customer>()
                .WithMany()
                .HasForeignKey(qe => qe.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.HasOne<Domain.Services.ServiceType>()
                .WithMany()
                .HasForeignKey(qe => qe.ServiceTypeId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.HasOne<Domain.Staff.StaffMember>()
                .WithMany()
                .HasForeignKey(qe => qe.StaffMemberId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Indexes
            builder.HasIndex("QueueId", "Position");
            builder.HasIndex("QueueId", "TokenNumber").IsUnique();
            builder.HasIndex("QueueId", "Status");
        }
    }
}
