-- Create active queues for the test salons to enable anonymous queue join
-- Run this script against the BoaHost MySQL database

-- ============================================================================
-- CREATE ACTIVE QUEUE FOR ELITE BARBERSHOP DOWNTOWN
-- ============================================================================

INSERT INTO Queues (
  Id, 
  LocationId, 
  QueueDate, 
  IsActive, 
  MaxSize, 
  LateClientCapTimeInMinutes, 
  CreatedAt, 
  CreatedBy, 
  IsDeleted, 
  RowVersion
)
VALUES
  (
    '99999999-9999-9999-9999-999999999991',
    '55555555-5555-5555-5555-555555555555',  -- Elite Barbershop Downtown
    CAST(NOW() AS DATE),
    1,  -- IsActive = true
    100,  -- MaxSize
    15,  -- LateClientCapTimeInMinutes
    NOW(),
    'system',
    0,  -- IsDeleted = false
    0x0000000000000000
  );

-- ============================================================================
-- CREATE ACTIVE QUEUE FOR QUICK CUTS EXPRESS
-- ============================================================================

INSERT INTO Queues (
  Id, 
  LocationId, 
  QueueDate, 
  IsActive, 
  MaxSize, 
  LateClientCapTimeInMinutes, 
  CreatedAt, 
  CreatedBy, 
  IsDeleted, 
  RowVersion
)
VALUES
  (
    '99999999-9999-9999-9999-999999999992',
    '66666666-6666-6666-6666-666666666666',  -- Quick Cuts Express
    CAST(NOW() AS DATE),
    1,  -- IsActive = true
    80,  -- MaxSize
    10,  -- LateClientCapTimeInMinutes
    NOW(),
    'system',
    0,  -- IsDeleted = false
    0x0000000000000000
  );

-- ============================================================================
-- VERIFY QUEUES WERE CREATED
-- ============================================================================

SELECT 'Active queues created successfully for test salons' as Status;

-- Show active queues
SELECT 'Active Queues:' as Info;
SELECT 
  Q.Id as QueueId,
  L.Name as SalonName,
  Q.QueueDate,
  Q.IsActive,
  Q.MaxSize,
  COUNT(QE.Id) as CurrentEntries
FROM Queues Q
INNER JOIN Locations L ON Q.LocationId = L.Id
LEFT JOIN QueueEntries QE ON Q.Id = QE.QueueId AND QE.IsDeleted = 0
WHERE Q.LocationId IN (
  '55555555-5555-5555-5555-555555555555',  -- Elite Barbershop Downtown
  '66666666-6666-6666-6666-666666666666'   -- Quick Cuts Express
)
AND Q.IsDeleted = 0
GROUP BY Q.Id, L.Name, Q.QueueDate, Q.IsActive, Q.MaxSize;

