using GrandeTech.QueueHub.API.Domain.Promotions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GrandeTech.QueueHub.API.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Entity Framework configuration for Coupon entity
    /// </summary>
    public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
    {
        public void Configure(EntityTypeBuilder<Coupon> builder)
        {
            // Table name
            builder.ToTable("Coupons");
            
            // Key
            builder.HasKey(c => c.Id);
            
            // Properties
            builder.Property(c => c.Code)
                .IsRequired()
                .HasMaxLength(50);
                
            // Index for faster lookup by code
            builder.HasIndex(c => c.Code)
                .IsUnique();
                  builder.Property(c => c.Description)
                .HasMaxLength(500);
                
            builder.Property(c => c.DiscountPercentage)
                .IsRequired()
                .HasColumnType("decimal(5,2)");
                
            // Money Value Object (nullable)
            builder.OwnsOne(c => c.FixedDiscountAmount, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("FixedDiscountAmount")
                    .HasColumnType("decimal(18,2)");
                    
                money.Property(m => m.Currency)
                    .HasColumnName("FixedDiscountCurrency")
                    .HasMaxLength(3);
            });
                
            builder.Property(c => c.StartDate)
                .IsRequired();
                
            builder.Property(c => c.EndDate)
                .IsRequired();
                
            builder.Property(c => c.MaxUsageCount)
                .IsRequired();
                
            builder.Property(c => c.CurrentUsageCount)
                .IsRequired()
                .HasDefaultValue(0);
                
            builder.Property(c => c.RequiresLogin)
                .IsRequired()
                .HasDefaultValue(false);
                
            builder.Property(c => c.ServicesProviderId)
                .IsRequired();
                
            builder.Property(c => c.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
                
            // Audit fields
            builder.Property(c => c.CreatedBy)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(c => c.LastModifiedBy)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(c => c.CreatedAt)
                .IsRequired();
                
            builder.Property(c => c.LastModifiedAt)
                .IsRequired();
                
            // Relationships
            builder.HasOne<Domain.ServicesProviders.ServicesProvider>()
                .WithMany()
                .HasForeignKey(c => c.ServicesProviderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
