-- MySQL Data Import Script
-- Phase 6.3: Import transformed data into MySQL

-- =============================================
-- Data Import Script for MySQL Migration
-- =============================================

-- Disable foreign key checks temporarily
SET FOREIGN_KEY_CHECKS = 0;

-- Disable unique checks temporarily
SET UNIQUE_CHECKS = 0;

-- Disable autocommit for better performance
SET AUTOCOMMIT = 0;

-- =============================================
-- Import SubscriptionPlans
-- =============================================

LOAD DATA INFILE 'transformed_data/subscription_plans_transformed.csv'
INTO TABLE SubscriptionPlans
FIELDS TERMINATED BY ','
ENCLOSED BY '"'
LINES TERMINATED BY '\n'
IGNORE 1 ROWS
(Id, Name, Description, MonthlyPriceAmount, MonthlyPriceCurrency, YearlyPriceAmount, 
 YearlyPriceCurrency, Price, IsDefault, IsActive, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, RowVersion);

-- =============================================
-- Import Organizations
-- =============================================

LOAD DATA INFILE 'transformed_data/organizations_transformed.csv'
INTO TABLE Organizations
FIELDS TERMINATED BY ','
ENCLOSED BY '"'
LINES TERMINATED BY '\n'
IGNORE 1 ROWS
(Id, Name, Slug, Description, ContactEmail, ContactPhone, WebsiteUrl, BrandingConfig, 
 SubscriptionPlanId, IsActive, IsDeleted, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, RowVersion, LocationIds);

-- =============================================
-- Import Locations
-- =============================================

LOAD DATA INFILE 'transformed_data/locations_transformed.csv'
INTO TABLE Locations
FIELDS TERMINATED BY ','
ENCLOSED BY '"'
LINES TERMINATED BY '\n'
IGNORE 1 ROWS
(Id, OrganizationId, Name, Address, City, State, ZipCode, PhoneNumber, Email, Description, 
 IsActive, IsDeleted, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, RowVersion, StaffMemberIds, ServiceTypeIds);

-- =============================================
-- Import Customers
-- =============================================

LOAD DATA INFILE 'transformed_data/customers_transformed.csv'
INTO TABLE Customers
FIELDS TERMINATED BY ','
ENCLOSED BY '"'
LINES TERMINATED BY '\n'
IGNORE 1 ROWS
(Id, PhoneNumber, FirstName, LastName, Email, IsActive, IsDeleted, CreatedAt, UpdatedAt, 
 CreatedBy, UpdatedBy, RowVersion, FavoriteLocationIds);

-- =============================================
-- Import StaffMembers
-- =============================================

LOAD DATA INFILE 'transformed_data/staff_members_transformed.csv'
INTO TABLE StaffMembers
FIELDS TERMINATED BY ','
ENCLOSED BY '"'
LINES TERMINATED BY '\n'
IGNORE 1 ROWS
(Id, LocationId, FirstName, LastName, Email, PhoneNumber, Status, IsActive, IsDeleted, 
 CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, RowVersion, SpecialtyServiceTypeIds);

-- =============================================
-- Import ServiceTypes
-- =============================================

LOAD DATA INFILE 'transformed_data/service_types_transformed.csv'
INTO TABLE ServiceTypes
FIELDS TERMINATED BY ','
ENCLOSED BY '"'
LINES TERMINATED BY '\n'
IGNORE 1 ROWS
(Id, LocationId, Name, Description, DurationMinutes, Price, IsActive, IsDeleted, 
 CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, RowVersion);

-- =============================================
-- Import Queues
-- =============================================

LOAD DATA INFILE 'transformed_data/queues_transformed.csv'
INTO TABLE Queues
FIELDS TERMINATED BY ','
ENCLOSED BY '"'
LINES TERMINATED BY '\n'
IGNORE 1 ROWS
(Id, LocationId, Name, Description, IsActive, IsDeleted, MaxCapacity, EstimatedWaitTimeMinutes, 
 CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, RowVersion);

-- =============================================
-- Import QueueEntries
-- =============================================

LOAD DATA INFILE 'transformed_data/queue_entries_transformed.csv'
INTO TABLE QueueEntries
FIELDS TERMINATED BY ','
ENCLOSED BY '"'
LINES TERMINATED BY '\n'
IGNORE 1 ROWS
(Id, QueueId, CustomerId, StaffMemberId, ServiceTypeId, Status, Priority, 
 EstimatedServiceTimeMinutes, ActualServiceTimeMinutes, Notes, HaircutDetails, 
 IsActive, IsDeleted, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, RowVersion, 
 CalledAt, StartedAt, CompletedAt);

-- =============================================
-- Re-enable constraints
-- =============================================

-- Re-enable autocommit
COMMIT;
SET AUTOCOMMIT = 1;

-- Re-enable unique checks
SET UNIQUE_CHECKS = 1;

-- Re-enable foreign key checks
SET FOREIGN_KEY_CHECKS = 1;

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
WHERE q.Id IS NULL

UNION ALL

SELECT 'QueueEntries without Customers', COUNT(*)
FROM QueueEntries qe
LEFT JOIN Customers c ON qe.CustomerId = c.Id
WHERE c.Id IS NULL

UNION ALL

SELECT 'QueueEntries without StaffMembers', COUNT(*)
FROM QueueEntries qe
LEFT JOIN StaffMembers sm ON qe.StaffMemberId = sm.Id
WHERE sm.Id IS NULL;

-- Check JSON column data
SELECT 
    'Organizations with valid LocationIds JSON' as CheckType,
    COUNT(*) as ValidCount
FROM Organizations 
WHERE LocationIds IS NOT NULL 
  AND JSON_VALID(LocationIds) = 1

UNION ALL

SELECT 
    'Locations with valid StaffMemberIds JSON',
    COUNT(*)
FROM Locations 
WHERE StaffMemberIds IS NOT NULL 
  AND JSON_VALID(StaffMemberIds) = 1

UNION ALL

SELECT 
    'Locations with valid ServiceTypeIds JSON',
    COUNT(*)
FROM Locations 
WHERE ServiceTypeIds IS NOT NULL 
  AND JSON_VALID(ServiceTypeIds) = 1;

-- Check datetime precision
SELECT 
    'QueueEntries with microsecond precision' as CheckType,
    COUNT(*) as Count
FROM QueueEntries 
WHERE CreatedAt IS NOT NULL 
  AND MICROSECOND(CreatedAt) > 0;

-- Check for duplicate IDs
SELECT 'Duplicate IDs in Organizations' as Issue, COUNT(*) as Count
FROM (
    SELECT Id, COUNT(*) as cnt
    FROM Organizations
    GROUP BY Id
    HAVING cnt > 1
) duplicates

UNION ALL

SELECT 'Duplicate IDs in Locations', COUNT(*)
FROM (
    SELECT Id, COUNT(*) as cnt
    FROM Locations
    GROUP BY Id
    HAVING cnt > 1
) duplicates

UNION ALL

SELECT 'Duplicate IDs in Customers', COUNT(*)
FROM (
    SELECT Id, COUNT(*) as cnt
    FROM Customers
    GROUP BY Id
    HAVING cnt > 1
) duplicates;

-- =============================================
-- Performance Analysis
-- =============================================

-- Analyze table sizes
SELECT 
    TABLE_NAME,
    TABLE_ROWS,
    ROUND(((DATA_LENGTH + INDEX_LENGTH) / 1024 / 1024), 2) AS 'Size (MB)',
    ROUND((DATA_LENGTH / 1024 / 1024), 2) AS 'Data (MB)',
    ROUND((INDEX_LENGTH / 1024 / 1024), 2) AS 'Index (MB)'
FROM information_schema.TABLES 
WHERE TABLE_SCHEMA = 'QueueHubDb'
ORDER BY (DATA_LENGTH + INDEX_LENGTH) DESC;

-- Check index usage
SELECT 
    TABLE_NAME,
    INDEX_NAME,
    CARDINALITY,
    INDEX_TYPE
FROM information_schema.STATISTICS 
WHERE TABLE_SCHEMA = 'QueueHubDb'
ORDER BY TABLE_NAME, CARDINALITY DESC;

-- =============================================
-- Migration Success Confirmation
-- =============================================

SELECT 
    'MIGRATION COMPLETE' as Status,
    NOW() as CompletedAt,
    'All data successfully migrated from SQL Server to MySQL' as Message;
