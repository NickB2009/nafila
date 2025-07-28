using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Grande.Fila.API.Domain.Customers;

namespace Grande.Fila.API.Infrastructure.Data.Configurations
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            // Table configuration
            builder.ToTable("Customers");
            builder.HasKey(c => c.Id);
            
            // Basic properties
            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.IsAnonymous)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(c => c.NotificationsEnabled)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(c => c.PreferredNotificationChannel)
                .HasMaxLength(20);

            builder.Property(c => c.UserId)
                .HasMaxLength(50);

            // Value objects are now configured in QueueHubDbContext.ConfigureValueObjects()
            // No need to ignore them anymore

            // Indexes
            builder.HasIndex(c => c.Name)
                .HasDatabaseName("IX_Customers_Name");

            builder.HasIndex(c => c.IsAnonymous)
                .HasDatabaseName("IX_Customers_IsAnonymous");

            builder.HasIndex(c => c.UserId)
                .HasDatabaseName("IX_Customers_UserId");

            // Index on Email for searches
            builder.HasIndex("Email")
                .HasDatabaseName("IX_Customers_Email");

            // Index on PhoneNumber for searches
            builder.HasIndex("PhoneNumber")
                .HasDatabaseName("IX_Customers_PhoneNumber");
        }
    }
} 