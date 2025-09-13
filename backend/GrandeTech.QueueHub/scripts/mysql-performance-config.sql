-- MySQL Performance Configuration
-- Phase 8.2: Performance tuning and optimization

-- =============================================
-- MySQL Configuration Optimization
-- =============================================

-- Set optimal buffer pool size (adjust based on available RAM)
-- For 8GB RAM: SET GLOBAL innodb_buffer_pool_size = 6G;
-- For 16GB RAM: SET GLOBAL innodb_buffer_pool_size = 12G;
-- For 32GB RAM: SET GLOBAL innodb_buffer_pool_size = 24G;
SET GLOBAL innodb_buffer_pool_size = 1G; -- Adjust based on your system

-- InnoDB optimization settings
SET GLOBAL innodb_log_file_size = 256M;
SET GLOBAL innodb_log_buffer_size = 16M;
SET GLOBAL innodb_flush_log_at_trx_commit = 2; -- Better performance, slight risk
SET GLOBAL innodb_flush_method = O_DIRECT;
SET GLOBAL innodb_file_per_table = ON;

-- Connection and thread settings
SET GLOBAL max_connections = 200;
SET GLOBAL thread_cache_size = 16;
SET GLOBAL table_open_cache = 2000;
SET GLOBAL table_definition_cache = 1400;

-- Query cache settings (MySQL 8.0+ removed query cache, but keeping for reference)
-- SET GLOBAL query_cache_type = ON;
-- SET GLOBAL query_cache_size = 64M;

-- Temporary table settings
SET GLOBAL tmp_table_size = 64M;
SET GLOBAL max_heap_table_size = 64M;

-- Sort and join settings
SET GLOBAL sort_buffer_size = 2M;
SET GLOBAL join_buffer_size = 2M;
SET GLOBAL read_buffer_size = 2M;
SET GLOBAL read_rnd_buffer_size = 8M;

-- Binary logging (for replication)
SET GLOBAL binlog_format = ROW;
SET GLOBAL sync_binlog = 1;

-- =============================================
-- Application-Specific Optimizations
-- =============================================

-- Enable query profiling for development
SET GLOBAL profiling = ON;

-- Set timezone for consistent datetime handling
SET GLOBAL time_zone = '+00:00';

-- Optimize for JSON operations
SET GLOBAL innodb_adaptive_hash_index = ON;

-- =============================================
-- Monitoring and Diagnostics
-- =============================================

-- Enable performance schema
SET GLOBAL performance_schema = ON;

-- Enable slow query log
SET GLOBAL slow_query_log = ON;
SET GLOBAL long_query_time = 2; -- Log queries taking more than 2 seconds
SET GLOBAL log_queries_not_using_indexes = ON;

-- Enable general query log (disable in production)
-- SET GLOBAL general_log = ON;

-- =============================================
-- Create Performance Monitoring Views
-- =============================================

-- Create view for monitoring queue performance
CREATE OR REPLACE VIEW v_queue_performance AS
SELECT 
    q.Id as QueueId,
    q.Name as QueueName,
    l.Name as LocationName,
    o.Name as OrganizationName,
    COUNT(qe.Id) as TotalEntries,
    COUNT(CASE WHEN qe.Status = 'Waiting' THEN 1 END) as WaitingEntries,
    COUNT(CASE WHEN qe.Status = 'InService' THEN 1 END) as InServiceEntries,
    COUNT(CASE WHEN qe.Status = 'Completed' THEN 1 END) as CompletedEntries,
    AVG(qe.EstimatedServiceTimeMinutes) as AvgServiceTime,
    AVG(TIMESTAMPDIFF(MINUTE, qe.CreatedAt, qe.CompletedAt)) as AvgActualServiceTime
FROM Queues q
JOIN Locations l ON q.LocationId = l.Id
JOIN Organizations o ON l.OrganizationId = o.Id
LEFT JOIN QueueEntries qe ON q.Id = qe.QueueId
WHERE q.IsActive = 1 AND l.IsActive = 1 AND o.IsActive = 1
GROUP BY q.Id, q.Name, l.Name, o.Name;

-- Create view for monitoring staff performance
CREATE OR REPLACE VIEW v_staff_performance AS
SELECT 
    sm.Id as StaffId,
    sm.FirstName,
    sm.LastName,
    l.Name as LocationName,
    o.Name as OrganizationName,
    COUNT(qe.Id) as TotalServices,
    COUNT(CASE WHEN qe.Status = 'Completed' THEN 1 END) as CompletedServices,
    AVG(TIMESTAMPDIFF(MINUTE, qe.CreatedAt, qe.CompletedAt)) as AvgServiceTime,
    sm.Status as CurrentStatus
FROM StaffMembers sm
JOIN Locations l ON sm.LocationId = l.Id
JOIN Organizations o ON l.OrganizationId = o.Id
LEFT JOIN QueueEntries qe ON sm.Id = qe.StaffMemberId
WHERE sm.IsActive = 1 AND l.IsActive = 1 AND o.IsActive = 1
GROUP BY sm.Id, sm.FirstName, sm.LastName, l.Name, o.Name, sm.Status;

-- Create view for monitoring organization metrics
CREATE OR REPLACE VIEW v_organization_metrics AS
SELECT 
    o.Id as OrganizationId,
    o.Name as OrganizationName,
    COUNT(DISTINCT l.Id) as TotalLocations,
    COUNT(DISTINCT sm.Id) as TotalStaff,
    COUNT(DISTINCT q.Id) as TotalQueues,
    COUNT(DISTINCT qe.Id) as TotalQueueEntries,
    COUNT(DISTINCT c.Id) as TotalCustomers,
    AVG(q.EstimatedWaitTimeMinutes) as AvgWaitTime
FROM Organizations o
LEFT JOIN Locations l ON o.Id = l.OrganizationId AND l.IsActive = 1
LEFT JOIN StaffMembers sm ON l.Id = sm.LocationId AND sm.IsActive = 1
LEFT JOIN Queues q ON l.Id = q.LocationId AND q.IsActive = 1
LEFT JOIN QueueEntries qe ON q.Id = qe.QueueId
LEFT JOIN Customers c ON qe.CustomerId = c.Id AND c.IsActive = 1
WHERE o.IsActive = 1
GROUP BY o.Id, o.Name;

-- =============================================
-- Performance Analysis Queries
-- =============================================

-- Check for missing indexes
SELECT 
    t.TABLE_NAME,
    t.TABLE_ROWS,
    ROUND(((t.DATA_LENGTH + t.INDEX_LENGTH) / 1024 / 1024), 2) AS 'Size (MB)',
    COUNT(i.INDEX_NAME) as IndexCount
FROM information_schema.TABLES t
LEFT JOIN information_schema.STATISTICS i ON t.TABLE_NAME = i.TABLE_NAME AND t.TABLE_SCHEMA = i.TABLE_SCHEMA
WHERE t.TABLE_SCHEMA = 'QueueHubDb'
GROUP BY t.TABLE_NAME, t.TABLE_ROWS, t.DATA_LENGTH, t.INDEX_LENGTH
ORDER BY t.TABLE_ROWS DESC;

-- Check index usage statistics
SELECT 
    OBJECT_SCHEMA,
    OBJECT_NAME,
    INDEX_NAME,
    COUNT_FETCH,
    COUNT_INSERT,
    COUNT_UPDATE,
    COUNT_DELETE
FROM performance_schema.table_io_waits_summary_by_index_usage
WHERE OBJECT_SCHEMA = 'QueueHubDb'
ORDER BY COUNT_FETCH DESC;

-- Check for slow queries
SELECT 
    DIGEST_TEXT,
    COUNT_STAR as Executions,
    ROUND(AVG_TIMER_WAIT/1000000000, 2) AS avg_time_seconds,
    ROUND(MAX_TIMER_WAIT/1000000000, 2) AS max_time_seconds,
    ROUND(SUM_TIMER_WAIT/1000000000, 2) AS total_time_seconds
FROM performance_schema.events_statements_summary_by_digest 
WHERE SCHEMA_NAME = 'QueueHubDb'
  AND AVG_TIMER_WAIT > 1000000000 -- Queries taking more than 1 second
ORDER BY AVG_TIMER_WAIT DESC
LIMIT 20;
