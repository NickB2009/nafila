# MySQL Data Type Mapping Reference
## SQL Server to MySQL Migration

### Core Data Type Mappings

| SQL Server Type | MySQL Type | Notes | Example |
|----------------|------------|-------|---------|
| `uniqueidentifier` | `CHAR(36)` | GUID as string | `550e8400-e29b-41d4-a716-446655440000` |
| `nvarchar(max)` | `LONGTEXT` | Large text | `LONGTEXT CHARACTER SET utf8mb4` |
| `varbinary(max)` | `LONGBLOB` | Binary data | `LONGBLOB` |
| `datetime2` | `DATETIME(6)` | High precision datetime | `2024-01-15 14:30:25.123456` |
| `bit` | `TINYINT(1)` | Boolean | `0` or `1` |
| `rowversion` | `TIMESTAMP` | Auto-updating timestamp | `CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP` |
| `decimal(18,2)` | `DECIMAL(18,2)` | Same precision | `123.45` |
| `float` | `DOUBLE` | Floating point | `3.14159` |
| `nvarchar(n)` | `VARCHAR(n)` | Variable character | `VARCHAR(255) CHARACTER SET utf8mb4` |
| `int` | `INT` | Integer | `42` |
| `bigint` | `BIGINT` | Large integer | `9223372036854775807` |
| `text` | `LONGTEXT` | Text data | `LONGTEXT CHARACTER SET utf8mb4` |
| `image` | `LONGBLOB` | Binary image data | `LONGBLOB` |

### JSON Column Mappings

| SQL Server | MySQL | Benefits |
|------------|-------|----------|
| `NVARCHAR(MAX)` with JSON | `JSON` | Native JSON validation, indexing, functions |
| Manual JSON serialization | Native JSON operators | Better performance, type safety |

### Concurrency Token Mappings

| SQL Server | MySQL | Behavior |
|------------|-------|----------|
| `rowversion` | `TIMESTAMP` | Auto-increment on update |
| `varbinary(8)` | `TIMESTAMP` | MySQL handles concurrency differently |

### Character Set and Collation

- **Character Set**: `utf8mb4` (supports full Unicode including emojis)
- **Collation**: `utf8mb4_unicode_ci` (case-insensitive Unicode)

### Default Value Mappings

| SQL Server Function | MySQL Function | Purpose |
|-------------------|----------------|---------|
| `GETUTCDATE()` | `CURRENT_TIMESTAMP(6)` | Current UTC timestamp |
| `NEWID()` | `UUID()` | Generate GUID |
| `0x` | `0x0000000000000000` | Empty binary |

### Index Considerations

| SQL Server | MySQL | Notes |
|------------|-------|-------|
| Clustered indexes | Primary key (always clustered) | MySQL auto-clusters on PK |
| Non-clustered indexes | Secondary indexes | Same functionality |
| JSON indexes | Generated columns + indexes | MySQL 8.0+ JSON indexing |

### Performance Optimizations

1. **JSON Columns**: Use native `JSON` type instead of `LONGTEXT`
2. **Indexes**: Create composite indexes for common query patterns
3. **Partitioning**: Consider partitioning large tables by date
4. **Connection Pooling**: Configure appropriate pool sizes

### Migration Script Examples

#### GUID Conversion
```sql
-- SQL Server format: {550e8400-e29b-41d4-a716-446655440000}
-- MySQL format: 550e8400-e29b-41d4-a716-446655440000

UPDATE TableName 
SET Id = REPLACE(REPLACE(REPLACE(Id, '{', ''), '}', ''), '-', '');
```

#### Boolean Conversion
```sql
-- SQL Server bit to MySQL TINYINT(1)
UPDATE TableName 
SET IsActive = CASE WHEN IsActive = 1 THEN 1 ELSE 0 END;
```

#### DateTime Conversion
```sql
-- SQL Server datetime2 to MySQL DATETIME(6)
UPDATE TableName 
SET CreatedAt = CONVERT_TZ(CreatedAt, '+00:00', '+00:00');
```

#### JSON Data Migration
```sql
-- Convert string JSON to native MySQL JSON
UPDATE TableName 
SET JsonColumn = CAST(JsonColumn AS JSON);
```

### Validation Queries

#### Check Data Integrity
```sql
-- Validate GUID format
SELECT COUNT(*) as InvalidGuids
FROM TableName 
WHERE Id NOT REGEXP '^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$';

-- Validate JSON columns
SELECT COUNT(*) as InvalidJson
FROM TableName 
WHERE JSON_VALID(JsonColumn) = 0;

-- Check foreign key relationships
SELECT 
    o.Name as OrganizationName,
    COUNT(l.Id) as LocationCount
FROM Organizations o
LEFT JOIN Locations l ON o.Id = l.OrganizationId
WHERE o.IsDeleted = 0
GROUP BY o.Id, o.Name;
```

### Common Issues and Solutions

1. **GUID Format**: Remove braces and hyphens for MySQL compatibility
2. **Boolean Values**: Convert bit to TINYINT(1)
3. **DateTime Precision**: Ensure microsecond precision is preserved
4. **JSON Validation**: Validate JSON data before migration
5. **Character Encoding**: Ensure UTF-8 compatibility
6. **Index Rebuilding**: Rebuild indexes after data migration
7. **Constraint Validation**: Verify all foreign key constraints

### Performance Monitoring

```sql
-- Monitor query performance
EXPLAIN SELECT * FROM QueueEntries WHERE Status = 'waiting';

-- Check index usage
SHOW INDEX FROM QueueEntries;

-- Monitor JSON column performance
SELECT 
    JSON_LENGTH(LocationIds) as LocationCount,
    JSON_VALID(LocationIds) as IsValidJson
FROM Organizations;
```
