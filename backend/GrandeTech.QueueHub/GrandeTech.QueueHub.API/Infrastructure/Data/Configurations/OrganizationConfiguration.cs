using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Grande.Fila.API.Domain.Organizations;

namespace Grande.Fila.API.Infrastructure.Data.Configurations
{
    public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
    {
        public void Configure(EntityTypeBuilder<Organization> builder)
        {
            // Table configuration
            builder.ToTable("Organizations");
            builder.HasKey(o => o.Id);
            
            // Basic properties
            builder.Property(o => o.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(o => o.Description)
                .HasMaxLength(1000);

            builder.Property(o => o.WebsiteUrl)
                .HasMaxLength(500);

            builder.Property(o => o.SubscriptionPlanId)
                .IsRequired();

            builder.Property(o => o.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(o => o.SharesDataForAnalytics)
                .IsRequired()
                .HasDefaultValue(false);

            // Ignore complex value objects for now - will add them back later
            builder.Ignore(o => o.Slug);
            builder.Ignore(o => o.ContactEmail);
            builder.Ignore(o => o.ContactPhone);
            builder.Ignore(o => o.BrandingConfig);
            builder.Ignore(o => o.LocationIds);

            // Basic indexes on simple properties
            builder.HasIndex(o => o.Name)
                .HasDatabaseName("IX_Organizations_Name");

            builder.HasIndex(o => o.SubscriptionPlanId)
                .HasDatabaseName("IX_Organizations_SubscriptionPlanId");

            builder.HasIndex(o => o.IsActive)
                .HasDatabaseName("IX_Organizations_IsActive");
        }
    }
} 