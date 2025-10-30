using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Grande.Fila.API.Domain.Promotions;

namespace Grande.Fila.API.Infrastructure.Data.Configurations
{
    public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
    {
        public void Configure(EntityTypeBuilder<Coupon> builder)
        {
            builder.ToTable("Coupons");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Code)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(c => c.Description)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(c => c.LocationId)
                .IsRequired();

            builder.Property(c => c.DiscountPercentage)
                .IsRequired();

            builder.Property(c => c.StartDate)
                .IsRequired();

            builder.Property(c => c.EndDate)
                .IsRequired();

            builder.Property(c => c.MaxUsageCount)
                .IsRequired();

            builder.Property(c => c.CurrentUsageCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(c => c.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(c => c.RequiresLogin)
                .IsRequired()
                .HasDefaultValue(false);

            // Indexes
            builder.HasIndex(c => c.Code)
                .IsUnique()
                .HasDatabaseName("IX_Coupons_Code");

            builder.HasIndex(c => c.LocationId)
                .HasDatabaseName("IX_Coupons_LocationId");

            builder.HasIndex(c => c.IsActive)
                .HasDatabaseName("IX_Coupons_IsActive");
        }
    }
}

