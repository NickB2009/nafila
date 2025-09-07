using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Grande.Fila.API.Domain.Users;

namespace Grande.Fila.API.Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // Table configuration
            builder.ToTable("Users");
            builder.HasKey(u => u.Id);

            // Configure only known properties
            builder.Property(u => u.FullName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(u => u.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(u => u.Role)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            // Ignore all complex properties for now
            // These will be added back when we implement full value object support
            builder.Ignore("OrganizationId");
            builder.Ignore("IsActive");
            builder.Ignore("Permissions");

            // Basic indexes on known properties
            builder.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");

            builder.HasIndex(u => u.PhoneNumber)
                .IsUnique()
                .HasDatabaseName("IX_Users_PhoneNumber");
        }
    }
} 