-- COMPLETE SEEDING: Now includes all major entities with proper value object support

-- Clear existing data (delete in order due to foreign key constraints)
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

-- Insert test data for SubscriptionPlans (must come first due to foreign key references)
INSERT INTO SubscriptionPlans (
  Id, Name, Description, Price, MonthlyPriceAmount, MonthlyPriceCurrency, YearlyPriceAmount, YearlyPriceCurrency,
  IsActive, IsDefault, MaxLocations, MaxStaffPerLocation, IncludesAnalytics, IncludesAdvancedReporting, 
  IncludesCustomBranding, IncludesAdvertising, IncludesMultipleLocations, MaxQueueEntriesPerDay, IsFeatured,
  CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy, IsDeleted
)
VALUES
  ('77777777-7777-7777-7777-777777777771', 'Basic Plan', 'Perfect for single location barbershops starting their digital journey', 29.99, 29.99, 'USD', 299.99, 'USD', 1, 1, 1, 3, 0, 0, 0, 0, 0, 50, 0, GETDATE(), 'seed', GETDATE(), 'seed', 0),
  ('77777777-7777-7777-7777-777777777772', 'Professional Plan', 'Ideal for growing businesses with multiple staff members', 59.99, 59.99, 'USD', 599.99, 'USD', 1, 0, 1, 10, 1, 0, 1, 0, 0, 200, 1, GETDATE(), 'seed', GETDATE(), 'seed', 0),
  ('77777777-7777-7777-7777-777777777773', 'Business Plan', 'Advanced features for multi-location barbershop chains', 99.99, 99.99, 'USD', 999.99, 'USD', 1, 0, 5, 15, 1, 1, 1, 1, 1, 500, 1, GETDATE(), 'seed', GETDATE(), 'seed', 0),
  ('77777777-7777-7777-7777-777777777774', 'Enterprise Plan', 'Full-featured solution for large barbershop enterprises', 199.99, 199.99, 'USD', 1999.99, 'USD', 1, 0, 99, 50, 1, 1, 1, 1, 1, 9999, 0, GETDATE(), 'seed', GETDATE(), 'seed', 0);

-- Insert test data for Users
INSERT INTO Users (
  Id, Username, Email, PasswordHash, Role, LastLoginAt, IsLocked, RequiresTwoFactor, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy, IsDeleted
)
VALUES
  ('55555555-5555-5555-5555-555555555551', 'platformadmin_test', 'platformadmin@test.com', '$2a$11$8qkrKVPZzSuQGKYvHJgRMuZRvXJgKZ1K8XJJzT4yE.FQtq8WzYkO6', 'PlatformAdmin', NULL, 0, 0, GETDATE(), 'seed', GETDATE(), 'seed', 0),
  ('55555555-5555-5555-5555-555555555552', 'owner_test', 'owner@test.com', '$2a$11$7nKLTUVsN1WqP2rJrJ5R1e8D8lxqJ2zQ5JjF5sN8xQ5xJ8xN5sQ8x', 'Owner', NULL, 0, 0, GETDATE(), 'seed', GETDATE(), 'seed', 0),
  ('55555555-5555-5555-5555-555555555553', 'staff_test', 'staff@test.com', '$2a$11$9mJsY3xP5vJ2mJ8mJ5sN8xQ5xJ8xN5sQ8x2zQ5JjF5sN8xQ5xJ8xN', 'Staff', NULL, 0, 0, GETDATE(), 'seed', GETDATE(), 'seed', 0),
  ('55555555-5555-5555-5555-555555555554', 'customer_test', 'customer@test.com', '$2a$11$6lIsX2yO4wI1lI7lI4rM7yP4yI7yM4rP7y1zP4IiE4rM7yP4yI7yM', 'Customer', NULL, 0, 0, GETDATE(), 'seed', GETDATE(), 'seed', 0),
  ('55555555-5555-5555-5555-555555555555', 'service_test', 'service@test.com', '$2a$11$5kHrW1zN3vH0kH6kH3qL6zO3zH6zL3qO6z0yO3HhD3qL6zO3zH6zL', 'ServiceAccount', NULL, 0, 0, GETDATE(), 'seed', GETDATE(), 'seed', 0);

-- Insert test data for Organizations
INSERT INTO Organizations (
  Id, Name, Slug, Description, ContactEmail, ContactPhone, WebsiteUrl, SubscriptionPlanId, IsActive, SharesDataForAnalytics,
  BrandingPrimaryColor, BrandingSecondaryColor, BrandingLogoUrl, BrandingFaviconUrl, BrandingCompanyName, BrandingTagLine, BrandingFontFamily,
  LocationIds, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy, IsDeleted
)
VALUES
  ('99999999-9999-9999-9999-999999999991', 'Grande Tech Barbershops', 'grande-tech-barbershops', 'Premium barbershop chain with modern styling', 'info@grandetech.com', '+1-555-0100', 'https://grandetech.com', '77777777-7777-7777-7777-777777777771', 1, 1,
   '#3B82F6', '#1E40AF', 'https://grandetech.com/logo.png', 'https://grandetech.com/favicon.ico', 'Grande Tech', 'Premium cuts, premium experience', 'Roboto, Arial, sans-serif',
   '["99999999-9999-9999-9999-999999999991","99999999-9999-9999-9999-999999999992"]', GETDATE(), 'seed', GETDATE(), 'seed', 0),
  ('99999999-9999-9999-9999-999999999992', 'Classic Cuts Co', 'classic-cuts-co', 'Traditional barbershop with old-school charm', 'hello@classiccuts.com', '+1-555-0200', 'https://classiccuts.com', '77777777-7777-7777-7777-777777777772', 1, 0,
   '#DC2626', '#991B1B', '', '', 'Classic Cuts Co', 'Traditional cuts, timeless style', 'Georgia, serif',
   '["99999999-9999-9999-9999-999999999993"]', GETDATE(), 'seed', GETDATE(), 'seed', 0);

-- Insert test data for Locations
INSERT INTO Locations (
  Id, Name, Slug, Description, OrganizationId, 
  AddressStreet, AddressNumber, AddressComplement, AddressNeighborhood, AddressCity, AddressState, AddressCountry, AddressPostalCode, AddressLatitude, AddressLongitude,
  ContactEmail, ContactPhone, IsQueueEnabled, MaxQueueSize, LateClientCapTimeInMinutes, IsActive, AverageServiceTimeInMinutes, LastAverageTimeReset,
  BusinessHoursStart, BusinessHoursEnd, 
  CustomBrandingPrimaryColor, CustomBrandingSecondaryColor, CustomBrandingLogoUrl, CustomBrandingFaviconUrl, CustomBrandingCompanyName, CustomBrandingTagLine, CustomBrandingFontFamily,
  StaffMemberIds, ServiceTypeIds, AdvertisementIds,
  CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy, IsDeleted
)
VALUES
  ('99999999-9999-9999-9999-999999999991', 'Grande Tech Downtown', 'grande-tech-downtown', 'Our flagship downtown location', '99999999-9999-9999-9999-999999999991',
   'Main Street', '123', 'Suite 100', 'Downtown', 'São Paulo', 'SP', 'Brazil', '01310-100', -23.5505, -46.6333,
   'downtown@grandetech.com', '+1-555-0101', 1, 50, 15, 1, 35.0, GETDATE(),
   '08:00:00', '20:00:00',
   NULL, NULL, NULL, NULL, NULL, NULL, NULL,
   '["66666666-6666-6666-6666-666666666661","66666666-6666-6666-6666-666666666662"]', '["88888888-8888-8888-8888-888888888881","88888888-8888-8888-8888-888888888882"]', '[]',
   GETDATE(), 'seed', GETDATE(), 'seed', 0),
  ('99999999-9999-9999-9999-999999999992', 'Grande Tech Mall', 'grande-tech-mall', 'Convenient mall location', '99999999-9999-9999-9999-999999999991',
   'Shopping Mall Blvd', '456', 'Store 205', 'Vila Madalena', 'São Paulo', 'SP', 'Brazil', '05426-200', -23.5518, -46.6944,
   'mall@grandetech.com', '+1-555-0102', 1, 30, 10, 1, 25.0, GETDATE(),
   '10:00:00', '22:00:00',
   '#E11D48', '#BE185D', '', '', 'Grande Tech Mall', 'Quick cuts while you shop', 'Arial, sans-serif',
   '["66666666-6666-6666-6666-666666666663"]', '["88888888-8888-8888-8888-888888888881"]', '[]',
   GETDATE(), 'seed', GETDATE(), 'seed', 0),
  ('99999999-9999-9999-9999-999999999993', 'Classic Cuts Main', 'classic-cuts-main', 'Our original location since 1985', '99999999-9999-9999-9999-999999999992',
   'Heritage Avenue', '789', '', 'Historic District', 'Rio de Janeiro', 'RJ', 'Brazil', '20040-020', -22.9068, -43.1729,
   'main@classiccuts.com', '+1-555-0201', 1, 20, 20, 1, 45.0, GETDATE(),
   '09:00:00', '18:00:00',
   NULL, NULL, NULL, NULL, NULL, NULL, NULL,
   '["66666666-6666-6666-6666-666666666664","66666666-6666-6666-6666-666666666665"]', '["88888888-8888-8888-8888-888888888883","88888888-8888-8888-8888-888888888884"]', '[]',
   GETDATE(), 'seed', GETDATE(), 'seed', 0);

-- Insert test data for Customers
INSERT INTO Customers (
  Id, Name, Email, PhoneNumber, IsAnonymous, NotificationsEnabled, PreferredNotificationChannel, UserId,
  FavoriteLocationIds, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy, IsDeleted
)
VALUES
  ('88888888-8888-8888-8888-888888888881', 'João Silva', 'joao.silva@email.com', '+55-11-99999-0001', 0, 1, 'whatsapp', NULL,
   '["99999999-9999-9999-9999-999999999991"]', GETDATE(), 'seed', GETDATE(), 'seed', 0),
  ('88888888-8888-8888-8888-888888888882', 'Maria Santos', 'maria.santos@email.com', '+55-11-99999-0002', 0, 1, 'email', NULL,
   '["99999999-9999-9999-9999-999999999991","99999999-9999-9999-9999-999999999992"]', GETDATE(), 'seed', GETDATE(), 'seed', 0),
  ('88888888-8888-8888-8888-888888888883', 'Carlos Oliveira', 'carlos.oliveira@email.com', '+55-21-99999-0003', 0, 1, 'whatsapp', NULL,
   '["99999999-9999-9999-9999-999999999993"]', GETDATE(), 'seed', GETDATE(), 'seed', 0),
  ('88888888-8888-8888-8888-888888888884', 'Ana Costa', NULL, '+55-11-99999-0004', 0, 0, 'sms', NULL,
   '[]', GETDATE(), 'seed', GETDATE(), 'seed', 0),
  ('88888888-8888-8888-8888-888888888885', 'Walk-in Customer', NULL, NULL, 1, 0, 'none', NULL,
   '[]', GETDATE(), 'seed', GETDATE(), 'seed', 0);

-- Update Queues to reference actual Location IDs
UPDATE Queues SET LocationId = '99999999-9999-9999-9999-999999999991' WHERE Id = '11111111-1111-1111-1111-111111111111';
UPDATE Queues SET LocationId = '99999999-9999-9999-9999-999999999992' WHERE Id = '11111111-1111-1111-1111-111111111112';
UPDATE Queues SET LocationId = '99999999-9999-9999-9999-999999999993' WHERE Id = '11111111-1111-1111-1111-111111111113';

-- Insert test data for Queues (using actual Location IDs)
INSERT INTO Queues (
  Id, LocationId, QueueDate, IsActive, MaxSize, LateClientCapTimeInMinutes, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy, IsDeleted
)
VALUES
  ('11111111-1111-1111-1111-111111111111', '99999999-9999-9999-9999-999999999991', CAST(GETDATE() AS DATE), 1, 50, 15, GETDATE(), 'seed', GETDATE(), 'seed', 0),
  ('11111111-1111-1111-1111-111111111112', '99999999-9999-9999-9999-999999999992', CAST(GETDATE() AS DATE), 1, 10, 5, GETDATE(), 'seed', GETDATE(), 'seed', 0),
  ('11111111-1111-1111-1111-111111111113', '99999999-9999-9999-9999-999999999993', CAST(GETDATE() AS DATE), 1, 20, 10, GETDATE(), 'seed', GETDATE(), 'seed', 0);

-- Insert test data for QueueEntries (using actual Customer IDs)
INSERT INTO QueueEntries (
  Id, QueueId, CustomerId, CustomerName, Position, Status, StaffMemberId, ServiceTypeId, Notes, EnteredAt, CalledAt, CheckedInAt, CompletedAt, CancelledAt, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy, IsDeleted
)
VALUES
  ('22222222-2222-2222-2222-222222222221', '11111111-1111-1111-1111-111111111111', '88888888-8888-8888-8888-888888888881', 'João Silva', 1, 'Waiting', NULL, NULL, 'Regular haircut', GETDATE(), NULL, NULL, NULL, NULL, GETDATE(), 'seed', GETDATE(), 'seed', 0),
  ('22222222-2222-2222-2222-222222222222', '11111111-1111-1111-1111-111111111111', '88888888-8888-8888-8888-888888888882', 'Maria Santos', 2, 'Waiting', NULL, NULL, 'Wash and cut', GETDATE(), NULL, NULL, NULL, NULL, GETDATE(), 'seed', GETDATE(), 'seed', 0),
  ('22222222-2222-2222-2222-222222222223', '11111111-1111-1111-1111-111111111112', '88888888-8888-8888-8888-888888888883', 'Carlos Oliveira', 1, 'CheckedIn', '55555555-5555-5555-5555-555555555553', NULL, 'VIP service', DATEADD(MINUTE, -10, GETDATE()), DATEADD(MINUTE, -8, GETDATE()), DATEADD(MINUTE, -5, GETDATE()), NULL, NULL, GETDATE(), 'seed', GETDATE(), 'seed', 0),
  ('22222222-2222-2222-2222-222222222224', '11111111-1111-1111-1111-111111111113', '88888888-8888-8888-8888-888888888884', 'Ana Costa', 1, 'Waiting', NULL, NULL, 'Appointment scheduled', GETDATE(), NULL, NULL, NULL, NULL, GETDATE(), 'seed', GETDATE(), 'seed', 0),
  ('22222222-2222-2222-2222-222222222225', '11111111-1111-1111-1111-111111111111', '88888888-8888-8888-8888-888888888885', 'Walk-in Customer', 3, 'Waiting', NULL, NULL, 'Quick trim', GETDATE(), NULL, NULL, NULL, NULL, GETDATE(), 'seed', GETDATE(), 'seed', 0);

-- Insert test data for StaffMembers
INSERT INTO StaffMembers (
  Id, Name, LocationId, Email, PhoneNumber, ProfilePictureUrl, Role, IsActive, IsOnDuty, StaffStatus, 
  UserId, AverageServiceTimeInMinutes, CompletedServicesCount, EmployeeCode, Username, SpecialtyServiceTypeIds,
  CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy, IsDeleted
)
VALUES
  ('66666666-6666-6666-6666-666666666661', 'Marco Silva', '99999999-9999-9999-9999-999999999991', 'marco.silva@grandetech.com', '+55-11-99998-0001', 'https://grandetech.com/staff/marco.jpg', 'Senior Barber', 1, 1, 'available', '55555555-5555-5555-5555-555555555553', 28.5, 156, 'EMP001', 'marco.silva', '["88888888-8888-8888-8888-888888888881","88888888-8888-8888-8888-888888888882"]', GETDATE(), 'seed', GETDATE(), 'seed', 0),
  ('66666666-6666-6666-6666-666666666662', 'Ana Costa', '99999999-9999-9999-9999-999999999991', 'ana.costa@grandetech.com', '+55-11-99998-0002', 'https://grandetech.com/staff/ana.jpg', 'Stylist', 1, 1, 'busy', NULL, 35.2, 89, 'EMP002', 'ana.costa', '["88888888-8888-8888-8888-888888888883"]', GETDATE(), 'seed', GETDATE(), 'seed', 0),
  ('66666666-6666-6666-6666-666666666663', 'Pedro Santos', '99999999-9999-9999-9999-999999999992', 'pedro.santos@grandetech.com', '+55-11-99998-0003', NULL, 'Barber', 1, 1, 'available', NULL, 22.8, 67, 'EMP003', 'pedro.santos', '["88888888-8888-8888-8888-888888888881"]', GETDATE(), 'seed', GETDATE(), 'seed', 0),
  ('66666666-6666-6666-6666-666666666664', 'Carlos Oliveira', '99999999-9999-9999-9999-999999999993', 'carlos.oliveira@classiccuts.com', '+55-21-99998-0004', 'https://classiccuts.com/staff/carlos.jpg', 'Master Barber', 1, 1, 'available', NULL, 45.0, 234, 'EMP004', 'carlos.oliveira', '["88888888-8888-8888-8888-888888888883","88888888-8888-8888-8888-888888888884"]', GETDATE(), 'seed', GETDATE(), 'seed', 0),
  ('66666666-6666-6666-6666-666666666665', 'Roberto Lima', '99999999-9999-9999-9999-999999999993', 'roberto.lima@classiccuts.com', '+55-21-99998-0005', NULL, 'Barber', 1, 0, 'off_duty', NULL, 38.5, 145, 'EMP005', 'roberto.lima', '["88888888-8888-8888-8888-888888888884"]', GETDATE(), 'seed', GETDATE(), 'seed', 0);

-- Insert test data for ServicesOffered
INSERT INTO ServicesOffered (
  Id, Name, Description, LocationId, EstimatedDurationMinutes, PriceAmount, PriceCurrency, ImageUrl, 
  IsActive, TimesProvided, ActualAverageDurationMinutes, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy, IsDeleted
)
VALUES
  ('88888888-8888-8888-8888-888888888881', 'Classic Haircut', 'Traditional men''s haircut with scissors and clipper', '99999999-9999-9999-9999-999999999991', 30, 25.00, 'USD', 'https://grandetech.com/services/classic-cut.jpg', 1, 45, 28.5, GETDATE(), 'seed', GETDATE(), 'seed', 0),
  ('88888888-8888-8888-8888-888888888882', 'Beard Trim', 'Professional beard trimming and shaping', '99999999-9999-9999-9999-999999999991', 20, 15.00, 'USD', 'https://grandetech.com/services/beard-trim.jpg', 1, 32, 18.2, GETDATE(), 'seed', GETDATE(), 'seed', 0),
  ('88888888-8888-8888-8888-888888888883', 'Premium Styling', 'Full styling service with wash, cut, and finish', '99999999-9999-9999-9999-999999999991', 45, 45.00, 'USD', 'https://grandetech.com/services/premium-styling.jpg', 1, 28, 42.8, GETDATE(), 'seed', GETDATE(), 'seed', 0),
  ('88888888-8888-8888-8888-888888888884', 'Quick Trim', 'Fast 15-minute cleanup cut', '99999999-9999-9999-9999-999999999992', 15, 18.00, 'USD', NULL, 1, 67, 14.5, GETDATE(), 'seed', GETDATE(), 'seed', 0),
  ('88888888-8888-8888-8888-888888888885', 'Traditional Shave', 'Classic straight razor shave with hot towel', '99999999-9999-9999-9999-999999999993', 35, 30.00, 'USD', 'https://classiccuts.com/services/traditional-shave.jpg', 1, 23, 38.2, GETDATE(), 'seed', GETDATE(), 'seed', 0),
  ('88888888-8888-8888-8888-888888888886', 'Father & Son Cut', 'Special package for father and son haircuts', '99999999-9999-9999-9999-999999999993', 50, 40.00, 'USD', NULL, 1, 15, 52.5, GETDATE(), 'seed', GETDATE(), 'seed', 0);

-- Update Location StaffMemberIds and ServiceTypeIds to reference actual data
UPDATE Locations SET StaffMemberIds = '["66666666-6666-6666-6666-666666666661","66666666-6666-6666-6666-666666666662"]', ServiceTypeIds = '["88888888-8888-8888-8888-888888888881","88888888-8888-8888-8888-888888888882","88888888-8888-8888-8888-888888888883"]' WHERE Id = '99999999-9999-9999-9999-999999999991';
UPDATE Locations SET StaffMemberIds = '["66666666-6666-6666-6666-666666666663"]', ServiceTypeIds = '["88888888-8888-8888-8888-888888888884"]' WHERE Id = '99999999-9999-9999-9999-999999999992';
UPDATE Locations SET StaffMemberIds = '["66666666-6666-6666-6666-666666666664","66666666-6666-6666-6666-666666666665"]', ServiceTypeIds = '["88888888-8888-8888-8888-888888888885","88888888-8888-8888-8888-888888888886"]' WHERE Id = '99999999-9999-9999-9999-999999999993';

-- Now all entities are properly seeded with realistic relationships and comprehensive business data 