using GrandeTech.QueueHub.API.Domain.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GrandeTech.QueueHub.API.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Entity Framework configuration for Organization entity
    /// </summary>
    public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
    {
        public void Configure(EntityTypeBuilder<Organization> builder)
        {
            // Table name
            builder.ToTable("Organizations");
            
            // Key
            builder.HasKey(o => o.Id);
            
            // Properties
            builder.Property(o => o.Name)
                .IsRequired()
                .HasMaxLength(100);
                  // Value Objects
            builder.OwnsOne(o => o.Slug, slug =>
            {
                slug.Property(s => s.Value)
                    .HasColumnName("Slug")
                    .IsRequired()
                    .HasMaxLength(100);
                    
                // Index for faster lookup by slug
                slug.HasIndex(s => s.Value)
                    .IsUnique();
            });
            
            builder.Property(o => o.Description)
                .HasMaxLength(500);
                
            builder.OwnsOne(o => o.ContactEmail, email =>
            {
                email.Property(e => e.Value)
                    .HasColumnName("ContactEmail")
                    .HasMaxLength(255);
            });
              builder.OwnsOne(o => o.ContactPhone, phone =>
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
              builder.Property(o => o.WebsiteUrl)
                .HasMaxLength(255);
                  builder.OwnsOne(o => o.BrandingConfig, branding =>
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
                    
                branding.Property(b => b.FaviconUrl)
                    .HasColumnName("BrandingFaviconUrl")
                    .HasMaxLength(255);
                    
                branding.Property(b => b.CompanyName)
                    .HasColumnName("BrandingCompanyName")
                    .HasMaxLength(100);
                    
                branding.Property(b => b.TagLine)
                    .HasColumnName("BrandingTagLine")
                    .HasMaxLength(200);
                    
                branding.Property(b => b.FontFamily)
                    .HasColumnName("BrandingFontFamily")
                    .HasMaxLength(50);
            });
            
            builder.Property(o => o.SubscriptionPlanId)
                .IsRequired();
                
            builder.Property(o => o.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
                
            builder.Property(o => o.SharesDataForAnalytics)
                .IsRequired()
                .HasDefaultValue(false);
                
            // Audit fields
            builder.Property(o => o.CreatedBy)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(o => o.LastModifiedBy)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(o => o.CreatedAt)
                .IsRequired();
                
            builder.Property(o => o.LastModifiedAt)
                .IsRequired();
                
            // Relationships
            // The relationship to ServicesProviders is maintained
            // via foreign keys in the ServicesProviders table
        }
    }
}
