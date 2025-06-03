using GrandeTech.QueueHub.API.Domain.ServiceProviders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceProviderEntity = GrandeTech.QueueHub.API.Domain.ServiceProviders.ServiceProvider;

namespace GrandeTech.QueueHub.API.Infrastructure.Persistence.Configurations
{    /// <summary>
    /// Entity Framework configuration for ServiceProvider entity
    /// </summary>
    public class ServiceProviderConfiguration : IEntityTypeConfiguration<ServiceProviderEntity>
    {
        public void Configure(EntityTypeBuilder<ServiceProviderEntity> builder)
        {
            // Table name
            builder.ToTable("ServiceProviders");
            
            // Key
            builder.HasKey(sp => sp.Id);
            
            // Properties
            builder.Property(sp => sp.Name)
                .IsRequired()
                .HasMaxLength(100);
                
            // Value Objects
            builder.OwnsOne(sp => sp.Slug, slug =>
            {
                slug.Property(s => s.Value)
                    .HasColumnName("Slug")
                    .IsRequired()
                    .HasMaxLength(100);
                    
                // Index for faster lookup by slug
                slug.HasIndex(s => s.Value)
                    .IsUnique();
            });
            
            builder.Property(sp => sp.Description)
                .HasMaxLength(500);
                
            builder.Property(sp => sp.OrganizationId)
                .IsRequired();            builder.OwnsOne(sp => sp.Location, loc =>
            {
                loc.Property(a => a.Street)
                    .HasColumnName("Street")
                    .IsRequired()
                    .HasMaxLength(100);
                    
                loc.Property(a => a.Number)
                    .HasColumnName("Number")
                    .IsRequired()
                    .HasMaxLength(20);
                    
                loc.Property(a => a.Complement)
                    .HasColumnName("Complement")
                    .HasMaxLength(50);
                    
                loc.Property(a => a.Neighborhood)
                    .HasColumnName("Neighborhood")
                    .IsRequired()
                    .HasMaxLength(100);
                    
                loc.Property(a => a.City)
                    .HasColumnName("City")
                    .IsRequired()
                    .HasMaxLength(50);
                    
                loc.Property(a => a.State)
                    .HasColumnName("State")
                    .IsRequired()
                    .HasMaxLength(50);
                    
                loc.Property(a => a.Country)
                    .HasColumnName("Country")
                    .IsRequired()
                    .HasMaxLength(50);
                    
                loc.Property(a => a.PostalCode)
                    .HasColumnName("PostalCode")
                    .IsRequired()
                    .HasMaxLength(20);
                    
                loc.Property(a => a.Latitude)
                    .HasColumnName("Latitude");
                    
                loc.Property(a => a.Longitude)
                    .HasColumnName("Longitude");
            });
              builder.OwnsOne(sp => sp.ContactPhone, phone =>
            {
                phone.Property(p => p.Value)
                    .HasColumnName("ContactPhone")
                    .HasMaxLength(20);
                    
                phone.Property(p => p.CountryCode)
                    .HasColumnName("ContactPhoneCountryCode")
                    .HasMaxLength(5);
                    
                phone.Property(p => p.NationalNumber)
                    .HasColumnName("ContactPhoneNationalNumber")
                    .HasMaxLength(15);
            });
            
            builder.OwnsOne(sp => sp.ContactEmail, email =>
            {
                email.Property(e => e.Value)
                    .HasColumnName("ContactEmail")
                    .HasMaxLength(255);
            });
              builder.OwnsOne(sp => sp.CustomBranding, branding =>
            {
                branding.Property(b => b.PrimaryColor)
                    .HasColumnName("BrandingPrimaryColor")
                    .HasMaxLength(7);
                    
                branding.Property(b => b.SecondaryColor)
                    .HasColumnName("BrandingSecondaryColor")
                    .HasMaxLength(7);
                    
                branding.Property(b => b.LogoUrl)
                    .HasColumnName("BrandingLogoUrl")
                    .HasMaxLength(255);
                    
                branding.Property(b => b.FontFamily)
                    .HasColumnName("BrandingFontFamily")
                    .HasMaxLength(50);
            });
            
            builder.OwnsOne(sp => sp.BusinessHours, hours =>
            {
                hours.Property(h => h.Start)
                    .HasColumnName("BusinessHoursStart");
                    
                hours.Property(h => h.End)
                    .HasColumnName("BusinessHoursEnd");
            });
            
            builder.Property(sp => sp.IsQueueEnabled)
                .IsRequired()
                .HasDefaultValue(true);
                
            builder.Property(sp => sp.MaxQueueSize)
                .IsRequired()
                .HasDefaultValue(50);
                
            builder.Property(sp => sp.LateClientCapTimeInMinutes)
                .IsRequired()
                .HasDefaultValue(10);
                
            builder.Property(sp => sp.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
                
            builder.Property(sp => sp.AverageServiceTimeInMinutes)
                .IsRequired()
                .HasDefaultValue(15);
                
            builder.Property(sp => sp.LastAverageTimeReset)
                .IsRequired();
                
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
                
            // Relationships
            builder.HasOne<Domain.Organizations.Organization>()
                .WithMany()
                .HasForeignKey(sp => sp.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
