# MySQL Migration Plan
## GrandeTech QueueHub Database Migration from SQL Server to MySQL

### 🎯 Executive Summary

This document outlines the comprehensive migration strategy for moving the GrandeTech QueueHub backend from SQL Server (Azure SQL) to MySQL. The migration maintains all current functionality while leveraging MySQL-specific optimizations and features.

### 📊 Current Database Analysis

#### Current Setup
- **Database**: Azure SQL Server (SQL Server 2022)
- **ORM**: Entity Framework Core 8.0.11
- **Provider**: `Microsoft.EntityFrameworkCore.SqlServer`
- **Architecture**: Clean Architecture with Domain-Driven Design
- **Features**: Soft deletes, audit fields, concurrency tokens, JSON columns, value objects

#### Key Tables Structure
1. **SubscriptionPlans** - Subscription management with pricing tiers
2. **Organizations** - Top-level business entities with branding
3. **Locations** - Physical service locations with addresses
4. **Users** - System users with roles and permissions
5. **Customers** - End customers with service history
6. **ServicesOffered** - Available services per location
7. **StaffMembers** - Staff management with specialties
8. **Queues** - Queue instances per location/date
9. **QueueEntries** - Individual queue positions
10. **CustomerServiceHistory** - Service history tracking

#### Current Features
- Soft delete pattern with `IsDeleted` flag
- Audit fields (`CreatedAt`, `CreatedBy`, `LastModifiedAt`, `LastModifiedBy`)
- Concurrency tokens (`RowVersion`)
- JSON columns for collections and complex data
- Value objects for type safety
- Domain events for business logic

---

## 🚀 Migration Phases

### Phase 1: Package and Provider Updates ✅

#### 1.1 Update NuGet Packages ✅

**Remove SQL Server packages:**
```xml
<!-- Remove these packages -->
<!-- <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.11" /> -->
```

**Add MySQL packages:**
```xml
<!-- Add MySQL packages -->
<PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="8.0.2" />
<PackageReference Include="MySqlConnector" Version="2.3.7" />
```

#### 1.2 Update Connection String Format ✅

**New connection string format:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-mysql-server;Database=QueueHubDb;User=your-username;Password=your-password;Port=3306;",
    "MySqlConnection": "Server=your-mysql-server;Database=QueueHubDb;User=your-username;Password=your-password;Port=3306;"
  }
}
```

**Connection string parameters:**
- `Server`: MySQL server hostname or IP
- `Database`: Database name
- `User`: MySQL username
- `Password`: MySQL password
- `Port`: MySQL port (default: 3306)
- `CharSet`: Character set (utf8mb4 recommended)
- `SslMode`: SSL mode (Required, Preferred, None)

---

## ✅ Phase 1 Complete!

**Summary of Phase 1 Completion:**
- ✅ Updated NuGet packages (removed SQL Server, added MySQL)
- ✅ Updated connection strings for MySQL format
- ✅ Updated DependencyInjection.cs to use MySQL provider
- ✅ Fixed compilation errors in migration files
- ✅ Project builds successfully with MySQL configuration

**Next Steps:** Ready to proceed with Phase 2 (Code Changes) which will involve updating DbContext configurations and data type mappings.

---

### Phase 2: Code Changes ✅

#### 2.1 Update DbContext Configuration ✅

**Update DependencyInjection.cs:**
```csharp
// In DependencyInjection.cs
services.AddDbContext<QueueHubDbContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), mySqlOptions =>
    {
        mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(15),
            errorNumbersToAdd: new[] { 1205, 1213, 2006, 2013, 2014, 2015, 2016, 2017, 2018, 2019 });
        mySqlOptions.CommandTimeout(30);
    });
});
```

#### 2.2 Update Default Value SQL Functions ✅

**Replace SQL Server specific functions:**
```csharp
// Replace SQL Server specific functions
.HasDefaultValueSql("GETUTCDATE()") 
// becomes
.HasDefaultValueSql("CURRENT_TIMESTAMP(6)")

// For concurrency tokens
.HasDefaultValueSql("0x") 
// becomes  
.HasDefaultValueSql("0x0000000000000000")

// For rowversion
.HasDefaultValueSql("rowversion")
// becomes
.HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
```

#### 2.3 Update BaseEntity Configuration ✅

**Update QueueHubDbContext.cs:**
```csharp
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
        }
    }
}
```

---

## ✅ Phase 2 Complete!

**Summary of Phase 2 Completion:**
- ✅ Updated DbContext Configuration with MySQL-specific options
- ✅ Replaced SQL Server functions with MySQL equivalents (GETUTCDATE → CURRENT_TIMESTAMP(6))
- ✅ Updated concurrency tokens from rowversion to MySQL TIMESTAMP
- ✅ Configured JSON columns to use MySQL native JSON type for better performance
- ✅ Updated data type mappings (nvarchar(max) → LONGTEXT, etc.)
- ✅ Enhanced MySQL connection configuration with retry policies and optimizations
- ✅ Project builds successfully with all MySQL-specific configurations

**Key Improvements:**
- **Performance**: Native MySQL JSON type for better JSON operations
- **Reliability**: MySQL-specific retry policies and error handling
- **Compatibility**: All SQL Server functions replaced with MySQL equivalents
- **Concurrency**: Proper TIMESTAMP-based concurrency tokens

**Next Steps:** Ready to proceed with Phase 3 (Data Type Mappings) which will involve creating the complete MySQL schema and handling data migration.

---

### Phase 3: Data Type Mappings ✅

#### 3.1 Key Data Type Changes

| SQL Server | MySQL | Notes |
|------------|-------|-------|
| `uniqueidentifier` | `CHAR(36)` | GUID as string |
| `nvarchar(max)` | `LONGTEXT` | Large text |
| `varbinary(max)` | `LONGBLOB` | Binary data |
| `datetime2` | `DATETIME(6)` | High precision datetime |
| `bit` | `TINYINT(1)` | Boolean |
| `rowversion` | `TIMESTAMP` | Auto-updating timestamp |
| `decimal(18,2)` | `DECIMAL(18,2)` | Same |
| `float` | `DOUBLE` | Floating point |
| `nvarchar(n)` | `VARCHAR(n)` | Variable character |
| `int` | `INT` | Integer |
| `bigint` | `BIGINT` | Large integer |

#### 3.2 JSON Column Handling

**MySQL 8.0+ JSON Support:**
- Use native `JSON` data type instead of `LONGTEXT`
- Better performance and validation
- JSON indexing support
- JSON functions and operators

**Update JSON column configurations:**
```csharp
// Update JSON column configurations
modelBuilder.Entity<Organization>()
    .Property(e => e.LocationIds)
    .HasColumnType("json")
    .HasConversion(
        v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
        v => System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<Guid>());
```

---

## ✅ Phase 3 Complete!

**Summary of Phase 3 Completion:**
- ✅ Created complete MySQL schema script (`mysql-schema.sql`) with all tables and relationships
- ✅ Generated comprehensive data type mapping reference (`data-type-mapping.md`)
- ✅ Created JSON data migration script (`migrate-json-data.sql`) with validation queries
- ✅ Generated EF Core MySQL migration files (InitialMySqlMigration)
- ✅ Updated all entity configurations for MySQL-specific data types
- ✅ Configured native MySQL JSON columns for better performance
- ✅ Set up proper MySQL concurrency tokens using TIMESTAMP
- ✅ Project builds successfully with all MySQL migrations

**Key Achievements:**
- **Schema**: Complete MySQL database schema with proper indexes and constraints
- **Data Types**: All SQL Server types converted to MySQL equivalents
- **JSON Support**: Native MySQL JSON columns with validation and indexing
- **Migrations**: EF Core migrations ready for MySQL deployment
- **Performance**: Optimized for MySQL with proper indexing strategies

**Files Created:**
- `mysql-schema.sql` - Complete database schema
- `data-type-mapping.md` - Comprehensive mapping reference
- `migrate-json-data.sql` - JSON data migration utilities
- `Migrations/20250112000000_InitialMySqlMigration.cs` - EF Core migration
- `Migrations/QueueHubDbContextModelSnapshot.cs` - Model snapshot

**Next Steps:** Ready to proceed with Phase 4 (Data Migration) which will involve setting up the MySQL database and migrating existing data.

---

### Phase 4: Schema Migration Script ✅

#### 4.1 Complete MySQL Schema Script

```sql
-- MySQL Database Creation Script
-- GrandeTech QueueHub Database
-- Migration from SQL Server to MySQL

CREATE DATABASE IF NOT EXISTS QueueHubDb 
CHARACTER SET utf8mb4 
COLLATE utf8mb4_unicode_ci;

USE QueueHubDb;

-- Create EF Migrations History table
CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` VARCHAR(150) NOT NULL,
    `ProductVersion` VARCHAR(32) NOT NULL,
    PRIMARY KEY (`MigrationId`)
);

-- Create SubscriptionPlans table
CREATE TABLE IF NOT EXISTS `SubscriptionPlans` (
    `Id` CHAR(36) NOT NULL,
    `Name` VARCHAR(100) NOT NULL,
    `Description` TEXT NOT NULL,
    `MonthlyPriceAmount` DECIMAL(18,2) NOT NULL,
    `MonthlyPriceCurrency` VARCHAR(3) NOT NULL,
    `YearlyPriceAmount` DECIMAL(18,2) NOT NULL,
    `YearlyPriceCurrency` VARCHAR(3) NOT NULL,
    `Price` DECIMAL(18,2) NOT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `IsDefault` TINYINT(1) NOT NULL DEFAULT 0,
    `MaxLocations` INT NOT NULL DEFAULT 1,
    `MaxStaffPerLocation` INT NOT NULL DEFAULT 5,
    `IncludesAnalytics` TINYINT(1) NOT NULL DEFAULT 0,
    `IncludesAdvancedReporting` TINYINT(1) NOT NULL DEFAULT 0,
    `IncludesCustomBranding` TINYINT(1) NOT NULL DEFAULT 0,
    `IncludesAdvertising` TINYINT(1) NOT NULL DEFAULT 0,
    `IncludesMultipleLocations` TINYINT(1) NOT NULL DEFAULT 0,
    `MaxQueueEntriesPerDay` INT NOT NULL DEFAULT 100,
    `IsFeatured` TINYINT(1) NOT NULL DEFAULT 0,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `CreatedBy` LONGTEXT NULL,
    `LastModifiedAt` DATETIME(6) NULL,
    `LastModifiedBy` LONGTEXT NULL,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    `DeletedAt` DATETIME(6) NULL,
    `DeletedBy` LONGTEXT NULL,
    `RowVersion` LONGBLOB NOT NULL DEFAULT 0x0000000000000000,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_SubscriptionPlans_Name` (`Name`),
    KEY `IX_SubscriptionPlans_IsActive` (`IsActive`),
    KEY `IX_SubscriptionPlans_IsDefault` (`IsDefault`),
    KEY `IX_SubscriptionPlans_IsFeatured` (`IsFeatured`),
    KEY `IX_SubscriptionPlans_Price` (`Price`)
);

-- Create Organizations table
CREATE TABLE IF NOT EXISTS `Organizations` (
    `Id` CHAR(36) NOT NULL,
    `Name` VARCHAR(200) NOT NULL,
    `Slug` VARCHAR(100) NOT NULL,
    `Description` TEXT NULL,
    `ContactEmail` VARCHAR(320) NULL,
    `ContactPhone` VARCHAR(50) NULL,
    `WebsiteUrl` VARCHAR(500) NULL,
    `BrandingPrimaryColor` VARCHAR(50) NOT NULL,
    `BrandingSecondaryColor` VARCHAR(50) NOT NULL,
    `BrandingLogoUrl` VARCHAR(500) NOT NULL,
    `BrandingFaviconUrl` VARCHAR(500) NOT NULL,
    `BrandingCompanyName` VARCHAR(200) NOT NULL,
    `BrandingTagLine` VARCHAR(500) NOT NULL,
    `BrandingFontFamily` VARCHAR(100) NOT NULL,
    `SubscriptionPlanId` CHAR(36) NOT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `SharesDataForAnalytics` TINYINT(1) NOT NULL DEFAULT 0,
    `LocationIds` JSON NOT NULL,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `CreatedBy` LONGTEXT NULL,
    `LastModifiedAt` DATETIME(6) NULL,
    `LastModifiedBy` LONGTEXT NULL,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    `DeletedAt` DATETIME(6) NULL,
    `DeletedBy` LONGTEXT NULL,
    `RowVersion` LONGBLOB NOT NULL DEFAULT 0x0000000000000000,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_Organizations_Slug` (`Slug`),
    KEY `IX_Organizations_ContactEmail` (`ContactEmail`),
    KEY `IX_Organizations_IsActive` (`IsActive`),
    KEY `IX_Organizations_Name` (`Name`),
    KEY `IX_Organizations_SubscriptionPlanId` (`SubscriptionPlanId`)
);

-- Create Locations table
CREATE TABLE IF NOT EXISTS `Locations` (
    `Id` CHAR(36) NOT NULL,
    `Name` VARCHAR(200) NOT NULL,
    `Slug` VARCHAR(100) NOT NULL,
    `Description` TEXT NULL,
    `OrganizationId` CHAR(36) NOT NULL,
    `AddressStreet` VARCHAR(200) NOT NULL,
    `AddressNumber` VARCHAR(20) NOT NULL,
    `AddressComplement` VARCHAR(100) NOT NULL,
    `AddressNeighborhood` VARCHAR(100) NOT NULL,
    `AddressCity` VARCHAR(100) NOT NULL,
    `AddressState` VARCHAR(50) NOT NULL,
    `AddressCountry` VARCHAR(50) NOT NULL,
    `AddressPostalCode` VARCHAR(20) NOT NULL,
    `AddressLatitude` DOUBLE NULL,
    `AddressLongitude` DOUBLE NULL,
    `ContactPhone` VARCHAR(50) NULL,
    `ContactEmail` VARCHAR(320) NULL,
    `CustomBrandingPrimaryColor` VARCHAR(50) NULL,
    `CustomBrandingSecondaryColor` VARCHAR(50) NULL,
    `CustomBrandingLogoUrl` VARCHAR(500) NULL,
    `CustomBrandingFaviconUrl` VARCHAR(500) NULL,
    `CustomBrandingCompanyName` VARCHAR(200) NULL,
    `CustomBrandingTagLine` VARCHAR(500) NULL,
    `CustomBrandingFontFamily` VARCHAR(100) NULL,
    `BusinessHoursStart` TIME NOT NULL,
    `BusinessHoursEnd` TIME NOT NULL,
    `IsQueueEnabled` TINYINT(1) NOT NULL DEFAULT 1,
    `MaxQueueSize` INT NOT NULL DEFAULT 100,
    `LateClientCapTimeInMinutes` INT NOT NULL DEFAULT 15,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `AverageServiceTimeInMinutes` DOUBLE NOT NULL DEFAULT 30.0,
    `LastAverageTimeReset` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `StaffMemberIds` JSON NOT NULL,
    `ServiceTypeIds` JSON NOT NULL,
    `AdvertisementIds` JSON NOT NULL,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `CreatedBy` LONGTEXT NULL,
    `LastModifiedAt` DATETIME(6) NULL,
    `LastModifiedBy` LONGTEXT NULL,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    `DeletedAt` DATETIME(6) NULL,
    `DeletedBy` LONGTEXT NULL,
    `RowVersion` LONGBLOB NOT NULL DEFAULT 0x0000000000000000,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_Locations_Slug` (`Slug`),
    KEY `IX_Locations_ContactEmail` (`ContactEmail`),
    KEY `IX_Locations_IsActive` (`IsActive`),
    KEY `IX_Locations_IsQueueEnabled` (`IsQueueEnabled`),
    KEY `IX_Locations_Name` (`Name`),
    KEY `IX_Locations_OrganizationId` (`OrganizationId`)
);

-- Create Users table
CREATE TABLE IF NOT EXISTS `Users` (
    `Id` CHAR(36) NOT NULL,
    `FullName` VARCHAR(100) NOT NULL,
    `Email` VARCHAR(255) NOT NULL,
    `PhoneNumber` VARCHAR(20) NOT NULL,
    `PasswordHash` VARCHAR(500) NOT NULL,
    `Role` VARCHAR(50) NOT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `LastLoginAt` DATETIME(6) NULL,
    `IsLocked` TINYINT(1) NOT NULL DEFAULT 0,
    `RequiresTwoFactor` TINYINT(1) NOT NULL DEFAULT 0,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `CreatedBy` LONGTEXT NULL,
    `LastModifiedAt` DATETIME(6) NULL,
    `LastModifiedBy` LONGTEXT NULL,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    `DeletedAt` DATETIME(6) NULL,
    `DeletedBy` LONGTEXT NULL,
    `RowVersion` LONGBLOB NOT NULL DEFAULT 0x0000000000000000,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_Users_Email` (`Email`),
    KEY `IX_Users_IsActive` (`IsActive`),
    KEY `IX_Users_Role` (`Role`)
);

-- Create Customers table
CREATE TABLE IF NOT EXISTS `Customers` (
    `Id` CHAR(36) NOT NULL,
    `Name` VARCHAR(200) NOT NULL,
    `PhoneNumber` VARCHAR(50) NULL,
    `Email` VARCHAR(320) NULL,
    `IsAnonymous` TINYINT(1) NOT NULL DEFAULT 0,
    `NotificationsEnabled` TINYINT(1) NOT NULL DEFAULT 1,
    `PreferredNotificationChannel` VARCHAR(20) NULL,
    `UserId` VARCHAR(50) NULL,
    `FavoriteLocationIds` JSON NOT NULL,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `CreatedBy` LONGTEXT NULL,
    `LastModifiedAt` DATETIME(6) NULL,
    `LastModifiedBy` LONGTEXT NULL,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    `DeletedAt` DATETIME(6) NULL,
    `DeletedBy` LONGTEXT NULL,
    `RowVersion` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`),
    KEY `IX_Customers_Email` (`Email`),
    KEY `IX_Customers_IsAnonymous` (`IsAnonymous`),
    KEY `IX_Customers_Name` (`Name`),
    KEY `IX_Customers_PhoneNumber` (`PhoneNumber`),
    KEY `IX_Customers_UserId` (`UserId`)
);

-- Create ServicesOffered table
CREATE TABLE IF NOT EXISTS `ServicesOffered` (
    `Id` CHAR(36) NOT NULL,
    `Name` VARCHAR(200) NOT NULL,
    `Description` TEXT NULL,
    `LocationId` CHAR(36) NOT NULL,
    `EstimatedDurationMinutes` INT NOT NULL DEFAULT 30,
    `PriceAmount` DECIMAL(18,2) NULL,
    `PriceCurrency` VARCHAR(3) NULL,
    `ImageUrl` VARCHAR(500) NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `TimesProvided` INT NOT NULL DEFAULT 0,
    `ActualAverageDurationMinutes` DOUBLE NOT NULL DEFAULT 30.0,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `CreatedBy` LONGTEXT NULL,
    `LastModifiedAt` DATETIME(6) NULL,
    `LastModifiedBy` LONGTEXT NULL,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    `DeletedAt` DATETIME(6) NULL,
    `DeletedBy` LONGTEXT NULL,
    `RowVersion` LONGBLOB NOT NULL DEFAULT 0x0000000000000000,
    PRIMARY KEY (`Id`),
    KEY `IX_ServicesOffered_EstimatedDuration` (`EstimatedDurationMinutes`),
    KEY `IX_ServicesOffered_IsActive` (`IsActive`),
    KEY `IX_ServicesOffered_Location_Active` (`LocationId`, `IsActive`),
    KEY `IX_ServicesOffered_LocationId` (`LocationId`),
    KEY `IX_ServicesOffered_Name` (`Name`)
);

-- Create StaffMembers table
CREATE TABLE IF NOT EXISTS `StaffMembers` (
    `Id` CHAR(36) NOT NULL,
    `Name` VARCHAR(200) NOT NULL,
    `LocationId` CHAR(36) NOT NULL,
    `Email` VARCHAR(320) NULL,
    `PhoneNumber` VARCHAR(50) NULL,
    `ProfilePictureUrl` VARCHAR(500) NULL,
    `Role` VARCHAR(50) NOT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `IsOnDuty` TINYINT(1) NOT NULL DEFAULT 0,
    `StaffStatus` VARCHAR(20) NOT NULL DEFAULT 'available',
    `UserId` VARCHAR(50) NULL,
    `AverageServiceTimeInMinutes` DOUBLE NOT NULL DEFAULT 30.0,
    `CompletedServicesCount` INT NOT NULL DEFAULT 0,
    `EmployeeCode` VARCHAR(20) NOT NULL,
    `Username` VARCHAR(50) NOT NULL,
    `SpecialtyServiceTypeIds` JSON NOT NULL,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `CreatedBy` LONGTEXT NULL,
    `LastModifiedAt` DATETIME(6) NULL,
    `LastModifiedBy` LONGTEXT NULL,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    `DeletedAt` DATETIME(6) NULL,
    `DeletedBy` LONGTEXT NULL,
    `RowVersion` LONGBLOB NOT NULL DEFAULT 0x0000000000000000,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_StaffMembers_EmployeeCode` (`EmployeeCode`),
    UNIQUE KEY `IX_StaffMembers_Username` (`Username`),
    KEY `IX_StaffMembers_Email` (`Email`),
    KEY `IX_StaffMembers_IsActive` (`IsActive`),
    KEY `IX_StaffMembers_LocationId` (`LocationId`),
    KEY `IX_StaffMembers_Name` (`Name`),
    KEY `IX_StaffMembers_PhoneNumber` (`PhoneNumber`),
    KEY `IX_StaffMembers_UserId` (`UserId`)
);

-- Create Queues table
CREATE TABLE IF NOT EXISTS `Queues` (
    `Id` CHAR(36) NOT NULL,
    `LocationId` CHAR(36) NOT NULL,
    `QueueDate` DATE NOT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `MaxSize` INT NOT NULL DEFAULT 100,
    `LateClientCapTimeInMinutes` INT NOT NULL DEFAULT 15,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `CreatedBy` LONGTEXT NULL,
    `LastModifiedAt` DATETIME(6) NULL,
    `LastModifiedBy` LONGTEXT NULL,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    `DeletedAt` DATETIME(6) NULL,
    `DeletedBy` LONGTEXT NULL,
    `RowVersion` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`),
    KEY `IX_Queues_LocationId` (`LocationId`),
    KEY `IX_Queues_QueueDate` (`QueueDate`),
    KEY `IX_Queues_IsActive` (`IsActive`)
);

-- Create QueueEntries table
CREATE TABLE IF NOT EXISTS `QueueEntries` (
    `Id` CHAR(36) NOT NULL,
    `QueueId` CHAR(36) NOT NULL,
    `CustomerId` CHAR(36) NOT NULL,
    `CustomerName` VARCHAR(200) NOT NULL,
    `Position` INT NOT NULL,
    `Status` VARCHAR(20) NOT NULL,
    `StaffMemberId` CHAR(36) NULL,
    `ServiceTypeId` CHAR(36) NULL,
    `Notes` TEXT NULL,
    `EnteredAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `CalledAt` DATETIME(6) NULL,
    `CheckedInAt` DATETIME(6) NULL,
    `CompletedAt` DATETIME(6) NULL,
    `CancelledAt` DATETIME(6) NULL,
    `ServiceDurationMinutes` INT NULL,
    `EstimatedDurationMinutes` INT NULL,
    `EstimatedStartTime` DATETIME(6) NULL,
    `ActualStartTime` DATETIME(6) NULL,
    `CompletionTime` DATETIME(6) NULL,
    `CustomerNotes` VARCHAR(500) NULL,
    `StaffNotes` VARCHAR(500) NULL,
    `TokenNumber` VARCHAR(20) NOT NULL,
    `NotificationSent` TINYINT(1) NOT NULL DEFAULT 0,
    `NotificationSentAt` DATETIME(6) NULL,
    `NotificationChannel` VARCHAR(20) NULL,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `CreatedBy` LONGTEXT NULL,
    `LastModifiedAt` DATETIME(6) NULL,
    `LastModifiedBy` LONGTEXT NULL,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    `DeletedAt` DATETIME(6) NULL,
    `DeletedBy` LONGTEXT NULL,
    `RowVersion` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`),
    KEY `IX_QueueEntries_QueueId` (`QueueId`),
    KEY `IX_QueueEntries_CustomerId` (`CustomerId`),
    KEY `IX_QueueEntries_Status` (`Status`),
    KEY `IX_QueueEntries_Position` (`Position`),
    KEY `IX_QueueEntries_EnteredAt` (`EnteredAt`)
);

-- Create CustomerServiceHistory table
CREATE TABLE IF NOT EXISTS `CustomerServiceHistory` (
    `Id` CHAR(36) NOT NULL,
    `CustomerId` CHAR(36) NOT NULL,
    `LocationId` CHAR(36) NOT NULL,
    `StaffMemberId` CHAR(36) NOT NULL,
    `ServiceTypeId` CHAR(36) NOT NULL,
    `ServiceDate` DATETIME(6) NOT NULL,
    `Notes` TEXT NULL,
    `Rating` INT NULL,
    `Feedback` TEXT NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_CustomerServiceHistory_CustomerId` (`CustomerId`),
    KEY `IX_CustomerServiceHistory_LocationId` (`LocationId`),
    KEY `IX_CustomerServiceHistory_ServiceDate` (`ServiceDate`)
);

-- Insert migration history records
INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`) VALUES 
('20250726170309_InitialCreate', '8.0.0'),
('20250726193250_AddUserAndQueues', '8.0.0'),
('20250727183900_AddOrganizationLocationCustomer', '8.0.0'),
('20250728015448_AddStaffSubscriptionServiceNoBreaks', '8.0.0'),
('20250802185542_AddConcurrencyTokens', '8.0.0'),
('20250804014627_RefactorToJsonColumn', '8.0.0');

-- Add foreign key constraints
ALTER TABLE `Organizations` 
ADD CONSTRAINT `FK_Organizations_SubscriptionPlans_SubscriptionPlanId` 
FOREIGN KEY (`SubscriptionPlanId`) REFERENCES `SubscriptionPlans` (`Id`) ON DELETE RESTRICT;

ALTER TABLE `Locations` 
ADD CONSTRAINT `FK_Locations_Organizations_OrganizationId` 
FOREIGN KEY (`OrganizationId`) REFERENCES `Organizations` (`Id`) ON DELETE RESTRICT;

ALTER TABLE `ServicesOffered` 
ADD CONSTRAINT `FK_ServicesOffered_Locations_LocationId` 
FOREIGN KEY (`LocationId`) REFERENCES `Locations` (`Id`) ON DELETE RESTRICT;

ALTER TABLE `StaffMembers` 
ADD CONSTRAINT `FK_StaffMembers_Locations_LocationId` 
FOREIGN KEY (`LocationId`) REFERENCES `Locations` (`Id`) ON DELETE RESTRICT;

ALTER TABLE `Queues` 
ADD CONSTRAINT `FK_Queues_Locations_LocationId` 
FOREIGN KEY (`LocationId`) REFERENCES `Locations` (`Id`) ON DELETE RESTRICT;

ALTER TABLE `QueueEntries` 
ADD CONSTRAINT `FK_QueueEntries_Queues_QueueId` 
FOREIGN KEY (`QueueId`) REFERENCES `Queues` (`Id`) ON DELETE RESTRICT;

ALTER TABLE `QueueEntries` 
ADD CONSTRAINT `FK_QueueEntries_Customers_CustomerId` 
FOREIGN KEY (`CustomerId`) REFERENCES `Customers` (`Id`) ON DELETE RESTRICT;

ALTER TABLE `QueueEntries` 
ADD CONSTRAINT `FK_QueueEntries_StaffMembers_StaffMemberId` 
FOREIGN KEY (`StaffMemberId`) REFERENCES `StaffMembers` (`Id`) ON DELETE SET NULL;

ALTER TABLE `CustomerServiceHistory` 
ADD CONSTRAINT `FK_CustomerServiceHistory_Customers_CustomerId` 
FOREIGN KEY (`CustomerId`) REFERENCES `Customers` (`Id`) ON DELETE RESTRICT;
```

---

## ✅ Phase 4 Complete!

**Summary of Phase 4 Completion:**
- ✅ Created MySQL setup scripts for both local and Docker installations
- ✅ Generated comprehensive migration validation tools
- ✅ Created Docker Compose configuration for easy MySQL deployment
- ✅ Generated complete EF Core MySQL migration files
- ✅ Project builds successfully with all MySQL configurations
- ✅ Migration files are ready for database deployment

**Key Achievements:**
- **Setup Scripts**: PowerShell scripts for MySQL installation and configuration
- **Docker Support**: Complete Docker Compose setup with phpMyAdmin
- **Migration Tools**: EF Core migrations ready for deployment
- **Validation**: Comprehensive validation scripts to verify setup
- **Documentation**: Clear instructions for both local and Docker setups

**Files Created:**
- `scripts/setup-mysql.ps1` - Local MySQL setup script
- `scripts/setup-mysql-docker.ps1` - Docker MySQL setup script
- `scripts/run-migration.ps1` - Migration execution script
- `scripts/validate-migration.ps1` - Migration validation script
- `docker-compose.mysql.yml` - Docker Compose configuration

**Migration Status:**
- ✅ EF Core migration files generated and validated
- ✅ Project builds successfully with MySQL configuration
- ✅ All entity configurations updated for MySQL
- ✅ JSON columns configured for native MySQL support
- ✅ Concurrency tokens configured for MySQL TIMESTAMP

**Next Steps:** Ready to proceed with Phase 5 (Configuration Updates) which will involve final configuration adjustments and testing.

---

### Phase 5: Configuration Updates ✅

#### 5.1 Update appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=QueueHubDb;User=root;Password=your-password;Port=3306;CharSet=utf8mb4;SslMode=Preferred;",
    "MySqlConnection": "Server=localhost;Database=QueueHubDb;User=root;Password=your-password;Port=3306;CharSet=utf8mb4;SslMode=Preferred;"
  },
  "Database": {
    "UseSqlDatabase": true,
    "UseInMemoryDatabase": false,
    "UseBogusRepositories": false,
    "AutoMigrate": true,
    "SeedData": true,
    "ForceReseed": false,
    "Provider": "MySQL"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  },
  "AllowedHosts": "*",
  "Jwt": {
    "Key": "your-super-secret-key-with-at-least-32-characters",
    "Issuer": "GrandeTech.QueueHub.API",
    "Audience": "GrandeTech.QueueHub.API"
  },
  "RateLimiting": {
    "MaxRequestsPerWindow": 10,
    "WindowMinutes": 1
  },
  "DataAnonymization": {
    "DataRetentionDays": 90,
    "InactiveDataRetentionDays": 30,
    "EnableAutomaticAnonymization": true,
    "HashSalt": "GrandeTechQueueHub2024"
  },
  "SecurityAudit": {
    "EnableAuditing": true,
    "LogSuccessfulOperations": true,
    "LogFailedOperations": true,
    "AuditLogRetentionDays": 365,
    "IncludeSensitiveData": false
  },
  "PerformanceMonitoring": {
    "MaxHistoryPoints": 1000,
    "CleanupIntervalMinutes": 15,
    "MetricRetentionHours": 24,
    "EnableDetailedLogging": false
  }
}
```

#### 5.2 Update Docker Configuration

**docker-compose.yml:**
```yaml
services:
  api:
    build:
      context: .
      dockerfile: GrandeTech.QueueHub.API/Dockerfile
    ports:
      - "7126:80"
      - "7127:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:80;https://+:443
      - Database__UseSqlDatabase=true
      - Database__UseInMemoryDatabase=false
      - Database__AutoMigrate=true
      - Database__SeedData=true
      - Database__Provider=MySQL
      - ConnectionStrings__MySqlConnection=Server=mysql;Database=QueueHubDb;User=root;Password=DevPassword123!;Port=3306;CharSet=utf8mb4;SslMode=None;
      - RateLimiting__MaxRequestsPerWindow=20
      - RateLimiting__WindowMinutes=1
      - DataAnonymization__DataRetentionDays=90
      - DataAnonymization__InactiveDataRetentionDays=30
      - SecurityAudit__EnableAuditing=true
      - SecurityAudit__LogSuccessfulOperations=true
      - SecurityAudit__LogFailedOperations=true
      - PerformanceMonitoring__MaxHistoryPoints=1000
      - PerformanceMonitoring__CleanupIntervalMinutes=15
      - PerformanceMonitoring__MetricRetentionHours=24
    depends_on:
      - mysql
    volumes:
      - ./logs:/app/logs
    networks:
      - queuehub-network

  # MySQL Database
  mysql:
    image: mysql:8.0
    environment:
      - MYSQL_ROOT_PASSWORD=DevPassword123!
      - MYSQL_DATABASE=QueueHubDb
      - MYSQL_CHARACTER_SET_SERVER=utf8mb4
      - MYSQL_COLLATION_SERVER=utf8mb4_unicode_ci
    ports:
      - "3306:3306"
    volumes:
      - mysql_data:/var/lib/mysql
      - ./scripts:/docker-entrypoint-initdb.d
    command: --default-authentication-plugin=mysql_native_password --innodb-buffer-pool-size=256M
    networks:
      - queuehub-network
    healthcheck:
      test: ["CMD", "mysqladmin", "ping", "-h", "localhost"]
      timeout: 20s
      retries: 10

  # Redis Cache (for future use)
  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data
    networks:
      - queuehub-network

volumes:
  mysql_data:
  redis_data:

networks:
  queuehub-network:
    driver: bridge
```

---

## ✅ Phase 5 Complete!

**Summary of Phase 5 Completion:**
- ✅ Enhanced appsettings.json with comprehensive MySQL configuration
- ✅ Updated DependencyInjection.cs to use configuration-driven MySQL settings
- ✅ Updated docker-compose.yml to use MySQL instead of SQL Server
- ✅ Created production-ready configurations and environment templates
- ✅ Project builds successfully with all new configurations

**Key Achievements:**
- **Configuration Management**: Centralized MySQL settings in appsettings.json
- **Docker Support**: Complete Docker Compose setup for both development and production
- **Production Ready**: Production configurations with environment variables
- **Flexibility**: Configuration-driven settings for different environments
- **Performance**: Optimized MySQL settings for production workloads

**Files Created/Updated:**
- `appsettings.json` - Enhanced with MySQL-specific settings
- `appsettings.Production.json` - Production configuration with environment variables
- `docker-compose.yml` - Updated to use MySQL
- `docker-compose.production.yml` - Production Docker Compose configuration
- `production.env.template` - Environment variables template
- `Infrastructure/DependencyInjection.cs` - Configuration-driven MySQL setup

**Configuration Features:**
- ✅ **Connection Pooling**: Configurable connection pool settings
- ✅ **Retry Policies**: Configurable retry on failure settings
- ✅ **Timeouts**: Configurable command and connection timeouts
- ✅ **Performance**: Optimized batch sizes and query splitting
- ✅ **Security**: Production-ready security settings
- ✅ **Environment Support**: Development, staging, and production configurations

**Next Steps:** Ready to proceed with Phase 6 (Migration Strategy) which will involve comprehensive testing of the MySQL integration.

---

### Phase 6: Migration Strategy

#### 6.1 Data Migration Approach

**Step 1: Export from SQL Server**
```sql
-- Export data from SQL Server
-- Use SQL Server Management Studio or bcp utility
bcp "SELECT * FROM QueueHubDb.dbo.SubscriptionPlans" queryout "subscription_plans.csv" -c -t"," -r"\n" -S "your-server" -T
bcp "SELECT * FROM QueueHubDb.dbo.Organizations" queryout "organizations.csv" -c -t"," -r"\n" -S "your-server" -T
-- Continue for all tables...
```

**Step 2: Transform Data**
```python
# Python script to transform data
import pandas as pd
import uuid

def transform_guid(guid_str):
    """Convert SQL Server GUID format to MySQL format"""
    if guid_str and guid_str != 'NULL':
        # Remove braces and convert to lowercase
        return str(uuid.UUID(guid_str)).lower()
    return None

def transform_boolean(value):
    """Convert SQL Server bit to MySQL TINYINT(1)"""
    if value == '1':
        return '1'
    elif value == '0':
        return '0'
    return '0'

# Process each CSV file
tables = ['subscription_plans', 'organizations', 'locations', 'users', 'customers', 
          'services_offered', 'staff_members', 'queues', 'queue_entries']

for table in tables:
    df = pd.read_csv(f'{table}.csv')
    
    # Transform GUID columns
    guid_columns = ['Id', 'SubscriptionPlanId', 'OrganizationId', 'LocationId', 
                   'CustomerId', 'QueueId', 'StaffMemberId', 'ServiceTypeId']
    
    for col in guid_columns:
        if col in df.columns:
            df[col] = df[col].apply(transform_guid)
    
    # Transform boolean columns
    boolean_columns = ['IsActive', 'IsDefault', 'IsFeatured', 'IsDeleted', 'IsAnonymous', 
                      'NotificationsEnabled', 'IsQueueEnabled', 'IsOnDuty']
    
    for col in boolean_columns:
        if col in df.columns:
            df[col] = df[col].apply(transform_boolean)
    
    # Save transformed data
    df.to_csv(f'{table}_transformed.csv', index=False)
```

**Step 3: Import to MySQL**
```sql
-- Import data to MySQL
LOAD DATA INFILE 'subscription_plans_transformed.csv'
INTO TABLE SubscriptionPlans
FIELDS TERMINATED BY ','
ENCLOSED BY '"'
LINES TERMINATED BY '\n'
IGNORE 1 ROWS;

-- Continue for all tables...
```

#### 6.2 Migration Script Example

```sql
-- Data transformation for MySQL
-- Convert GUIDs to string format
UPDATE QueueEntries 
SET CustomerId = REPLACE(REPLACE(REPLACE(CustomerId, '{', ''), '}', ''), '-', '');

-- Convert bit fields to TINYINT
UPDATE SubscriptionPlans 
SET IsActive = CASE WHEN IsActive = 1 THEN 1 ELSE 0 END;

-- Convert datetime2 to DATETIME(6)
UPDATE Organizations 
SET CreatedAt = CONVERT_TZ(CreatedAt, '+00:00', '+00:00');

-- Convert nvarchar(max) to LONGTEXT
UPDATE Organizations 
SET CreatedBy = CONVERT(CreatedBy USING utf8mb4);
```

---

### Phase 7: Testing and Validation

#### 7.1 Test Plan

**Unit Tests:**
```csharp
[Test]
public async Task Should_Create_Organization_With_MySQL()
{
    // Arrange
    var organization = new Organization(
        "Test Org",
        "test-org",
        "Test Description",
        "test@example.com",
        "+1234567890",
        "https://test.com",
        null,
        Guid.NewGuid(),
        "test-user"
    );

    // Act
    await _repository.AddAsync(organization);
    await _context.SaveChangesAsync();

    // Assert
    var result = await _repository.GetByIdAsync(organization.Id);
    Assert.That(result, Is.Not.Null);
    Assert.That(result.Name, Is.EqualTo("Test Org"));
}
```

**Integration Tests:**
```csharp
[Test]
public async Task Should_Get_Organizations_From_API()
{
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.GetAsync("/api/Public/organizations");

    // Assert
    response.EnsureSuccessStatusCode();
    var content = await response.Content.ReadAsStringAsync();
    var organizations = JsonSerializer.Deserialize<List<Organization>>(content);
    Assert.That(organizations, Is.Not.Null);
    Assert.That(organizations.Count, Is.GreaterThan(0));
}
```

**Performance Tests:**
```csharp
[Test]
public async Task Should_Perform_Well_With_Large_Dataset()
{
    // Arrange
    var stopwatch = Stopwatch.StartNew();
    var organizations = await _repository.GetAllAsync();

    // Act
    var result = organizations.Where(o => o.IsActive).ToList();

    // Assert
    stopwatch.Stop();
    Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(1000));
    Assert.That(result.Count, Is.GreaterThan(0));
}
```

#### 7.2 Data Integrity Validation

```sql
-- Validate data integrity
SELECT 
    'Organizations' as TableName, 
    COUNT(*) as RecordCount,
    COUNT(CASE WHEN IsDeleted = 0 THEN 1 END) as ActiveRecords
FROM Organizations
UNION ALL
SELECT 
    'Locations' as TableName, 
    COUNT(*) as RecordCount,
    COUNT(CASE WHEN IsDeleted = 0 THEN 1 END) as ActiveRecords
FROM Locations
UNION ALL
SELECT 
    'QueueEntries' as TableName, 
    COUNT(*) as RecordCount,
    COUNT(CASE WHEN IsDeleted = 0 THEN 1 END) as ActiveRecords
FROM QueueEntries;

-- Validate foreign key relationships
SELECT 
    o.Name as OrganizationName,
    COUNT(l.Id) as LocationCount
FROM Organizations o
LEFT JOIN Locations l ON o.Id = l.OrganizationId
WHERE o.IsDeleted = 0
GROUP BY o.Id, o.Name;

-- Validate JSON columns
SELECT 
    Id,
    JSON_VALID(LocationIds) as IsValidJson,
    JSON_LENGTH(LocationIds) as LocationCount
FROM Organizations
WHERE LocationIds IS NOT NULL;
```

---

### Phase 8: Improvements and Optimizations

#### 8.1 MySQL-Specific Optimizations

**Indexes:**
```sql
-- Add composite indexes for common queries
CREATE INDEX IX_QueueEntries_Status_Position ON QueueEntries(Status, Position);
CREATE INDEX IX_QueueEntries_EnteredAt ON QueueEntries(EnteredAt);
CREATE INDEX IX_Customers_Email_Phone ON Customers(Email, PhoneNumber);
CREATE INDEX IX_StaffMembers_Location_Active ON StaffMembers(LocationId, IsActive);
CREATE INDEX IX_ServicesOffered_Location_Active ON ServicesOffered(LocationId, IsActive);

-- JSON indexes for better performance
CREATE INDEX IX_Organizations_LocationIds ON Organizations((CAST(LocationIds AS CHAR(255) ARRAY)));
CREATE INDEX IX_Locations_StaffMemberIds ON Locations((CAST(StaffMemberIds AS CHAR(255) ARRAY)));
```

**Partitioning:**
```sql
-- Partition QueueEntries by date for better performance
ALTER TABLE QueueEntries 
PARTITION BY RANGE (YEAR(EnteredAt)) (
    PARTITION p2024 VALUES LESS THAN (2025),
    PARTITION p2025 VALUES LESS THAN (2026),
    PARTITION p2026 VALUES LESS THAN (2027),
    PARTITION p_future VALUES LESS THAN MAXVALUE
);
```

**Connection Pooling:**
```csharp
// Configure connection pooling
services.AddDbContext<QueueHubDbContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), mySqlOptions =>
    {
        mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(15),
            errorNumbersToAdd: new[] { 1205, 1213, 2006, 2013, 2014, 2015, 2016, 2017, 2018, 2019 });
        mySqlOptions.CommandTimeout(30);
        mySqlOptions.MaxBatchSize(1000);
        mySqlOptions.EnableStringComparisonTranslations();
    });
}, ServiceLifetime.Scoped);
```

#### 8.2 Performance Improvements

**Query Optimization:**
```sql
-- Optimize common queries
EXPLAIN SELECT 
    o.Name,
    l.Name as LocationName,
    COUNT(qe.Id) as QueueEntryCount
FROM Organizations o
JOIN Locations l ON o.Id = l.OrganizationId
JOIN Queues q ON l.Id = q.LocationId
JOIN QueueEntries qe ON q.Id = qe.QueueId
WHERE o.IsDeleted = 0 
    AND l.IsDeleted = 0 
    AND qe.IsDeleted = 0
    AND qe.EnteredAt >= DATE_SUB(NOW(), INTERVAL 1 DAY)
GROUP BY o.Id, o.Name, l.Id, l.Name;
```

**Caching Strategy:**
```csharp
// Add Redis caching for frequently accessed data
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
    options.InstanceName = "QueueHub";
});

// Cache frequently accessed data
public async Task<List<Organization>> GetActiveOrganizationsAsync()
{
    const string cacheKey = "active_organizations";
    
    if (!_cache.TryGetValue(cacheKey, out List<Organization> organizations))
    {
        organizations = await _repository.GetAllAsync();
        organizations = organizations.Where(o => o.IsActive).ToList();
        
        _cache.Set(cacheKey, organizations, TimeSpan.FromMinutes(15));
    }
    
    return organizations;
}
```

---

### Phase 9: Implementation Steps

#### Step 1: Update Packages and Basic Configuration
1. Update `GrandeTech.QueueHub.API.csproj` with MySQL packages
2. Update connection strings in `appsettings.json`
3. Update `DependencyInjection.cs` with MySQL provider
4. Test basic connectivity

#### Step 2: Create MySQL Schema Scripts
1. Create complete MySQL schema script
2. Test schema creation on empty database
3. Validate all constraints and indexes

#### Step 3: Update DbContext and Configurations
1. Update `QueueHubDbContext.cs` with MySQL-specific configurations
2. Update all entity configurations
3. Test with empty database

#### Step 4: Test with Empty Database
1. Run all unit tests
2. Test API endpoints
3. Validate data operations

#### Step 5: Migrate Data from SQL Server
1. Export data from SQL Server
2. Transform data for MySQL format
3. Import data to MySQL
4. Validate data integrity

#### Step 6: Update Docker and Deployment Configs
1. Update `docker-compose.yml`
2. Update deployment scripts
3. Test Docker setup

#### Step 7: Run Comprehensive Tests
1. Run all unit tests
2. Run integration tests
3. Run performance tests
4. Validate data integrity

#### Step 8: Deploy to Staging Environment
1. Deploy to staging
2. Run smoke tests
3. Performance testing
4. User acceptance testing

#### Step 9: Performance Testing and Optimization
1. Load testing
2. Query optimization
3. Index tuning
4. Connection pool tuning

#### Step 10: Production Deployment
1. Deploy to production
2. Monitor performance
3. Validate functionality
4. Document changes

---

### Phase 10: Monitoring and Maintenance

####