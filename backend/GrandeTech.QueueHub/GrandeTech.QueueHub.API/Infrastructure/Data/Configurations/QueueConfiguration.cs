using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Grande.Fila.API.Domain.Queues;

namespace Grande.Fila.API.Infrastructure.Data.Configurations
{
    public class QueueConfiguration : IEntityTypeConfiguration<Queue>
    {
        public void Configure(EntityTypeBuilder<Queue> builder)
        {
            // Table configuration
            builder.ToTable("Queues");
            builder.HasKey(q => q.Id);

            // Properties
            builder.Property(q => q.LocationId)
                .IsRequired();

            builder.Property(q => q.QueueDate)
                .IsRequired()
                .HasColumnType("date");

            builder.Property(q => q.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(q => q.MaxSize)
                .IsRequired()
                .HasDefaultValue(100);

            builder.Property(q => q.LateClientCapTimeInMinutes)
                .IsRequired()
                .HasDefaultValue(15);

            // Configure concurrency token
            builder.Property(q => q.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            // Configure the entries collection - this will be a separate table
            builder.HasMany(q => q.Entries)
                .WithOne()
                .HasForeignKey("QueueId")
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(q => q.LocationId)
                .HasDatabaseName("IX_Queues_LocationId");

            builder.HasIndex(q => q.QueueDate)
                .HasDatabaseName("IX_Queues_QueueDate");

            builder.HasIndex(q => new { q.LocationId, q.QueueDate })
                .IsUnique()
                .HasDatabaseName("IX_Queues_LocationId_QueueDate");

            builder.HasIndex(q => q.IsActive)
                .HasDatabaseName("IX_Queues_IsActive");
        }
    }
} 