-- MySQL Security Configuration
-- Phase 8.4: Security enhancements and best practices

-- =============================================
-- User Management and Access Control
-- =============================================

-- Create application user with minimal privileges
CREATE USER IF NOT EXISTS 'queuehub_app'@'%' IDENTIFIED BY 'SecureAppPassword123!';
CREATE USER IF NOT EXISTS 'queuehub_app'@'localhost' IDENTIFIED BY 'SecureAppPassword123!';

-- Grant only necessary privileges to application user
GRANT SELECT, INSERT, UPDATE, DELETE ON QueueHubDb.* TO 'queuehub_app'@'%';
GRANT SELECT, INSERT, UPDATE, DELETE ON QueueHubDb.* TO 'queuehub_app'@'localhost';

-- Create read-only user for reporting
CREATE USER IF NOT EXISTS 'queuehub_readonly'@'%' IDENTIFIED BY 'ReadOnlyPassword123!';
CREATE USER IF NOT EXISTS 'queuehub_readonly'@'localhost' IDENTIFIED BY 'ReadOnlyPassword123!';

-- Grant read-only privileges
GRANT SELECT ON QueueHubDb.* TO 'queuehub_readonly'@'%';
GRANT SELECT ON QueueHubDb.* TO 'queuehub_readonly'@'localhost';

-- Create backup user with limited privileges
CREATE USER IF NOT EXISTS 'queuehub_backup'@'localhost' IDENTIFIED BY 'BackupPassword123!';

-- Grant backup privileges
GRANT SELECT, LOCK TABLES, SHOW VIEW, EVENT, TRIGGER ON QueueHubDb.* TO 'queuehub_backup'@'localhost';

-- =============================================
-- Password Policy Configuration
-- =============================================

-- Set password validation policy
SET GLOBAL validate_password.policy = STRONG;
SET GLOBAL validate_password.length = 12;
SET GLOBAL validate_password.mixed_case_count = 1;
SET GLOBAL validate_password.number_count = 1;
SET GLOBAL validate_password.special_char_count = 1;

-- =============================================
-- SSL/TLS Configuration
-- =============================================

-- Require SSL for all connections
-- SET GLOBAL require_secure_transport = ON;

-- Check SSL status
SHOW VARIABLES LIKE 'have_ssl';
SHOW VARIABLES LIKE 'ssl_ca';
SHOW VARIABLES LIKE 'ssl_cert';
SHOW VARIABLES LIKE 'ssl_key';

-- =============================================
-- Network Security
-- =============================================

-- Disable remote root login
DELETE FROM mysql.user WHERE User='root' AND Host NOT IN ('localhost', '127.0.0.1', '::1');

-- Remove anonymous users
DELETE FROM mysql.user WHERE User='';

-- Remove test database
DROP DATABASE IF EXISTS test;
DELETE FROM mysql.db WHERE Db='test' OR Db='test\\_%';

-- =============================================
-- Audit Logging Configuration
-- =============================================

-- Install audit plugin (if available)
-- INSTALL PLUGIN audit_log SONAME 'audit_log.so';

-- Configure audit logging
-- SET GLOBAL audit_log_policy = ALL;
-- SET GLOBAL audit_log_format = JSON;
-- SET GLOBAL audit_log_file = '/var/log/mysql/audit.log';

-- =============================================
-- Data Encryption
-- =============================================

-- Enable InnoDB encryption
-- SET GLOBAL innodb_encryption_threads = 4;
-- SET GLOBAL innodb_encryption_rotate_key_age = 1;

-- Encrypt specific tables (example)
-- ALTER TABLE Organizations ENCRYPTION = 'Y';
-- ALTER TABLE Customers ENCRYPTION = 'Y';
-- ALTER TABLE StaffMembers ENCRYPTION = 'Y';

-- =============================================
-- Row-Level Security (MySQL 8.0+)
-- =============================================

-- Create policies for row-level security
-- Note: This is a conceptual example - actual implementation depends on specific requirements

-- Policy for organization data access
-- CREATE POLICY org_data_policy ON Organizations
--     FOR ALL TO 'queuehub_app'@'%'
--     USING (JSON_EXTRACT(LocationIds, '$') IS NOT NULL);

-- =============================================
-- Data Masking and Anonymization
-- =============================================

-- Create views for data masking
CREATE OR REPLACE VIEW v_customers_masked AS
SELECT 
    Id,
    CONCAT(LEFT(FirstName, 1), '***') as FirstName,
    CONCAT(LEFT(LastName, 1), '***') as LastName,
    CONCAT(LEFT(PhoneNumber, 3), '***-****') as PhoneNumber,
    CONCAT(LEFT(Email, 3), '***@***.***') as Email,
    IsActive,
    CreatedAt
FROM Customers;

-- Create view for staff data masking
CREATE OR REPLACE VIEW v_staff_masked AS
SELECT 
    Id,
    CONCAT(LEFT(FirstName, 1), '***') as FirstName,
    CONCAT(LEFT(LastName, 1), '***') as LastName,
    CONCAT(LEFT(Email, 3), '***@***.***') as Email,
    CONCAT(LEFT(PhoneNumber, 3), '***-****') as PhoneNumber,
    Status,
    IsActive,
    CreatedAt
FROM StaffMembers;

-- =============================================
-- Security Monitoring
-- =============================================

-- Create table for security events
CREATE TABLE IF NOT EXISTS security_events (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    event_type VARCHAR(50) NOT NULL,
    user_name VARCHAR(100),
    host_name VARCHAR(100),
    event_description TEXT,
    ip_address VARCHAR(45),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_event_type (event_type),
    INDEX idx_created_at (created_at),
    INDEX idx_user_name (user_name)
);

-- Create procedure to log security events
DELIMITER //

CREATE PROCEDURE LogSecurityEvent(
    IN p_event_type VARCHAR(50),
    IN p_user_name VARCHAR(100),
    IN p_host_name VARCHAR(100),
    IN p_event_description TEXT,
    IN p_ip_address VARCHAR(45)
)
BEGIN
    INSERT INTO security_events (event_type, user_name, host_name, event_description, ip_address, created_at)
    VALUES (p_event_type, p_user_name, p_host_name, p_event_description, p_ip_address, NOW());
END //

DELIMITER ;

-- =============================================
-- Data Retention and Cleanup
-- =============================================

-- Create procedure for data retention
DELIMITER //

CREATE PROCEDURE CleanupOldData()
BEGIN
    -- Clean up old queue entries (keep 90 days)
    DELETE FROM QueueEntries 
    WHERE CreatedAt < DATE_SUB(NOW(), INTERVAL 90 DAY)
      AND Status = 'Completed';
    
    -- Clean up old audit logs (keep 1 year)
    DELETE FROM security_events 
    WHERE created_at < DATE_SUB(NOW(), INTERVAL 365 DAY);
    
    -- Clean up old performance metrics (keep 30 days)
    DELETE FROM performance_metrics 
    WHERE recorded_at < DATE_SUB(NOW(), INTERVAL 30 DAY);
END //

DELIMITER ;

-- Create event for data cleanup
CREATE EVENT IF NOT EXISTS evt_cleanup_old_data
ON SCHEDULE EVERY 1 DAY
DO
  CALL CleanupOldData();

-- =============================================
-- Security Views and Reports
-- =============================================

-- View for failed login attempts
CREATE OR REPLACE VIEW v_failed_logins AS
SELECT 
    user_name,
    host_name,
    ip_address,
    COUNT(*) as failed_attempts,
    MAX(created_at) as last_attempt
FROM security_events
WHERE event_type = 'FAILED_LOGIN'
  AND created_at >= DATE_SUB(NOW(), INTERVAL 24 HOUR)
GROUP BY user_name, host_name, ip_address
ORDER BY failed_attempts DESC;

-- View for suspicious activities
CREATE OR REPLACE VIEW v_suspicious_activities AS
SELECT 
    event_type,
    user_name,
    host_name,
    ip_address,
    event_description,
    created_at
FROM security_events
WHERE event_type IN ('UNAUTHORIZED_ACCESS', 'PRIVILEGE_ESCALATION', 'DATA_EXPORT')
  AND created_at >= DATE_SUB(NOW(), INTERVAL 7 DAY)
ORDER BY created_at DESC;

-- View for data access patterns
CREATE OR REPLACE VIEW v_data_access_patterns AS
SELECT 
    user_name,
    host_name,
    COUNT(*) as access_count,
    COUNT(DISTINCT DATE(created_at)) as active_days,
    MIN(created_at) as first_access,
    MAX(created_at) as last_access
FROM security_events
WHERE event_type = 'DATA_ACCESS'
  AND created_at >= DATE_SUB(NOW(), INTERVAL 30 DAY)
GROUP BY user_name, host_name
ORDER BY access_count DESC;

-- =============================================
-- Security Configuration Verification
-- =============================================

-- Check user privileges
SELECT 
    User,
    Host,
    Select_priv,
    Insert_priv,
    Update_priv,
    Delete_priv,
    Create_priv,
    Drop_priv,
    Grant_priv
FROM mysql.user
WHERE User LIKE 'queuehub%'
ORDER BY User, Host;

-- Check database privileges
SELECT 
    User,
    Host,
    Db,
    Select_priv,
    Insert_priv,
    Update_priv,
    Delete_priv
FROM mysql.db
WHERE Db = 'QueueHubDb'
ORDER BY User, Host;

-- Check SSL connections
SELECT 
    User,
    Host,
    ssl_type,
    ssl_cipher,
    x509_issuer,
    x509_subject
FROM mysql.user
WHERE User LIKE 'queuehub%'
ORDER BY User, Host;

-- =============================================
-- Security Recommendations
-- =============================================

-- 1. Enable SSL/TLS for all connections
-- 2. Use strong passwords and rotate them regularly
-- 3. Implement network-level security (firewalls, VPNs)
-- 4. Regular security audits and penetration testing
-- 5. Monitor and log all database activities
-- 6. Implement data encryption at rest and in transit
-- 7. Regular backups and disaster recovery testing
-- 8. Keep MySQL server updated with latest security patches
-- 9. Implement least privilege principle
-- 10. Regular security training for database administrators
