-- Check if test salons have queue functionality enabled
SELECT 
  Id, 
  Name, 
  IsActive, 
  IsQueueEnabled,
  MaxQueueSize,
  OrganizationId
FROM Locations 
WHERE Id IN ('55555555-5555-5555-5555-555555555555', '66666666-6666-6666-6666-666666666666');

-- If IsQueueEnabled is 0, fix it:
UPDATE Locations 
SET IsQueueEnabled = 1, MaxQueueSize = 100
WHERE Id IN ('55555555-5555-5555-5555-555555555555', '66666666-6666-6666-6666-666666666666');

-- Verify
SELECT 'After Update:' as Status;
SELECT Id, Name, IsActive, IsQueueEnabled, MaxQueueSize
FROM Locations 
WHERE Id IN ('55555555-5555-5555-5555-555555555555', '66666666-6666-6666-6666-666666666666');

