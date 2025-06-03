using GrandeTech.QueueHub.API.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GrandeTech.QueueHub.API.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Entity Framework configuration for Customer entity
    /// </summary>
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            // Table name
            builder.ToTable("Customers");
            
            // Key
            builder.HasKey(c => c.Id);
            
            // Properties
            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(c => c.IsAnonymous)
                .IsRequired()
                .HasDefaultValue(false);
                
            builder.Property(c => c.NotificationsEnabled)
                .IsRequired()
                .HasDefaultValue(true);
                
            builder.Property(c => c.PreferredNotificationChannel)
                .HasMaxLength(50);
                
            builder.Property(c => c.UserId)
                .HasMaxLength(100);
                
            // Value Objects
            builder.OwnsOne(c => c.PhoneNumber, phone =>
            {
                phone.Property(p => p.Value)
                    .HasColumnName("PhoneNumber")
                    .HasMaxLength(20);
                    
                phone.Property(p => p.CountryCode)
                    .HasColumnName("PhoneCountryCode")
                    .HasMaxLength(5);
                    
                phone.Property(p => p.NationalNumber)
                    .HasColumnName("PhoneNationalNumber")
                    .HasMaxLength(15);
            });
            
            builder.OwnsOne(c => c.Email, email =>
            {
                email.Property(e => e.Value)
                    .HasColumnName("Email")
                    .HasMaxLength(320);
            });
                
            // Collections
            builder.OwnsMany(c => c.ServiceHistory, sh =>
            {
                sh.ToTable("CustomerServiceHistory");
                sh.WithOwner().HasForeignKey("CustomerId");
                sh.Property<Guid>("Id");
                sh.HasKey("Id");
                
                sh.Property(s => s.ServiceProviderId)
                    .IsRequired()
                    .HasColumnName("ServiceProviderId");
                    
                sh.Property(s => s.StaffMemberId)
                    .IsRequired()
                    .HasColumnName("StaffMemberId");
                    
                sh.Property(s => s.ServiceTypeId)
                    .IsRequired()
                    .HasColumnName("ServiceTypeId");
                    
                sh.Property(s => s.ServiceDate)
                    .IsRequired()
                    .HasColumnName("ServiceDate");
                    
                sh.Property(s => s.Notes)
                    .HasMaxLength(1000)
                    .HasColumnName("Notes");
                    
                sh.Property(s => s.Rating)
                    .HasColumnName("Rating");
                    
                sh.Property(s => s.Feedback)
                    .HasMaxLength(500)
                    .HasColumnName("Feedback");
            });
            
            // Simple collection for favorite service provider IDs
            builder.Property(c => c.FavoriteServiceProviderIds)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(Guid.Parse)
                          .ToList())
                .HasColumnName("FavoriteServiceProviderIds");
                
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
        }
    }
}
