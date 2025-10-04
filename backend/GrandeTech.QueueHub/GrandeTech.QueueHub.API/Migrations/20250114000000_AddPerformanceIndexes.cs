using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grande.Fila.API.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Queue Indexes
            migrationBuilder.Sql(@"
                CREATE INDEX IX_Queues_LocationId_IsActive_QueueDate 
                ON Queues (LocationId, IsActive, QueueDate) 
                WHERE IsActive = 1;");

            migrationBuilder.Sql(@"
                CREATE INDEX IX_Queues_LocationId_QueueDate 
                ON Queues (LocationId, QueueDate);");

            migrationBuilder.Sql(@"
                CREATE INDEX IX_Queues_LocationId_QueueDate_Desc 
                ON Queues (LocationId, QueueDate DESC);");

            migrationBuilder.Sql(@"
                CREATE INDEX IX_Queues_IsActive 
                ON Queues (IsActive);");

            // Queue Entry Indexes
            migrationBuilder.Sql(@"
                CREATE INDEX IX_QueueEntries_QueueId_Position 
                ON QueueEntries (QueueId, Position);");

            migrationBuilder.Sql(@"
                CREATE INDEX IX_QueueEntries_QueueId_Status_Position 
                ON QueueEntries (QueueId, Status, Position) 
                WHERE Status IN ('Waiting', 'Called', 'CheckedIn');");

            migrationBuilder.Sql(@"
                CREATE INDEX IX_QueueEntries_QueueId_CustomerId 
                ON QueueEntries (QueueId, CustomerId);");

            migrationBuilder.Sql(@"
                CREATE INDEX IX_QueueEntries_CustomerId_EnteredAt 
                ON QueueEntries (CustomerId, EnteredAt);");

            migrationBuilder.Sql(@"
                CREATE INDEX IX_QueueEntries_QueueId_EnteredAt_CompletedAt 
                ON QueueEntries (QueueId, EnteredAt, CompletedAt) 
                WHERE CompletedAt IS NOT NULL;");

            migrationBuilder.Sql(@"
                CREATE INDEX IX_QueueEntries_StaffMemberId_Status 
                ON QueueEntries (StaffMemberId, Status) 
                WHERE StaffMemberId IS NOT NULL;");

            migrationBuilder.Sql(@"
                CREATE INDEX IX_QueueEntries_TokenNumber 
                ON QueueEntries (TokenNumber) 
                WHERE TokenNumber IS NOT NULL;");

            migrationBuilder.Sql(@"
                CREATE INDEX IX_QueueEntries_ServiceTypeId_EnteredAt 
                ON QueueEntries (ServiceTypeId, EnteredAt) 
                WHERE ServiceTypeId IS NOT NULL;");

            // Location Indexes
            migrationBuilder.Sql(@"
                CREATE INDEX IX_Locations_OrganizationId_IsActive 
                ON Locations (OrganizationId, IsActive);");

            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX IX_Locations_Slug_IsActive 
                ON Locations (Slug, IsActive) 
                WHERE IsActive = 1;");

            migrationBuilder.Sql(@"
                CREATE INDEX IX_Locations_City_State_IsActive 
                ON Locations (City, State, IsActive);");

            migrationBuilder.Sql(@"
                CREATE INDEX IX_Locations_IsActive_IsOpen 
                ON Locations (IsActive, IsOpen);");

            migrationBuilder.Sql(@"
                CREATE INDEX IX_Locations_TimeZone 
                ON Locations (TimeZone);");

            // Organization Indexes
            migrationBuilder.Sql(@"
                CREATE INDEX IX_Organizations_IsActive 
                ON Organizations (IsActive);");

            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX IX_Organizations_Slug_IsActive 
                ON Organizations (Slug, IsActive) 
                WHERE IsActive = 1;");

            // Staff Indexes
            migrationBuilder.Sql(@"
                CREATE INDEX IX_StaffMembers_LocationId_IsActive 
                ON StaffMembers (LocationId, IsActive);");

            migrationBuilder.Sql(@"
                CREATE INDEX IX_StaffMembers_LocationId_IsActive_IsOnBreak 
                ON StaffMembers (LocationId, IsActive, IsOnBreak) 
                WHERE IsActive = 1;");

            migrationBuilder.Sql(@"
                CREATE INDEX IX_StaffMembers_LocationId_Role 
                ON StaffMembers (LocationId, Role);");

            // Service Offered Indexes
            migrationBuilder.Sql(@"
                CREATE INDEX IX_ServicesOffered_LocationId_IsActive 
                ON ServicesOffered (LocationId, IsActive);");

            migrationBuilder.Sql(@"
                CREATE INDEX IX_ServicesOffered_LocationId_PriceAmount_IsActive 
                ON ServicesOffered (LocationId, PriceAmount, IsActive) 
                WHERE IsActive = 1 AND PriceAmount IS NOT NULL;");

            migrationBuilder.Sql(@"
                CREATE INDEX IX_ServicesOffered_LocationId_Category_IsActive 
                ON ServicesOffered (LocationId, Category, IsActive) 
                WHERE IsActive = 1;");

            // Customer Indexes
            migrationBuilder.Sql(@"
                CREATE INDEX IX_Customers_UserId_IsAnonymous 
                ON Customers (UserId, IsAnonymous) 
                WHERE UserId IS NOT NULL;");

            migrationBuilder.Sql(@"
                CREATE INDEX IX_Customers_IsAnonymous_CreatedAt 
                ON Customers (IsAnonymous, CreatedAt) 
                WHERE IsAnonymous = 1;");

            migrationBuilder.Sql(@"
                CREATE INDEX IX_Customers_NotificationsEnabled_IsActive 
                ON Customers (NotificationsEnabled, IsActive);");

            migrationBuilder.Sql(@"
                CREATE INDEX IX_Customers_PhoneNumber 
                ON Customers (PhoneNumber) 
                WHERE PhoneNumber IS NOT NULL;");

            // User Indexes
            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX IX_Users_Email_IsActive 
                ON Users (Email, IsActive) 
                WHERE IsActive = 1;");

            migrationBuilder.Sql(@"
                CREATE INDEX IX_Users_FullName_IsActive 
                ON Users (FullName, IsActive) 
                WHERE IsActive = 1;");

            migrationBuilder.Sql(@"
                CREATE INDEX IX_Users_PhoneNumber_IsActive 
                ON Users (PhoneNumber, IsActive) 
                WHERE PhoneNumber IS NOT NULL AND IsActive = 1;");

            migrationBuilder.Sql(@"
                CREATE INDEX IX_Users_Role_IsActive 
                ON Users (Role, IsActive);");

            // Subscription Plan Indexes
            migrationBuilder.Sql(@"
                CREATE INDEX IX_SubscriptionPlans_IsActive_PlanType 
                ON SubscriptionPlans (IsActive, PlanType);");

            migrationBuilder.Sql(@"
                CREATE INDEX IX_SubscriptionPlans_MonthlyPriceAmount_IsActive 
                ON SubscriptionPlans (MonthlyPriceAmount, IsActive) 
                WHERE IsActive = 1;");

            migrationBuilder.Sql(@"
                CREATE INDEX IX_SubscriptionPlans_YearlyPriceAmount_IsActive 
                ON SubscriptionPlans (YearlyPriceAmount, IsActive) 
                WHERE IsActive = 1;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop indexes in reverse order
            // Subscription Plan Indexes
            migrationBuilder.Sql("DROP INDEX IX_SubscriptionPlans_YearlyPriceAmount_IsActive ON SubscriptionPlans;");
            migrationBuilder.Sql("DROP INDEX IX_SubscriptionPlans_MonthlyPriceAmount_IsActive ON SubscriptionPlans;");
            migrationBuilder.Sql("DROP INDEX IX_SubscriptionPlans_IsActive_PlanType ON SubscriptionPlans;");

            // User Indexes
            migrationBuilder.Sql("DROP INDEX IX_Users_Role_IsActive ON Users;");
            migrationBuilder.Sql("DROP INDEX IX_Users_PhoneNumber_IsActive ON Users;");
            migrationBuilder.Sql("DROP INDEX IX_Users_FullName_IsActive ON Users;");
            migrationBuilder.Sql("DROP INDEX IX_Users_Email_IsActive ON Users;");

            // Customer Indexes
            migrationBuilder.Sql("DROP INDEX IX_Customers_PhoneNumber ON Customers;");
            migrationBuilder.Sql("DROP INDEX IX_Customers_NotificationsEnabled_IsActive ON Customers;");
            migrationBuilder.Sql("DROP INDEX IX_Customers_IsAnonymous_CreatedAt ON Customers;");
            migrationBuilder.Sql("DROP INDEX IX_Customers_UserId_IsAnonymous ON Customers;");

            // Service Offered Indexes
            migrationBuilder.Sql("DROP INDEX IX_ServicesOffered_LocationId_Category_IsActive ON ServicesOffered;");
            migrationBuilder.Sql("DROP INDEX IX_ServicesOffered_LocationId_PriceAmount_IsActive ON ServicesOffered;");
            migrationBuilder.Sql("DROP INDEX IX_ServicesOffered_LocationId_IsActive ON ServicesOffered;");

            // Staff Indexes
            migrationBuilder.Sql("DROP INDEX IX_StaffMembers_LocationId_Role ON StaffMembers;");
            migrationBuilder.Sql("DROP INDEX IX_StaffMembers_LocationId_IsActive_IsOnBreak ON StaffMembers;");
            migrationBuilder.Sql("DROP INDEX IX_StaffMembers_LocationId_IsActive ON StaffMembers;");

            // Organization Indexes
            migrationBuilder.Sql("DROP INDEX IX_Organizations_Slug_IsActive ON Organizations;");
            migrationBuilder.Sql("DROP INDEX IX_Organizations_IsActive ON Organizations;");

            // Location Indexes
            migrationBuilder.Sql("DROP INDEX IX_Locations_TimeZone ON Locations;");
            migrationBuilder.Sql("DROP INDEX IX_Locations_IsActive_IsOpen ON Locations;");
            migrationBuilder.Sql("DROP INDEX IX_Locations_City_State_IsActive ON Locations;");
            migrationBuilder.Sql("DROP INDEX IX_Locations_Slug_IsActive ON Locations;");
            migrationBuilder.Sql("DROP INDEX IX_Locations_OrganizationId_IsActive ON Locations;");

            // Queue Entry Indexes
            migrationBuilder.Sql("DROP INDEX IX_QueueEntries_ServiceTypeId_EnteredAt ON QueueEntries;");
            migrationBuilder.Sql("DROP INDEX IX_QueueEntries_TokenNumber ON QueueEntries;");
            migrationBuilder.Sql("DROP INDEX IX_QueueEntries_StaffMemberId_Status ON QueueEntries;");
            migrationBuilder.Sql("DROP INDEX IX_QueueEntries_QueueId_EnteredAt_CompletedAt ON QueueEntries;");
            migrationBuilder.Sql("DROP INDEX IX_QueueEntries_CustomerId_EnteredAt ON QueueEntries;");
            migrationBuilder.Sql("DROP INDEX IX_QueueEntries_QueueId_CustomerId ON QueueEntries;");
            migrationBuilder.Sql("DROP INDEX IX_QueueEntries_QueueId_Status_Position ON QueueEntries;");
            migrationBuilder.Sql("DROP INDEX IX_QueueEntries_QueueId_Position ON QueueEntries;");

            // Queue Indexes
            migrationBuilder.Sql("DROP INDEX IX_Queues_IsActive ON Queues;");
            migrationBuilder.Sql("DROP INDEX IX_Queues_LocationId_QueueDate_Desc ON Queues;");
            migrationBuilder.Sql("DROP INDEX IX_Queues_LocationId_QueueDate ON Queues;");
            migrationBuilder.Sql("DROP INDEX IX_Queues_LocationId_IsActive_QueueDate ON Queues;");
        }
    }
}
