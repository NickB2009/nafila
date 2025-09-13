-- SQL Server Data Export Script
-- Phase 6.1: Export data from SQL Server for MySQL migration

-- =============================================
-- Export Scripts for SQL Server to MySQL Migration
-- =============================================

-- Export SubscriptionPlans
SELECT 
    Id,
    Name,
    Description,
    MonthlyPriceAmount,
    MonthlyPriceCurrency,
    YearlyPriceAmount,
    YearlyPriceCurrency,
    Price,
    IsDefault,
    IsActive,
    CreatedAt,
    UpdatedAt,
    CreatedBy,
    UpdatedBy,
    RowVersion
FROM SubscriptionPlans
ORDER BY CreatedAt;

-- Export Organizations
SELECT 
    Id,
    Name,
    Slug,
    Description,
    ContactEmail,
    ContactPhone,
    WebsiteUrl,
    BrandingConfig,
    SubscriptionPlanId,
    IsActive,
    IsDeleted,
    CreatedAt,
    UpdatedAt,
    CreatedBy,
    UpdatedBy,
    RowVersion,
    LocationIds
FROM Organizations
ORDER BY CreatedAt;

-- Export Locations
SELECT 
    Id,
    OrganizationId,
    Name,
    Address,
    City,
    State,
    ZipCode,
    PhoneNumber,
    Email,
    Description,
    IsActive,
    IsDeleted,
    CreatedAt,
    UpdatedAt,
    CreatedBy,
    UpdatedBy,
    RowVersion,
    StaffMemberIds,
    ServiceTypeIds
FROM Locations
ORDER BY CreatedAt;

-- Export Customers
SELECT 
    Id,
    PhoneNumber,
    FirstName,
    LastName,
    Email,
    IsActive,
    IsDeleted,
    CreatedAt,
    UpdatedAt,
    CreatedBy,
    UpdatedBy,
    RowVersion,
    FavoriteLocationIds
FROM Customers
ORDER BY CreatedAt;

-- Export StaffMembers
SELECT 
    Id,
    LocationId,
    FirstName,
    LastName,
    Email,
    PhoneNumber,
    Status,
    IsActive,
    IsDeleted,
    CreatedAt,
    UpdatedAt,
    CreatedBy,
    UpdatedBy,
    RowVersion,
    SpecialtyServiceTypeIds
FROM StaffMembers
ORDER BY CreatedAt;

-- Export ServiceTypes
SELECT 
    Id,
    LocationId,
    Name,
    Description,
    DurationMinutes,
    Price,
    IsActive,
    IsDeleted,
    CreatedAt,
    UpdatedAt,
    CreatedBy,
    UpdatedBy,
    RowVersion
FROM ServiceTypes
ORDER BY CreatedAt;

-- Export Queues
SELECT 
    Id,
    LocationId,
    Name,
    Description,
    IsActive,
    IsDeleted,
    MaxCapacity,
    EstimatedWaitTimeMinutes,
    CreatedAt,
    UpdatedAt,
    CreatedBy,
    UpdatedBy,
    RowVersion
FROM Queues
ORDER BY CreatedAt;

-- Export QueueEntries
SELECT 
    Id,
    QueueId,
    CustomerId,
    StaffMemberId,
    ServiceTypeId,
    Status,
    Priority,
    EstimatedServiceTimeMinutes,
    ActualServiceTimeMinutes,
    Notes,
    HaircutDetails,
    IsActive,
    IsDeleted,
    CreatedAt,
    UpdatedAt,
    CreatedBy,
    UpdatedBy,
    RowVersion,
    CalledAt,
    StartedAt,
    CompletedAt
FROM QueueEntries
ORDER BY CreatedAt;

-- =============================================
-- Export with BCP Commands (for command line)
-- =============================================

/*
-- Use these BCP commands to export data to CSV files
-- Run these commands from command prompt or PowerShell

-- Export SubscriptionPlans
bcp "SELECT Id, Name, Description, MonthlyPriceAmount, MonthlyPriceCurrency, YearlyPriceAmount, YearlyPriceCurrency, Price, IsDefault, IsActive, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, RowVersion FROM QueueHubDb.dbo.SubscriptionPlans" queryout "subscription_plans.csv" -c -t"," -r"\n" -S "your-server" -T

-- Export Organizations
bcp "SELECT Id, Name, Slug, Description, ContactEmail, ContactPhone, WebsiteUrl, BrandingConfig, SubscriptionPlanId, IsActive, IsDeleted, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, RowVersion, LocationIds FROM QueueHubDb.dbo.Organizations" queryout "organizations.csv" -c -t"," -r"\n" -S "your-server" -T

-- Export Locations
bcp "SELECT Id, OrganizationId, Name, Address, City, State, ZipCode, PhoneNumber, Email, Description, IsActive, IsDeleted, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, RowVersion, StaffMemberIds, ServiceTypeIds FROM QueueHubDb.dbo.Locations" queryout "locations.csv" -c -t"," -r"\n" -S "your-server" -T

-- Export Customers
bcp "SELECT Id, PhoneNumber, FirstName, LastName, Email, IsActive, IsDeleted, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, RowVersion, FavoriteLocationIds FROM QueueHubDb.dbo.Customers" queryout "customers.csv" -c -t"," -r"\n" -S "your-server" -T

-- Export StaffMembers
bcp "SELECT Id, LocationId, FirstName, LastName, Email, PhoneNumber, Status, IsActive, IsDeleted, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, RowVersion, SpecialtyServiceTypeIds FROM QueueHubDb.dbo.StaffMembers" queryout "staff_members.csv" -c -t"," -r"\n" -S "your-server" -T

-- Export ServiceTypes
bcp "SELECT Id, LocationId, Name, Description, DurationMinutes, Price, IsActive, IsDeleted, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, RowVersion FROM QueueHubDb.dbo.ServiceTypes" queryout "service_types.csv" -c -t"," -r"\n" -S "your-server" -T

-- Export Queues
bcp "SELECT Id, LocationId, Name, Description, IsActive, IsDeleted, MaxCapacity, EstimatedWaitTimeMinutes, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, RowVersion FROM QueueHubDb.dbo.Queues" queryout "queues.csv" -c -t"," -r"\n" -S "your-server" -T

-- Export QueueEntries
bcp "SELECT Id, QueueId, CustomerId, StaffMemberId, ServiceTypeId, Status, Priority, EstimatedServiceTimeMinutes, ActualServiceTimeMinutes, Notes, HaircutDetails, IsActive, IsDeleted, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, RowVersion, CalledAt, StartedAt, CompletedAt FROM QueueHubDb.dbo.QueueEntries" queryout "queue_entries.csv" -c -t"," -r"\n" -S "your-server" -T
*/

-- =============================================
-- Data Validation Queries
-- =============================================

-- Count records in each table
SELECT 'SubscriptionPlans' as TableName, COUNT(*) as RecordCount FROM SubscriptionPlans
UNION ALL
SELECT 'Organizations', COUNT(*) FROM Organizations
UNION ALL
SELECT 'Locations', COUNT(*) FROM Locations
UNION ALL
SELECT 'Customers', COUNT(*) FROM Customers
UNION ALL
SELECT 'StaffMembers', COUNT(*) FROM StaffMembers
UNION ALL
SELECT 'ServiceTypes', COUNT(*) FROM ServiceTypes
UNION ALL
SELECT 'Queues', COUNT(*) FROM Queues
UNION ALL
SELECT 'QueueEntries', COUNT(*) FROM QueueEntries;

-- Check for data integrity issues
SELECT 'Organizations without SubscriptionPlans' as Issue, COUNT(*) as Count
FROM Organizations o
LEFT JOIN SubscriptionPlans sp ON o.SubscriptionPlanId = sp.Id
WHERE sp.Id IS NULL

UNION ALL

SELECT 'Locations without Organizations', COUNT(*)
FROM Locations l
LEFT JOIN Organizations o ON l.OrganizationId = o.Id
WHERE o.Id IS NULL

UNION ALL

SELECT 'Queues without Locations', COUNT(*)
FROM Queues q
LEFT JOIN Locations l ON q.LocationId = l.Id
WHERE l.Id IS NULL

UNION ALL

SELECT 'QueueEntries without Queues', COUNT(*)
FROM QueueEntries qe
LEFT JOIN Queues q ON qe.QueueId = q.Id
WHERE q.Id IS NULL;
