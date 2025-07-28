using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Grande.Fila.API.Domain.Subscriptions;

namespace Grande.Fila.API.Infrastructure.Data.Configurations
{
    public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
    {
        public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
        {
            // Table configuration
            builder.ToTable("SubscriptionPlans");
            builder.HasKey(s => s.Id);
            
            // Basic properties
            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.Description)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(s => s.Price)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(s => s.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(s => s.IsDefault)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(s => s.MaxLocations)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(s => s.MaxStaffPerLocation)
                .IsRequired()
                .HasDefaultValue(5);

            builder.Property(s => s.IncludesAnalytics)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(s => s.IncludesAdvancedReporting)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(s => s.IncludesCustomBranding)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(s => s.IncludesAdvertising)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(s => s.IncludesMultipleLocations)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(s => s.MaxQueueEntriesPerDay)
                .IsRequired()
                .HasDefaultValue(100);

            builder.Property(s => s.IsFeatured)
                .IsRequired()
                .HasDefaultValue(false);

            // Money value objects are configured in QueueHubDbContext.ConfigureValueObjects()

            // Indexes
            builder.HasIndex(s => s.Name)
                .IsUnique()
                .HasDatabaseName("IX_SubscriptionPlans_Name");

            builder.HasIndex(s => s.IsActive)
                .HasDatabaseName("IX_SubscriptionPlans_IsActive");

            builder.HasIndex(s => s.IsDefault)
                .HasDatabaseName("IX_SubscriptionPlans_IsDefault");

            builder.HasIndex(s => s.IsFeatured)
                .HasDatabaseName("IX_SubscriptionPlans_IsFeatured");

            builder.HasIndex(s => s.Price)
                .HasDatabaseName("IX_SubscriptionPlans_Price");
        }
    }
} 