using System;
using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Domain.Advertising;
using GrandeTech.QueueHub.API.Domain.Common;
using GrandeTech.QueueHub.API.Domain.Customers;
using GrandeTech.QueueHub.API.Domain.Notifications;
using GrandeTech.QueueHub.API.Domain.Organizations;
using GrandeTech.QueueHub.API.Domain.Promotions;
using GrandeTech.QueueHub.API.Domain.Queues;
using GrandeTech.QueueHub.API.Domain.ServiceProviders;
using GrandeTech.QueueHub.API.Domain.Services;
using GrandeTech.QueueHub.API.Domain.Staff;
using GrandeTech.QueueHub.API.Domain.Subscriptions;
using Microsoft.EntityFrameworkCore;
using ServiceProviderEntity = GrandeTech.QueueHub.API.Domain.ServiceProviders.ServiceProvider;

namespace GrandeTech.QueueHub.API.Infrastructure.Persistence
{
    /// <summary>
    /// The main database context for the application
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Organizations
        /// </summary>
        public DbSet<Organization> Organizations { get; set; }
          /// <summary>
        /// Service Providers
        /// </summary>
        public DbSet<ServiceProviderEntity> ServiceProviders { get; set; }
        
        /// <summary>
        /// Queues
        /// </summary>
        public DbSet<Queue> Queues { get; set; }
        
        /// <summary>
        /// Queue Entries
        /// </summary>
        public DbSet<QueueEntry> QueueEntries { get; set; }
        
        /// <summary>
        /// Customers
        /// </summary>
        public DbSet<Customer> Customers { get; set; }
        
        /// <summary>
        /// Staff Members
        /// </summary>
        public DbSet<StaffMember> StaffMembers { get; set; }
        
        /// <summary>
        /// Staff Breaks
        /// </summary>
        public DbSet<StaffBreak> StaffBreaks { get; set; }
        
        /// <summary>
        /// Service Types
        /// </summary>
        public DbSet<ServiceType> ServiceTypes { get; set; }
        
        /// <summary>
        /// Subscription Plans
        /// </summary>
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        
        /// <summary>
        /// Advertisements
        /// </summary>
        public DbSet<Advertisement> Advertisements { get; set; }
        
        /// <summary>
        /// Coupons
        /// </summary>
        public DbSet<Coupon> Coupons { get; set; }
        
        /// <summary>
        /// Notifications
        /// </summary>
        public DbSet<Notification> Notifications { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Configure the entity mappings and relationships
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Apply all configurations from this assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
        
        /// <summary>
        /// Override SaveChanges to handle audit fields
        /// </summary>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Update audit fields
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        entry.Entity.LastModifiedAt = DateTime.UtcNow;
                        break;
                    
                    case EntityState.Modified:
                        entry.Entity.LastModifiedAt = DateTime.UtcNow;
                        break;
                }
            }
            
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
