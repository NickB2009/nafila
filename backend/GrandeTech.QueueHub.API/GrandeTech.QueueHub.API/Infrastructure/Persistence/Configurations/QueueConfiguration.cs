using GrandeTech.QueueHub.API.Domain.Queues;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GrandeTech.QueueHub.API.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Entity Framework configuration for Queue entity
    /// </summary>
    public class QueueConfiguration : IEntityTypeConfiguration<Queue>
    {
        public void Configure(EntityTypeBuilder<Queue> builder)
        {
            // Table name
            builder.ToTable("Queues");
            
            // Key
            builder.HasKey(q => q.Id);
            
            // Properties
            builder.Property(q => q.ServicesProviderId)
                .IsRequired();
                
            builder.Property(q => q.QueueDate)
                .IsRequired();
                
            builder.Property(q => q.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
                
            builder.Property(q => q.MaxSize)
                .IsRequired()
                .HasDefaultValue(50);
                
            builder.Property(q => q.LateClientCapTimeInMinutes)
                .IsRequired()
                .HasDefaultValue(10);
                
            // Audit fields
            builder.Property(q => q.CreatedBy)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(q => q.LastModifiedBy)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(q => q.CreatedAt)
                .IsRequired();
                
            builder.Property(q => q.LastModifiedAt)
                .IsRequired();
                
            // Relationships
            builder.HasOne<Domain.ServicesProviders.ServicesProvider>()
                .WithMany()
                .HasForeignKey(q => q.ServicesProviderId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.HasMany(q => q.Entries)
                .WithOne()
                .HasForeignKey("QueueId")
                .OnDelete(DeleteBehavior.Cascade);
                
            // Indexes
            builder.HasIndex(q => new { q.ServicesProviderId, q.QueueDate })
                .IsUnique();
        }
    }
}
