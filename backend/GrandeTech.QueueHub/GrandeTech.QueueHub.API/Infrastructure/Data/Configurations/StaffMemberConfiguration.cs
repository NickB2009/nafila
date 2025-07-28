using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Grande.Fila.API.Domain.Staff;

namespace Grande.Fila.API.Infrastructure.Data.Configurations
{
    public class StaffMemberConfiguration : IEntityTypeConfiguration<StaffMember>
    {
        public void Configure(EntityTypeBuilder<StaffMember> builder)
        {
            // Table configuration
            builder.ToTable("StaffMembers");
            builder.HasKey(s => s.Id);
            
            // Basic properties
            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(s => s.LocationId)
                .IsRequired();

            builder.Property(s => s.ProfilePictureUrl)
                .HasMaxLength(500);

            builder.Property(s => s.Role)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(s => s.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(s => s.IsOnDuty)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(s => s.StaffStatus)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("available");

            builder.Property(s => s.UserId)
                .HasMaxLength(50);

            builder.Property(s => s.AverageServiceTimeInMinutes)
                .IsRequired()
                .HasDefaultValue(30.0);

            builder.Property(s => s.CompletedServicesCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(s => s.EmployeeCode)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(s => s.Username)
                .IsRequired()
                .HasMaxLength(50);

            // Value objects are configured in QueueHubDbContext.ConfigureValueObjects()

            // Note: StaffBreaks collection temporarily disabled due to EF Core configuration conflicts
            // This can be re-enabled later when the owned entity configuration is properly resolved

            // Indexes
            builder.HasIndex(s => s.Name)
                .HasDatabaseName("IX_StaffMembers_Name");

            builder.HasIndex(s => s.LocationId)
                .HasDatabaseName("IX_StaffMembers_LocationId");

            builder.HasIndex(s => s.IsActive)
                .HasDatabaseName("IX_StaffMembers_IsActive");

            builder.HasIndex(s => s.EmployeeCode)
                .IsUnique()
                .HasDatabaseName("IX_StaffMembers_EmployeeCode");

            builder.HasIndex(s => s.Username)
                .IsUnique()
                .HasDatabaseName("IX_StaffMembers_Username");

            builder.HasIndex(s => s.UserId)
                .HasDatabaseName("IX_StaffMembers_UserId");

            // Index on Email for searches
            builder.HasIndex("Email")
                .HasDatabaseName("IX_StaffMembers_Email");

            // Index on PhoneNumber for searches
            builder.HasIndex("PhoneNumber")
                .HasDatabaseName("IX_StaffMembers_PhoneNumber");
        }
    }
} 