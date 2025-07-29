-- Azure SQL Database Data Insert Script (Corrected)
-- GrandeTech QueueHub Database
-- Generated for Azure SQL Database with correct column names

USE GrandeTechQueueHub;
GO

-- Clear existing data to avoid duplicates
DELETE FROM QueueEntries WHERE IsDeleted = 0;
DELETE FROM Queues WHERE IsDeleted = 0;
DELETE FROM StaffMembers WHERE IsDeleted = 0;
DELETE FROM ServicesOffered WHERE IsDeleted = 0;
DELETE FROM Locations WHERE IsDeleted = 0;
DELETE FROM Users WHERE IsDeleted = 0;
DELETE FROM Organizations WHERE IsDeleted = 0;
DELETE FROM SubscriptionPlans WHERE IsDeleted = 0;

-- Insert SubscriptionPlans (must be first due to foreign key constraints)
INSERT INTO SubscriptionPlans (Id, Name, Description, MonthlyPriceAmount, MonthlyPriceCurrency, YearlyPriceAmount, YearlyPriceCurrency, Price, IsActive, IsDefault, MaxLocations, MaxStaffPerLocation, IncludesAnalytics, IncludesAdvancedReporting, IncludesCustomBranding, IncludesAdvertising, IncludesMultipleLocations, MaxQueueEntriesPerDay, IsFeatured, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy, IsDeleted, DeletedAt, DeletedBy) VALUES 
('550e8400-e29b-41d4-a716-446655440009', 'Barber Gold', 'Premium barbershop features with advanced analytics', 99.99, 'USD', 999.99, 'USD', 99.99, 1, 0, 5, 10, 1, 1, 1, 0, 1, 500, 1, '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL),
('550e8400-e29b-41d4-a716-446655440010', 'Clinic Pro', 'Professional clinic management with patient tracking', 149.99, 'USD', 1499.99, 'USD', 149.99, 1, 0, 10, 20, 1, 1, 1, 1, 1, 1000, 1, '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL);

-- Insert Organizations (with correct column names)
INSERT INTO Organizations (Id, Name, Slug, Description, ContactEmail, ContactPhone, WebsiteUrl, BrandingPrimaryColor, BrandingSecondaryColor, BrandingLogoUrl, BrandingFaviconUrl, BrandingCompanyName, BrandingTagLine, BrandingFontFamily, SubscriptionPlanId, IsActive, SharesDataForAnalytics, LocationIds, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy, IsDeleted, DeletedAt, DeletedBy) VALUES 
('550e8400-e29b-41d4-a716-446655440001', 'Downtown Barbers', 'downtown-barbers', 'Premium barbershop services', 'info@downtownbarbers.com', '(555) 123-4567', 'https://downtownbarbers.com', '#8B4513', '#D2691E', 'https://downtownbarbers.com/logo.png', 'https://downtownbarbers.com/favicon.ico', 'Downtown Barbers', 'Where Style Meets Tradition', 'Arial, sans-serif', '550e8400-e29b-41d4-a716-446655440009', 1, 0, '["550e8400-e29b-41d4-a716-446655440007"]', '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL),
('550e8400-e29b-41d4-a716-446655440002', 'Healthy Life Clinic', 'healthy-life-clinic', 'Professional healthcare services', 'contact@healthylifeclinic.com', '(555) 987-6543', 'https://healthylifeclinic.com', '#32CD32', '#228B22', 'https://healthylifeclinic.com/logo.png', 'https://healthylifeclinic.com/favicon.ico', 'Healthy Life Clinic', 'Your Health, Our Priority', 'Arial, sans-serif', '550e8400-e29b-41d4-a716-446655440010', 1, 0, '["550e8400-e29b-41d4-a716-446655440008"]', '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL);

-- Insert Users
INSERT INTO Users (Id, Username, Email, PasswordHash, Role, LastLoginAt, IsLocked, RequiresTwoFactor, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy, IsDeleted, DeletedAt, DeletedBy) VALUES 
('550e8400-e29b-41d4-a716-446655440003', 'admin', 'admin@grandetech.com', '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'PlatformAdmin', NULL, 0, 0, '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL),
('550e8400-e29b-41d4-a716-446655440004', 'owner_test', 'owner@downtownbarbers.com', '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Admin', NULL, 0, 0, '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL),
('550e8400-e29b-41d4-a716-446655440005', 'barber_test', 'barber@downtownbarbers.com', '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Barber', NULL, 0, 0, '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL),
('550e8400-e29b-41d4-a716-446655440006', 'customer_test', 'customer@example.com', '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Client', NULL, 0, 0, '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL);

-- Insert Locations (with correct column names)
INSERT INTO Locations (Id, Name, Slug, Description, OrganizationId, AddressStreet, AddressNumber, AddressComplement, AddressNeighborhood, AddressCity, AddressState, AddressCountry, AddressPostalCode, AddressLatitude, AddressLongitude, ContactPhone, ContactEmail, CustomBrandingPrimaryColor, CustomBrandingSecondaryColor, CustomBrandingLogoUrl, CustomBrandingFaviconUrl, CustomBrandingCompanyName, CustomBrandingTagLine, CustomBrandingFontFamily, BusinessHoursStart, BusinessHoursEnd, IsQueueEnabled, MaxQueueSize, LateClientCapTimeInMinutes, IsActive, AverageServiceTimeInMinutes, LastAverageTimeReset, StaffMemberIds, ServiceTypeIds, AdvertisementIds, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy, IsDeleted, DeletedAt, DeletedBy) VALUES 
('550e8400-e29b-41d4-a716-446655440007', 'Main Street Barbers', 'main-street-barbers', 'Our flagship barbershop location', '550e8400-e29b-41d4-a716-446655440001', 'Main Street', '123', 'Suite 1', 'Downtown', 'Downtown', 'CA', 'USA', '12345', 34.0522, -118.2437, '(555) 123-4567', 'main@downtownbarbers.com', '#8B4513', '#D2691E', 'https://downtownbarbers.com/logo.png', 'https://downtownbarbers.com/favicon.ico', 'Downtown Barbers', 'Where Style Meets Tradition', 'Arial, sans-serif', '09:00:00', '18:00:00', 1, 100, 15, 1, 30.0, '2025-01-27T10:00:00.000Z', '["550e8400-e29b-41d4-a716-446655440017"]', '["550e8400-e29b-41d4-a716-446655440011","550e8400-e29b-41d4-a716-446655440012","550e8400-e29b-41d4-a716-446655440013"]', '[]', '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL),
('550e8400-e29b-41d4-a716-446655440008', 'Pediatrics Office', 'pediatrics-office', 'Specialized pediatric care', '550e8400-e29b-41d4-a716-446655440002', 'Health Ave', '456', 'Building A', 'Medical District', 'Medical District', 'CA', 'USA', '67890', 34.0522, -118.2437, '(555) 987-6543', 'pediatrics@healthylifeclinic.com', '#32CD32', '#228B22', 'https://healthylifeclinic.com/logo.png', 'https://healthylifeclinic.com/favicon.ico', 'Healthy Life Clinic', 'Your Health, Our Priority', 'Arial, sans-serif', '08:00:00', '17:00:00', 1, 50, 10, 1, 45.0, '2025-01-27T10:00:00.000Z', '["550e8400-e29b-41d4-a716-446655440018"]', '["550e8400-e29b-41d4-a716-446655440014","550e8400-e29b-41d4-a716-446655440015","550e8400-e29b-41d4-a716-446655440016"]', '[]', '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL);

-- Insert ServicesOffered
INSERT INTO ServicesOffered (Id, Name, Description, LocationId, EstimatedDurationMinutes, PriceAmount, PriceCurrency, ImageUrl, IsActive, TimesProvided, ActualAverageDurationMinutes, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy, IsDeleted, DeletedAt, DeletedBy) VALUES 
('550e8400-e29b-41d4-a716-446655440011', 'Haircut', 'Classic men''s haircut with wash and style', '550e8400-e29b-41d4-a716-446655440007', 30, 25.00, 'USD', NULL, 1, 0, 30.0, '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL),
('550e8400-e29b-41d4-a716-446655440012', 'Beard Trim', 'Professional beard trimming and shaping', '550e8400-e29b-41d4-a716-446655440007', 20, 15.00, 'USD', NULL, 1, 0, 20.0, '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL),
('550e8400-e29b-41d4-a716-446655440013', 'Haircut & Beard', 'Complete grooming package', '550e8400-e29b-41d4-a716-446655440007', 45, 35.00, 'USD', NULL, 1, 0, 45.0, '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL),
('550e8400-e29b-41d4-a716-446655440014', 'Check-up', 'General health check-up', '550e8400-e29b-41d4-a716-446655440008', 30, 75.00, 'USD', NULL, 1, 0, 30.0, '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL),
('550e8400-e29b-41d4-a716-446655440015', 'Vaccination', 'Standard vaccination service', '550e8400-e29b-41d4-a716-446655440008', 15, 45.00, 'USD', NULL, 1, 0, 15.0, '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL),
('550e8400-e29b-41d4-a716-446655440016', 'Consultation', 'Doctor consultation', '550e8400-e29b-41d4-a716-446655440008', 45, 120.00, 'USD', NULL, 1, 0, 45.0, '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL);

-- Insert StaffMembers
INSERT INTO StaffMembers (Id, Name, LocationId, Email, PhoneNumber, ProfilePictureUrl, Role, IsActive, IsOnDuty, StaffStatus, UserId, AverageServiceTimeInMinutes, EmployeeCode, Username, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy, IsDeleted, DeletedAt, DeletedBy) VALUES 
('550e8400-e29b-41d4-a716-446655440017', 'Tony the Barber', '550e8400-e29b-41d4-a716-446655440007', 'tony@downtownbarbers.com', '(555) 123-4567', NULL, 'Barber', 1, 1, 'available', 'user5', 30.0, 'EMP001', 'tony.barber', '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL),
('550e8400-e29b-41d4-a716-446655440018', 'Dr. Sarah Johnson', '550e8400-e29b-41d4-a716-446655440008', 'sarah@healthylifeclinic.com', '(555) 987-6543', NULL, 'Doctor', 1, 1, 'available', NULL, 45.0, 'EMP002', 'sarah.johnson', '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL);

-- Insert Queues
INSERT INTO Queues (Id, LocationId, QueueDate, IsActive, MaxSize, LateClientCapTimeInMinutes, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy, IsDeleted, DeletedAt, DeletedBy) VALUES 
('550e8400-e29b-41d4-a716-446655440019', '550e8400-e29b-41d4-a716-446655440007', '2025-01-27', 1, 100, 15, '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL),
('550e8400-e29b-41d4-a716-446655440020', '550e8400-e29b-41d4-a716-446655440008', '2025-01-27', 1, 50, 10, '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL);

-- Insert QueueEntries
INSERT INTO QueueEntries (Id, QueueId, CustomerId, CustomerName, Position, Status, StaffMemberId, ServiceTypeId, Notes, EnteredAt, CalledAt, CheckedInAt, CompletedAt, CancelledAt, ServiceDurationMinutes, EstimatedDurationMinutes, EstimatedStartTime, ActualStartTime, CustomerNotes, StaffNotes, TokenNumber, NotificationSent, NotificationSentAt, NotificationChannel, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy, IsDeleted, DeletedAt, DeletedBy) VALUES 
('550e8400-e29b-41d4-a716-446655440021', '550e8400-e29b-41d4-a716-446655440019', '550e8400-e29b-41d4-a716-446655440006', 'Customer Test', 1, 'Waiting', NULL, '550e8400-e29b-41d4-a716-446655440011', NULL, '2025-01-27T10:00:00.000Z', NULL, NULL, NULL, NULL, NULL, 30, NULL, NULL, NULL, NULL, 'T001', 0, NULL, NULL, '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL),
('550e8400-e29b-41d4-a716-446655440022', '550e8400-e29b-41d4-a716-446655440019', '00000000-0000-0000-0000-000000000000', 'Walk-in Customer', 2, 'Waiting', NULL, '550e8400-e29b-41d4-a716-446655440012', NULL, '2025-01-27T10:15:00.000Z', NULL, NULL, NULL, NULL, NULL, 20, NULL, NULL, NULL, NULL, 'T002', 0, NULL, NULL, '2025-01-27T10:15:00.000Z', 'system', NULL, NULL, 0, NULL, NULL),
('550e8400-e29b-41d4-a716-446655440023', '550e8400-e29b-41d4-a716-446655440020', '00000000-0000-0000-0000-000000000000', 'Walk-in Patient', 1, 'Waiting', NULL, '550e8400-e29b-41d4-a716-446655440014', NULL, '2025-01-27T10:00:00.000Z', NULL, NULL, NULL, NULL, NULL, 30, NULL, NULL, NULL, NULL, 'P001', 0, NULL, NULL, '2025-01-27T10:00:00.000Z', 'system', NULL, NULL, 0, NULL, NULL);

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