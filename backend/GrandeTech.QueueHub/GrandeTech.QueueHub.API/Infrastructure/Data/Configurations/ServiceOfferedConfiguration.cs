using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Grande.Fila.API.Domain.ServicesOffered;

namespace Grande.Fila.API.Infrastructure.Data.Configurations
{
    public class ServiceOfferedConfiguration : IEntityTypeConfiguration<ServiceOffered>
    {
        public void Configure(EntityTypeBuilder<ServiceOffered> builder)
        {
            // Table configuration
            builder.ToTable("ServicesOffered");
            builder.HasKey(s => s.Id);
            
            // Basic properties
            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(s => s.Description)
                .HasMaxLength(1000);

            builder.Property(s => s.LocationId)
                .IsRequired();

            builder.Property(s => s.EstimatedDurationMinutes)
                .IsRequired()
                .HasDefaultValue(30);

            builder.Property(s => s.ImageUrl)
                .HasMaxLength(500);

            builder.Property(s => s.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(s => s.TimesProvided)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(s => s.ActualAverageDurationMinutes)
                .IsRequired()
                .HasDefaultValue(30.0);

            // Money value object is configured in QueueHubDbContext.ConfigureValueObjects()

            // Indexes
            builder.HasIndex(s => s.Name)
                .HasDatabaseName("IX_ServicesOffered_Name");

            builder.HasIndex(s => s.LocationId)
                .HasDatabaseName("IX_ServicesOffered_LocationId");

            builder.HasIndex(s => s.IsActive)
                .HasDatabaseName("IX_ServicesOffered_IsActive");

            builder.HasIndex(s => s.EstimatedDurationMinutes)
                .HasDatabaseName("IX_ServicesOffered_EstimatedDuration");

            // Composite index for location + active services
            builder.HasIndex(s => new { s.LocationId, s.IsActive })
                .HasDatabaseName("IX_ServicesOffered_Location_Active");
        }
    }
} 