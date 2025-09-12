-- MySQL JSON Data Migration Script
-- Convert SQL Server JSON strings to MySQL native JSON format

-- =============================================
-- JSON Data Validation and Migration
-- =============================================

-- 1. Validate existing JSON data before migration
SELECT 
    'Organizations' as TableName,
    COUNT(*) as TotalRecords,
    COUNT(CASE WHEN JSON_VALID(LocationIds) = 1 THEN 1 END) as ValidJsonRecords,
    COUNT(CASE WHEN JSON_VALID(LocationIds) = 0 THEN 1 END) as InvalidJsonRecords
FROM Organizations
UNION ALL
SELECT 
    'Locations' as TableName,
    COUNT(*) as TotalRecords,
    COUNT(CASE WHEN JSON_VALID(StaffMemberIds) = 1 THEN 1 END) as ValidJsonRecords,
    COUNT(CASE WHEN JSON_VALID(StaffMemberIds) = 0 THEN 1 END) as InvalidJsonRecords
FROM Locations
UNION ALL
SELECT 
    'Customers' as TableName,
    COUNT(*) as TotalRecords,
    COUNT(CASE WHEN JSON_VALID(FavoriteLocationIds) = 1 THEN 1 END) as ValidJsonRecords,
    COUNT(CASE WHEN JSON_VALID(FavoriteLocationIds) = 0 THEN 1 END) as InvalidJsonRecords
FROM Customers
UNION ALL
SELECT 
    'StaffMembers' as TableName,
    COUNT(*) as TotalRecords,
    COUNT(CASE WHEN JSON_VALID(SpecialtyServiceTypeIds) = 1 THEN 1 END) as ValidJsonRecords,
    COUNT(CASE WHEN JSON_VALID(SpecialtyServiceTypeIds) = 0 THEN 1 END) as InvalidJsonRecords
FROM StaffMembers;

-- 2. Fix invalid JSON data (if any)
-- Convert empty strings to empty JSON arrays
UPDATE Organizations 
SET LocationIds = '[]' 
WHERE LocationIds = '' OR LocationIds IS NULL;

UPDATE Locations 
SET StaffMemberIds = '[]' 
WHERE StaffMemberIds = '' OR StaffMemberIds IS NULL;

UPDATE Locations 
SET ServiceTypeIds = '[]' 
WHERE ServiceTypeIds = '' OR ServiceTypeIds IS NULL;

UPDATE Locations 
SET AdvertisementIds = '[]' 
WHERE AdvertisementIds = '' OR AdvertisementIds IS NULL;

UPDATE Customers 
SET FavoriteLocationIds = '[]' 
WHERE FavoriteLocationIds = '' OR FavoriteLocationIds IS NULL;

UPDATE StaffMembers 
SET SpecialtyServiceTypeIds = '[]' 
WHERE SpecialtyServiceTypeIds = '' OR SpecialtyServiceTypeIds IS NULL;

-- 3. Validate JSON structure and content
-- Check Organization LocationIds
SELECT 
    Id,
    Name,
    JSON_LENGTH(LocationIds) as LocationCount,
    JSON_VALID(LocationIds) as IsValidJson,
    LocationIds
FROM Organizations
WHERE JSON_VALID(LocationIds) = 0
LIMIT 10;

-- Check Location StaffMemberIds
SELECT 
    Id,
    Name,
    JSON_LENGTH(StaffMemberIds) as StaffCount,
    JSON_VALID(StaffMemberIds) as IsValidJson,
    StaffMemberIds
FROM Locations
WHERE JSON_VALID(StaffMemberIds) = 0
LIMIT 10;

-- Check Customer FavoriteLocationIds
SELECT 
    Id,
    Name,
    JSON_LENGTH(FavoriteLocationIds) as FavoriteCount,
    JSON_VALID(FavoriteLocationIds) as IsValidJson,
    FavoriteLocationIds
FROM Customers
WHERE JSON_VALID(FavoriteLocationIds) = 0
LIMIT 10;

-- Check StaffMember SpecialtyServiceTypeIds
SELECT 
    Id,
    Name,
    JSON_LENGTH(SpecialtyServiceTypeIds) as SpecialtyCount,
    JSON_VALID(SpecialtyServiceTypeIds) as IsValidJson,
    SpecialtyServiceTypeIds
FROM StaffMembers
WHERE JSON_VALID(SpecialtyServiceTypeIds) = 0
LIMIT 10;

-- 4. Create JSON indexes for better performance (MySQL 8.0+)
-- Note: These indexes will be created after data migration is complete

-- Organization LocationIds index
-- CREATE INDEX IX_Organizations_LocationIds ON Organizations((CAST(LocationIds AS CHAR(255) ARRAY)));

-- Location StaffMemberIds index  
-- CREATE INDEX IX_Locations_StaffMemberIds ON Locations((CAST(StaffMemberIds AS CHAR(255) ARRAY)));

-- Location ServiceTypeIds index
-- CREATE INDEX IX_Locations_ServiceTypeIds ON Locations((CAST(ServiceTypeIds AS CHAR(255) ARRAY)));

-- Customer FavoriteLocationIds index
-- CREATE INDEX IX_Customers_FavoriteLocationIds ON Customers((CAST(FavoriteLocationIds AS CHAR(255) ARRAY)));

-- StaffMember SpecialtyServiceTypeIds index
-- CREATE INDEX IX_StaffMembers_SpecialtyServiceTypeIds ON StaffMembers((CAST(SpecialtyServiceTypeIds AS CHAR(255) ARRAY)));

-- 5. JSON Query Examples for Testing
-- Find organizations with specific location IDs
SELECT 
    o.Name,
    o.Slug,
    JSON_LENGTH(o.LocationIds) as LocationCount
FROM Organizations o
WHERE JSON_CONTAINS(o.LocationIds, '"550e8400-e29b-41d4-a716-446655440000"');

-- Find locations with specific staff member IDs
SELECT 
    l.Name,
    l.Slug,
    JSON_LENGTH(l.StaffMemberIds) as StaffCount
FROM Locations l
WHERE JSON_CONTAINS(l.StaffMemberIds, '"550e8400-e29b-41d4-a716-446655440000"');

-- Find customers with specific favorite locations
SELECT 
    c.Name,
    c.Email,
    JSON_LENGTH(c.FavoriteLocationIds) as FavoriteCount
FROM Customers c
WHERE JSON_CONTAINS(c.FavoriteLocationIds, '"550e8400-e29b-41d4-a716-446655440000"');

-- Find staff members with specific specialties
SELECT 
    s.Name,
    s.Role,
    JSON_LENGTH(s.SpecialtyServiceTypeIds) as SpecialtyCount
FROM StaffMembers s
WHERE JSON_CONTAINS(s.SpecialtyServiceTypeIds, '"550e8400-e29b-41d4-a716-446655440000"');

-- 6. Performance monitoring queries
-- Check JSON column sizes
SELECT 
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    COLUMN_TYPE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'QueueHubDb' 
AND DATA_TYPE = 'json'
ORDER BY TABLE_NAME, COLUMN_NAME;

-- Monitor JSON query performance
EXPLAIN FORMAT=JSON 
SELECT 
    o.Name,
    JSON_LENGTH(o.LocationIds) as LocationCount
FROM Organizations o
WHERE JSON_CONTAINS(o.LocationIds, '"550e8400-e29b-41d4-a716-446655440000"');

-- 7. Data integrity validation
-- Ensure all JSON arrays contain valid GUIDs
SELECT 
    'Organizations.LocationIds' as ColumnName,
    COUNT(*) as TotalRecords,
    COUNT(CASE WHEN JSON_VALID(LocationIds) = 1 THEN 1 END) as ValidJson,
    COUNT(CASE WHEN JSON_LENGTH(LocationIds) > 0 THEN 1 END) as NonEmptyArrays
FROM Organizations
UNION ALL
SELECT 
    'Locations.StaffMemberIds' as ColumnName,
    COUNT(*) as TotalRecords,
    COUNT(CASE WHEN JSON_VALID(StaffMemberIds) = 1 THEN 1 END) as ValidJson,
    COUNT(CASE WHEN JSON_LENGTH(StaffMemberIds) > 0 THEN 1 END) as NonEmptyArrays
FROM Locations
UNION ALL
SELECT 
    'Customers.FavoriteLocationIds' as ColumnName,
    COUNT(*) as TotalRecords,
    COUNT(CASE WHEN JSON_VALID(FavoriteLocationIds) = 1 THEN 1 END) as ValidJson,
    COUNT(CASE WHEN JSON_LENGTH(FavoriteLocationIds) > 0 THEN 1 END) as NonEmptyArrays
FROM Customers
UNION ALL
SELECT 
    'StaffMembers.SpecialtyServiceTypeIds' as ColumnName,
    COUNT(*) as TotalRecords,
    COUNT(CASE WHEN JSON_VALID(SpecialtyServiceTypeIds) = 1 THEN 1 END) as ValidJson,
    COUNT(CASE WHEN JSON_LENGTH(SpecialtyServiceTypeIds) > 0 THEN 1 END) as NonEmptyArrays
FROM StaffMembers;
