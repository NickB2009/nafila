-- MySQL Monitoring and Logging Setup
-- Phase 8.3: Monitoring and logging configuration

-- =============================================
-- Performance Schema Configuration
-- =============================================

-- Enable performance schema
SET GLOBAL performance_schema = ON;

-- Enable all performance schema consumers
UPDATE performance_schema.setup_consumers SET ENABLED = 'YES';

-- Enable all performance schema instruments
UPDATE performance_schema.setup_instruments SET ENABLED = 'YES', TIMED = 'YES';

-- =============================================
-- Slow Query Log Configuration
-- =============================================

-- Enable slow query log
SET GLOBAL slow_query_log = ON;
SET GLOBAL slow_query_log_file = '/var/log/mysql/slow.log';

-- Log queries taking more than 2 seconds
SET GLOBAL long_query_time = 2;

-- Log queries not using indexes
SET GLOBAL log_queries_not_using_indexes = ON;

-- Log slow admin statements
SET GLOBAL log_slow_admin_statements = ON;

-- =============================================
-- General Query Log (Development Only)
-- =============================================

-- Enable general query log for development
-- SET GLOBAL general_log = ON;
-- SET GLOBAL general_log_file = '/var/log/mysql/general.log';

-- =============================================
-- Error Log Configuration
-- =============================================

-- Set error log file
SET GLOBAL log_error = '/var/log/mysql/error.log';

-- Set log level (1=ERROR, 2=WARNING, 3=INFORMATION, 4=DEBUG)
SET GLOBAL log_error_verbosity = 2;

-- =============================================
-- Binary Log Configuration
-- =============================================

-- Enable binary logging
SET GLOBAL log_bin = ON;
SET GLOBAL binlog_format = ROW;
SET GLOBAL sync_binlog = 1;

-- Set binary log file size
SET GLOBAL max_binlog_size = 100M;

-- Set binary log retention (7 days)
SET GLOBAL binlog_expire_logs_seconds = 604800;

-- =============================================
-- Create Monitoring Tables
-- =============================================

-- Create table for storing performance metrics
CREATE TABLE IF NOT EXISTS performance_metrics (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    metric_name VARCHAR(100) NOT NULL,
    metric_value DECIMAL(20,4) NOT NULL,
    metric_unit VARCHAR(20),
    recorded_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_metric_name (metric_name),
    INDEX idx_recorded_at (recorded_at)
);

-- Create table for storing slow query logs
CREATE TABLE IF NOT EXISTS slow_query_logs (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    query_text TEXT NOT NULL,
    execution_time DECIMAL(10,4) NOT NULL,
    rows_examined INT,
    rows_sent INT,
    recorded_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_execution_time (execution_time),
    INDEX idx_recorded_at (recorded_at)
);

-- Create table for storing connection metrics
CREATE TABLE IF NOT EXISTS connection_metrics (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    total_connections INT NOT NULL,
    active_connections INT NOT NULL,
    max_connections INT NOT NULL,
    connection_usage_percent DECIMAL(5,2) NOT NULL,
    recorded_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_recorded_at (recorded_at)
);

-- =============================================
-- Create Monitoring Stored Procedures
-- =============================================

DELIMITER //

-- Procedure to collect performance metrics
CREATE PROCEDURE CollectPerformanceMetrics()
BEGIN
    DECLARE done INT DEFAULT FALSE;
    DECLARE var_name VARCHAR(100);
    DECLARE var_value VARCHAR(100);
    
    -- Cursor for performance schema variables
    DECLARE cur CURSOR FOR 
        SELECT VARIABLE_NAME, VARIABLE_VALUE 
        FROM performance_schema.global_status 
        WHERE VARIABLE_NAME IN (
            'Innodb_buffer_pool_read_requests',
            'Innodb_buffer_pool_reads',
            'Innodb_buffer_pool_pages_data',
            'Innodb_buffer_pool_pages_free',
            'Innodb_buffer_pool_pages_total',
            'Innodb_data_reads',
            'Innodb_data_writes',
            'Innodb_log_writes',
            'Questions',
            'Queries',
            'Slow_queries',
            'Threads_connected',
            'Threads_running',
            'Uptime'
        );
    
    DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = TRUE;
    
    OPEN cur;
    
    read_loop: LOOP
        FETCH cur INTO var_name, var_value;
        IF done THEN
            LEAVE read_loop;
        END IF;
        
        INSERT INTO performance_metrics (metric_name, metric_value, metric_unit, recorded_at)
        VALUES (var_name, CAST(var_value AS DECIMAL(20,4)), 'count', NOW());
    END LOOP;
    
    CLOSE cur;
END //

-- Procedure to collect connection metrics
CREATE PROCEDURE CollectConnectionMetrics()
BEGIN
    DECLARE total_conn INT;
    DECLARE active_conn INT;
    DECLARE max_conn INT;
    DECLARE usage_percent DECIMAL(5,2);
    
    -- Get connection metrics
    SELECT VARIABLE_VALUE INTO total_conn FROM performance_schema.global_status WHERE VARIABLE_NAME = 'Threads_connected';
    SELECT VARIABLE_VALUE INTO active_conn FROM performance_schema.global_status WHERE VARIABLE_NAME = 'Threads_running';
    SELECT VARIABLE_VALUE INTO max_conn FROM performance_schema.global_variables WHERE VARIABLE_NAME = 'max_connections';
    
    -- Calculate usage percentage
    SET usage_percent = (total_conn / max_conn) * 100;
    
    -- Insert metrics
    INSERT INTO connection_metrics (total_connections, active_connections, max_connections, connection_usage_percent, recorded_at)
    VALUES (total_conn, active_conn, max_conn, usage_percent, NOW());
END //

-- Procedure to analyze slow queries
CREATE PROCEDURE AnalyzeSlowQueries()
BEGIN
    INSERT INTO slow_query_logs (query_text, execution_time, rows_examined, rows_sent, recorded_at)
    SELECT 
        DIGEST_TEXT,
        ROUND(AVG_TIMER_WAIT/1000000000, 4),
        ROUND(SUM_ROWS_EXAMINED/COUNT_STAR),
        ROUND(SUM_ROWS_SENT/COUNT_STAR),
        NOW()
    FROM performance_schema.events_statements_summary_by_digest 
    WHERE SCHEMA_NAME = 'QueueHubDb'
      AND AVG_TIMER_WAIT > 2000000000 -- Queries taking more than 2 seconds
      AND COUNT_STAR > 0
    ORDER BY AVG_TIMER_WAIT DESC
    LIMIT 10;
END //

DELIMITER ;

-- =============================================
-- Create Monitoring Views
-- =============================================

-- View for current performance metrics
CREATE OR REPLACE VIEW v_current_performance AS
SELECT 
    metric_name,
    metric_value,
    metric_unit,
    recorded_at
FROM performance_metrics
WHERE recorded_at >= DATE_SUB(NOW(), INTERVAL 1 HOUR)
ORDER BY recorded_at DESC;

-- View for connection usage trends
CREATE OR REPLACE VIEW v_connection_trends AS
SELECT 
    DATE_FORMAT(recorded_at, '%Y-%m-%d %H:00:00') as hour,
    AVG(total_connections) as avg_connections,
    AVG(active_connections) as avg_active_connections,
    AVG(connection_usage_percent) as avg_usage_percent,
    MAX(total_connections) as max_connections,
    MAX(connection_usage_percent) as max_usage_percent
FROM connection_metrics
WHERE recorded_at >= DATE_SUB(NOW(), INTERVAL 24 HOUR)
GROUP BY DATE_FORMAT(recorded_at, '%Y-%m-%d %H:00:00')
ORDER BY hour DESC;

-- View for slow query analysis
CREATE OR REPLACE VIEW v_slow_query_analysis AS
SELECT 
    query_text,
    execution_time,
    rows_examined,
    rows_sent,
    recorded_at
FROM slow_query_logs
WHERE recorded_at >= DATE_SUB(NOW(), INTERVAL 24 HOUR)
ORDER BY execution_time DESC;

-- =============================================
-- Create Monitoring Events (Scheduled Tasks)
-- =============================================

-- Enable event scheduler
SET GLOBAL event_scheduler = ON;

-- Create event to collect performance metrics every 5 minutes
CREATE EVENT IF NOT EXISTS evt_collect_performance_metrics
ON SCHEDULE EVERY 5 MINUTE
DO
  CALL CollectPerformanceMetrics();

-- Create event to collect connection metrics every minute
CREATE EVENT IF NOT EXISTS evt_collect_connection_metrics
ON SCHEDULE EVERY 1 MINUTE
DO
  CALL CollectConnectionMetrics();

-- Create event to analyze slow queries every hour
CREATE EVENT IF NOT EXISTS evt_analyze_slow_queries
ON SCHEDULE EVERY 1 HOUR
DO
  CALL AnalyzeSlowQueries();

-- Create event to clean up old metrics (keep 30 days)
CREATE EVENT IF NOT EXISTS evt_cleanup_old_metrics
ON SCHEDULE EVERY 1 DAY
DO
  DELETE FROM performance_metrics WHERE recorded_at < DATE_SUB(NOW(), INTERVAL 30 DAY);
  DELETE FROM connection_metrics WHERE recorded_at < DATE_SUB(NOW(), INTERVAL 30 DAY);
  DELETE FROM slow_query_logs WHERE recorded_at < DATE_SUB(NOW(), INTERVAL 7 DAY);

-- =============================================
-- Create Alerting Queries
-- =============================================

-- Query to check for high connection usage
SELECT 
    'HIGH_CONNECTION_USAGE' as alert_type,
    total_connections,
    max_connections,
    connection_usage_percent,
    recorded_at
FROM connection_metrics
WHERE connection_usage_percent > 80
ORDER BY recorded_at DESC
LIMIT 1;

-- Query to check for slow queries
SELECT 
    'SLOW_QUERY_DETECTED' as alert_type,
    query_text,
    execution_time,
    recorded_at
FROM slow_query_logs
WHERE execution_time > 5
ORDER BY recorded_at DESC
LIMIT 5;

-- Query to check for high buffer pool usage
SELECT 
    'HIGH_BUFFER_POOL_USAGE' as alert_type,
    metric_value as buffer_pool_usage_percent,
    recorded_at
FROM performance_metrics
WHERE metric_name = 'Innodb_buffer_pool_pages_data'
  AND metric_value / (SELECT metric_value FROM performance_metrics WHERE metric_name = 'Innodb_buffer_pool_pages_total' ORDER BY recorded_at DESC LIMIT 1) > 0.9
ORDER BY recorded_at DESC
LIMIT 1;
