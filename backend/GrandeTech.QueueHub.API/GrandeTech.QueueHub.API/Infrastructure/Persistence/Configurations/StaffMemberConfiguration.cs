using GrandeTech.QueueHub.API.Domain.Staff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GrandeTech.QueueHub.API.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Entity Framework configuration for StaffMember entity
    /// </summary>
    public class StaffMemberConfiguration : IEntityTypeConfiguration<StaffMember>
    {
        public void Configure(EntityTypeBuilder<StaffMember> builder)
        {
            // Table name
            builder.ToTable("StaffMembers");
            
            // Key
            builder.HasKey(s => s.Id);
            
            // Properties
            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(s => s.Role)
                .IsRequired()
                .HasMaxLength(50);
            
            builder.Property(s => s.IsActive)
                .IsRequired();
            
            builder.Property(s => s.IsOnDuty)
                .IsRequired();
            
            builder.Property(s => s.StaffStatus)
                .IsRequired()
                .HasMaxLength(20);
            
            builder.Property(s => s.UserId)
                .HasMaxLength(100);
            
            builder.Property(s => s.AverageServiceTimeInMinutes)
                .IsRequired();
            
            builder.Property(s => s.CompletedServicesCount)
                .IsRequired();
            
            builder.Property(s => s.EmployeeCode)
                .IsRequired()
                .HasMaxLength(50);
            
            // Value Objects
            builder.OwnsOne(s => s.Email, email =>
            {
                email.Property(e => e.Value)
                    .HasColumnName("Email")
                    .HasMaxLength(320);
            });
            
            builder.OwnsOne(s => s.PhoneNumber, phone =>
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
            
            builder.Property(s => s.ProfilePictureUrl)
                .HasMaxLength(255);
            
            // Audit fields
            builder.Property(s => s.CreatedBy)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(s => s.LastModifiedBy)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(s => s.CreatedAt)
                .IsRequired();
            builder.Property(s => s.LastModifiedAt)
                .IsRequired();
        }
    }
} 