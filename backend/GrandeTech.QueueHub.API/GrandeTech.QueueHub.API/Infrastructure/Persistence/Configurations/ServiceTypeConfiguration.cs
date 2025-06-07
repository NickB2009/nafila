using GrandeTech.QueueHub.API.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GrandeTech.QueueHub.API.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Entity Framework configuration for ServiceType entity
    /// </summary>
    public class ServiceTypeConfiguration : IEntityTypeConfiguration<ServiceType>
    {
        public void Configure(EntityTypeBuilder<ServiceType> builder)
        {
            // Table name
            builder.ToTable("ServiceTypes");
            
            // Key
            builder.HasKey(st => st.Id);
            
            // Properties
            builder.Property(st => st.Name)
                .IsRequired()
                .HasMaxLength(100);
                  builder.Property(st => st.Description)
                .HasMaxLength(500);
                
            builder.Property(st => st.EstimatedDurationMinutes)
                .IsRequired();
                
            builder.Property(st => st.ActualAverageDurationMinutes)
                .IsRequired();
                
            builder.Property(st => st.TimesProvided)
                .IsRequired()
                .HasDefaultValue(0);
                
            builder.Property(st => st.ImageUrl)
                .HasMaxLength(255);
                
            builder.Property(st => st.ServicesProviderId)
                .IsRequired();
                
            // Money Value Object (nullable)
            builder.OwnsOne(st => st.Price, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("PriceAmount")
                    .HasColumnType("decimal(18,2)");
                    
                money.Property(m => m.Currency)
                    .HasColumnName("PriceCurrency")
                    .HasMaxLength(3);
            });
            
            builder.Property(st => st.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
                
            // Audit fields
            builder.Property(st => st.CreatedBy)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(st => st.LastModifiedBy)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(st => st.CreatedAt)
                .IsRequired();
                
            builder.Property(st => st.LastModifiedAt)
                .IsRequired();
                
            // Relationships
            builder.HasOne<Domain.ServicesProviders.ServicesProvider>()
                .WithMany()
                .HasForeignKey(st => st.ServicesProviderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
