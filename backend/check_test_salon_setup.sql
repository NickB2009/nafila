-- Check if test salons have all required data

-- Check Locations
SELECT 'Checking Locations...' as Status;
SELECT Id, Name, OrganizationId, IsActive 
FROM Locations 
WHERE Id IN (
  '55555555-5555-5555-5555-555555555555',
  '66666666-6666-6666-6666-666666666666'
);

-- Check if they have Organizations
SELECT 'Checking Organizations...' as Status;
SELECT O.Id, O.Name 
FROM Organizations O
INNER JOIN Locations L ON O.Id = L.OrganizationId
WHERE L.Id IN (
  '55555555-5555-5555-5555-555555555555',
  '66666666-6666-6666-6666-666666666666'
);

-- Check Services
SELECT 'Checking Services...' as Status;
SELECT COUNT(*) as ServiceCount, LocationId
FROM ServicesOffered
WHERE LocationId IN (
  '55555555-5555-5555-5555-555555555555',
  '66666666-6666-6666-6666-666666666666'
)
AND IsDeleted = 0
GROUP BY LocationId;

-- Check Staff
SELECT 'Checking Staff...' as Status;
SELECT COUNT(*) as StaffCount, LocationId, SUM(IsActive AND IsOnDuty) as ActiveStaff
FROM StaffMembers
WHERE LocationId IN (
  '55555555-5555-5555-5555-555555555555',
  '66666666-6666-6666-6666-666666666666'
)
AND IsDeleted = 0
GROUP BY LocationId;

-- Check Queues
SELECT 'Checking Queues...' as Status;
SELECT Id, LocationId, QueueDate, IsActive, MaxSize
FROM Queues
WHERE LocationId IN (
  '55555555-5555-5555-5555-555555555555',
  '66666666-6666-6666-6666-666666666666'
)
AND IsDeleted = 0
AND IsActive = 1;

