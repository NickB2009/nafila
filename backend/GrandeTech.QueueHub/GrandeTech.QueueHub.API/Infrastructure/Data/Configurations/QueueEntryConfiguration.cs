using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Grande.Fila.API.Domain.Queues;

namespace Grande.Fila.API.Infrastructure.Data.Configurations
{
    public class QueueEntryConfiguration : IEntityTypeConfiguration<QueueEntry>
    {
        public void Configure(EntityTypeBuilder<QueueEntry> builder)
        {
            // Table configuration
            builder.ToTable("QueueEntries");
            builder.HasKey(qe => qe.Id);

            // Properties
            builder.Property(qe => qe.QueueId)
                .IsRequired();

            builder.Property(qe => qe.CustomerId)
                .IsRequired();

            builder.Property(qe => qe.CustomerName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(qe => qe.Position)
                .IsRequired();

            builder.Property(qe => qe.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(qe => qe.StaffMemberId);

            builder.Property(qe => qe.ServiceTypeId);

            builder.Property(qe => qe.Notes)
                .HasMaxLength(1000);

            builder.Property(qe => qe.EnteredAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            builder.Property(qe => qe.CalledAt);
            builder.Property(qe => qe.CheckedInAt);
            builder.Property(qe => qe.CompletedAt);
            builder.Property(qe => qe.CancelledAt);

            builder.Property(qe => qe.ServiceDurationMinutes);
            builder.Property(qe => qe.EstimatedDurationMinutes);
            builder.Property(qe => qe.EstimatedStartTime);
            builder.Property(qe => qe.ActualStartTime);
            builder.Property(qe => qe.CompletionTime);

            builder.Property(qe => qe.CustomerNotes)
                .HasMaxLength(500);

            builder.Property(qe => qe.StaffNotes)
                .HasMaxLength(500);

            builder.Property(qe => qe.TokenNumber)
                .HasMaxLength(20);

            builder.Property(qe => qe.NotificationSent)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(qe => qe.NotificationSentAt);

            builder.Property(qe => qe.NotificationChannel)
                .HasMaxLength(50);

            // Indexes for performance
            builder.HasIndex(qe => qe.QueueId)
                .HasDatabaseName("IX_QueueEntries_QueueId");

            builder.HasIndex(qe => qe.CustomerId)
                .HasDatabaseName("IX_QueueEntries_CustomerId");

            builder.HasIndex(qe => qe.Status)
                .HasDatabaseName("IX_QueueEntries_Status");

            builder.HasIndex(qe => new { qe.QueueId, qe.Position })
                .HasDatabaseName("IX_QueueEntries_QueueId_Position");

            builder.HasIndex(qe => new { qe.QueueId, qe.Status })
                .HasDatabaseName("IX_QueueEntries_QueueId_Status");

            builder.HasIndex(qe => qe.EnteredAt)
                .HasDatabaseName("IX_QueueEntries_EnteredAt");

            builder.HasIndex(qe => qe.StaffMemberId)
                .HasDatabaseName("IX_QueueEntries_StaffMemberId");

            builder.HasIndex(qe => qe.ServiceTypeId)
                .HasDatabaseName("IX_QueueEntries_ServiceTypeId");

            // RowVersion removed during simplification
            // builder.Property(qe => qe.RowVersion)
            //     .IsConcurrencyToken();
        }
    }
} 