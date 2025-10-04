using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.Locations;
using Grande.Fila.API.Domain.Organizations;
using Grande.Fila.API.Domain.Staff;
using Grande.Fila.API.Domain.ServicesOffered;
using Grande.Fila.API.Domain.Customers;
using Grande.Fila.API.Domain.Users;
using Grande.Fila.API.Domain.Subscriptions;

namespace Grande.Fila.API.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Database index configuration for performance optimization
    /// This class defines strategic indexes based on query patterns and performance analysis
    /// </summary>
    public static class DatabaseIndexesConfiguration
    {
        /// <summary>
        /// Configure all database indexes for optimal performance
        /// </summary>
        public static void ConfigureIndexes(ModelBuilder modelBuilder)
        {
            ConfigureQueueIndexes(modelBuilder);
            ConfigureQueueEntryIndexes(modelBuilder);
            ConfigureLocationIndexes(modelBuilder);
            ConfigureOrganizationIndexes(modelBuilder);
            ConfigureStaffIndexes(modelBuilder);
            ConfigureServiceOfferedIndexes(modelBuilder);
            ConfigureCustomerIndexes(modelBuilder);
            ConfigureUserIndexes(modelBuilder);
            ConfigureSubscriptionPlanIndexes(modelBuilder);
        }

        #region Queue Indexes

        private static void ConfigureQueueIndexes(ModelBuilder modelBuilder)
        {
            var queueEntity = modelBuilder.Entity<Queue>();

            // Primary query: Get active queue by location (most frequent)
            queueEntity.HasIndex(q => new { q.LocationId, q.IsActive, q.QueueDate })
                .HasDatabaseName("IX_Queues_LocationId_IsActive_QueueDate")
                .HasFilter("IsActive = 1");

            // Analytics queries: Queue counts by date range
            queueEntity.HasIndex(q => new { q.LocationId, q.QueueDate })
                .HasDatabaseName("IX_Queues_LocationId_QueueDate");

            // Queue management: Latest queue per location
            queueEntity.HasIndex(q => new { q.LocationId, q.QueueDate })
                .HasDatabaseName("IX_Queues_LocationId_QueueDate_Desc")
                .IsDescending(false, true); // LocationId ASC, QueueDate DESC

            // Queue status queries
            queueEntity.HasIndex(q => q.IsActive)
                .HasDatabaseName("IX_Queues_IsActive");
        }

        #endregion

        #region Queue Entry Indexes

        private static void ConfigureQueueEntryIndexes(ModelBuilder modelBuilder)
        {
            var queueEntryEntity = modelBuilder.Entity<QueueEntry>();

            // Primary query: Get entries by queue (most frequent)
            queueEntryEntity.HasIndex(qe => new { qe.QueueId, qe.Position })
                .HasDatabaseName("IX_QueueEntries_QueueId_Position");

            // Status filtering: Active entries only
            queueEntryEntity.HasIndex(qe => new { qe.QueueId, qe.Status, qe.Position })
                .HasDatabaseName("IX_QueueEntries_QueueId_Status_Position")
                .HasFilter("Status IN ('Waiting', 'Called', 'CheckedIn')");

            // Customer queries: Find customer in queue
            queueEntryEntity.HasIndex(qe => new { qe.QueueId, qe.CustomerId })
                .HasDatabaseName("IX_QueueEntries_QueueId_CustomerId");

            // Analytics: Customer history
            queueEntryEntity.HasIndex(qe => new { qe.CustomerId, qe.EnteredAt })
                .HasDatabaseName("IX_QueueEntries_CustomerId_EnteredAt");

            // Analytics: Wait time calculations
            queueEntryEntity.HasIndex(qe => new { qe.QueueId, qe.EnteredAt, qe.CompletedAt })
                .HasDatabaseName("IX_QueueEntries_QueueId_EnteredAt_CompletedAt")
                .HasFilter("CompletedAt IS NOT NULL");

            // Staff assignments
            queueEntryEntity.HasIndex(qe => new { qe.StaffMemberId, qe.Status })
                .HasDatabaseName("IX_QueueEntries_StaffMemberId_Status")
                .HasFilter("StaffMemberId IS NOT NULL");

            // Token number lookups (for kiosk)
            queueEntryEntity.HasIndex(qe => qe.TokenNumber)
                .HasDatabaseName("IX_QueueEntries_TokenNumber")
                .HasFilter("TokenNumber IS NOT NULL");

            // Service type analytics
            queueEntryEntity.HasIndex(qe => new { qe.ServiceTypeId, qe.EnteredAt })
                .HasDatabaseName("IX_QueueEntries_ServiceTypeId_EnteredAt")
                .HasFilter("ServiceTypeId IS NOT NULL");
        }

        #endregion

        #region Location Indexes

        private static void ConfigureLocationIndexes(ModelBuilder modelBuilder)
        {
            var locationEntity = modelBuilder.Entity<Location>();

            // Primary query: Get location by organization
            locationEntity.HasIndex(l => new { l.OrganizationId, l.IsActive })
                .HasDatabaseName("IX_Locations_OrganizationId_IsActive");

            // Slug-based lookups (for public APIs)
            locationEntity.HasIndex(l => new { l.Slug, l.IsActive })
                .HasDatabaseName("IX_Locations_Slug_IsActive")
                .IsUnique()
                .HasFilter("IsActive = 1");

            // Business hours queries
            locationEntity.HasIndex(l => new { l.IsActive, l.IsQueueEnabled })
                .HasDatabaseName("IX_Locations_IsActive_IsQueueEnabled");
        }

        #endregion

        #region Organization Indexes

        private static void ConfigureOrganizationIndexes(ModelBuilder modelBuilder)
        {
            var organizationEntity = modelBuilder.Entity<Organization>();

            // Active organizations
            organizationEntity.HasIndex(o => o.IsActive)
                .HasDatabaseName("IX_Organizations_IsActive");

            // Organization slug lookups
            organizationEntity.HasIndex(o => new { o.Slug, o.IsActive })
                .HasDatabaseName("IX_Organizations_Slug_IsActive")
                .IsUnique()
                .HasFilter("IsActive = 1");
        }

        #endregion

        #region Staff Indexes

        private static void ConfigureStaffIndexes(ModelBuilder modelBuilder)
        {
            var staffEntity = modelBuilder.Entity<StaffMember>();

            // Primary query: Get staff by location
            staffEntity.HasIndex(s => new { s.LocationId, s.IsActive })
                .HasDatabaseName("IX_StaffMembers_LocationId_IsActive")
                .HasFilter("IsActive = 1");

            // Staff performance analytics
            staffEntity.HasIndex(s => new { s.LocationId, s.Role })
                .HasDatabaseName("IX_StaffMembers_LocationId_Role");
        }

        #endregion

        #region Service Offered Indexes

        private static void ConfigureServiceOfferedIndexes(ModelBuilder modelBuilder)
        {
            var serviceEntity = modelBuilder.Entity<ServiceOffered>();

            // Primary query: Get services by location
            serviceEntity.HasIndex(s => new { s.LocationId, s.IsActive })
                .HasDatabaseName("IX_ServicesOffered_LocationId_IsActive");

            // Price range queries
            serviceEntity.HasIndex(s => new { s.LocationId, s.PriceAmount, s.IsActive })
                .HasDatabaseName("IX_ServicesOffered_LocationId_PriceAmount_IsActive")
                .HasFilter("IsActive = 1 AND PriceAmount IS NOT NULL");

            // Service name queries
            serviceEntity.HasIndex(s => new { s.LocationId, s.Name, s.IsActive })
                .HasDatabaseName("IX_ServicesOffered_LocationId_Name_IsActive")
                .HasFilter("IsActive = 1");
        }

        #endregion

        #region Customer Indexes

        private static void ConfigureCustomerIndexes(ModelBuilder modelBuilder)
        {
            var customerEntity = modelBuilder.Entity<Customer>();

            // User-based lookups
            customerEntity.HasIndex(c => new { c.UserId, c.IsAnonymous })
                .HasDatabaseName("IX_Customers_UserId_IsAnonymous")
                .HasFilter("UserId IS NOT NULL");

            // Anonymous customer queries
            customerEntity.HasIndex(c => new { c.IsAnonymous, c.CreatedAt })
                .HasDatabaseName("IX_Customers_IsAnonymous_CreatedAt")
                .HasFilter("IsAnonymous = 1");

            // Notification preferences
            customerEntity.HasIndex(c => new { c.NotificationsEnabled, c.IsAnonymous })
                .HasDatabaseName("IX_Customers_NotificationsEnabled_IsAnonymous");

            // Phone number lookups (for anonymous customers)
            customerEntity.HasIndex(c => c.PhoneNumber)
                .HasDatabaseName("IX_Customers_PhoneNumber")
                .HasFilter("PhoneNumber IS NOT NULL");
        }

        #endregion

        #region User Indexes

        private static void ConfigureUserIndexes(ModelBuilder modelBuilder)
        {
            var userEntity = modelBuilder.Entity<User>();

            // Authentication queries
            userEntity.HasIndex(u => new { u.Email, u.IsActive })
                .HasDatabaseName("IX_Users_Email_IsActive")
                .IsUnique()
                .HasFilter("IsActive = 1");

            userEntity.HasIndex(u => new { u.FullName, u.IsActive })
                .HasDatabaseName("IX_Users_FullName_IsActive")
                .HasFilter("IsActive = 1");

            // Phone-based authentication
            userEntity.HasIndex(u => new { u.PhoneNumber, u.IsActive })
                .HasDatabaseName("IX_Users_PhoneNumber_IsActive")
                .HasFilter("PhoneNumber IS NOT NULL AND IsActive = 1");

            // Role-based queries
            userEntity.HasIndex(u => new { u.Role, u.IsActive })
                .HasDatabaseName("IX_Users_Role_IsActive");
        }

        #endregion

        #region Subscription Plan Indexes

        private static void ConfigureSubscriptionPlanIndexes(ModelBuilder modelBuilder)
        {
            var subscriptionEntity = modelBuilder.Entity<SubscriptionPlan>();

            // Active subscription plans
            subscriptionEntity.HasIndex(s => new { s.IsActive, s.Name })
                .HasDatabaseName("IX_SubscriptionPlans_IsActive_Name");

            // Price-based queries
            subscriptionEntity.HasIndex(s => new { s.MonthlyPriceAmount, s.IsActive })
                .HasDatabaseName("IX_SubscriptionPlans_MonthlyPriceAmount_IsActive")
                .HasFilter("IsActive = 1");

            subscriptionEntity.HasIndex(s => new { s.YearlyPriceAmount, s.IsActive })
                .HasDatabaseName("IX_SubscriptionPlans_YearlyPriceAmount_IsActive")
                .HasFilter("IsActive = 1");
        }

        #endregion
    }
}