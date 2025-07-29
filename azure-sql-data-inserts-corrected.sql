-- Azure SQL Database Data Insert Script (Corrected)
-- GrandeTech QueueHub Database
-- Generated for Azure SQL Database with correct column names

USE GrandeTechQueueHub;
GO

-- Clear existing data (optional - uncomment if you want to start fresh)
-- DELETE FROM QueueEntries WHERE IsDeleted = 0;
-- DELETE FROM Queues WHERE IsDeleted = 0;
-- DELETE FROM StaffMembers WHERE IsDeleted = 0;
-- DELETE FROM ServicesOffered WHERE IsDeleted = 0;
-- DELETE FROM SubscriptionPlans WHERE IsDeleted = 0;
-- DELETE FROM Locations WHERE IsDeleted = 0;
-- DELETE FROM Users WHERE IsDeleted = 0;
-- DELETE FROM Organizations WHERE IsDeleted = 0;

-- Insert SubscriptionPlans (must be first due to foreign key constraints)
INSERT INTO SubscriptionPlans (Id, Name, Description, Price_Amount, Price_Currency, Features, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy, IsDeleted, DeletedAt, DeletedBy) VALUES 
('550e8400-e29b-41d4-a716-446655440009', 'Barber Gold', 'Premium barbershop features with advanced analytics', 99.99, 'USD', 'Unlimited queues, Analytics, SMS notifications, Custom branding', '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL),
('550e8400-e29b-41d4-a716-446655440010', 'Clinic Pro', 'Professional clinic management with patient tracking', 149.99, 'USD', 'Patient management, Appointment scheduling, Health records, Multi-location support', '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL);

-- Insert Organizations (with correct column names)
INSERT INTO Organizations (Id, Name, Slug, Description, ContactEmail, ContactPhone, WebsiteUrl, BrandingPrimaryColor, BrandingSecondaryColor, BrandingLogoUrl, BrandingFaviconUrl, BrandingCompanyName, BrandingTagLine, BrandingFontFamily, SubscriptionPlanId, IsActive, SharesDataForAnalytics, LocationIds, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy, IsDeleted, DeletedAt, DeletedBy) VALUES 
('550e8400-e29b-41d4-a716-446655440001', 'Downtown Barbers', 'downtown-barbers', 'Premium barbershop services', 'info@downtownbarbers.com', '(555) 123-4567', 'https://downtownbarbers.com', '#8B4513', '#D2691E', 'https://downtownbarbers.com/logo.png', 'https://downtownbarbers.com/favicon.ico', 'Downtown Barbers', 'Where Style Meets Tradition', 'Arial, sans-serif', '550e8400-e29b-41d4-a716-446655440009', 1, 0, '["550e8400-e29b-41d4-a716-446655440007"]', '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL),
('550e8400-e29b-41d4-a716-446655440002', 'Healthy Life Clinic', 'healthy-life-clinic', 'Professional healthcare services', 'contact@healthylifeclinic.com', '(555) 987-6543', 'https://healthylifeclinic.com', '#32CD32', '#228B22', 'https://healthylifeclinic.com/logo.png', 'https://healthylifeclinic.com/favicon.ico', 'Healthy Life Clinic', 'Your Health, Our Priority', 'Arial, sans-serif', '550e8400-e29b-41d4-a716-446655440010', 1, 0, '["550e8400-e29b-41d4-a716-446655440008"]', '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL);

-- Insert Users
INSERT INTO Users (Id, Username, Email, PasswordHash, Role, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy, IsDeleted, DeletedAt, DeletedBy) VALUES 
('550e8400-e29b-41d4-a716-446655440003', 'admin', 'admin@grandetech.com', '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'PlatformAdmin', '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL),
('550e8400-e29b-41d4-a716-446655440004', 'owner_test', 'owner@downtownbarbers.com', '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Admin', '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL),
('550e8400-e29b-41d4-a716-446655440005', 'barber_test', 'barber@downtownbarbers.com', '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Barber', '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL),
('550e8400-e29b-41d4-a716-446655440006', 'customer_test', 'customer@example.com', '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Client', '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL);

-- Insert Locations (with correct column names)
INSERT INTO Locations (Id, Name, Slug, Description, OrganizationId, AddressStreet, AddressNumber, AddressComplement, AddressNeighborhood, AddressCity, AddressState, AddressCountry, AddressPostalCode, AddressLatitude, AddressLongitude, ContactPhone, ContactEmail, CustomBrandingPrimaryColor, CustomBrandingSecondaryColor, CustomBrandingLogoUrl, CustomBrandingFaviconUrl, CustomBrandingCompanyName, CustomBrandingTagLine, CustomBrandingFontFamily, BusinessHoursStart, BusinessHoursEnd, IsQueueEnabled, MaxQueueSize, LateClientCapTimeInMinutes, IsActive, AverageServiceTimeInMinutes, LastAverageTimeReset, StaffMemberIds, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy, IsDeleted, DeletedAt, DeletedBy) VALUES 
('550e8400-e29b-41d4-a716-446655440007', 'Main Street Barbers', 'main-street-barbers', 'Our flagship barbershop location', '550e8400-e29b-41d4-a716-446655440001', 'Main Street', '123', 'Suite 1', 'Downtown', 'Downtown', 'CA', 'USA', '12345', 34.0522, -118.2437, '(555) 123-4567', 'main@downtownbarbers.com', '#8B4513', '#D2691E', 'https://downtownbarbers.com/logo.png', 'https://downtownbarbers.com/favicon.ico', 'Downtown Barbers', 'Where Style Meets Tradition', 'Arial, sans-serif', '09:00:00', '18:00:00', 1, 100, 15, 1, 30.0, '2025-01-27T10:00:00.000Z', '["550e8400-e29b-41d4-a716-446655440017"]', '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL),
('550e8400-e29b-41d4-a716-446655440008', 'Pediatrics Office', 'pediatrics-office', 'Specialized pediatric care', '550e8400-e29b-41d4-a716-446655440002', 'Health Ave', '456', 'Building A', 'Medical District', 'Medical District', 'CA', 'USA', '67890', 34.0522, -118.2437, '(555) 987-6543', 'pediatrics@healthylifeclinic.com', '#32CD32', '#228B22', 'https://healthylifeclinic.com/logo.png', 'https://healthylifeclinic.com/favicon.ico', 'Healthy Life Clinic', 'Your Health, Our Priority', 'Arial, sans-serif', '08:00:00', '17:00:00', 1, 50, 10, 1, 45.0, '2025-01-27T10:00:00.000Z', '["550e8400-e29b-41d4-a716-446655440018"]', '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL);

-- Insert ServicesOffered
INSERT INTO ServicesOffered (Id, OrganizationId, Name, Description, Duration, Price_Amount, Price_Currency, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy, IsDeleted, DeletedAt, DeletedBy) VALUES 
('550e8400-e29b-41d4-a716-446655440011', '550e8400-e29b-41d4-a716-446655440001', 'Haircut', 'Classic men''s haircut with wash and style', 30, 25.00, 'USD', '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL),
('550e8400-e29b-41d4-a716-446655440012', '550e8400-e29b-41d4-a716-446655440001', 'Beard Trim', 'Professional beard trimming and shaping', 20, 15.00, 'USD', '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL),
('550e8400-e29b-41d4-a716-446655440013', '550e8400-e29b-41d4-a716-446655440001', 'Haircut & Beard', 'Complete grooming package', 45, 35.00, 'USD', '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL),
('550e8400-e29b-41d4-a716-446655440014', '550e8400-e29b-41d4-a716-446655440002', 'Check-up', 'General health check-up', 30, 75.00, 'USD', '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL),
('550e8400-e29b-41d4-a716-446655440015', '550e8400-e29b-41d4-a716-446655440002', 'Vaccination', 'Standard vaccination service', 15, 45.00, 'USD', '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL),
('550e8400-e29b-41d4-a716-446655440016', '550e8400-e29b-41d4-a716-446655440002', 'Consultation', 'Doctor consultation', 45, 120.00, 'USD', '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL);

-- Insert StaffMembers
INSERT INTO StaffMembers (Id, OrganizationId, UserId, Name, Email, Phone, Skills, IsAvailable, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy, IsDeleted, DeletedAt, DeletedBy) VALUES 
('550e8400-e29b-41d4-a716-446655440017', '550e8400-e29b-41d4-a716-446655440001', '550e8400-e29b-41d4-a716-446655440005', 'Tony the Barber', 'tony@downtownbarbers.com', '(555) 123-4567', 'Haircuts, Beard trimming, Styling', 1, '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL),
('550e8400-e29b-41d4-a716-446655440018', '550e8400-e29b-41d4-a716-446655440002', NULL, 'Dr. Sarah Johnson', 'sarah@healthylifeclinic.com', '(555) 987-6543', 'Pediatrics, General medicine', 1, '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL);

-- Insert Queues
INSERT INTO Queues (Id, LocationId, Date, Status, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy, IsDeleted, DeletedAt, DeletedBy) VALUES 
('550e8400-e29b-41d4-a716-446655440019', '550e8400-e29b-41d4-a716-446655440007', '2025-01-27', 'Active', '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL),
('550e8400-e29b-41d4-a716-446655440020', '550e8400-e29b-41d4-a716-446655440008', '2025-01-27', 'Active', '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL);

-- Insert QueueEntries
INSERT INTO QueueEntries (Id, QueueId, CustomerId, Position, Status, EstimatedWaitTime, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy, IsDeleted, DeletedAt, DeletedBy) VALUES 
('550e8400-e29b-41d4-a716-446655440021', '550e8400-e29b-41d4-a716-446655440019', '550e8400-e29b-41d4-a716-446655440006', 1, 'Waiting', 15, '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL),
('550e8400-e29b-41d4-a716-446655440022', '550e8400-e29b-41d4-a716-446655440019', NULL, 2, 'Waiting', 30, '2025-01-27T10:15:00.000Z', 'system', NULL, NULL, 0, NULL, NULL),
('550e8400-e29b-41d4-a716-446655440023', '550e8400-e29b-41d4-a716-446655440020', NULL, 1, 'Waiting', 20, '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL);

-- Verify the data
SELECT 'Organizations' as TableName, COUNT(*) as RecordCount FROM Organizations WHERE IsDeleted = 0
UNION ALL
SELECT 'Users' as TableName, COUNT(*) as RecordCount FROM Users WHERE IsDeleted = 0
UNION ALL
SELECT 'Locations' as TableName, COUNT(*) as RecordCount FROM Locations WHERE IsDeleted = 0
UNION ALL
SELECT 'SubscriptionPlans' as TableName, COUNT(*) as RecordCount FROM SubscriptionPlans WHERE IsDeleted = 0
UNION ALL
SELECT 'ServicesOffered' as TableName, COUNT(*) as RecordCount FROM ServicesOffered WHERE IsDeleted = 0
UNION ALL
SELECT 'StaffMembers' as TableName, COUNT(*) as RecordCount FROM StaffMembers WHERE IsDeleted = 0
UNION ALL
SELECT 'Queues' as TableName, COUNT(*) as RecordCount FROM Queues WHERE IsDeleted = 0
UNION ALL
SELECT 'QueueEntries' as TableName, COUNT(*) as RecordCount FROM QueueEntries WHERE IsDeleted = 0;

PRINT 'Data insertion completed successfully!';