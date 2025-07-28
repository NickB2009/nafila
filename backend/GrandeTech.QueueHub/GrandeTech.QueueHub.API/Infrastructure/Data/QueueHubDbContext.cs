using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
        
        // These will be added back once we create proper entity configurations
        // public DbSet<StaffMember> StaffMembers { get; set; }
        // public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        // public DbSet<ServiceOffered> ServicesOffered { get; set; }
        // public DbSet<Coupon> Coupons { get; set; }
        // public DbSet<Advertisement> Advertisements { get; set; }
        // public DbSet<AuditLogEntry> AuditLogs { get; set; }
        // public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply entity configurations
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new QueueConfiguration());
            modelBuilder.ApplyConfiguration(new QueueEntryConfiguration());
            modelBuilder.ApplyConfiguration(new OrganizationConfiguration());
            modelBuilder.ApplyConfiguration(new LocationConfiguration());
            modelBuilder.ApplyConfiguration(new CustomerConfiguration());

            // Configure base entity properties
            ConfigureBaseEntityProperties(modelBuilder);

            // Configure value objects
            ConfigureValueObjects(modelBuilder);

            // Configure domain events (they won't be persisted)
            IgnoreDomainEvents(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Fallback configuration - should not be used in production
                optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=GrandeTechQueueHub;Trusted_Connection=True;MultipleActiveResultSets=true");
            }

            // Enable sensitive data logging in development
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();
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
                        .HasDefaultValueSql("GETUTCDATE()");

                    modelBuilder.Entity(entityType.ClrType)
                        .Property<DateTime?>("LastModifiedAt");

                    // Add global query filter for soft delete
                    modelBuilder.Entity(entityType.ClrType)
                        .HasQueryFilter(GetSoftDeleteFilter(entityType.ClrType));
                }
            }
        }

        private static void ConfigureValueObjects(ModelBuilder modelBuilder)
        {
            // ================================
            // ORGANIZATION VALUE OBJECTS
            // ================================
            
            // Configure Slug value object
            modelBuilder.Entity<Organization>()
                .Property(e => e.Slug)
                .HasConversion(
                    v => v.Value,
                    v => Slug.Create(v))
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

            // Configure LocationIds collection as JSON
            modelBuilder.Entity<Organization>()
                .Property(e => e.LocationIds)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<Guid>())
                .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<IReadOnlyCollection<Guid>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

            // ================================
            // LOCATION VALUE OBJECTS
            // ================================
            
            // Configure Slug for Location
            modelBuilder.Entity<Location>()
                .Property(e => e.Slug)
                .HasConversion(
                    v => v.Value,
                    v => Slug.Create(v))
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

            // Configure BusinessHours as owned type
            modelBuilder.Entity<Location>()
                .OwnsOne(e => e.BusinessHours, hours =>
                {
                    hours.Property(h => h.Start)
                        .HasColumnName("BusinessHoursStart");
                    hours.Property(h => h.End)
                        .HasColumnName("BusinessHoursEnd");
                });

            // Configure collections as JSON
            modelBuilder.Entity<Location>()
                .Property(e => e.StaffMemberIds)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<Guid>())
                .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<IReadOnlyCollection<Guid>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

            modelBuilder.Entity<Location>()
                .Property(e => e.ServiceTypeIds)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<Guid>())
                .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<IReadOnlyCollection<Guid>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

            modelBuilder.Entity<Location>()
                .Property(e => e.AdvertisementIds)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<Guid>())
                .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<IReadOnlyCollection<Guid>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

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

            // Configure ServiceHistory as owned collection
            modelBuilder.Entity<Customer>()
                .OwnsMany(e => e.ServiceHistory, history =>
                {
                    history.ToTable("CustomerServiceHistory");
                    history.WithOwner().HasForeignKey("CustomerId");
                    history.HasKey("Id");
                    history.Property(h => h.LocationId);
                    history.Property(h => h.StaffMemberId);
                    history.Property(h => h.ServiceTypeId);
                    history.Property(h => h.ServiceDate);
                    history.Property(h => h.Notes).HasMaxLength(1000);
                    history.Property(h => h.Rating);
                    history.Property(h => h.Feedback).HasMaxLength(2000);
                });

            // Configure FavoriteLocationIds as JSON
            modelBuilder.Entity<Customer>()
                .Property(e => e.FavoriteLocationIds)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<Guid>())
                .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<IReadOnlyCollection<Guid>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));
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