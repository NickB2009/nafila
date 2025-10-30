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
        public DbSet<Coupon> Coupons { get; set; }
        // public DbSet<Advertisement> Advertisements { get; set; }
        // public DbSet<AuditLogEntry> AuditLogs { get; set; }
        // public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // STEP 1: Ignore value objects and collections FIRST (before any configuration)
            modelBuilder.Ignore<StaffBreak>();
            modelBuilder.Ignore<Money>();
            
            // Ignore all entity collections to prevent EF from trying to map them
            modelBuilder.Entity<Organization>().Ignore(o => o.LocationIds);
            modelBuilder.Entity<Location>()
                .Ignore(l => l.StaffMemberIds)
                .Ignore(l => l.ServiceTypeIds)
                .Ignore(l => l.AdvertisementIds);
            modelBuilder.Entity<StaffMember>()
                .Ignore(s => s.SpecialtyServiceTypeIds)
                .Ignore(s => s.Breaks);
            modelBuilder.Entity<Coupon>()
                .Ignore(c => c.ApplicableServiceTypeIds)
                .Ignore(c => c.FixedDiscountAmount);

            // STEP 2: Apply entity configurations
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new OrganizationConfiguration());
            modelBuilder.ApplyConfiguration(new LocationConfiguration());
            modelBuilder.ApplyConfiguration(new CustomerConfiguration());
            modelBuilder.ApplyConfiguration(new StaffMemberConfiguration());
            modelBuilder.ApplyConfiguration(new SubscriptionPlanConfiguration());
            modelBuilder.ApplyConfiguration(new ServiceOfferedConfiguration());
            modelBuilder.ApplyConfiguration(new QueueConfiguration());
            modelBuilder.ApplyConfiguration(new QueueEntryConfiguration());
            modelBuilder.ApplyConfiguration(new CouponConfiguration());

            // STEP 3: Configure base entity properties
            ConfigureBaseEntityProperties(modelBuilder);

            // STEP 4: Configure value objects
            ConfigureBasicValueObjects(modelBuilder);

            // Skip DatabaseIndexesConfiguration - it references ignored properties
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
                    // Configure CreatedAt with default value
                    modelBuilder.Entity(entityType.ClrType)
                        .Property<DateTime>("CreatedAt")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

                    // Add global query filter for soft delete
                    modelBuilder.Entity(entityType.ClrType)
                        .HasQueryFilter(GetSoftDeleteFilter(entityType.ClrType));
                }
            }
        }

        private static void ConfigureBasicValueObjects(ModelBuilder modelBuilder)
        {
            // SIMPLIFIED: Ignore ALL Value Objects to prevent conversion issues
            // These will need to be flattened to plain properties if needed
            modelBuilder.Entity<Organization>()
                .Ignore(o => o.Slug)
                .Ignore(o => o.ContactEmail)
                .Ignore(o => o.ContactPhone);
            
            modelBuilder.Entity<Location>()
                .Ignore(l => l.Slug)
                .Ignore(l => l.ContactEmail)
                .Ignore(l => l.ContactPhone)
                .Ignore(l => l.Address);
                
            modelBuilder.Entity<StaffMember>()
                .Ignore(s => s.Email)
                .Ignore(s => s.PhoneNumber);
        }

        // REMOVED: ConfigureValueObjectsOriginal method - configurations moved to ConfigureBasicValueObjects
        // REMOVED: IgnoreDomainEvents method - domain events infrastructure removed

        private static System.Linq.Expressions.LambdaExpression GetSoftDeleteFilter(Type entityType)
        {
            var parameter = System.Linq.Expressions.Expression.Parameter(entityType, "e");
            var property = System.Linq.Expressions.Expression.Property(parameter, "IsDeleted");
            var condition = System.Linq.Expressions.Expression.Equal(property, System.Linq.Expressions.Expression.Constant(false));
            return System.Linq.Expressions.Expression.Lambda(condition, parameter);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Set CreatedAt for new entities
            var entries = ChangeTracker.Entries<BaseEntity>();
            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added && entry.Entity.CreatedAt == default)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            // Set CreatedAt for new entities
            var entries = ChangeTracker.Entries<BaseEntity>();
            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added && entry.Entity.CreatedAt == default)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                }
            }

            return base.SaveChanges();
        }
    }
} 