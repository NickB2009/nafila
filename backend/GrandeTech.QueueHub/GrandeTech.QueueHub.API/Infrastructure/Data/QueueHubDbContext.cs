using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq;
using Grande.Fila.API.Domain.Common;
using Grande.Fila.API.Domain.Users;
using Grande.Fila.API.Domain.Organizations;
using Grande.Fila.API.Domain.Locations;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.Customers;
using Grande.Fila.API.Domain.Staff;
using Grande.Fila.API.Domain.Subscriptions;
using Grande.Fila.API.Domain.AuditLogs;
using Grande.Fila.API.Domain.ServicesOffered;
using Grande.Fila.API.Domain.Promotions;
using Grande.Fila.API.Domain.Advertising;
using Grande.Fila.API.Domain.Notifications;
using Grande.Fila.API.Domain.Common.ValueObjects;
using Grande.Fila.API.Infrastructure.Data.Configurations;

namespace Grande.Fila.API.Infrastructure.Data
{
    public class QueueHubDbContext : DbContext
    {
        public QueueHubDbContext(DbContextOptions<QueueHubDbContext> options) : base(options)
        {
        }

        // Aggregate Roots
        public DbSet<User> Users { get; set; }
        public DbSet<Queue> Queues { get; set; }
        public DbSet<QueueEntry> QueueEntries { get; set; }
        
        // Now enabled with proper value object configurations
        public DbSet<Organization> Organizations { get; set; }  
        public DbSet<Location> Locations { get; set; }
        public DbSet<Customer> Customers { get; set; }
        
        // Newly enabled entities with complete repository implementations
        public DbSet<StaffMember> StaffMembers { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<ServiceOffered> ServicesOffered { get; set; }
        
        // These remain disabled for now
        // public DbSet<Coupon> Coupons { get; set; }
        // public DbSet<Advertisement> Advertisements { get; set; }
        // public DbSet<AuditLogEntry> AuditLogs { get; set; }
        // public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ignore entities that should only exist as owned types (do this first)
            modelBuilder.Ignore<StaffBreak>();
            modelBuilder.Ignore<ServiceHistoryItem>();
            // WeeklyBusinessHours is now configured as JSON in LocationConfiguration
            // BrandingConfig is configured as owned type, not ignored

            // Apply ALL entity configurations (now compatible with value object mappings)
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new OrganizationConfiguration());
            modelBuilder.ApplyConfiguration(new LocationConfiguration());
            modelBuilder.ApplyConfiguration(new CustomerConfiguration());
            modelBuilder.ApplyConfiguration(new StaffMemberConfiguration());
            modelBuilder.ApplyConfiguration(new SubscriptionPlanConfiguration());
            modelBuilder.ApplyConfiguration(new ServiceOfferedConfiguration());
            modelBuilder.ApplyConfiguration(new QueueConfiguration());
            modelBuilder.ApplyConfiguration(new QueueEntryConfiguration());

            // Configure base entity properties
            ConfigureBaseEntityProperties(modelBuilder);

            // Use basic value object configuration for stability
            ConfigureBasicValueObjects(modelBuilder);

            // Configure domain events (they won't be persisted)
            IgnoreDomainEvents(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Fallback configuration - should not be used in production
                optionsBuilder.UseMySql("Server=localhost;Database=QueueHubDb;User=root;Password=DevPassword123!;Port=3306;CharSet=utf8mb4;SslMode=None;ConnectionTimeout=30;", ServerVersion.AutoDetect("Server=localhost;Database=QueueHubDb;User=root;Password=DevPassword123!;Port=3306;CharSet=utf8mb4;SslMode=None;ConnectionTimeout=30;"));
            }

            // Enable sensitive data logging and detailed errors only in development
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (environment == "Development")
            {
                optionsBuilder.EnableSensitiveDataLogging();
                optionsBuilder.EnableDetailedErrors();
            }
        }

        private static void ConfigureBaseEntityProperties(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    // Configure common properties
                    modelBuilder.Entity(entityType.ClrType)
                        .Property<DateTime>("CreatedAt")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

                    modelBuilder.Entity(entityType.ClrType)
                        .Property<DateTime?>("LastModifiedAt");

                    // Add global query filter for soft delete
                    modelBuilder.Entity(entityType.ClrType)
                        .HasQueryFilter(GetSoftDeleteFilter(entityType.ClrType));

                    // Temporarily ignore RowVersion across all entities to avoid provider collection mapping issues
                    modelBuilder.Entity(entityType.ClrType)
                        .Ignore("RowVersion");
                }
            }
        }

        private static void ConfigureBasicValueObjects(ModelBuilder modelBuilder)
        {
            // Only ignore collections, not value objects (entity configs expect them)
            modelBuilder.Entity<User>()
                .Ignore(e => e.Permissions);

            // Configure Organization value objects (required by entity configuration indexes)
            modelBuilder.Entity<Organization>()
                .Ignore(e => e.LocationIds); // Only ignore collections
                
            modelBuilder.Entity<Organization>()
                .Property(e => e.Slug)
                .HasConversion(
                    v => v != null ? v.Value : null,
                    v => !string.IsNullOrEmpty(v) ? Slug.Create(v) : null)
                .HasColumnName("Slug")
                .HasMaxLength(100);

            modelBuilder.Entity<Organization>()
                .Property(e => e.ContactEmail)
                .HasConversion(
                    v => v != null ? v.Value : null,
                    v => !string.IsNullOrWhiteSpace(v) ? Email.Create(v) : null)
                .HasColumnName("ContactEmail")
                .HasMaxLength(320);

            modelBuilder.Entity<Organization>()
                .Property(e => e.ContactPhone)
                .HasConversion(
                    v => v != null ? v.Value : null,
                    v => !string.IsNullOrWhiteSpace(v) ? PhoneNumber.Create(v) : null)
                .HasColumnName("ContactPhone")
                .HasMaxLength(50);

            modelBuilder.Entity<Organization>()
                .OwnsOne(e => e.BrandingConfig, branding =>
                {
                    branding.Property(b => b.PrimaryColor).HasColumnName("BrandingPrimaryColor").HasMaxLength(50);
                    branding.Property(b => b.SecondaryColor).HasColumnName("BrandingSecondaryColor").HasMaxLength(50);
                    branding.Property(b => b.LogoUrl).HasColumnName("BrandingLogoUrl").HasMaxLength(500);
                    branding.Property(b => b.FaviconUrl).HasColumnName("BrandingFaviconUrl").HasMaxLength(500);
                    branding.Property(b => b.CompanyName).HasColumnName("BrandingCompanyName").HasMaxLength(200);
                    branding.Property(b => b.TagLine).HasColumnName("BrandingTagLine").HasMaxLength(500);
                    branding.Property(b => b.FontFamily).HasColumnName("BrandingFontFamily").HasMaxLength(100);
                });

            // Configure Location value objects (required by entity configuration indexes)
            modelBuilder.Entity<Location>()
                .Ignore(e => e.StaffMemberIds)
                .Ignore(e => e.ServiceTypeIds)
                .Ignore(e => e.AdvertisementIds); // Only ignore collections

            modelBuilder.Entity<Location>()
                .Property(e => e.Slug)
                .HasConversion(
                    v => v != null ? v.Value : null,
                    v => !string.IsNullOrEmpty(v) ? Slug.Create(v) : null)
                .HasColumnName("Slug")
                .HasMaxLength(100);

            modelBuilder.Entity<Location>()
                .Property(e => e.ContactEmail)
                .HasConversion(
                    v => v != null ? v.Value : null,
                    v => !string.IsNullOrWhiteSpace(v) ? Email.Create(v) : null)
                .HasColumnName("ContactEmail")
                .HasMaxLength(320);

            modelBuilder.Entity<Location>()
                .Property(e => e.ContactPhone)
                .HasConversion(
                    v => v != null ? v.Value : null,
                    v => !string.IsNullOrWhiteSpace(v) ? PhoneNumber.Create(v) : null)
                .HasColumnName("ContactPhone")
                .HasMaxLength(50);

            modelBuilder.Entity<Location>()
                .OwnsOne(e => e.Address, address =>
                {
                    address.Property(a => a.Street).HasColumnName("AddressStreet").HasMaxLength(200);
                    address.Property(a => a.Number).HasColumnName("AddressNumber").HasMaxLength(20);
                    address.Property(a => a.Complement).HasColumnName("AddressComplement").HasMaxLength(100);
                    address.Property(a => a.Neighborhood).HasColumnName("AddressNeighborhood").HasMaxLength(100);
                    address.Property(a => a.City).HasColumnName("AddressCity").HasMaxLength(100);
                    address.Property(a => a.State).HasColumnName("AddressState").HasMaxLength(50);
                    address.Property(a => a.Country).HasColumnName("AddressCountry").HasMaxLength(50);
                    address.Property(a => a.PostalCode).HasColumnName("AddressPostalCode").HasMaxLength(20);
                    address.Property(a => a.Latitude).HasColumnName("AddressLatitude");
                    address.Property(a => a.Longitude).HasColumnName("AddressLongitude");
                });

            // Configure CustomBranding as owned type (nullable)
            modelBuilder.Entity<Location>()
                .OwnsOne(e => e.CustomBranding, branding =>
                {
                    branding.Property(b => b.PrimaryColor)
                        .HasColumnName("CustomBrandingPrimaryColor")
                        .HasMaxLength(50);
                    branding.Property(b => b.SecondaryColor)
                        .HasColumnName("CustomBrandingSecondaryColor")
                        .HasMaxLength(50);
                    branding.Property(b => b.LogoUrl)
                        .HasColumnName("CustomBrandingLogoUrl")
                        .HasMaxLength(500);
                    branding.Property(b => b.FaviconUrl)
                        .HasColumnName("CustomBrandingFaviconUrl")
                        .HasMaxLength(500);
                    branding.Property(b => b.CompanyName)
                        .HasColumnName("CustomBrandingCompanyName")
                        .HasMaxLength(200);
                    branding.Property(b => b.TagLine)
                        .HasColumnName("CustomBrandingTagLine")
                        .HasMaxLength(500);
                    branding.Property(b => b.FontFamily)
                        .HasColumnName("CustomBrandingFontFamily")
                        .HasMaxLength(100);
                });

            // Configure Customer with value objects
            modelBuilder.Entity<Customer>()
                .Ignore(e => e.FavoriteLocationIds)
                .Ignore(e => e.ServiceHistory); // Only ignore collections

            // Configure Email for Customer
            modelBuilder.Entity<Customer>()
                .Property(e => e.Email)
                .HasConversion(
                    v => v != null ? v.Value : null,
                    v => !string.IsNullOrWhiteSpace(v) ? Email.Create(v) : null)
                .HasColumnName("Email")
                .HasMaxLength(320);

            // Configure PhoneNumber for Customer
            modelBuilder.Entity<Customer>()
                .Property(e => e.PhoneNumber)
                .HasConversion(
                    v => v != null ? v.Value : null,
                    v => !string.IsNullOrWhiteSpace(v) ? PhoneNumber.Create(v) : null)
                .HasColumnName("PhoneNumber")
                .HasMaxLength(50);

            // Configure StaffMember with value objects
            modelBuilder.Entity<StaffMember>()
                .Ignore(e => e.SpecialtyServiceTypeIds)
                .Ignore(e => e.Breaks); // Only ignore collections

            // Configure Email for StaffMember
            modelBuilder.Entity<StaffMember>()
                .Property(e => e.Email)
                .HasConversion(
                    v => v != null ? v.Value : null,
                    v => !string.IsNullOrWhiteSpace(v) ? Email.Create(v) : null)
                .HasColumnName("Email")
                .HasMaxLength(320);

            // Configure PhoneNumber for StaffMember
            modelBuilder.Entity<StaffMember>()
                .Property(e => e.PhoneNumber)
                .HasConversion(
                    v => v != null ? v.Value : null,
                    v => !string.IsNullOrWhiteSpace(v) ? PhoneNumber.Create(v) : null)
                .HasColumnName("PhoneNumber")
                .HasMaxLength(50);

            // Configure Money value objects for SubscriptionPlan
            modelBuilder.Entity<SubscriptionPlan>()
                .OwnsOne(e => e.MonthlyPrice, money =>
                {
                    money.Property(m => m.Amount)
                        .HasColumnName("MonthlyPriceAmount")
                        .HasColumnType("decimal(18,2)");
                    money.Property(m => m.Currency)
                        .HasColumnName("MonthlyPriceCurrency")
                        .HasMaxLength(3);
                });

            modelBuilder.Entity<SubscriptionPlan>()
                .OwnsOne(e => e.YearlyPrice, money =>
                {
                    money.Property(m => m.Amount)
                        .HasColumnName("YearlyPriceAmount")
                        .HasColumnType("decimal(18,2)");
                    money.Property(m => m.Currency)
                        .HasColumnName("YearlyPriceCurrency")
                        .HasMaxLength(3);
                });

            // Configure Money value object for ServiceOffered
            modelBuilder.Entity<ServiceOffered>()
                .OwnsOne(e => e.Price, money =>
                {
                    money.Property(m => m.Amount)
                        .HasColumnName("PriceAmount")
                        .HasColumnType("decimal(18,2)");
                    money.Property(m => m.Currency)
                        .HasColumnName("PriceCurrency")
                        .HasMaxLength(3);
                });

            // Explicitly ignore primitive collections that are exposed as read-only collections
            modelBuilder.Entity<Organization>()
                .Ignore(e => e.LocationIds);

            modelBuilder.Entity<Customer>()
                .Ignore(e => e.FavoriteLocationIds)
                .Ignore(e => e.ServiceHistory);

            modelBuilder.Entity<Location>()
                .Ignore(e => e.StaffMemberIds)
                .Ignore(e => e.ServiceTypeIds)
                .Ignore(e => e.AdvertisementIds);

            modelBuilder.Entity<StaffMember>()
                .Ignore(e => e.SpecialtyServiceTypeIds)
                .Ignore(e => e.Breaks);

            modelBuilder.Entity<User>()
                .Ignore(e => e.Permissions);

            // Coupon configuration moved to be consistent with ignore statement below

            // As a safety net, ignore any primitive collections that might slip through and cause EF mapping issues
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var clrType = entityType.ClrType;
                var properties = clrType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                foreach (var prop in properties)
                {
                    var propType = prop.PropertyType;
                    // Skip non-problematic types
                    if (propType == typeof(string) || propType == typeof(byte[]) || prop.Name == "RowVersion")
                        continue;

                    if (typeof(System.Collections.IEnumerable).IsAssignableFrom(propType))
                    {
                        Type? elementType = null;
                        if (propType.IsArray)
                        {
                            elementType = propType.GetElementType();
                        }
                        else if (propType.IsGenericType)
                        {
                            elementType = propType.GetGenericArguments().FirstOrDefault();
                        }

                        if (elementType != null)
                        {
                            var isPrimitiveCollection = elementType == typeof(string) || elementType == typeof(Guid) || elementType.IsPrimitive;
                            var isEntityType = typeof(BaseEntity).IsAssignableFrom(elementType);

                            if (isPrimitiveCollection && !isEntityType)
                            {
                                // Ignore primitive collections (e.g., List<Guid>, List<string>) unless explicitly mapped
                                modelBuilder.Entity(clrType).Ignore(prop.Name);
                            }
                        }
                    }
                }
            }
        }

        private static void ConfigureValueObjectsOriginal(ModelBuilder modelBuilder)
        {
            // ================================
            // ORGANIZATION VALUE OBJECTS
            // ================================
            
            // Configure Slug value object
            modelBuilder.Entity<Organization>()
                .Property(e => e.Slug)
                .HasConversion(
                    v => v != null ? v.Value : null,
                    v => !string.IsNullOrEmpty(v) ? Slug.Create(v) : null)
                .HasColumnName("Slug")
                .HasMaxLength(100);

            // Configure Email value objects
            modelBuilder.Entity<Organization>()
                .Property(e => e.ContactEmail)
                .HasConversion(
                    v => v != null ? v.Value : null,
                    v => v != null ? Email.Create(v) : null)
                .HasColumnName("ContactEmail")
                .HasMaxLength(320);

            // Configure PhoneNumber value objects
            modelBuilder.Entity<Organization>()
                .Property(e => e.ContactPhone)
                .HasConversion(
                    v => v != null ? v.Value : null,
                    v => v != null ? PhoneNumber.Create(v) : null)
                .HasColumnName("ContactPhone")
                .HasMaxLength(50);

            // Configure BrandingConfig as owned type (complex value object)
            modelBuilder.Entity<Organization>()
                .OwnsOne(e => e.BrandingConfig, branding =>
                {
                    branding.Property(b => b.PrimaryColor)
                        .HasColumnName("BrandingPrimaryColor")
                        .HasMaxLength(50);
                    branding.Property(b => b.SecondaryColor)
                        .HasColumnName("BrandingSecondaryColor")
                        .HasMaxLength(50);
                    branding.Property(b => b.LogoUrl)
                        .HasColumnName("BrandingLogoUrl")
                        .HasMaxLength(500);
                    branding.Property(b => b.FaviconUrl)
                        .HasColumnName("BrandingFaviconUrl")
                        .HasMaxLength(500);
                    branding.Property(b => b.CompanyName)
                        .HasColumnName("BrandingCompanyName")
                        .HasMaxLength(200);
                    branding.Property(b => b.TagLine)
                        .HasColumnName("BrandingTagLine")
                        .HasMaxLength(500);
                    branding.Property(b => b.FontFamily)
                        .HasColumnName("BrandingFontFamily")
                        .HasMaxLength(100);
                });

            // Temporarily ignore LocationIds to avoid EF Core collection mapping issues
            modelBuilder.Entity<Organization>()
                .Ignore(e => e.LocationIds);

            // ================================
            // LOCATION VALUE OBJECTS
            // ================================
            
            // Configure Slug for Location
            modelBuilder.Entity<Location>()
                .Property(e => e.Slug)
                .HasConversion(
                    v => v != null ? v.Value : null,
                    v => !string.IsNullOrEmpty(v) ? Slug.Create(v) : null)
                .HasColumnName("Slug")
                .HasMaxLength(100);

            // Configure Email for Location
            modelBuilder.Entity<Location>()
                .Property(e => e.ContactEmail)
                .HasConversion(
                    v => v != null ? v.Value : null,
                    v => v != null ? Email.Create(v) : null)
                .HasColumnName("ContactEmail")
                .HasMaxLength(320);

            // Configure PhoneNumber for Location
            modelBuilder.Entity<Location>()
                .Property(e => e.ContactPhone)
                .HasConversion(
                    v => v != null ? v.Value : null,
                    v => v != null ? PhoneNumber.Create(v) : null)
                .HasColumnName("ContactPhone")
                .HasMaxLength(50);

            // Configure Address as owned type
            modelBuilder.Entity<Location>()
                .OwnsOne(e => e.Address, address =>
                {
                    address.Property(a => a.Street)
                        .HasColumnName("AddressStreet")
                        .HasMaxLength(200);
                    address.Property(a => a.Number)
                        .HasColumnName("AddressNumber")
                        .HasMaxLength(20);
                    address.Property(a => a.Complement)
                        .HasColumnName("AddressComplement")
                        .HasMaxLength(100);
                    address.Property(a => a.Neighborhood)
                        .HasColumnName("AddressNeighborhood")
                        .HasMaxLength(100);
                    address.Property(a => a.City)
                        .HasColumnName("AddressCity")
                        .HasMaxLength(100);
                    address.Property(a => a.State)
                        .HasColumnName("AddressState")
                        .HasMaxLength(50);
                    address.Property(a => a.Country)
                        .HasColumnName("AddressCountry")
                        .HasMaxLength(50);
                    address.Property(a => a.PostalCode)
                        .HasColumnName("AddressPostalCode")
                        .HasMaxLength(20);
                    address.Property(a => a.Latitude)
                        .HasColumnName("AddressLatitude");
                    address.Property(a => a.Longitude)
                        .HasColumnName("AddressLongitude");
                });

            // Configure CustomBranding as owned type (nullable)
            modelBuilder.Entity<Location>()
                .OwnsOne(e => e.CustomBranding, branding =>
                {
                    branding.Property(b => b.PrimaryColor)
                        .HasColumnName("CustomBrandingPrimaryColor")
                        .HasMaxLength(50);
                    branding.Property(b => b.SecondaryColor)
                        .HasColumnName("CustomBrandingSecondaryColor")
                        .HasMaxLength(50);
                    branding.Property(b => b.LogoUrl)
                        .HasColumnName("CustomBrandingLogoUrl")
                        .HasMaxLength(500);
                    branding.Property(b => b.FaviconUrl)
                        .HasColumnName("CustomBrandingFaviconUrl")
                        .HasMaxLength(500);
                    branding.Property(b => b.CompanyName)
                        .HasColumnName("CustomBrandingCompanyName")
                        .HasMaxLength(200);
                    branding.Property(b => b.TagLine)
                        .HasColumnName("CustomBrandingTagLine")
                        .HasMaxLength(500);
                    branding.Property(b => b.FontFamily)
                        .HasColumnName("CustomBrandingFontFamily")
                        .HasMaxLength(100);
                });

            // Configure WeeklyBusinessHours as simple string storage (temporary fix)
            modelBuilder.Entity<Location>()
                .Property(e => e.WeeklyHours)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => !string.IsNullOrEmpty(v) ? System.Text.Json.JsonSerializer.Deserialize<WeeklyBusinessHours>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? WeeklyBusinessHours.CreateUniform(TimeSpan.FromHours(9), TimeSpan.FromHours(17)) : WeeklyBusinessHours.CreateUniform(TimeSpan.FromHours(9), TimeSpan.FromHours(17)))
                .HasColumnName("WeeklyBusinessHours")
                .HasColumnType("LONGTEXT");

            // Temporarily ignore collections to avoid EF Core collection mapping issues
            modelBuilder.Entity<Location>()
                .Ignore(e => e.StaffMemberIds)
                .Ignore(e => e.ServiceTypeIds)
                .Ignore(e => e.AdvertisementIds);

            // ================================
            // CUSTOMER VALUE OBJECTS
            // ================================
            
            // Configure Email for Customer
            modelBuilder.Entity<Customer>()
                .Property(e => e.Email)
                .HasConversion(
                    v => v != null ? v.Value : null,
                    v => v != null ? Email.Create(v) : null)
                .HasColumnName("Email")
                .HasMaxLength(320);

            // Configure PhoneNumber for Customer
            modelBuilder.Entity<Customer>()
                .Property(e => e.PhoneNumber)
                .HasConversion(
                    v => v != null ? v.Value : null,
                    v => v != null ? PhoneNumber.Create(v) : null)
                .HasColumnName("PhoneNumber")
                .HasMaxLength(50);

            // ServiceHistory configuration removed to avoid conflicts

            // Temporarily ignore collections to avoid EF Core collection mapping issues
            modelBuilder.Entity<Customer>()
                .Ignore(e => e.FavoriteLocationIds)
                .Ignore(e => e.ServiceHistory);

            // ================================
            // STAFFMEMBER VALUE OBJECTS
            // ================================
            
            // Configure Email for StaffMember
            modelBuilder.Entity<StaffMember>()
                .Property(e => e.Email)
                .HasConversion(
                    v => v != null ? v.Value : null,
                    v => v != null ? Email.Create(v) : null)
                .HasColumnName("Email")
                .HasMaxLength(320);

            // Configure PhoneNumber for StaffMember
            modelBuilder.Entity<StaffMember>()
                .Property(e => e.PhoneNumber)
                .HasConversion(
                    v => v != null ? v.Value : null,
                    v => v != null ? PhoneNumber.Create(v) : null)
                .HasColumnName("PhoneNumber")
                .HasMaxLength(50);

            // Temporarily ignore collections to avoid EF Core collection mapping issues
            modelBuilder.Entity<StaffMember>()
                .Ignore(e => e.SpecialtyServiceTypeIds)
                .Ignore(e => e.Breaks);

            // Ignore User collections to avoid EF Core collection mapping issues
            modelBuilder.Entity<User>()
                .Ignore(e => e.Permissions);

            // ================================
            // SUBSCRIPTIONPLAN VALUE OBJECTS
            // ================================
            
            // Configure MonthlyPrice as owned type
            modelBuilder.Entity<SubscriptionPlan>()
                .OwnsOne(e => e.MonthlyPrice, money =>
                {
                    money.Property(m => m.Amount)
                        .HasColumnName("MonthlyPriceAmount")
                        .HasColumnType("decimal(18,2)");
                    money.Property(m => m.Currency)
                        .HasColumnName("MonthlyPriceCurrency")
                        .HasMaxLength(3);
                });

            // Configure YearlyPrice as owned type
            modelBuilder.Entity<SubscriptionPlan>()
                .OwnsOne(e => e.YearlyPrice, money =>
                {
                    money.Property(m => m.Amount)
                        .HasColumnName("YearlyPriceAmount")
                        .HasColumnType("decimal(18,2)");
                    money.Property(m => m.Currency)
                        .HasColumnName("YearlyPriceCurrency")
                        .HasMaxLength(3);
                });

            // ================================
            // SERVICEOFFERED VALUE OBJECTS
            // ================================
            
            // Configure Price as owned type (nullable)
            modelBuilder.Entity<ServiceOffered>()
                .OwnsOne(e => e.Price, money =>
                {
                    money.Property(m => m.Amount)
                        .HasColumnName("PriceAmount")
                        .HasColumnType("decimal(18,2)");
                    money.Property(m => m.Currency)
                        .HasColumnName("PriceCurrency")
                        .HasMaxLength(3);
                });

            // COUPON: ignore complex parts not needed now to avoid shared-type conflicts
            modelBuilder.Ignore<Coupon>();
            modelBuilder.Ignore<CouponRedemption>();
        }

        private static void IgnoreDomainEvents(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Ignore("DomainEvents");
                }
            }
        }

        private static System.Linq.Expressions.LambdaExpression GetSoftDeleteFilter(Type entityType)
        {
            var parameter = System.Linq.Expressions.Expression.Parameter(entityType, "e");
            var property = System.Linq.Expressions.Expression.Property(parameter, "IsDeleted");
            var condition = System.Linq.Expressions.Expression.Equal(property, System.Linq.Expressions.Expression.Constant(false));
            return System.Linq.Expressions.Expression.Lambda(condition, parameter);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Update audit fields before saving
            UpdateAuditFields();

            var result = await base.SaveChangesAsync(cancellationToken);

            // Dispatch domain events after successful save
            await DispatchDomainEvents();

            return result;
        }

        public override int SaveChanges()
        {
            UpdateAuditFields();
            var result = base.SaveChanges();
            DispatchDomainEvents().GetAwaiter().GetResult();
            return result;
        }

        private void UpdateAuditFields()
        {
            var entries = ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        // CreatedBy will be set by application logic
                        break;

                    case EntityState.Modified:
                        entry.Entity.LastModifiedAt = DateTime.UtcNow;
                        // LastModifiedBy will be set by application logic
                        break;
                }
            }
        }

        private async Task DispatchDomainEvents()
        {
            var entities = ChangeTracker.Entries<BaseEntity>()
                .Where(e => e.Entity.DomainEvents.Any())
                .Select(e => e.Entity)
                .ToList();

            var domainEvents = entities
                .SelectMany(e => e.DomainEvents)
                .ToList();

            // Clear domain events
            entities.ForEach(e => e.ClearDomainEvents());

            // Here you would dispatch events to handlers
            // For now, we'll just clear them
            // TODO: Implement domain event dispatcher when needed
            await Task.CompletedTask;
        }
    }
} 