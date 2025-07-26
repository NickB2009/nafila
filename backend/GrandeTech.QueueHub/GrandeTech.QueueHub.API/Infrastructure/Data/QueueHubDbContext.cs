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
using Grande.Fila.API.Infrastructure.Data.Configurations;

namespace Grande.Fila.API.Infrastructure.Data
{
    public class QueueHubDbContext : DbContext
    {
        public QueueHubDbContext(DbContextOptions<QueueHubDbContext> options) : base(options)
        {
        }

        // Aggregate Roots - Starting with known working entities
        public DbSet<User> Users { get; set; }
        public DbSet<Queue> Queues { get; set; }
        public DbSet<QueueEntry> QueueEntries { get; set; }
        
        // Temporarily disabled to isolate Slug value object issues
        // public DbSet<Organization> Organizations { get; set; }  
        // public DbSet<Location> Locations { get; set; }
        // public DbSet<Customer> Customers { get; set; }
        
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

            // Apply only specific entity configurations to avoid auto-discovery of problematic ones
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new QueueConfiguration());
            modelBuilder.ApplyConfiguration(new QueueEntryConfiguration());
            // modelBuilder.ApplyConfiguration(new OrganizationConfiguration());  // Disabled due to Slug issues

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
            // Configure value objects as owned types
            // This will be expanded in entity configurations
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