-- Create staff members for the test salons
-- This enables queue operations that require staff

-- ============================================================================
-- ADD STAFF FOR ELITE BARBERSHOP DOWNTOWN
-- ============================================================================

INSERT INTO StaffMembers (
  Id, 
  Name, 
  LocationId, 
  Email, 
  PhoneNumber, 
  ProfilePictureUrl, 
  Role, 
  IsActive, 
  IsOnDuty, 
  StaffStatus, 
  UserId, 
  AverageServiceTimeInMinutes, 
  CompletedServicesCount, 
  EmployeeCode, 
  Username, 
  SpecialtyServiceTypeIds,
  CreatedAt, 
  CreatedBy, 
  IsDeleted, 
  RowVersion
)
VALUES
  (
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
    'John Smith',
    '55555555-5555-5555-5555-555555555555',  -- Elite Barbershop Downtown
    'john.smith@elitebarbershop.com',
    '+1-555-0001',
    NULL,
    'Senior Barber',
    1,  -- IsActive
    1,  -- IsOnDuty
    'available',
    NULL,
    30.0,
    100,
    'ELITE001',
    'john.smith',
    '["77777777-7777-7777-7777-777777777771","77777777-7777-7777-7777-777777777772"]',
    NOW(),
    'system',
    0,
    0x0000000000000000
  ),
  (
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
    'Mike Johnson',
    '55555555-5555-5555-5555-555555555555',  -- Elite Barbershop Downtown
    'mike.johnson@elitebarbershop.com',
    '+1-555-0002',
    NULL,
    'Barber',
    1,  -- IsActive
    1,  -- IsOnDuty
    'available',
    NULL,
    25.0,
    85,
    'ELITE002',
    'mike.johnson',
    '["77777777-7777-7777-7777-777777777771"]',
    NOW(),
    'system',
    0,
    0x0000000000000000
  );

-- ============================================================================
-- ADD STAFF FOR QUICK CUTS EXPRESS
-- ============================================================================

INSERT INTO StaffMembers (
  Id, 
  Name, 
  LocationId, 
  Email, 
  PhoneNumber, 
  ProfilePictureUrl, 
  Role, 
  IsActive, 
  IsOnDuty, 
  StaffStatus, 
  UserId, 
  AverageServiceTimeInMinutes, 
  CompletedServicesCount, 
  EmployeeCode, 
  Username, 
  SpecialtyServiceTypeIds,
  CreatedAt, 
  CreatedBy, 
  IsDeleted, 
  RowVersion
)
VALUES
  (
    'cccccccc-cccc-cccc-cccc-cccccccccccc',
    'Sarah Williams',
    '66666666-6666-6666-6666-666666666666',  -- Quick Cuts Express
    'sarah.williams@quickcuts.com',
    '+1-555-0003',
    NULL,
    'Senior Stylist',
    1,  -- IsActive
    1,  -- IsOnDuty
    'available',
    NULL,
    20.0,
    150,
    'QUICK001',
    'sarah.williams',
    '["88888888-8888-8888-8888-888888888881","88888888-8888-8888-8888-888888888882"]',
    NOW(),
    'system',
    0,
    0x0000000000000000
  ),
  (
    'dddddddd-dddd-dddd-dddd-dddddddddddd',
    'Tom Brown',
    '66666666-6666-6666-6666-666666666666',  -- Quick Cuts Express
    'tom.brown@quickcuts.com',
    '+1-555-0004',
    NULL,
    'Barber',
    1,  -- IsActive
    1,  -- IsOnDuty
    'available',
    NULL,
    18.0,
    120,
    'QUICK002',
    'tom.brown',
    '["88888888-8888-8888-8888-888888888881"]',
    NOW(),
    'system',
    0,
    0x0000000000000000
  );

-- ============================================================================
-- VERIFY STAFF WERE CREATED
-- ============================================================================

SELECT 'Staff members created successfully for test salons' as Status;

-- Show staff for test salons
SELECT 'Staff Members:' as Info;
SELECT 
  S.Id as StaffId,
  S.Name as StaffName,
  L.Name as SalonName,
  S.Role,
  S.IsActive,
  S.IsOnDuty,
  S.StaffStatus
FROM StaffMembers S
INNER JOIN Locations L ON S.LocationId = L.Id
WHERE S.LocationId IN (
  '55555555-5555-5555-5555-555555555555',
  '66666666-6666-6666-6666-666666666666'
)
AND S.IsDeleted = 0;

