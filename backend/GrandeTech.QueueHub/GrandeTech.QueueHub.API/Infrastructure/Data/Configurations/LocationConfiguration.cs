using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Grande.Fila.API.Domain.Locations;

namespace Grande.Fila.API.Infrastructure.Data.Configurations
{
    public class LocationConfiguration : IEntityTypeConfiguration<Location>
    {
        public void Configure(EntityTypeBuilder<Location> builder)
        {
            // Table configuration
            builder.ToTable("Locations");
            builder.HasKey(l => l.Id);
            
            // Basic properties
            builder.Property(l => l.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(l => l.Description)
                .HasMaxLength(1000);

            builder.Property(l => l.OrganizationId)
                .IsRequired();

            builder.Property(l => l.IsQueueEnabled)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(l => l.MaxQueueSize)
                .IsRequired()
                .HasDefaultValue(100);

            builder.Property(l => l.LateClientCapTimeInMinutes)
                .IsRequired()
                .HasDefaultValue(15);

            builder.Property(l => l.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(l => l.AverageServiceTimeInMinutes)
                .IsRequired()
                .HasDefaultValue(30.0);

            builder.Property(l => l.LastAverageTimeReset)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Value objects are now configured in QueueHubDbContext.ConfigureValueObjects()
            // No need to ignore them anymore

            // Indexes
            builder.HasIndex(l => l.Name)
                .HasDatabaseName("IX_Locations_Name");

            builder.HasIndex(l => l.OrganizationId)
                .HasDatabaseName("IX_Locations_OrganizationId");

            builder.HasIndex(l => l.IsActive)
                .HasDatabaseName("IX_Locations_IsActive");

            builder.HasIndex(l => l.IsQueueEnabled)
                .HasDatabaseName("IX_Locations_IsQueueEnabled");

            // Index on Slug for faster lookups
            builder.HasIndex("Slug")
                .IsUnique()
                .HasDatabaseName("IX_Locations_Slug");

            // Index on ContactEmail for searches
            builder.HasIndex("ContactEmail")
                .HasDatabaseName("IX_Locations_ContactEmail");
        }
    }
} 