-- Check current queue dates
SELECT Id, LocationId, QueueDate, IsActive 
FROM Queues 
WHERE LocationId IN ('55555555-5555-5555-5555-555555555555', '66666666-6666-6666-6666-666666666666')
AND IsDeleted = 0;

-- Update queues to TODAY's date
UPDATE Queues 
SET QueueDate = CAST(NOW() AS DATE), IsActive = 1
WHERE LocationId IN ('55555555-5555-5555-5555-555555555555', '66666666-6666-6666-6666-666666666666')
AND IsDeleted = 0;

-- Verify
SELECT 'Updated queues to today:' as Status;
SELECT Id, LocationId, QueueDate, IsActive 
FROM Queues 
WHERE LocationId IN ('55555555-5555-5555-5555-555555555555', '66666666-6666-6666-6666-666666666666')
AND IsDeleted = 0;


