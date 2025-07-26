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

            // Configure only known properties
            builder.Property(l => l.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(l => l.Description)
                .HasMaxLength(1000);

            builder.Property(l => l.OrganizationId)
                .IsRequired();

            // Ignore complex properties for now
            builder.Ignore("IsActive");
            builder.Ignore("Timezone");
            builder.Ignore("Address");
            builder.Ignore("ContactInfo");
            builder.Ignore("BusinessHours");
            builder.Ignore("QueueConfig");

            // Basic indexes
            builder.HasIndex(l => l.OrganizationId)
                .HasDatabaseName("IX_Locations_OrganizationId");

            builder.HasIndex(l => l.Name)
                .HasDatabaseName("IX_Locations_Name");
        }
    }
} 