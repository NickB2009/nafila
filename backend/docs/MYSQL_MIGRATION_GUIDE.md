# MySQL Migration Guide
## GrandeTech QueueHub - SQL Server to MySQL Migration

### Overview
This guide documents the complete migration of GrandeTech QueueHub from SQL Server to MySQL, including all phases, optimizations, and best practices.

---

## Table of Contents
1. [Migration Summary](#migration-summary)
2. [Prerequisites](#prerequisites)
3. [Phase-by-Phase Guide](#phase-by-phase-guide)
4. [Configuration Files](#configuration-files)
5. [Performance Optimizations](#performance-optimizations)
6. [Security Configuration](#security-configuration)
7. [Monitoring and Logging](#monitoring-and-logging)
8. [Troubleshooting](#troubleshooting)
9. [Best Practices](#best-practices)
10. [Rollback Procedures](#rollback-procedures)

---

## Migration Summary

### What Was Migrated
- **Database Engine**: SQL Server → MySQL 8.0
- **ORM**: Entity Framework Core with Pomelo.MySql provider
- **Data Types**: Complete mapping from SQL Server to MySQL equivalents
- **JSON Handling**: Native MySQL JSON columns
- **Concurrency**: SQL Server rowversion → MySQL TIMESTAMP
- **Configuration**: Environment-specific settings
- **Docker**: Complete Docker Compose setup

### Key Benefits
- ✅ **Cost Reduction**: MySQL is open-source and cost-effective
- ✅ **Performance**: Optimized for web applications
- ✅ **JSON Support**: Native JSON data type with indexing
- ✅ **Cross-Platform**: Better Linux support
- ✅ **Community**: Large community and ecosystem

---

## Prerequisites

### System Requirements
- **MySQL**: 8.0 or higher
- **.NET**: 8.0 or higher
- **Docker**: 20.10+ (optional, for containerized setup)
- **PowerShell**: 7.0+ (for automation scripts)

### Required Software
```bash
# MySQL Server
sudo apt-get install mysql-server-8.0

# .NET 8.0 SDK
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install dotnet-sdk-8.0

# Docker (optional)
sudo apt-get install docker.io docker-compose
```

---

## Phase-by-Phase Guide

### Phase 1: Package and Provider Updates ✅
**Objective**: Update NuGet packages and connection strings

**Files Modified**:
- `GrandeTech.QueueHub.API.csproj`
- `GrandeTech.QueueHub.Tests.csproj`
- `appsettings.json`

**Key Changes**:
```xml
<!-- Removed -->
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.11" />

<!-- Added -->
<PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="8.0.2" />
<PackageReference Include="MySqlConnector" Version="2.3.7" />
```

**Connection String**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=QueueHubDb;User=root;Password=DevPassword123!;Port=3306;CharSet=utf8mb4;SslMode=Preferred;"
  }
}
```

### Phase 2: Code Changes ✅
**Objective**: Update code to use MySQL-specific types and methods

**Files Modified**:
- `Program.cs`
- `DependencyInjection.cs`
- `QueueHubDbContext.cs`
- `QueueHubDbContextFactory.cs`
- All entity configuration files

**Key Changes**:
```csharp
// Exception handling
catch (MySqlException ex) // instead of SqlException

// Connection string parsing
var connectionStringBuilder = new MySqlConnectionStringBuilder(connectionString);

// DbContext configuration
options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), mySqlOptions => {
    mySqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(15), errorNumbersToAdd: null);
    mySqlOptions.CommandTimeout(30);
    mySqlOptions.EnableStringComparisonTranslations();
});
```

### Phase 3: Data Type Mappings ✅
**Objective**: Convert SQL Server data types to MySQL equivalents

**Data Type Mappings**:
| SQL Server | MySQL | Notes |
|------------|-------|-------|
| `uniqueidentifier` | `CHAR(36)` | GUID as string |
| `nvarchar(max)` | `LONGTEXT` | Large text |
| `varbinary(max)` | `LONGBLOB` | Binary data |
| `datetime2` | `DATETIME(6)` | High precision datetime |
| `bit` | `TINYINT(1)` | Boolean |
| `rowversion` | `TIMESTAMP` | Auto-updating timestamp |
| `decimal(18,2)` | `DECIMAL(18,2)` | Same precision |

**JSON Columns**:
```csharp
// SQL Server: NVARCHAR(MAX) with JSON serialization
// MySQL: Native JSON type
.HasColumnType("json")
```

### Phase 4: Data Migration ✅
**Objective**: Create migration scripts and database setup

**Files Created**:
- `mysql-schema.sql` - Complete database schema
- `migrate-json-data.sql` - JSON data migration utilities
- `setup-mysql.ps1` - MySQL setup script
- `docker-compose.mysql.yml` - Docker setup

### Phase 5: Configuration Updates ✅
**Objective**: Update all configuration files for MySQL

**Files Modified**:
- `appsettings.json` - Enhanced with MySQL settings
- `appsettings.Production.json` - Production configuration
- `docker-compose.yml` - Updated for MySQL
- `docker-compose.production.yml` - Production Docker setup

### Phase 6: Migration Strategy ✅
**Objective**: Document migration approach and testing

**Migration Approach**:
1. **Export from SQL Server**: Use SQL Server export tools
2. **Transform Data**: Convert data types and formats
3. **Import to MySQL**: Use MySQL import tools
4. **Validate**: Run comprehensive tests

### Phase 7: Testing and Validation ✅
**Objective**: Comprehensive testing of MySQL integration

**Test Coverage**:
- Unit tests for MySQL data types
- Integration tests with MySQL database
- Performance tests with large datasets
- Data migration validation tests

---

## Configuration Files

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=QueueHubDb;User=root;Password=DevPassword123!;Port=3306;CharSet=utf8mb4;SslMode=Preferred;ConnectionTimeout=30;CommandTimeout=30;",
    "MySqlConnectionPool": "Server=localhost;Database=QueueHubDb;User=root;Password=DevPassword123!;Port=3306;CharSet=utf8mb4;SslMode=Preferred;ConnectionTimeout=30;CommandTimeout=30;Pooling=true;MinimumPoolSize=5;MaximumPoolSize=100;"
  },
  "MySql": {
    "ServerVersion": "8.0.0",
    "EnableRetryOnFailure": true,
    "MaxRetryCount": 3,
    "MaxRetryDelay": 15,
    "CommandTimeout": 30,
    "EnableSensitiveDataLogging": false,
    "EnableDetailedErrors": true,
    "EnableServiceProviderCaching": true,
    "EnableQuerySplittingBehavior": "SplitQuery",
    "MaxBatchSize": 1000,
    "EnableStringComparisonTranslations": true
  }
}
```

### Docker Compose
```yaml
services:
  mysql:
    image: mysql:8.0
    environment:
      - MYSQL_ROOT_PASSWORD=DevPassword123!
      - MYSQL_DATABASE=QueueHubDb
      - MYSQL_USER=queuehub
      - MYSQL_PASSWORD=DevPassword123!
    ports:
      - "3306:3306"
    volumes:
      - mysql_data:/var/lib/mysql
      - ./mysql-schema.sql:/docker-entrypoint-initdb.d/01-schema.sql
    command: --default-authentication-plugin=mysql_native_password
```

---

## Performance Optimizations

### Indexes
```sql
-- Primary indexes
CREATE INDEX idx_organizations_name ON Organizations(Name);
CREATE INDEX idx_locations_organization ON Locations(OrganizationId);
CREATE INDEX idx_queues_location ON Queues(LocationId);

-- Composite indexes
CREATE INDEX idx_queue_entry_status_created ON QueueEntries(QueueId, Status, CreatedAt);

-- JSON indexes (MySQL 8.0+)
CREATE INDEX idx_organizations_location_ids ON Organizations((CAST(LocationIds AS CHAR(50) ARRAY)));

-- Full-text search
CREATE FULLTEXT INDEX idx_organizations_search ON Organizations(Name, Description);
```

### Configuration
```sql
-- Buffer pool optimization
SET GLOBAL innodb_buffer_pool_size = 1G;

-- Connection settings
SET GLOBAL max_connections = 200;
SET GLOBAL thread_cache_size = 16;

-- Query optimization
SET GLOBAL sort_buffer_size = 2M;
SET GLOBAL join_buffer_size = 2M;
```

---

## Security Configuration

### User Management
```sql
-- Application user
CREATE USER 'queuehub_app'@'%' IDENTIFIED BY 'SecureAppPassword123!';
GRANT SELECT, INSERT, UPDATE, DELETE ON QueueHubDb.* TO 'queuehub_app'@'%';

-- Read-only user
CREATE USER 'queuehub_readonly'@'%' IDENTIFIED BY 'ReadOnlyPassword123!';
GRANT SELECT ON QueueHubDb.* TO 'queuehub_readonly'@'%';
```

### SSL/TLS
```sql
-- Require SSL
SET GLOBAL require_secure_transport = ON;
```

### Data Masking
```sql
-- Masked customer view
CREATE VIEW v_customers_masked AS
SELECT 
    Id,
    CONCAT(LEFT(FirstName, 1), '***') as FirstName,
    CONCAT(LEFT(LastName, 1), '***') as LastName,
    CONCAT(LEFT(PhoneNumber, 3), '***-****') as PhoneNumber
FROM Customers;
```

---

## Monitoring and Logging

### Performance Monitoring
```sql
-- Enable performance schema
SET GLOBAL performance_schema = ON;

-- Slow query log
SET GLOBAL slow_query_log = ON;
SET GLOBAL long_query_time = 2;
```

### Custom Monitoring
```sql
-- Performance metrics table
CREATE TABLE performance_metrics (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    metric_name VARCHAR(100) NOT NULL,
    metric_value DECIMAL(20,4) NOT NULL,
    recorded_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

---

## Troubleshooting

### Common Issues

#### Connection Issues
```bash
# Check MySQL status
sudo systemctl status mysql

# Check port availability
netstat -tlnp | grep 3306

# Test connection
mysql -u root -p -h localhost
```

#### Performance Issues
```sql
-- Check slow queries
SELECT * FROM mysql.slow_log ORDER BY start_time DESC LIMIT 10;

-- Check index usage
SELECT * FROM performance_schema.table_io_waits_summary_by_index_usage;

-- Check buffer pool usage
SHOW STATUS LIKE 'Innodb_buffer_pool%';
```

#### Data Type Issues
```sql
-- Check data type conversions
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    NUMERIC_PRECISION,
    NUMERIC_SCALE
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = 'QueueHubDb'
ORDER BY TABLE_NAME, ORDINAL_POSITION;
```

### Error Codes
| Error Code | Description | Solution |
|------------|-------------|----------|
| 1045 | Access denied | Check username/password |
| 2006 | MySQL server has gone away | Check connection timeout |
| 1213 | Deadlock found | Implement retry logic |
| 1205 | Lock wait timeout | Optimize queries |

---

## Best Practices

### Development
1. **Use Connection Pooling**: Configure appropriate pool sizes
2. **Implement Retry Logic**: Handle transient failures
3. **Monitor Performance**: Use performance schema
4. **Test Thoroughly**: Comprehensive testing before production

### Production
1. **Enable SSL**: Secure all connections
2. **Regular Backups**: Automated backup strategy
3. **Monitor Resources**: CPU, memory, disk usage
4. **Update Regularly**: Keep MySQL updated

### Security
1. **Least Privilege**: Minimal required permissions
2. **Strong Passwords**: Complex password policies
3. **Network Security**: Firewalls and VPNs
4. **Audit Logging**: Monitor all activities

---

## Rollback Procedures

### Emergency Rollback
1. **Stop Application**: Stop the application service
2. **Restore SQL Server**: Restore from backup
3. **Update Configuration**: Revert to SQL Server settings
4. **Restart Application**: Start with SQL Server

### Planned Rollback
1. **Schedule Maintenance**: Plan maintenance window
2. **Backup MySQL**: Create full backup
3. **Restore SQL Server**: Restore from backup
4. **Update Code**: Revert to SQL Server code
5. **Test Thoroughly**: Validate functionality
6. **Deploy**: Deploy reverted application

---

## Support and Resources

### Documentation
- [MySQL 8.0 Reference Manual](https://dev.mysql.com/doc/refman/8.0/en/)
- [Pomelo.EntityFrameworkCore.MySql](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)

### Community
- [MySQL Forums](https://forums.mysql.com/)
- [Stack Overflow](https://stackoverflow.com/questions/tagged/mysql)
- [GitHub Issues](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql/issues)

### Monitoring Tools
- [MySQL Workbench](https://www.mysql.com/products/workbench/)
- [phpMyAdmin](https://www.phpmyadmin.net/)
- [Percona Monitoring and Management](https://www.percona.com/software/database-tools/percona-monitoring-and-management)

---

## Conclusion

The migration from SQL Server to MySQL has been completed successfully with comprehensive testing, optimization, and documentation. The system is now running on MySQL with improved performance, cost savings, and better cross-platform support.

For any questions or issues, please refer to the troubleshooting section or contact the development team.

---

**Last Updated**: January 2025  
**Version**: 1.0  
**Status**: Complete ✅
