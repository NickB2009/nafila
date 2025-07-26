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

            // Configure only known properties
            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(200);

            // Ignore all other properties for minimal configuration
            builder.Ignore("PhoneNumber");
            builder.Ignore("Email");
            builder.Ignore("IsActive");
            builder.Ignore("OrganizationId");
            builder.Ignore("PreferredServices");
            builder.Ignore("Tags");
        }
    }
} 