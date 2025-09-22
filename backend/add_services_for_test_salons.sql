-- Add services for test salons to enable queue join functionality
-- This script adds services to the existing test salons

-- ============================================================================
-- ADD SERVICES FOR ELITE BARBERSHOP DOWNTOWN (55555555-5555-5555-5555-555555555555)
-- ============================================================================

INSERT INTO ServicesOffered (
  Id, Name, Description, LocationId, EstimatedDurationMinutes, PriceAmount, PriceCurrency, ImageUrl, 
  IsActive, TimesProvided, ActualAverageDurationMinutes, CreatedAt, CreatedBy, IsDeleted, RowVersion
)
VALUES
  ('77777777-7777-7777-7777-777777777771', 'Classic Haircut', 'Traditional men''s haircut with scissors and clipper', '55555555-5555-5555-5555-555555555555', 30, 25.00, 'USD', 'https://example.com/services/classic-cut.jpg', 1, 0, 30.0, NOW(), 'admin', 0, 0x0000000000000000),
  ('77777777-7777-7777-7777-777777777772', 'Beard Trim', 'Professional beard trimming and shaping', '55555555-5555-5555-5555-555555555555', 20, 15.00, 'USD', 'https://example.com/services/beard-trim.jpg', 1, 0, 20.0, NOW(), 'admin', 0, 0x0000000000000000),
  ('77777777-7777-7777-7777-777777777773', 'Premium Styling', 'Full styling service with wash, cut, and finish', '55555555-5555-5555-5555-555555555555', 45, 45.00, 'USD', 'https://example.com/services/premium-styling.jpg', 1, 0, 45.0, NOW(), 'admin', 0, 0x0000000000000000),
  ('77777777-7777-7777-7777-777777777774', 'Quick Trim', 'Fast 15-minute cleanup cut', '55555555-5555-5555-5555-555555555555', 15, 18.00, 'USD', NULL, 1, 0, 15.0, NOW(), 'admin', 0, 0x0000000000000000);

-- ============================================================================
-- ADD SERVICES FOR QUICK CUTS EXPRESS (66666666-6666-6666-6666-666666666666)
-- ============================================================================

INSERT INTO ServicesOffered (
  Id, Name, Description, LocationId, EstimatedDurationMinutes, PriceAmount, PriceCurrency, ImageUrl, 
  IsActive, TimesProvided, ActualAverageDurationMinutes, CreatedAt, CreatedBy, IsDeleted, RowVersion
)
VALUES
  ('88888888-8888-8888-8888-888888888881', 'Express Haircut', 'Quick and efficient haircut for busy professionals', '66666666-6666-6666-6666-666666666666', 20, 20.00, 'USD', 'https://example.com/services/express-cut.jpg', 1, 0, 20.0, NOW(), 'admin', 0, 0x0000000000000000),
  ('88888888-8888-8888-8888-888888888882', 'Beard Trim', 'Professional beard trimming and shaping', '66666666-6666-6666-6666-666666666666', 15, 12.00, 'USD', 'https://example.com/services/beard-trim.jpg', 1, 0, 15.0, NOW(), 'admin', 0, 0x0000000000000000),
  ('88888888-8888-8888-8888-888888888883', 'Wash & Cut', 'Hair wash followed by precision cut', '66666666-6666-6666-6666-666666666666', 25, 28.00, 'USD', 'https://example.com/services/wash-cut.jpg', 1, 0, 25.0, NOW(), 'admin', 0, 0x0000000000000000),
  ('88888888-8888-8888-8888-888888888884', 'Styling Service', 'Complete styling with product application', '66666666-6666-6666-6666-666666666666', 30, 35.00, 'USD', 'https://example.com/services/styling.jpg', 1, 0, 30.0, NOW(), 'admin', 0, 0x0000000000000000);

-- ============================================================================
-- VERIFY SERVICES WERE ADDED
-- ============================================================================

SELECT 'Services added successfully for test salons' as Status;

-- Show services for Elite Barbershop Downtown
SELECT 'Elite Barbershop Downtown Services:' as Info;
SELECT Id, Name, Description, LocationId, EstimatedDurationMinutes, PriceAmount, IsActive 
FROM ServicesOffered 
WHERE LocationId = '55555555-5555-5555-5555-555555555555' AND IsDeleted = 0;

-- Show services for Quick Cuts Express  
SELECT 'Quick Cuts Express Services:' as Info;
SELECT Id, Name, Description, LocationId, EstimatedDurationMinutes, PriceAmount, IsActive 
FROM ServicesOffered 
WHERE LocationId = '66666666-6666-6666-6666-666666666666' AND IsDeleted = 0;
