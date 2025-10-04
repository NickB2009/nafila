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

            // Configure database indexes for performance optimization
            DatabaseIndexesConfiguration.ConfigureIndexes(modelBuilder);
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

            // Configure flattened branding properties for Organization
            modelBuilder.Entity<Organization>()
                .Property(e => e.PrimaryColor)
                    .HasColumnName("BrandingPrimaryColor")
                    .HasMaxLength(7);
            
            modelBuilder.Entity<Organization>()
                .Property(e => e.SecondaryColor)
                    .HasColumnName("BrandingSecondaryColor")
                    .HasMaxLength(7);
            
            modelBuilder.Entity<Organization>()
                .Property(e => e.LogoUrl)
                    .HasColumnName("BrandingLogoUrl")
                    .HasMaxLength(500);
            
            modelBuilder.Entity<Organization>()
                .Property(e => e.FaviconUrl)
                    .HasColumnName("BrandingFaviconUrl")
                    .HasMaxLength(500);
            
            modelBuilder.Entity<Organization>()
                .Property(e => e.CompanyName)
                    .HasColumnName("BrandingCompanyName")
                    .HasMaxLength(200);
            
            modelBuilder.Entity<Organization>()
                .Property(e => e.TagLine)
                    .HasColumnName("BrandingTagLine")
                    .HasMaxLength(500);
            
            modelBuilder.Entity<Organization>()
                .Property(e => e.FontFamily)
                    .HasColumnName("BrandingFontFamily")
                    .HasMaxLength(100);

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

            // Configure flattened branding properties for Location
            modelBuilder.Entity<Location>()
                .Property(e => e.PrimaryColor)
                    .HasColumnName("CustomBrandingPrimaryColor")
                    .HasMaxLength(50);
            
            modelBuilder.Entity<Location>()
                .Property(e => e.SecondaryColor)
                    .HasColumnName("CustomBrandingSecondaryColor")
                    .HasMaxLength(50);
            
            modelBuilder.Entity<Location>()
                .Property(e => e.LogoUrl)
                    .HasColumnName("CustomBrandingLogoUrl")
                    .HasMaxLength(500);
            
            modelBuilder.Entity<Location>()
                .Property(e => e.FaviconUrl)
                    .HasColumnName("CustomBrandingFaviconUrl")
                    .HasMaxLength(500);
            
            modelBuilder.Entity<Location>()
                .Property(e => e.CompanyName)
                    .HasColumnName("CustomBrandingCompanyName")
                    .HasMaxLength(200);
            
            modelBuilder.Entity<Location>()
                .Property(e => e.TagLine)
                    .HasColumnName("CustomBrandingTagLine")
                    .HasMaxLength(500);
            
            modelBuilder.Entity<Location>()
                .Property(e => e.FontFamily)
                    .HasColumnName("CustomBrandingFontFamily")
                    .HasMaxLength(100);

            // Configure flattened weekly hours properties for Location
            modelBuilder.Entity<Location>()
                .Property(e => e.MondayOpenTime)
                    .HasColumnName("MondayOpenTime")
                    .HasColumnType("TIME");
            
            modelBuilder.Entity<Location>()
                .Property(e => e.MondayCloseTime)
                    .HasColumnName("MondayCloseTime")
                    .HasColumnType("TIME");
            
            modelBuilder.Entity<Location>()
                .Property(e => e.MondayIsClosed)
                    .HasColumnName("MondayIsClosed")
                    .HasColumnType("BIT");

            modelBuilder.Entity<Location>()
                .Property(e => e.TuesdayOpenTime)
                    .HasColumnName("TuesdayOpenTime")
                    .HasColumnType("TIME");
            
            modelBuilder.Entity<Location>()
                .Property(e => e.TuesdayCloseTime)
                    .HasColumnName("TuesdayCloseTime")
                    .HasColumnType("TIME");
            
            modelBuilder.Entity<Location>()
                .Property(e => e.TuesdayIsClosed)
                    .HasColumnName("TuesdayIsClosed")
                    .HasColumnType("BIT");

            modelBuilder.Entity<Location>()
                .Property(e => e.WednesdayOpenTime)
                    .HasColumnName("WednesdayOpenTime")
                    .HasColumnType("TIME");
            
            modelBuilder.Entity<Location>()
                .Property(e => e.WednesdayCloseTime)
                    .HasColumnName("WednesdayCloseTime")
                    .HasColumnType("TIME");
            
            modelBuilder.Entity<Location>()
                .Property(e => e.WednesdayIsClosed)
                    .HasColumnName("WednesdayIsClosed")
                    .HasColumnType("BIT");

            modelBuilder.Entity<Location>()
                .Property(e => e.ThursdayOpenTime)
                    .HasColumnName("ThursdayOpenTime")
                    .HasColumnType("TIME");
            
            modelBuilder.Entity<Location>()
                .Property(e => e.ThursdayCloseTime)
                    .HasColumnName("ThursdayCloseTime")
                    .HasColumnType("TIME");
            
            modelBuilder.Entity<Location>()
                .Property(e => e.ThursdayIsClosed)
                    .HasColumnName("ThursdayIsClosed")
                    .HasColumnType("BIT");

            modelBuilder.Entity<Location>()
                .Property(e => e.FridayOpenTime)
                    .HasColumnName("FridayOpenTime")
                    .HasColumnType("TIME");
            
            modelBuilder.Entity<Location>()
                .Property(e => e.FridayCloseTime)
                    .HasColumnName("FridayCloseTime")
                    .HasColumnType("TIME");
            
            modelBuilder.Entity<Location>()
                .Property(e => e.FridayIsClosed)
                    .HasColumnName("FridayIsClosed")
                    .HasColumnType("BIT");

            modelBuilder.Entity<Location>()
                .Property(e => e.SaturdayOpenTime)
                    .HasColumnName("SaturdayOpenTime")
                    .HasColumnType("TIME");
            
            modelBuilder.Entity<Location>()
                .Property(e => e.SaturdayCloseTime)
                    .HasColumnName("SaturdayCloseTime")
                    .HasColumnType("TIME");
            
            modelBuilder.Entity<Location>()
                .Property(e => e.SaturdayIsClosed)
                    .HasColumnName("SaturdayIsClosed")
                    .HasColumnType("BIT");

            modelBuilder.Entity<Location>()
                .Property(e => e.SundayOpenTime)
                    .HasColumnName("SundayOpenTime")
                    .HasColumnType("TIME");
            
            modelBuilder.Entity<Location>()
                .Property(e => e.SundayCloseTime)
                    .HasColumnName("SundayCloseTime")
                    .HasColumnType("TIME");
            
            modelBuilder.Entity<Location>()
                .Property(e => e.SundayIsClosed)
                    .HasColumnName("SundayIsClosed")
                    .HasColumnType("BIT");

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

            // Configure flattened price properties for SubscriptionPlan
            modelBuilder.Entity<SubscriptionPlan>()
                .Property(e => e.MonthlyPriceAmount)
                        .HasColumnName("MonthlyPriceAmount")
                        .HasColumnType("decimal(18,2)");
            
            modelBuilder.Entity<SubscriptionPlan>()
                .Property(e => e.MonthlyPriceCurrency)
                        .HasColumnName("MonthlyPriceCurrency")
                        .HasMaxLength(3);

            modelBuilder.Entity<SubscriptionPlan>()
                .Property(e => e.YearlyPriceAmount)
                        .HasColumnName("YearlyPriceAmount")
                        .HasColumnType("decimal(18,2)");
            
            modelBuilder.Entity<SubscriptionPlan>()
                .Property(e => e.YearlyPriceCurrency)
                        .HasColumnName("YearlyPriceCurrency")
                        .HasMaxLength(3);

            // Remove the old conflicting Price property - it should not exist in the entity
            // The old Price column in the migration will be removed in the next migration

            // Configure flattened price properties for ServiceOffered
            modelBuilder.Entity<ServiceOffered>()
                .Property(e => e.PriceAmount)
                        .HasColumnName("PriceAmount")
                        .HasColumnType("decimal(18,2)");
            
            modelBuilder.Entity<ServiceOffered>()
                .Property(e => e.PriceCurrency)
                        .HasColumnName("PriceCurrency")
                        .HasMaxLength(3);

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

        // REMOVED: ConfigureValueObjectsOriginal method - configurations moved to ConfigureBasicValueObjects

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