-- Test script to check if seeding worked
SELECT 'Users Table' as TableName, COUNT(*) as RecordCount FROM Users
UNION ALL
SELECT 'Queues Table' as TableName, COUNT(*) as RecordCount FROM Queues  
UNION ALL
SELECT 'QueueEntries Table' as TableName, COUNT(*) as RecordCount FROM QueueEntries;

-- Show sample data
SELECT 'Sample Users:' as Info;
SELECT TOP 3 Id, Username, Email, Role FROM Users;

SELECT 'Sample Queues:' as Info;
SELECT TOP 3 Id, LocationId, QueueDate, IsActive, MaxSize FROM Queues;

SELECT 'Sample QueueEntries:' as Info;
SELECT TOP 3 Id, QueueId, CustomerName, Position, Status FROM QueueEntries; 