-- MySQL Post-Migration Optimization Script
-- Phase 8: Optimizations specifically for migrated data

-- =============================================
-- Post-Migration Data Analysis and Optimization
-- =============================================

-- Analyze migrated data patterns
SELECT 
    'Data Migration Analysis' as AnalysisType,
    COUNT(*) as TotalOrganizations,
    COUNT(CASE WHEN LocationIds IS NOT NULL AND JSON_LENGTH(LocationIds) > 0 THEN 1 END) as OrganizationsWithLocations,
    COUNT(CASE WHEN ContactEmail IS NOT NULL AND ContactEmail != '' THEN 1 END) as OrganizationsWithEmail,
    COUNT(CASE WHEN ContactPhone IS NOT NULL AND ContactPhone != '' THEN 1 END) as OrganizationsWithPhone
FROM Organizations;

-- Analyze JSON column usage patterns
SELECT 
    'JSON Column Analysis' as AnalysisType,
    AVG(JSON_LENGTH(LocationIds)) as AvgLocationIdsPerOrg,
    MAX(JSON_LENGTH(LocationIds)) as MaxLocationIdsPerOrg,
    COUNT(CASE WHEN JSON_LENGTH(LocationIds) > 5 THEN 1 END) as OrgsWithManyLocations
FROM Organizations 
WHERE LocationIds IS NOT NULL;

-- =============================================
-- Performance Optimization for Migrated Data
-- =============================================

-- Create additional indexes based on migrated data patterns
CREATE INDEX IF NOT EXISTS idx_organizations_contact_info ON Organizations(ContactEmail, ContactPhone);
CREATE INDEX IF NOT EXISTS idx_organizations_json_locations ON Organizations((CAST(LocationIds AS CHAR(50) ARRAY)));

-- Optimize for common query patterns in migrated data
CREATE INDEX IF NOT EXISTS idx_locations_org_active ON Locations(OrganizationId, IsActive, CreatedAt);
CREATE INDEX IF NOT EXISTS idx_queues_location_active ON Queues(LocationId, IsActive, CreatedAt);
CREATE INDEX IF NOT EXISTS idx_queue_entries_status_created ON QueueEntries(Status, CreatedAt, QueueId);

-- =============================================
-- Data Quality Checks for Migrated Data
-- =============================================

-- Check for data quality issues that might have occurred during migration
SELECT 
    'Data Quality Issues' as CheckType,
    'Organizations with invalid emails' as Issue,
    COUNT(*) as Count
FROM Organizations 
WHERE ContactEmail IS NOT NULL 
  AND ContactEmail != '' 
  AND ContactEmail NOT LIKE '%@%.%'

UNION ALL

SELECT 
    'Data Quality Issues',
    'Organizations with invalid phone numbers',
    COUNT(*)
FROM Organizations 
WHERE ContactPhone IS NOT NULL 
  AND ContactPhone != '' 
  AND ContactPhone NOT REGEXP '^[0-9\\-\\+\\(\\)\\s]+$'

UNION ALL

SELECT 
    'Data Quality Issues',
    'Organizations with invalid JSON in LocationIds',
    COUNT(*)
FROM Organizations 
WHERE LocationIds IS NOT NULL 
  AND LocationIds != '' 
  AND JSON_VALID(LocationIds) = 0;

-- =============================================
-- Migration-Specific Performance Tuning
-- =============================================

-- Optimize for bulk operations that might be needed after migration
SET GLOBAL innodb_buffer_pool_size = 1G;
SET GLOBAL innodb_log_file_size = 256M;
SET GLOBAL innodb_flush_log_at_trx_commit = 2;

-- Optimize for JSON operations (common in migrated data)
SET GLOBAL innodb_adaptive_hash_index = ON;

-- =============================================
-- Create Migration-Specific Views
-- =============================================

-- View for migrated data analysis
CREATE OR REPLACE VIEW v_migration_analysis AS
SELECT 
    'Organizations' as EntityType,
    COUNT(*) as TotalRecords,
    COUNT(CASE WHEN CreatedAt >= DATE_SUB(NOW(), INTERVAL 1 DAY) THEN 1 END) as RecentRecords,
    AVG(LENGTH(Name)) as AvgNameLength,
    COUNT(CASE WHEN LocationIds IS NOT NULL AND JSON_LENGTH(LocationIds) > 0 THEN 1 END) as RecordsWithJSON
FROM Organizations

UNION ALL

SELECT 
    'Locations',
    COUNT(*),
    COUNT(CASE WHEN CreatedAt >= DATE_SUB(NOW(), INTERVAL 1 DAY) THEN 1 END),
    AVG(LENGTH(Name)),
    COUNT(CASE WHEN StaffMemberIds IS NOT NULL AND JSON_LENGTH(StaffMemberIds) > 0 THEN 1 END)
FROM Locations

UNION ALL

SELECT 
    'Customers',
    COUNT(*),
    COUNT(CASE WHEN CreatedAt >= DATE_SUB(NOW(), INTERVAL 1 DAY) THEN 1 END),
    AVG(LENGTH(CONCAT(FirstName, ' ', LastName))),
    COUNT(CASE WHEN FavoriteLocationIds IS NOT NULL AND JSON_LENGTH(FavoriteLocationIds) > 0 THEN 1 END)
FROM Customers;

-- View for migration data validation
CREATE OR REPLACE VIEW v_migration_validation AS
SELECT 
    'Data Integrity' as ValidationType,
    'Organizations without SubscriptionPlans' as CheckName,
    COUNT(*) as IssueCount
FROM Organizations o
LEFT JOIN SubscriptionPlans sp ON o.SubscriptionPlanId = sp.Id
WHERE sp.Id IS NULL

UNION ALL

SELECT 
    'Data Integrity',
    'Locations without Organizations',
    COUNT(*)
FROM Locations l
LEFT JOIN Organizations o ON l.OrganizationId = o.Id
WHERE o.Id IS NULL

UNION ALL

SELECT 
    'Data Integrity',
    'Queues without Locations',
    COUNT(*)
FROM Queues q
LEFT JOIN Locations l ON q.LocationId = l.Id
WHERE l.Id IS NULL

UNION ALL

SELECT 
    'Data Integrity',
    'QueueEntries without Queues',
    COUNT(*)
FROM QueueEntries qe
LEFT JOIN Queues q ON qe.QueueId = q.Id
WHERE q.Id IS NULL;

-- =============================================
-- Migration Performance Monitoring
-- =============================================

-- Create table for migration performance metrics
CREATE TABLE IF NOT EXISTS migration_performance_metrics (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    metric_name VARCHAR(100) NOT NULL,
    metric_value DECIMAL(20,4) NOT NULL,
    metric_unit VARCHAR(20),
    recorded_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_metric_name (metric_name),
    INDEX idx_recorded_at (recorded_at)
);

-- Insert initial migration performance metrics
INSERT INTO migration_performance_metrics (metric_name, metric_value, metric_unit, recorded_at)
SELECT 
    'Total Organizations Migrated',
    COUNT(*),
    'records',
    NOW()
FROM Organizations

UNION ALL

SELECT 
    'Total Locations Migrated',
    COUNT(*),
    'records',
    NOW()
FROM Locations

UNION ALL

SELECT 
    'Total Customers Migrated',
    COUNT(*),
    'records',
    NOW()
FROM Customers

UNION ALL

SELECT 
    'Total QueueEntries Migrated',
    COUNT(*),
    'records',
    NOW()
FROM QueueEntries;

-- =============================================
-- Migration-Specific Maintenance Procedures
-- =============================================

-- Procedure to analyze migrated data performance
DELIMITER //

CREATE PROCEDURE AnalyzeMigratedDataPerformance()
BEGIN
    DECLARE done INT DEFAULT FALSE;
    DECLARE table_name VARCHAR(100);
    DECLARE record_count INT;
    DECLARE table_size_mb DECIMAL(10,2);
    
    -- Cursor for table analysis
    DECLARE table_cursor CURSOR FOR 
        SELECT 
            TABLE_NAME,
            TABLE_ROWS,
            ROUND(((DATA_LENGTH + INDEX_LENGTH) / 1024 / 1024), 2)
        FROM information_schema.TABLES 
        WHERE TABLE_SCHEMA = 'QueueHubDb'
        ORDER BY (DATA_LENGTH + INDEX_LENGTH) DESC;
    
    DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = TRUE;
    
    -- Clear previous analysis
    DELETE FROM migration_performance_metrics WHERE metric_name LIKE 'Table Analysis%';
    
    OPEN table_cursor;
    
    read_loop: LOOP
        FETCH table_cursor INTO table_name, record_count, table_size_mb;
        IF done THEN
            LEAVE read_loop;
        END IF;
        
        -- Insert table analysis metrics
        INSERT INTO migration_performance_metrics (metric_name, metric_value, metric_unit, recorded_at)
        VALUES 
            (CONCAT('Table Analysis - ', table_name, ' - Record Count'), record_count, 'records', NOW()),
            (CONCAT('Table Analysis - ', table_name, ' - Size MB'), table_size_mb, 'MB', NOW());
    END LOOP;
    
    CLOSE table_cursor;
    
    -- Insert summary metrics
    INSERT INTO migration_performance_metrics (metric_name, metric_value, metric_unit, recorded_at)
    SELECT 
        'Migration Analysis Complete',
        1,
        'status',
        NOW();
END //

DELIMITER ;

-- =============================================
-- Run Post-Migration Analysis
-- =============================================

-- Execute the analysis procedure
CALL AnalyzeMigratedDataPerformance();

-- Display migration analysis results
SELECT 
    metric_name,
    metric_value,
    metric_unit,
    recorded_at
FROM migration_performance_metrics
WHERE recorded_at >= DATE_SUB(NOW(), INTERVAL 1 HOUR)
ORDER BY recorded_at DESC, metric_name;

-- Display data quality check results
SELECT * FROM v_migration_validation;

-- Display migration analysis summary
SELECT * FROM v_migration_analysis;

-- =============================================
-- Migration Success Confirmation
-- =============================================

SELECT 
    'POST-MIGRATION OPTIMIZATION COMPLETE' as Status,
    NOW() as CompletedAt,
    'All migrated data has been analyzed and optimized for MySQL' as Message;
