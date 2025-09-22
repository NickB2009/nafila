-- ============================================================================
-- FINAL DATABASE SEEDING SCRIPT FOR MYSQL
-- This script creates comprehensive test data for the QueueHub system
-- Run this after migrations have been applied
-- ============================================================================

-- ============================================================================
-- STEP 1: CLEAR EXISTING DATA (in proper order due to foreign key constraints)
-- ============================================================================

SELECT 'Clearing existing data...' as Status;

-- Clear data in reverse dependency order
DELETE FROM QueueEntries;
DELETE FROM Queues;
DELETE FROM StaffMembers;
DELETE FROM ServicesOffered;
DELETE FROM CustomerServiceHistory;
DELETE FROM Customers;
DELETE FROM Locations;
DELETE FROM Organizations;
DELETE FROM SubscriptionPlans;
DELETE FROM Users;

SELECT 'Existing data cleared successfully.' as Status;

-- ============================================================================
-- STEP 2: INSERT SUBSCRIPTION PLANS (must come first)
-- ============================================================================

SELECT 'Inserting subscription plans...' as Status;

INSERT INTO SubscriptionPlans (
  Id, Name, Description, Price, MonthlyPriceAmount, MonthlyPriceCurrency, YearlyPriceAmount, YearlyPriceCurrency,
  IsActive, IsDefault, MaxLocations, MaxStaffPerLocation, IncludesAnalytics, IncludesAdvancedReporting, 
  IncludesCustomBranding, IncludesAdvertising, IncludesMultipleLocations, MaxQueueEntriesPerDay, IsFeatured,
  CreatedAt, CreatedBy, IsDeleted, RowVersion
)
VALUES
  ('77777777-7777-7777-7777-777777777771', 'Basic Plan', 'Perfect for single location barbershops starting their digital journey', 29.99, 29.99, 'USD', 299.99, 'USD', 1, 1, 1, 3, 0, 0, 0, 0, 0, 50, 0, NOW(), 'seed', 0, 0x0000000000000000),
  ('77777777-7777-7777-7777-777777777772', 'Professional Plan', 'Ideal for growing businesses with multiple staff members', 59.99, 59.99, 'USD', 599.99, 'USD', 1, 0, 1, 10, 1, 0, 1, 0, 0, 200, 1, NOW(), 'seed', 0, 0x0000000000000000),
  ('77777777-7777-7777-7777-777777777773', 'Business Plan', 'Advanced features for multi-location barbershop chains', 99.99, 99.99, 'USD', 999.99, 'USD', 1, 0, 5, 15, 1, 1, 1, 1, 1, 500, 1, NOW(), 'seed', 0, 0x0000000000000000);

SELECT 'Subscription plans inserted successfully.' as Status;

-- ============================================================================
-- STEP 3: INSERT USERS
-- ============================================================================

SELECT 'Inserting users...' as Status;

INSERT INTO Users (
  Id, Username, Email, PasswordHash, Role, LastLoginAt, IsLocked, RequiresTwoFactor, CreatedAt, CreatedBy, IsDeleted, RowVersion
)
VALUES
  ('55555555-5555-5555-5555-555555555551', 'platformadmin_test', 'platformadmin@test.com', '$2a$11$8qkrKVPZzSuQGKYvHJgRMuZRvXJgKZ1K8XJJzT4yE.FQtq8WzYkO6', 'PlatformAdmin', NULL, 0, 0, NOW(), 'seed', 0, 0x0000000000000000),
  ('55555555-5555-5555-5555-555555555552', 'owner_test', 'owner@test.com', '$2a$11$7nKLTUVsN1WqP2rJrJ5R1e8D8lxqJ2zQ5JjF5sN8xQ5xJ8xN5sQ8x', 'Owner', NULL, 0, 0, NOW(), 'seed', 0, 0x0000000000000000),
  ('55555555-5555-5555-5555-555555555553', 'staff_test', 'staff@test.com', '$2a$11$9mJsY3xP5vJ2mJ8mJ5sN8xQ5xJ8xN5sQ8x2zQ5JjF5sN8xQ5xJ8xN', 'Staff', NULL, 0, 0, NOW(), 'seed', 0, 0x0000000000000000),
  ('55555555-5555-5555-5555-555555555554', 'customer_test', 'customer@test.com', '$2a$11$6lIsX2yO4wI1lI7lI4rM7yP4yI7yM4rP7y1zP4IiE4rM7yP4yI7yM', 'Customer', NULL, 0, 0, NOW(), 'seed', 0, 0x0000000000000000),
  ('55555555-5555-5555-5555-555555555555', 'service_test', 'service@test.com', '$2a$11$5kHrW1zN3vH0kH6kH3qL6zO3zH6zL3qO6z0yO3HhD3qL6zO3zH6zL', 'ServiceAccount', NULL, 0, 0, NOW(), 'seed', 0, 0x0000000000000000);

SELECT 'Users inserted successfully.' as Status;

-- ============================================================================
-- STEP 4: INSERT ORGANIZATIONS
-- ============================================================================

SELECT 'Inserting organizations...' as Status;

INSERT INTO Organizations (
  Id, Name, Slug, Description, ContactEmail, ContactPhone, WebsiteUrl, SubscriptionPlanId, IsActive, SharesDataForAnalytics,
  BrandingPrimaryColor, BrandingSecondaryColor, BrandingLogoUrl, BrandingFaviconUrl, BrandingCompanyName, BrandingTagLine, BrandingFontFamily,
  LocationIds, CreatedAt, CreatedBy, IsDeleted, RowVersion
)
VALUES
  ('99999999-9999-9999-9999-999999999991', 'Grande Tech Barbershops', 'grande-tech-barbershops', 'Premium barbershop chain with modern styling', 'info@grandetech.com', '+1-555-0100', 'https://grandetech.com', '77777777-7777-7777-7777-777777777771', 1, 1,
   '#3B82F6', '#1E40AF', 'https://grandetech.com/logo.png', 'https://grandetech.com/favicon.ico', 'Grande Tech', 'Premium cuts, premium experience', 'Roboto, Arial, sans-serif',
   '["99999999-9999-9999-9999-999999999991","99999999-9999-9999-9999-999999999992"]', NOW(), 'seed', 0, 0x0000000000000000),
  ('99999999-9999-9999-9999-999999999992', 'Classic Cuts Co', 'classic-cuts-co', 'Traditional barbershop with old-school charm', 'hello@classiccuts.com', '+1-555-0200', 'https://classiccuts.com', '77777777-7777-7777-7777-777777777772', 1, 0,
   '#DC2626', '#991B1B', '', '', 'Classic Cuts Co', 'Traditional cuts, timeless style', 'Georgia, serif',
   '["99999999-9999-9999-9999-999999999993"]', NOW(), 'seed', 0, 0x0000000000000000);

SELECT 'Organizations inserted successfully.' as Status;

-- ============================================================================
-- STEP 5: INSERT LOCATIONS
-- ============================================================================

SELECT 'Inserting locations...' as Status;

INSERT INTO Locations (
  Id, Name, Slug, Description, OrganizationId, 
  AddressStreet, AddressNumber, AddressComplement, AddressNeighborhood, AddressCity, AddressState, AddressCountry, AddressPostalCode, AddressLatitude, AddressLongitude,
  ContactEmail, ContactPhone, IsQueueEnabled, MaxQueueSize, LateClientCapTimeInMinutes, IsActive, AverageServiceTimeInMinutes, LastAverageTimeReset,
  WeeklyBusinessHours, 
  CustomBrandingPrimaryColor, CustomBrandingSecondaryColor, CustomBrandingLogoUrl, CustomBrandingFaviconUrl, CustomBrandingCompanyName, CustomBrandingTagLine, CustomBrandingFontFamily,
  StaffMemberIds, ServiceTypeIds, AdvertisementIds,
  CreatedAt, CreatedBy, IsDeleted, RowVersion
)
VALUES
  ('99999999-9999-9999-9999-999999999991', 'Grande Tech Downtown', 'grande-tech-downtown', 'Our flagship downtown location', '99999999-9999-9999-9999-999999999991',
   'Main Street', '123', 'Suite 100', 'Downtown', 'São Paulo', 'SP', 'Brazil', '01310-100', -23.5505, -46.6333,
   'downtown@grandetech.com', '+1-555-0101', 1, 50, 15, 1, 35.0, NOW(),
   '{"monday":{"isOpen":true,"openTime":"08:00:00","closeTime":"20:00:00"},"tuesday":{"isOpen":true,"openTime":"08:00:00","closeTime":"20:00:00"},"wednesday":{"isOpen":true,"openTime":"08:00:00","closeTime":"20:00:00"},"thursday":{"isOpen":true,"openTime":"08:00:00","closeTime":"20:00:00"},"friday":{"isOpen":true,"openTime":"08:00:00","closeTime":"20:00:00"},"saturday":{"isOpen":true,"openTime":"08:00:00","closeTime":"20:00:00"},"sunday":{"isOpen":false,"openTime":null,"closeTime":null}}',
   NULL, NULL, NULL, NULL, NULL, NULL, NULL,
   '["66666666-6666-6666-6666-666666666661","66666666-6666-6666-6666-666666666662"]', '["88888888-8888-8888-8888-888888888881","88888888-8888-8888-8888-888888888882","88888888-8888-8888-8888-888888888883"]', '[]',
   NOW(), 'seed', 0, 0x0000000000000000),
  ('99999999-9999-9999-9999-999999999992', 'Grande Tech Mall', 'grande-tech-mall', 'Convenient mall location', '99999999-9999-9999-9999-999999999991',
   'Shopping Mall Blvd', '456', 'Store 205', 'Vila Madalena', 'São Paulo', 'SP', 'Brazil', '05426-200', -23.5518, -46.6944,
   'mall@grandetech.com', '+1-555-0102', 1, 30, 10, 1, 25.0, NOW(),
   '{"monday":{"isOpen":true,"openTime":"10:00:00","closeTime":"22:00:00"},"tuesday":{"isOpen":true,"openTime":"10:00:00","closeTime":"22:00:00"},"wednesday":{"isOpen":true,"openTime":"10:00:00","closeTime":"22:00:00"},"thursday":{"isOpen":true,"openTime":"10:00:00","closeTime":"22:00:00"},"friday":{"isOpen":true,"openTime":"10:00:00","closeTime":"22:00:00"},"saturday":{"isOpen":true,"openTime":"10:00:00","closeTime":"22:00:00"},"sunday":{"isOpen":true,"openTime":"10:00:00","closeTime":"22:00:00"}}',
   '#E11D48', '#BE185D', '', '', 'Grande Tech Mall', 'Quick cuts while you shop', 'Arial, sans-serif',
   '["66666666-6666-6666-6666-666666666663"]', '["88888888-8888-8888-8888-888888888884"]', '[]',
   NOW(), 'seed', 0, 0x0000000000000000),
  ('99999999-9999-9999-9999-999999999993', 'Classic Cuts Main', 'classic-cuts-main', 'Our original location since 1985', '99999999-9999-9999-9999-999999999992',
   'Heritage Avenue', '789', '', 'Historic District', 'Rio de Janeiro', 'RJ', 'Brazil', '20040-020', -22.9068, -43.1729,
   'main@classiccuts.com', '+1-555-0201', 1, 20, 20, 1, 45.0, NOW(),
   '{"monday":{"isOpen":true,"openTime":"09:00:00","closeTime":"18:00:00"},"tuesday":{"isOpen":true,"openTime":"09:00:00","closeTime":"18:00:00"},"wednesday":{"isOpen":true,"openTime":"09:00:00","closeTime":"18:00:00"},"thursday":{"isOpen":true,"openTime":"09:00:00","closeTime":"18:00:00"},"friday":{"isOpen":true,"openTime":"09:00:00","closeTime":"18:00:00"},"saturday":{"isOpen":true,"openTime":"09:00:00","closeTime":"18:00:00"},"sunday":{"isOpen":false,"openTime":null,"closeTime":null}}',
   NULL, NULL, NULL, NULL, NULL, NULL, NULL,
   '["66666666-6666-6666-6666-666666666664","66666666-6666-6666-6666-666666666665"]', '["88888888-8888-8888-8888-888888888885","88888888-8888-8888-8888-888888888886"]', '[]',
   NOW(), 'seed', 0, 0x0000000000000000);

SELECT 'Locations inserted successfully.' as Status;

-- ============================================================================
-- STEP 6: INSERT CUSTOMERS
-- ============================================================================

SELECT 'Inserting customers...' as Status;

INSERT INTO Customers (
  Id, Name, Email, PhoneNumber, IsAnonymous, NotificationsEnabled, PreferredNotificationChannel, UserId,
  FavoriteLocationIds, CreatedAt, CreatedBy, IsDeleted, RowVersion
)
VALUES
  ('88888888-8888-8888-8888-888888888881', 'João Silva', 'joao.silva@email.com', '+55-11-99999-0001', 0, 1, 'whatsapp', NULL,
   '["99999999-9999-9999-9999-999999999991"]', NOW(), 'seed', 0, 0x0000000000000000),
  ('88888888-8888-8888-8888-888888888882', 'Maria Santos', 'maria.santos@email.com', '+55-11-99999-0002', 0, 1, 'email', NULL,
   '["99999999-9999-9999-9999-999999999991","99999999-9999-9999-9999-999999999992"]', NOW(), 'seed', 0, 0x0000000000000000),
  ('88888888-8888-8888-8888-888888888883', 'Carlos Oliveira', 'carlos.oliveira@email.com', '+55-21-99999-0003', 0, 1, 'whatsapp', NULL,
   '["99999999-9999-9999-9999-999999999993"]', NOW(), 'seed', 0, 0x0000000000000000),
  ('88888888-8888-8888-8888-888888888884', 'Ana Costa', NULL, '+55-11-99999-0004', 0, 0, 'sms', NULL,
   '[]', NOW(), 'seed', 0, 0x0000000000000000),
  ('88888888-8888-8888-8888-888888888885', 'Walk-in Customer', NULL, NULL, 1, 0, 'none', NULL,
   '[]', NOW(), 'seed', 0, 0x0000000000000000);

SELECT 'Customers inserted successfully.' as Status;

-- ============================================================================
-- STEP 7: INSERT STAFF MEMBERS
-- ============================================================================

SELECT 'Inserting staff members...' as Status;

INSERT INTO StaffMembers (
  Id, Name, LocationId, Email, PhoneNumber, ProfilePictureUrl, Role, IsActive, IsOnDuty, StaffStatus, 
  UserId, AverageServiceTimeInMinutes, CompletedServicesCount, EmployeeCode, Username, SpecialtyServiceTypeIds,
  CreatedAt, CreatedBy, IsDeleted, RowVersion
)
VALUES
  ('66666666-6666-6666-6666-666666666661', 'Marco Silva', '99999999-9999-9999-9999-999999999991', 'marco.silva@grandetech.com', '+55-11-99998-0001', 'https://grandetech.com/staff/marco.jpg', 'Senior Barber', 1, 1, 'available', '55555555-5555-5555-5555-555555555553', 28.5, 156, 'EMP001', 'marco.silva', '["88888888-8888-8888-8888-888888888881","88888888-8888-8888-8888-888888888882"]', NOW(), 'seed', 0, 0x0000000000000000),
  ('66666666-6666-6666-6666-666666666662', 'Ana Costa', '99999999-9999-9999-9999-999999999991', 'ana.costa@grandetech.com', '+55-11-99998-0002', 'https://grandetech.com/staff/ana.jpg', 'Stylist', 1, 1, 'busy', NULL, 35.2, 89, 'EMP002', 'ana.costa', '["88888888-8888-8888-8888-888888888883"]', NOW(), 'seed', 0, 0x0000000000000000),
  ('66666666-6666-6666-6666-666666666663', 'Pedro Santos', '99999999-9999-9999-9999-999999999992', 'pedro.santos@grandetech.com', '+55-11-99998-0003', NULL, 'Barber', 1, 1, 'available', NULL, 22.8, 67, 'EMP003', 'pedro.santos', '["88888888-8888-8888-8888-888888888884"]', NOW(), 'seed', 0, 0x0000000000000000),
  ('66666666-6666-6666-6666-666666666664', 'Carlos Oliveira', '99999999-9999-9999-9999-999999999993', 'carlos.oliveira@classiccuts.com', '+55-21-99998-0004', 'https://classiccuts.com/staff/carlos.jpg', 'Master Barber', 1, 1, 'available', NULL, 45.0, 234, 'EMP004', 'carlos.oliveira', '["88888888-8888-8888-8888-888888888885","88888888-8888-8888-8888-888888888886"]', NOW(), 'seed', 0, 0x0000000000000000),
  ('66666666-6666-6666-6666-666666666665', 'Roberto Lima', '99999999-9999-9999-9999-999999999993', 'roberto.lima@classiccuts.com', '+55-21-99998-0005', NULL, 'Barber', 1, 0, 'off_duty', NULL, 38.5, 145, 'EMP005', 'roberto.lima', '["88888888-8888-8888-8888-888888888886"]', NOW(), 'seed', 0, 0x0000000000000000);

SELECT 'Staff members inserted successfully.' as Status;

-- ============================================================================
-- STEP 8: INSERT SERVICES OFFERED
-- ============================================================================

SELECT 'Inserting services offered...' as Status;

INSERT INTO ServicesOffered (
  Id, Name, Description, LocationId, EstimatedDurationMinutes, PriceAmount, PriceCurrency, ImageUrl, 
  IsActive, TimesProvided, ActualAverageDurationMinutes, CreatedAt, CreatedBy, IsDeleted, RowVersion
)
VALUES
  ('88888888-8888-8888-8888-888888888881', 'Classic Haircut', 'Traditional men''s haircut with scissors and clipper', '99999999-9999-9999-9999-999999999991', 30, 25.00, 'USD', 'https://grandetech.com/services/classic-cut.jpg', 1, 45, 28.5, NOW(), 'seed', 0, 0x0000000000000000),
  ('88888888-8888-8888-8888-888888888882', 'Beard Trim', 'Professional beard trimming and shaping', '99999999-9999-9999-9999-999999999991', 20, 15.00, 'USD', 'https://grandetech.com/services/beard-trim.jpg', 1, 32, 18.2, NOW(), 'seed', 0, 0x0000000000000000),
  ('88888888-8888-8888-8888-888888888883', 'Premium Styling', 'Full styling service with wash, cut, and finish', '99999999-9999-9999-9999-999999999991', 45, 45.00, 'USD', 'https://grandetech.com/services/premium-styling.jpg', 1, 28, 42.8, NOW(), 'seed', 0, 0x0000000000000000),
  ('88888888-8888-8888-8888-888888888884', 'Quick Trim', 'Fast 15-minute cleanup cut', '99999999-9999-9999-9999-999999999992', 15, 18.00, 'USD', NULL, 1, 67, 14.5, NOW(), 'seed', 0, 0x0000000000000000),
  ('88888888-8888-8888-8888-888888888885', 'Traditional Shave', 'Classic straight razor shave with hot towel', '99999999-9999-9999-9999-999999999993', 35, 30.00, 'USD', 'https://classiccuts.com/services/traditional-shave.jpg', 1, 23, 38.2, NOW(), 'seed', 0, 0x0000000000000000),
  ('88888888-8888-8888-8888-888888888886', 'Father & Son Cut', 'Special package for father and son haircuts', '99999999-9999-9999-9999-999999999993', 50, 40.00, 'USD', NULL, 1, 15, 52.5, NOW(), 'seed', 0, 0x0000000000000000);

SELECT 'Services offered inserted successfully.' as Status;

-- ============================================================================
-- STEP 9: INSERT QUEUES
-- ============================================================================

SELECT 'Inserting queues...' as Status;

INSERT INTO Queues (
  Id, LocationId, QueueDate, IsActive, MaxSize, LateClientCapTimeInMinutes, CreatedAt, CreatedBy, IsDeleted, RowVersion
)
VALUES
  ('11111111-1111-1111-1111-111111111111', '99999999-9999-9999-9999-999999999991', CAST(NOW() AS DATE), 1, 50, 15, NOW(), 'seed', 0, 0x0000000000000000),
  ('11111111-1111-1111-1111-111111111112', '99999999-9999-9999-9999-999999999992', CAST(NOW() AS DATE), 1, 30, 10, NOW(), 'seed', 0, 0x0000000000000000),
  ('11111111-1111-1111-1111-111111111113', '99999999-9999-9999-9999-999999999993', CAST(NOW() AS DATE), 1, 20, 20, NOW(), 'seed', 0, 0x0000000000000000);

SELECT 'Queues inserted successfully.' as Status;

-- ============================================================================
-- STEP 10: INSERT QUEUE ENTRIES
-- ============================================================================

SELECT 'Inserting queue entries...' as Status;

INSERT INTO QueueEntries (
  Id, QueueId, CustomerId, CustomerName, Position, Status, StaffMemberId, ServiceTypeId, Notes, EnteredAt, CalledAt, CheckedInAt, CompletedAt, CancelledAt, CreatedAt, CreatedBy, IsDeleted, RowVersion
)
VALUES
  ('22222222-2222-2222-2222-222222222221', '11111111-1111-1111-1111-111111111111', '88888888-8888-8888-8888-888888888881', 'João Silva', 1, 'Waiting', NULL, '88888888-8888-8888-8888-888888888881', 'Regular haircut', NOW(), NULL, NULL, NULL, NULL, NOW(), 'seed', 0, 0x0000000000000000),
  ('22222222-2222-2222-2222-222222222222', '11111111-1111-1111-1111-111111111111', '88888888-8888-8888-8888-888888888882', 'Maria Santos', 2, 'Waiting', NULL, '88888888-8888-8888-8888-888888888883', 'Wash and cut', NOW(), NULL, NULL, NULL, NULL, NOW(), 'seed', 0, 0x0000000000000000),
  ('22222222-2222-2222-2222-222222222223', '11111111-1111-1111-1111-111111111112', '88888888-8888-8888-8888-888888888883', 'Carlos Oliveira', 1, 'CheckedIn', '66666666-6666-6666-6666-666666666663', '88888888-8888-8888-8888-888888888884', 'VIP service', DATE_SUB(NOW(), INTERVAL 10 MINUTE), DATE_SUB(NOW(), INTERVAL 8 MINUTE), DATE_SUB(NOW(), INTERVAL 5 MINUTE), NULL, NULL, NOW(), 'seed', 0, 0x0000000000000000),
  ('22222222-2222-2222-2222-222222222224', '11111111-1111-1111-1111-111111111113', '88888888-8888-8888-8888-888888888884', 'Ana Costa', 1, 'Waiting', NULL, '88888888-8888-8888-8888-888888888885', 'Appointment scheduled', NOW(), NULL, NULL, NULL, NULL, NOW(), 'seed', 0, 0x0000000000000000),
  ('22222222-2222-2222-2222-222222222225', '11111111-1111-1111-1111-111111111111', '88888888-8888-8888-8888-888888888885', 'Walk-in Customer', 3, 'Waiting', NULL, '88888888-8888-8888-8888-888888888882', 'Quick trim', NOW(), NULL, NULL, NULL, NULL, NOW(), 'seed', 0, 0x0000000000000000),
  ('22222222-2222-2222-2222-222222222226', '11111111-1111-1111-1111-111111111111', NULL, 'Anonymous Customer', 4, 'Called', '66666666-6666-6666-6666-666666666661', '88888888-8888-8888-8888-888888888881', 'Walk-in customer', DATE_SUB(NOW(), INTERVAL 15 MINUTE), DATE_SUB(NOW(), INTERVAL 2 MINUTE), NULL, NULL, NULL, NOW(), 'seed', 0, 0x0000000000000000),
  ('22222222-2222-2222-2222-222222222227', '11111111-1111-1111-1111-111111111112', NULL, 'Another Customer', 2, 'Completed', '66666666-6666-6666-6666-666666666663', '88888888-8888-8888-8888-888888888884', 'Completed service', DATE_SUB(NOW(), INTERVAL 45 MINUTE), DATE_SUB(NOW(), INTERVAL 40 MINUTE), DATE_SUB(NOW(), INTERVAL 38 MINUTE), DATE_SUB(NOW(), INTERVAL 15 MINUTE), NULL, NOW(), 'seed', 0, 0x0000000000000000);

SELECT 'Queue entries inserted successfully.' as Status;

-- ============================================================================
-- STEP 11: INSERT FOREIGN KEY CONSTRAINTS (with error handling)
-- ============================================================================

SELECT 'Adding foreign key constraints...' as Status;

-- Drop existing constraints if they exist, then recreate them
SET @sql = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
     WHERE TABLE_SCHEMA = DATABASE() 
     AND TABLE_NAME = 'Organizations' 
     AND CONSTRAINT_NAME = 'FK_Organizations_SubscriptionPlans_SubscriptionPlanId') > 0,
    'ALTER TABLE `Organizations` DROP FOREIGN KEY `FK_Organizations_SubscriptionPlans_SubscriptionPlanId`',
    'SELECT "Constraint does not exist"'
));
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

ALTER TABLE `Organizations` 
ADD CONSTRAINT `FK_Organizations_SubscriptionPlans_SubscriptionPlanId` 
FOREIGN KEY (`SubscriptionPlanId`) REFERENCES `SubscriptionPlans` (`Id`) ON DELETE RESTRICT;

-- Drop and recreate Locations constraint
SET @sql = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
     WHERE TABLE_SCHEMA = DATABASE() 
     AND TABLE_NAME = 'Locations' 
     AND CONSTRAINT_NAME = 'FK_Locations_Organizations_OrganizationId') > 0,
    'ALTER TABLE `Locations` DROP FOREIGN KEY `FK_Locations_Organizations_OrganizationId`',
    'SELECT "Constraint does not exist"'
));
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

ALTER TABLE `Locations` 
ADD CONSTRAINT `FK_Locations_Organizations_OrganizationId` 
FOREIGN KEY (`OrganizationId`) REFERENCES `Organizations` (`Id`) ON DELETE RESTRICT;

-- Drop and recreate ServicesOffered constraint
SET @sql = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
     WHERE TABLE_SCHEMA = DATABASE() 
     AND TABLE_NAME = 'ServicesOffered' 
     AND CONSTRAINT_NAME = 'FK_ServicesOffered_Locations_LocationId') > 0,
    'ALTER TABLE `ServicesOffered` DROP FOREIGN KEY `FK_ServicesOffered_Locations_LocationId`',
    'SELECT "Constraint does not exist"'
));
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

ALTER TABLE `ServicesOffered` 
ADD CONSTRAINT `FK_ServicesOffered_Locations_LocationId` 
FOREIGN KEY (`LocationId`) REFERENCES `Locations` (`Id`) ON DELETE RESTRICT;

-- Drop and recreate StaffMembers constraint
SET @sql = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
     WHERE TABLE_SCHEMA = DATABASE() 
     AND TABLE_NAME = 'StaffMembers' 
     AND CONSTRAINT_NAME = 'FK_StaffMembers_Locations_LocationId') > 0,
    'ALTER TABLE `StaffMembers` DROP FOREIGN KEY `FK_StaffMembers_Locations_LocationId`',
    'SELECT "Constraint does not exist"'
));
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

ALTER TABLE `StaffMembers` 
ADD CONSTRAINT `FK_StaffMembers_Locations_LocationId` 
FOREIGN KEY (`LocationId`) REFERENCES `Locations` (`Id`) ON DELETE RESTRICT;

-- Drop and recreate Queues constraint
SET @sql = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
     WHERE TABLE_SCHEMA = DATABASE() 
     AND TABLE_NAME = 'Queues' 
     AND CONSTRAINT_NAME = 'FK_Queues_Locations_LocationId') > 0,
    'ALTER TABLE `Queues` DROP FOREIGN KEY `FK_Queues_Locations_LocationId`',
    'SELECT "Constraint does not exist"'
));
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

ALTER TABLE `Queues` 
ADD CONSTRAINT `FK_Queues_Locations_LocationId` 
FOREIGN KEY (`LocationId`) REFERENCES `Locations` (`Id`) ON DELETE RESTRICT;

-- Drop and recreate QueueEntries constraints
SET @sql = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
     WHERE TABLE_SCHEMA = DATABASE() 
     AND TABLE_NAME = 'QueueEntries' 
     AND CONSTRAINT_NAME = 'FK_QueueEntries_Queues_QueueId') > 0,
    'ALTER TABLE `QueueEntries` DROP FOREIGN KEY `FK_QueueEntries_Queues_QueueId`',
    'SELECT "Constraint does not exist"'
));
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

ALTER TABLE `QueueEntries` 
ADD CONSTRAINT `FK_QueueEntries_Queues_QueueId` 
FOREIGN KEY (`QueueId`) REFERENCES `Queues` (`Id`) ON DELETE RESTRICT;

SET @sql = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
     WHERE TABLE_SCHEMA = DATABASE() 
     AND TABLE_NAME = 'QueueEntries' 
     AND CONSTRAINT_NAME = 'FK_QueueEntries_Customers_CustomerId') > 0,
    'ALTER TABLE `QueueEntries` DROP FOREIGN KEY `FK_QueueEntries_Customers_CustomerId`',
    'SELECT "Constraint does not exist"'
));
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

ALTER TABLE `QueueEntries` 
ADD CONSTRAINT `FK_QueueEntries_Customers_CustomerId` 
FOREIGN KEY (`CustomerId`) REFERENCES `Customers` (`Id`) ON DELETE RESTRICT;

SET @sql = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
     WHERE TABLE_SCHEMA = DATABASE() 
     AND TABLE_NAME = 'QueueEntries' 
     AND CONSTRAINT_NAME = 'FK_QueueEntries_StaffMembers_StaffMemberId') > 0,
    'ALTER TABLE `QueueEntries` DROP FOREIGN KEY `FK_QueueEntries_StaffMembers_StaffMemberId`',
    'SELECT "Constraint does not exist"'
));
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

ALTER TABLE `QueueEntries` 
ADD CONSTRAINT `FK_QueueEntries_StaffMembers_StaffMemberId` 
FOREIGN KEY (`StaffMemberId`) REFERENCES `StaffMembers` (`Id`) ON DELETE SET NULL;

-- Drop and recreate CustomerServiceHistory constraint
SET @sql = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
     WHERE TABLE_SCHEMA = DATABASE() 
     AND TABLE_NAME = 'CustomerServiceHistory' 
     AND CONSTRAINT_NAME = 'FK_CustomerServiceHistory_Customers_CustomerId') > 0,
    'ALTER TABLE `CustomerServiceHistory` DROP FOREIGN KEY `FK_CustomerServiceHistory_Customers_CustomerId`',
    'SELECT "Constraint does not exist"'
));
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

ALTER TABLE `CustomerServiceHistory` 
ADD CONSTRAINT `FK_CustomerServiceHistory_Customers_CustomerId` 
FOREIGN KEY (`CustomerId`) REFERENCES `Customers` (`Id`) ON DELETE RESTRICT;

SELECT 'Foreign key constraints added successfully.' as Status;

-- ============================================================================
-- STEP 12: VERIFICATION
-- ============================================================================

SELECT 'Verifying data insertion...' as Status;

-- Check counts
SELECT 'SubscriptionPlans' as TableName, COUNT(*) as RecordCount FROM SubscriptionPlans WHERE IsDeleted = 0
UNION ALL
SELECT 'Users' as TableName, COUNT(*) as RecordCount FROM Users WHERE IsDeleted = 0
UNION ALL
SELECT 'Organizations' as TableName, COUNT(*) as RecordCount FROM Organizations WHERE IsDeleted = 0
UNION ALL
SELECT 'Locations' as TableName, COUNT(*) as RecordCount FROM Locations WHERE IsDeleted = 0
UNION ALL
SELECT 'Customers' as TableName, COUNT(*) as RecordCount FROM Customers WHERE IsDeleted = 0
UNION ALL
SELECT 'StaffMembers' as TableName, COUNT(*) as RecordCount FROM StaffMembers WHERE IsDeleted = 0
UNION ALL
SELECT 'ServicesOffered' as TableName, COUNT(*) as RecordCount FROM ServicesOffered WHERE IsDeleted = 0
UNION ALL
SELECT 'Queues' as TableName, COUNT(*) as RecordCount FROM Queues WHERE IsDeleted = 0
UNION ALL
SELECT 'QueueEntries' as TableName, COUNT(*) as RecordCount FROM QueueEntries WHERE IsDeleted = 0;

SELECT 'Database seeding completed successfully!' as Status;
SELECT 'Total records inserted:' as Status;
SELECT '- 3 Subscription Plans' as Status;
SELECT '- 5 Users' as Status;
SELECT '- 2 Organizations' as Status;
SELECT '- 3 Locations' as Status;
SELECT '- 5 Customers' as Status;
SELECT '- 5 Staff Members' as Status;
SELECT '- 6 Services Offered' as Status;
SELECT '- 3 Queues' as Status;
SELECT '- 7 Queue Entries' as Status;
SELECT 'You can now test the API endpoints with this comprehensive test data!' as Status;
