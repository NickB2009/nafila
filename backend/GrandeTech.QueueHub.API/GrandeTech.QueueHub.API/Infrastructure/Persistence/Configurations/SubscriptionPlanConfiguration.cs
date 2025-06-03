using GrandeTech.QueueHub.API.Domain.Subscriptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GrandeTech.QueueHub.API.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Entity Framework configuration for SubscriptionPlan entity
    /// </summary>
    public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
    {
        public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
        {
            // Table name
            builder.ToTable("SubscriptionPlans");
            
            // Key
            builder.HasKey(sp => sp.Id);
            
            // Properties
            builder.Property(sp => sp.Name)
                .IsRequired()
                .HasMaxLength(100);
                  builder.Property(sp => sp.Description)
                .HasMaxLength(500);
                
            builder.Property(sp => sp.Price)
                .IsRequired()
                .HasColumnType("decimal(18,2)");
                
            builder.Property(sp => sp.IsDefault)
                .IsRequired()
                .HasDefaultValue(false);
                
            builder.Property(sp => sp.MaxServiceProviders)
                .IsRequired();
                
            builder.Property(sp => sp.MaxStaffPerServiceProvider)
                .IsRequired();
                
            builder.Property(sp => sp.MaxQueueEntriesPerDay)
                .IsRequired();
                
            builder.Property(sp => sp.IncludesAnalytics)
                .IsRequired()
                .HasDefaultValue(false);
                
            builder.Property(sp => sp.IncludesAdvancedReporting)
                .IsRequired()
                .HasDefaultValue(false);
                
            builder.Property(sp => sp.IncludesCustomBranding)
                .IsRequired()
                .HasDefaultValue(false);
                
            builder.Property(sp => sp.IncludesAdvertising)
                .IsRequired()
                .HasDefaultValue(false);
                
            builder.Property(sp => sp.IncludesMultipleLocations)
                .IsRequired()
                .HasDefaultValue(false);
                
            builder.Property(sp => sp.IsFeatured)
                .IsRequired()
                .HasDefaultValue(false);
                
            // Money Value Objects
            builder.OwnsOne(sp => sp.MonthlyPrice, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("MonthlyPriceAmount")
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");
                    
                money.Property(m => m.Currency)
                    .HasColumnName("MonthlyPriceCurrency")
                    .IsRequired()
                    .HasMaxLength(3);
            });
            
            builder.OwnsOne(sp => sp.YearlyPrice, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("YearlyPriceAmount")
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");
                    
                money.Property(m => m.Currency)
                    .HasColumnName("YearlyPriceCurrency")
                    .IsRequired()
                    .HasMaxLength(3);
            });
            
            builder.Property(sp => sp.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
                
            // Audit fields
            builder.Property(sp => sp.CreatedBy)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(sp => sp.LastModifiedBy)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(sp => sp.CreatedAt)
                .IsRequired();
                
            builder.Property(sp => sp.LastModifiedAt)
                .IsRequired();
        }
    }
}
