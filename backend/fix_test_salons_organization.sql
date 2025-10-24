-- Check if test salons have organizations assigned
SELECT Id, Name, OrganizationId FROM Locations WHERE Id IN ('55555555-5555-5555-5555-555555555555', '66666666-6666-6666-6666-666666666666');

-- If OrganizationId is NULL, create and assign organizations

-- Create organization for Elite Barbershop
INSERT IGNORE INTO Organizations (
  Id, Name, Slug, Description, Email, PhoneNumber, Website, SubscriptionPlanId, IsActive, IsPlatformAdmin,
  BrandingPrimaryColor, BrandingSecondaryColor, BrandingLogoUrl, BrandingFaviconUrl, BrandingCompanyName, BrandingTagline, BrandingFontFamily,
  LocationIds, CreatedAt, CreatedBy, IsDeleted, RowVersion
)
VALUES (
  'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee',
  'Elite Barbershop Co',
  'elite-barbershop-co',
  'Premium barbershop services',
  'info@elitebarbershop.com',
  '+1-555-1000',
  'https://elitebarbershop.com',
  NULL,
  1,
  0,
  '#3B82F6',
  '#1E40AF',
  NULL,
  NULL,
  'Elite Barbershop',
  'Premium cuts for the modern gentleman',
  'Arial, sans-serif',
  '["55555555-5555-5555-5555-555555555555"]',
  NOW(),
  'system',
  0,
  0x0000000000000000
);

-- Create organization for Quick Cuts
INSERT IGNORE INTO Organizations (
  Id, Name, Slug, Description, Email, PhoneNumber, Website, SubscriptionPlanId, IsActive, IsPlatformAdmin,
  BrandingPrimaryColor, BrandingSecondaryColor, BrandingLogoUrl, BrandingFaviconUrl, BrandingCompanyName, BrandingTagline, BrandingFontFamily,
  LocationIds, CreatedAt, CreatedBy, IsDeleted, RowVersion
)
VALUES (
  'ffffffff-ffff-ffff-ffff-ffffffffffff',
  'Quick Cuts Inc',
  'quick-cuts-inc',
  'Fast and efficient haircuts',
  'info@quickcuts.com',
  '+1-555-2000',
  'https://quickcuts.com',
  NULL,
  1,
  0,
  '#DC2626',
  '#991B1B',
  NULL,
  NULL,
  'Quick Cuts',
  'Quality cuts in half the time',
  'Arial, sans-serif',
  '["66666666-6666-6666-6666-666666666666"]',
  NOW(),
  'system',
  0,
  0x0000000000000000
);

-- Update locations to have organization IDs
UPDATE Locations 
SET OrganizationId = 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee'
WHERE Id = '55555555-5555-5555-5555-555555555555';

UPDATE Locations 
SET OrganizationId = 'ffffffff-ffff-ffff-ffff-ffffffffffff'
WHERE Id = '66666666-6666-6666-6666-666666666666';

-- Verify
SELECT 'Organizations and Locations Updated!' as Status;
SELECT L.Id, L.Name, O.Name as OrganizationName 
FROM Locations L
LEFT JOIN Organizations O ON L.OrganizationId = O.Id
WHERE L.Id IN ('55555555-5555-5555-5555-555555555555', '66666666-6666-6666-6666-666666666666');

