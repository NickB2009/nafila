-- FULL RESET: Delete all test data in the correct order to avoid FK constraint errors
DELETE FROM Customers;
DELETE FROM Locations;
DELETE FROM Organizations;
DELETE FROM SubscriptionPlans;

-- Insert test data for Organizations (Barber Shop and Health Clinic)
INSERT INTO Organizations (
  Id, Name, Slug, Description, ContactEmail, ContactPhone, WebsiteUrl, BrandingPrimaryColor, BrandingSecondaryColor, BrandingLogoUrl, BrandingFaviconUrl, BrandingCompanyName, BrandingTagLine, BrandingFontFamily, SubscriptionPlanId, IsActive, SharesDataForAnalytics, CreatedBy, CreatedAt, LastModifiedBy, LastModifiedAt, IsDeleted
)
VALUES
  ('11111111-1111-1111-1111-111111111111', 'Downtown Barbers', 'downtown-barbers', 'Trendy barber shop in the city center', 'contact@downtownbarbers.com', '555-1234', 'https://downtownbarbers.com', '#222222', '#FFD700', 'https://barbers.com/logo.png', 'https://barbers.com/favicon.ico', 'Downtown Barbers', 'Look sharp, feel sharp', 'Roboto', '22222222-2222-2222-2222-222222222222', 1, 1, 'seed', GETDATE(), 'seed', GETDATE(), 0),
  ('11111111-1111-1111-1111-111111111112', 'Healthy Life Clinic', 'healthy-life-clinic', 'Family health clinic with modern facilities', 'info@healthylife.com', '555-5678', 'https://healthylife.com', '#4CAF50', '#2196F3', 'https://healthylife.com/logo.png', 'https://healthylife.com/favicon.ico', 'Healthy Life', 'Your health, our priority', 'Open Sans', '22222222-2222-2222-2222-222222222223', 1, 0, 'seed', GETDATE(), 'seed', GETDATE(), 0);

-- Insert test data for SubscriptionPlans
INSERT INTO SubscriptionPlans (
  Id, Name, Description, Price, MaxLocations, MaxStaffPerLocation, MaxQueueEntriesPerDay, IncludesAnalytics, IncludesAdvancedReporting, IncludesAdvertising, IncludesMultipleLocations, IsDefault, IsActive, IsFeatured, CreatedBy, CreatedAt, LastModifiedBy, LastModifiedAt,
  MonthlyPriceAmount, MonthlyPriceCurrency, YearlyPriceAmount, YearlyPriceCurrency, IsDeleted
)
VALUES
  ('22222222-2222-2222-2222-222222222222', 'Barber Gold', 'Premium plan for barber shops', 49.99, 5, 20, 200, 1, 1, 1, 0, 1, 1, 1, 'seed', GETDATE(), 'seed', GETDATE(), 10, 'USD', 100, 'USD', 0),
  ('22222222-2222-2222-2222-222222222223', 'Clinic Pro', 'Professional plan for health clinics', 99.99, 10, 50, 500, 1, 1, 1, 1, 0, 1, 1, 'seed', GETDATE(), 'seed', GETDATE(), 20, 'USD', 200, 'USD', 0);

-- Insert test data for Locations (Barber Shop and Health Clinic)
INSERT INTO Locations (
  Id, Name, Slug, Description, OrganizationId, Street, Number, Complement, Neighborhood, City, State, Country, PostalCode, Latitude, Longitude,
  ContactPhone, ContactPhoneCountryCode, ContactPhoneNationalNumber, ContactEmail, BrandingPrimaryColor, BrandingSecondaryColor, BrandingLogoUrl, BrandingFontFamily,
  BusinessHoursStart, BusinessHoursEnd, IsQueueEnabled, MaxQueueSize, LateClientCapTimeInMinutes, IsActive, AverageServiceTimeInMinutes, LastAverageTimeReset,
  CreatedBy, CreatedAt, LastModifiedBy, LastModifiedAt, IsDeleted, DeletedAt, DeletedBy
) VALUES
  ('33333333-3333-3333-3333-333333333333', 'Main Street Barbers', 'main-street-barbers', 'Classic cuts and shaves', '11111111-1111-1111-1111-111111111111',
   '101 Main St', '10', '', 'Downtown', 'Metropolis', 'NY', 'USA', '10001', 40.7128, -74.0060,
   '555-1111', '+1', '5551111', 'main@barbers.com', '#333333', '#E91E63', 'https://barbers.com/mainlogo.png', 'Roboto',
   '08:00', '19:00', 1, 30, 5, 1, 20, GETDATE(),
   'seed', GETDATE(), 'seed', GETDATE(), 0, NULL, NULL),
  ('33333333-3333-3333-3333-333333333334', 'Healthy Life Pediatrics', 'healthy-life-pediatrics', 'Pediatric care for your family', '11111111-1111-1111-1111-111111111112',
   '202 Clinic Ave', '20', '', 'Medical District', 'Springfield', 'IL', 'USA', '62704', 39.7817, -89.6501,
   '555-2222', '+1', '5552222', 'peds@healthylife.com', '#8BC34A', '#03A9F4', 'https://healthylife.com/pedslogo.png', 'Open Sans',
   '09:00', '17:00', 1, 40, 10, 1, 30, GETDATE(),
   'seed', GETDATE(), 'seed', GETDATE(), 0, NULL, NULL);

-- Insert test data for Customers
INSERT INTO Customers (
  Id, Name, IsAnonymous, NotificationsEnabled, PreferredNotificationChannel, UserId, PhoneNumber, PhoneCountryCode, PhoneNationalNumber, Email, FavoriteLocationIds, CreatedBy, CreatedAt, LastModifiedBy, LastModifiedAt, IsDeleted
)
VALUES
  ('44444444-4444-4444-4444-444444444444', 'Clark Kent', 0, 1, 'sms', 'user-ck', '555-0001', '+1', '5550001', 'clark.kent@example.com', '33333333-3333-3333-3333-333333333333', 'seed', GETDATE(), 'seed', GETDATE(), 0),
  ('44444444-4444-4444-4444-444444444445', 'Lois Lane', 0, 1, 'email', 'user-ll', '555-0002', '+1', '5550002', 'lois.lane@example.com', '33333333-3333-3333-3333-333333333334', 'seed', GETDATE(), 'seed', GETDATE(), 0),
  ('44444444-4444-4444-4444-444444444446', 'Bruce Wayne', 0, 1, 'sms', 'user-bw', '555-0003', '+1', '5550003', 'bruce.wayne@example.com', '33333333-3333-3333-3333-333333333333', 'seed', GETDATE(), 'seed', GETDATE(), 0),
  ('44444444-4444-4444-4444-444444444447', 'Diana Prince', 0, 1, 'email', 'user-dp', '555-0004', '+1', '5550004', 'diana.prince@example.com', '33333333-3333-3333-3333-333333333334', 'seed', GETDATE(), 'seed', GETDATE(), 0); 