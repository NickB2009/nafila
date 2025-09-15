-- Simple seed data for testing /api/Public/salons endpoint

USE QueueHubDb;

-- Insert minimal subscription plan (required for organizations)
INSERT IGNORE INTO SubscriptionPlans (
  Id, Name, Description, Price, MonthlyPriceAmount, MonthlyPriceCurrency, 
  YearlyPriceAmount, YearlyPriceCurrency, IsActive, IsDefault, MaxLocations, 
  MaxStaffPerLocation, IncludesAnalytics, IncludesAdvancedReporting, 
  IncludesCustomBranding, IncludesAdvertising, IncludesMultipleLocations, 
  MaxQueueEntriesPerDay, IsFeatured, CreatedAt, CreatedBy, IsDeleted, RowVersion
)
VALUES
  ('77777777-7777-7777-7777-777777777771', 'Basic Plan', 'Test plan', 29.99, 29.99, 'USD', 299.99, 'USD', 1, 1, 1, 3, 0, 0, 0, 0, 0, 50, 0, NOW(), 'seed', 0, 0x0000000000000000);

-- Insert test organizations (these show up as "salons" in the API)
INSERT IGNORE INTO Organizations (
  Id, Name, Slug, Description, ContactEmail, ContactPhone, WebsiteUrl,
  BrandingPrimaryColor, BrandingSecondaryColor, BrandingLogoUrl, BrandingFaviconUrl, 
  BrandingCompanyName, BrandingTagLine, BrandingFontFamily,
  SubscriptionPlanId, IsActive, SharesDataForAnalytics, LocationIds,
  CreatedAt, CreatedBy, IsDeleted, RowVersion
)
VALUES
  ('33333333-3333-3333-3333-333333333333', 'Elite Barbershop', 'elite-barbershop', 'Premium barbershop services', 'info@elite.com', '+1234567890', 'https://elite.com',
   '#8B4513', '#D2691E', '', '', 'Elite Barbershop', 'Premium Service', 'Arial',
   '77777777-7777-7777-7777-777777777771', 1, 1, '[]',
   NOW(), 'seed', 0, 0x0000000000000000),
  ('44444444-4444-4444-4444-444444444444', 'Quick Cuts', 'quick-cuts', 'Fast and affordable haircuts', 'info@quickcuts.com', '+1234567891', 'https://quickcuts.com',
   '#FF6B35', '#004E89', '', '', 'Quick Cuts', 'Fast Service', 'Arial',
   '77777777-7777-7777-7777-777777777771', 1, 0, '[]',
   NOW(), 'seed', 0, 0x0000000000000000);

-- Insert test locations (salon locations)
INSERT IGNORE INTO Locations (
  Id, Name, Slug, Description, OrganizationId, IsQueueEnabled, MaxQueueSize,
  LateClientCapTimeInMinutes, IsActive, AverageServiceTimeInMinutes, LastAverageTimeReset,
  AddressStreet, AddressNumber, AddressComplement, AddressNeighborhood, AddressCity, 
  AddressState, AddressCountry, AddressPostalCode, AddressLatitude, AddressLongitude,
  ContactEmail, ContactPhone, WeeklyBusinessHours,
  StaffMemberIds, ServiceTypeIds, AdvertisementIds,
  CreatedAt, CreatedBy, IsDeleted, RowVersion
)
VALUES
  ('55555555-5555-5555-5555-555555555555', 'Elite Downtown', 'elite-downtown', 'Downtown location', '33333333-3333-3333-3333-333333333333', 1, 20, 10, 1, 30.0, NOW(),
   'Main St', '123', '', 'Downtown', 'New York', 'NY', 'USA', '10001', 40.7128, -74.0060,
   'downtown@elite.com', '+1234567892', '{"monday":{"isOpen":true,"start":"09:00","end":"18:00"}}',
   '[]', '[]', '[]',
   NOW(), 'seed', 0, 0x0000000000000000),
  ('66666666-6666-6666-6666-666666666666', 'Quick Cuts Express', 'quick-express', 'Express location', '44444444-4444-4444-4444-444444444444', 1, 15, 5, 1, 15.0, NOW(),
   'Business Ave', '456', '', 'Financial', 'New York', 'NY', 'USA', '10005', 40.7074, -74.0113,
   'express@quickcuts.com', '+1234567893', '{"monday":{"isOpen":true,"start":"07:00","end":"19:00"}}',
   '[]', '[]', '[]',
   NOW(), 'seed', 0, 0x0000000000000000);
