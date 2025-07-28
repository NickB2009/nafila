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

            // Value objects are now configured in QueueHubDbContext.ConfigureValueObjects()
            // No need to ignore them anymore

            // Indexes
            builder.HasIndex(o => o.Name)
                .HasDatabaseName("IX_Organizations_Name");

            builder.HasIndex(o => o.SubscriptionPlanId)
                .HasDatabaseName("IX_Organizations_SubscriptionPlanId");

            builder.HasIndex(o => o.IsActive)
                .HasDatabaseName("IX_Organizations_IsActive");

            // Index on Slug for faster lookups
            builder.HasIndex("Slug")
                .IsUnique()
                .HasDatabaseName("IX_Organizations_Slug");

            // Index on ContactEmail for searches
            builder.HasIndex("ContactEmail")
                .HasDatabaseName("IX_Organizations_ContactEmail");
        }
    }
} 