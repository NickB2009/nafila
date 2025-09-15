-- MySQL-Compatible Seed Data for QueueHub
-- Converted from SQL Server syntax

USE QueueHubDb;

-- Clear existing data (in proper order due to foreign key constraints)
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

-- Insert Subscription Plans (required first)
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

-- Insert Users
INSERT INTO Users (
  Id, FullName, Email, PhoneNumber, PasswordHash, Role, IsActive, IsLocked, RequiresTwoFactor, CreatedAt, CreatedBy, IsDeleted, RowVersion
)
VALUES
  ('11111111-1111-1111-1111-111111111111', 'Admin User', 'admin@queuehub.com', '+1234567890', '$2a$11$8K9K7K6K5K4K3K2K1K0K9K8K7K6K5K4K3K2K1K0K9K8K7K6K5K4K3K2K1', 'PlatformAdmin', 1, 0, 1, NOW(), 'seed', 0, 0x0000000000000000),
  ('22222222-2222-2222-2222-222222222222', 'Salon Owner', 'owner@barbershop.com', '+1234567891', '$2a$11$8K9K7K6K5K4K3K2K1K0K9K8K7K6K5K4K3K2K1K0K9K8K7K6K5K4K3K2K2', 'Owner', 1, 0, 0, NOW(), 'seed', 0, 0x0000000000000000);

-- Insert Organizations  
INSERT INTO Organizations (
  Id, Name, Slug, Description, ContactEmail, ContactPhone, WebsiteUrl,
  BrandingPrimaryColor, BrandingSecondaryColor, BrandingLogoUrl, BrandingFaviconUrl, 
  BrandingCompanyName, BrandingTagLine, BrandingFontFamily,
  SubscriptionPlanId, IsActive, SharesDataForAnalytics,
  LocationIds, CreatedAt, CreatedBy, IsDeleted, RowVersion
)
VALUES
  ('33333333-3333-3333-3333-333333333333', 'Elite Barbershop', 'elite-barbershop', 'Premium barbershop with traditional and modern services', 'info@elitebarbershop.com', '+1234567892', 'https://elitebarbershop.com',
   '#8B4513', '#D2691E', 'https://elitebarbershop.com/logo.png', 'https://elitebarbershop.com/favicon.ico',
   'Elite Barbershop', 'Where Style Meets Tradition', 'Arial',
   '77777777-7777-7777-7777-777777777772', 1, 1,
   '[]', NOW(), 'seed', 0, 0x0000000000000000),
  ('44444444-4444-4444-4444-444444444444', 'Quick Cuts', 'quick-cuts', 'Fast and affordable haircuts for busy people', 'contact@quickcuts.com', '+1234567893', 'https://quickcuts.com',
   '#FF6B35', '#004E89', 'https://quickcuts.com/logo.png', 'https://quickcuts.com/favicon.ico',
   'Quick Cuts', 'Fast. Fresh. Affordable.', 'Helvetica',
   '77777777-7777-7777-7777-777777777771', 1, 0,
   '[]', NOW(), 'seed', 0, 0x0000000000000000);

-- Insert Locations
INSERT INTO Locations (
  Id, Name, Slug, Description, OrganizationId, IsQueueEnabled, MaxQueueSize, 
  LateClientCapTimeInMinutes, IsActive, AverageServiceTimeInMinutes, LastAverageTimeReset,
  AddressStreet, AddressNumber, AddressNeighborhood, AddressCity, AddressState, AddressCountry, AddressPostalCode,
  ContactEmail, ContactPhone, WeeklyBusinessHours,
  StaffMemberIds, ServiceTypeIds, AdvertisementIds,
  CreatedAt, CreatedBy, IsDeleted, RowVersion
)
VALUES
  ('55555555-5555-5555-5555-555555555555', 'Elite Barbershop Downtown', 'elite-downtown', 'Our flagship location in the heart of downtown', '33333333-3333-3333-3333-333333333333', 1, 20, 10, 1, 30.0, NOW(),
   'Main Street', '123', 'Downtown', 'New York', 'NY', 'USA', '10001',
   'downtown@elitebarbershop.com', '+1234567894', '{"monday":{"start":"09:00","end":"18:00","isOpen":true},"tuesday":{"start":"09:00","end":"18:00","isOpen":true},"wednesday":{"start":"09:00","end":"18:00","isOpen":true},"thursday":{"start":"09:00","end":"18:00","isOpen":true},"friday":{"start":"09:00","end":"20:00","isOpen":true},"saturday":{"start":"08:00","end":"17:00","isOpen":true},"sunday":{"start":"10:00","end":"16:00","isOpen":true}}',
   '[]', '[]', '[]',
   NOW(), 'seed', 0, 0x0000000000000000),
  ('66666666-6666-6666-6666-666666666666', 'Quick Cuts Express', 'quick-cuts-express', 'Fast service location for busy professionals', '44444444-4444-4444-4444-444444444444', 1, 15, 5, 1, 15.0, NOW(),
   'Business District', '456', 'Financial Quarter', 'New York', 'NY', 'USA', '10005',
   'express@quickcuts.com', '+1234567895', '{"monday":{"start":"07:00","end":"19:00","isOpen":true},"tuesday":{"start":"07:00","end":"19:00","isOpen":true},"wednesday":{"start":"07:00","end":"19:00","isOpen":true},"thursday":{"start":"07:00","end":"19:00","isOpen":true},"friday":{"start":"07:00","end":"19:00","isOpen":true},"saturday":{"start":"08:00","end":"16:00","isOpen":true},"sunday":{"start":"09:00","end":"15:00","isOpen":true}}',
   '[]', '[]', '[]',
   NOW(), 'seed', 0, 0x0000000000000000);

-- Insert Services Offered
INSERT INTO ServicesOffered (
  Id, Name, Description, LocationId, EstimatedDurationMinutes, ImageUrl, IsActive, TimesProvided, ActualAverageDurationMinutes,
  PriceAmount, PriceCurrency, CreatedAt, CreatedBy, IsDeleted, RowVersion
)
VALUES
  ('77777777-aaaa-aaaa-aaaa-777777777777', 'Classic Haircut', 'Traditional barbershop haircut with scissor finish', '55555555-5555-5555-5555-555555555555', 30, NULL, 1, 150, 28.5, 35.00, 'USD', NOW(), 'seed', 0, 0x0000000000000000),
  ('77777777-bbbb-bbbb-bbbb-777777777777', 'Beard Trim', 'Professional beard shaping and trimming', '55555555-5555-5555-5555-555555555555', 15, NULL, 1, 89, 12.3, 20.00, 'USD', NOW(), 'seed', 0, 0x0000000000000000),
  ('77777777-cccc-cccc-cccc-777777777777', 'Quick Cut', 'Fast professional haircut for busy schedules', '66666666-6666-6666-6666-666666666666', 15, NULL, 1, 200, 14.2, 25.00, 'USD', NOW(), 'seed', 0, 0x0000000000000000),
  ('77777777-dddd-dddd-dddd-777777777777', 'Express Trim', 'Basic trim and cleanup', '66666666-6666-6666-6666-666666666666', 10, NULL, 1, 180, 9.8, 15.00, 'USD', NOW(), 'seed', 0, 0x0000000000000000);

-- Insert Staff Members
INSERT INTO StaffMembers (
  Id, Name, LocationId, Email, PhoneNumber, Role, IsActive, IsAvailable, 
  HourlyRate, CreatedAt, CreatedBy, IsDeleted, RowVersion
)
VALUES
  ('88888888-8888-8888-8888-888888888888', 'Tony Rodriguez', '55555555-5555-5555-5555-555555555555', 'tony@elitebarbershop.com', '+1234567896', 'Senior Barber', 1, 1, 25.00, NOW(), 'seed', 0, 0x0000000000000000),
  ('99999999-9999-9999-9999-999999999999', 'Mike Johnson', '66666666-6666-6666-6666-666666666666', 'mike@quickcuts.com', '+1234567897', 'Express Stylist', 1, 1, 20.00, NOW(), 'seed', 0, 0x0000000000000000);

-- Insert Customers
INSERT INTO Customers (
  Id, Name, Email, PhoneNumber, IsAnonymous, NotificationsEnabled, PreferredNotificationChannel,
  CreatedAt, CreatedBy, IsDeleted, RowVersion
)
VALUES
  ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'John Smith', 'john@example.com', '+1234567898', 0, 1, 'sms', NOW(), 'seed', 0, 0x0000000000000000),
  ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Anonymous Customer', NULL, '+1234567899', 1, 0, 'none', NOW(), 'seed', 0, 0x0000000000000000);

-- Insert Queues  
INSERT INTO Queues (
  Id, Name, LocationId, ServiceTypeId, Status, MaxSize, EstimatedWaitTimeMinutes, IsActive,
  CreatedAt, CreatedBy, IsDeleted, RowVersion
)
VALUES
  ('cccccccc-cccc-cccc-cccc-cccccccccccc', 'Main Queue', '55555555-5555-5555-5555-555555555555', '77777777-aaaa-aaaa-aaaa-777777777777', 'Active', 20, 45, 1, NOW(), 'seed', 0, 0x0000000000000000),
  ('dddddddd-dddd-dddd-dddd-dddddddddddd', 'Express Queue', '66666666-6666-6666-6666-666666666666', '77777777-cccc-cccc-cccc-777777777777', 'Active', 15, 20, 1, NOW(), 'seed', 0, 0x0000000000000000);
