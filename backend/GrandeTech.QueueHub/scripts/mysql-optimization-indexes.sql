-- MySQL Optimization Scripts
-- Phase 8.1: MySQL-specific optimizations and indexes

-- =============================================
-- Performance Indexes for MySQL
-- =============================================

-- Organizations table indexes
CREATE INDEX IF NOT EXISTS idx_organizations_name ON Organizations(Name);
CREATE INDEX IF NOT EXISTS idx_organizations_slug ON Organizations(Slug);
CREATE INDEX IF NOT EXISTS idx_organizations_subscription_plan ON Organizations(SubscriptionPlanId);
CREATE INDEX IF NOT EXISTS idx_organizations_active ON Organizations(IsActive);
CREATE INDEX IF NOT EXISTS idx_organizations_created_at ON Organizations(CreatedAt);
CREATE INDEX IF NOT EXISTS idx_organizations_contact_email ON Organizations(ContactEmail);

-- Locations table indexes
CREATE INDEX IF NOT EXISTS idx_locations_organization ON Locations(OrganizationId);
CREATE INDEX IF NOT EXISTS idx_locations_name ON Locations(Name);
CREATE INDEX IF NOT EXISTS idx_locations_city ON Locations(City);
CREATE INDEX IF NOT EXISTS idx_locations_state ON Locations(State);
CREATE INDEX IF NOT EXISTS idx_locations_active ON Locations(IsActive);
CREATE INDEX IF NOT EXISTS idx_locations_created_at ON Locations(CreatedAt);

-- Queues table indexes
CREATE INDEX IF NOT EXISTS idx_queues_location ON Queues(LocationId);
CREATE INDEX IF NOT EXISTS idx_queues_name ON Queues(Name);
CREATE INDEX IF NOT EXISTS idx_queues_active ON Queues(IsActive);
CREATE INDEX IF NOT EXISTS idx_queues_created_at ON Queues(CreatedAt);
CREATE INDEX IF NOT EXISTS idx_queues_estimated_wait ON Queues(EstimatedWaitTimeMinutes);

-- QueueEntries table indexes
CREATE INDEX IF NOT EXISTS idx_queue_entries_queue ON QueueEntries(QueueId);
CREATE INDEX IF NOT EXISTS idx_queue_entries_customer ON QueueEntries(CustomerId);
CREATE INDEX IF NOT EXISTS idx_queue_entries_staff ON QueueEntries(StaffMemberId);
CREATE INDEX IF NOT EXISTS idx_queue_entries_status ON QueueEntries(Status);
CREATE INDEX IF NOT EXISTS idx_queue_entries_created_at ON QueueEntries(CreatedAt);
CREATE INDEX IF NOT EXISTS idx_queue_entries_estimated_service_time ON QueueEntries(EstimatedServiceTimeMinutes);
CREATE INDEX IF NOT EXISTS idx_queue_entries_priority ON QueueEntries(Priority);

-- Customers table indexes
CREATE INDEX IF NOT EXISTS idx_customers_phone ON Customers(PhoneNumber);
CREATE INDEX IF NOT EXISTS idx_customers_email ON Customers(Email);
CREATE INDEX IF NOT EXISTS idx_customers_first_name ON Customers(FirstName);
CREATE INDEX IF NOT EXISTS idx_customers_last_name ON Customers(LastName);
CREATE INDEX IF NOT EXISTS idx_customers_active ON Customers(IsActive);
CREATE INDEX IF NOT EXISTS idx_customers_created_at ON Customers(CreatedAt);

-- StaffMembers table indexes
CREATE INDEX IF NOT EXISTS idx_staff_location ON StaffMembers(LocationId);
CREATE INDEX IF NOT EXISTS idx_staff_name ON StaffMembers(FirstName, LastName);
CREATE INDEX IF NOT EXISTS idx_staff_email ON StaffMembers(Email);
CREATE INDEX IF NOT EXISTS idx_staff_phone ON StaffMembers(PhoneNumber);
CREATE INDEX IF NOT EXISTS idx_staff_active ON StaffMembers(IsActive);
CREATE INDEX IF NOT EXISTS idx_staff_status ON StaffMembers(Status);
CREATE INDEX IF NOT EXISTS idx_staff_created_at ON StaffMembers(CreatedAt);

-- ServiceTypes table indexes
CREATE INDEX IF NOT EXISTS idx_service_types_location ON ServiceTypes(LocationId);
CREATE INDEX IF NOT EXISTS idx_service_types_name ON ServiceTypes(Name);
CREATE INDEX IF NOT EXISTS idx_service_types_active ON ServiceTypes(IsActive);
CREATE INDEX IF NOT EXISTS idx_service_types_created_at ON ServiceTypes(CreatedAt);

-- SubscriptionPlans table indexes
CREATE INDEX IF NOT EXISTS idx_subscription_plans_name ON SubscriptionPlans(Name);
CREATE INDEX IF NOT EXISTS idx_subscription_plans_active ON SubscriptionPlans(IsActive);
CREATE INDEX IF NOT EXISTS idx_subscription_plans_default ON SubscriptionPlans(IsDefault);
CREATE INDEX IF NOT EXISTS idx_subscription_plans_created_at ON SubscriptionPlans(CreatedAt);

-- =============================================
-- Composite Indexes for Complex Queries
-- =============================================

-- Organization + Location queries
CREATE INDEX IF NOT EXISTS idx_org_location_active ON Locations(OrganizationId, IsActive, CreatedAt);

-- Queue + QueueEntry queries
CREATE INDEX IF NOT EXISTS idx_queue_entry_status_created ON QueueEntries(QueueId, Status, CreatedAt);
CREATE INDEX IF NOT EXISTS idx_queue_entry_customer_status ON QueueEntries(CustomerId, Status, CreatedAt);
CREATE INDEX IF NOT EXISTS idx_queue_entry_staff_status ON QueueEntries(StaffMemberId, Status, CreatedAt);

-- Staff + QueueEntry queries
CREATE INDEX IF NOT EXISTS idx_staff_location_status ON StaffMembers(LocationId, Status, IsActive);

-- Customer + QueueEntry queries
CREATE INDEX IF NOT EXISTS idx_customer_phone_active ON Customers(PhoneNumber, IsActive);

-- =============================================
-- JSON Column Indexes (MySQL 8.0+)
-- =============================================

-- Organization LocationIds JSON index
CREATE INDEX IF NOT EXISTS idx_organizations_location_ids ON Organizations((CAST(LocationIds AS CHAR(50) ARRAY)));

-- Location StaffMemberIds JSON index
CREATE INDEX IF NOT EXISTS idx_locations_staff_member_ids ON Locations((CAST(StaffMemberIds AS CHAR(50) ARRAY)));

-- Location ServiceTypeIds JSON index
CREATE INDEX IF NOT EXISTS idx_locations_service_type_ids ON Locations((CAST(ServiceTypeIds AS CHAR(50) ARRAY)));

-- Customer FavoriteLocationIds JSON index
CREATE INDEX IF NOT EXISTS idx_customers_favorite_location_ids ON Customers((CAST(FavoriteLocationIds AS CHAR(50) ARRAY)));

-- StaffMember SpecialtyServiceTypeIds JSON index
CREATE INDEX IF NOT EXISTS idx_staff_specialty_service_type_ids ON StaffMembers((CAST(SpecialtyServiceTypeIds AS CHAR(50) ARRAY)));

-- =============================================
-- Full-Text Search Indexes
-- =============================================

-- Organization search
CREATE FULLTEXT INDEX IF NOT EXISTS idx_organizations_search ON Organizations(Name, Description);

-- Location search
CREATE FULLTEXT INDEX IF NOT EXISTS idx_locations_search ON Locations(Name, Address, City, State);

-- Customer search
CREATE FULLTEXT INDEX IF NOT EXISTS idx_customers_search ON Customers(FirstName, LastName, Email);

-- Staff search
CREATE FULLTEXT INDEX IF NOT EXISTS idx_staff_search ON StaffMembers(FirstName, LastName, Email);

-- =============================================
-- Performance Analysis Queries
-- =============================================

-- Check index usage
SELECT 
    TABLE_NAME,
    INDEX_NAME,
    CARDINALITY,
    SUB_PART,
    PACKED,
    NULLABLE,
    INDEX_TYPE
FROM information_schema.STATISTICS 
WHERE TABLE_SCHEMA = 'QueueHubDb'
ORDER BY TABLE_NAME, INDEX_NAME;

-- Check table sizes
SELECT 
    TABLE_NAME,
    ROUND(((DATA_LENGTH + INDEX_LENGTH) / 1024 / 1024), 2) AS 'Size (MB)',
    TABLE_ROWS
FROM information_schema.TABLES 
WHERE TABLE_SCHEMA = 'QueueHubDb'
ORDER BY (DATA_LENGTH + INDEX_LENGTH) DESC;

-- Check slow queries
SELECT 
    DIGEST_TEXT,
    COUNT_STAR,
    AVG_TIMER_WAIT/1000000000 AS avg_time_seconds,
    MAX_TIMER_WAIT/1000000000 AS max_time_seconds
FROM performance_schema.events_statements_summary_by_digest 
WHERE SCHEMA_NAME = 'QueueHubDb'
ORDER BY AVG_TIMER_WAIT DESC
LIMIT 10;
