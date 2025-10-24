-- Comprehensive verification of test salon setup

-- 1. Check Locations
SELECT '=== LOCATIONS ===' as Check_Type;
SELECT Id, Name, OrganizationId, IsActive, IsQueueEnabled 
FROM Locations 
WHERE Id IN ('55555555-5555-5555-5555-555555555555', '66666666-6666-6666-6666-666666666666');

-- 2. Check Organizations
SELECT '=== ORGANIZATIONS ===' as Check_Type;
SELECT O.Id, O.Name, O.IsActive, O.SubscriptionPlanId
FROM Organizations O
WHERE O.Id IN (
  SELECT OrganizationId FROM Locations WHERE Id IN ('55555555-5555-5555-5555-555555555555', '66666666-6666-6666-6666-666666666666')
);

-- 3. Check Services
SELECT '=== SERVICES ===' as Check_Type;
SELECT Id, Name, LocationId, IsActive
FROM ServicesOffered
WHERE LocationId IN ('55555555-5555-5555-5555-555555555555', '66666666-6666-6666-6666-666666666666')
AND IsDeleted = 0;

-- 4. Check Staff
SELECT '=== STAFF ===' as Check_Type;
SELECT Id, Name, LocationId, IsActive, IsOnDuty, StaffStatus
FROM StaffMembers
WHERE LocationId IN ('55555555-5555-5555-5555-555555555555', '66666666-6666-6666-6666-666666666666')
AND IsDeleted = 0;

-- 5. Check Queues
SELECT '=== QUEUES ===' as Check_Type;
SELECT Q.Id, L.Name as LocationName, Q.QueueDate, Q.IsActive, Q.MaxSize
FROM Queues Q
INNER JOIN Locations L ON Q.LocationId = L.Id
WHERE Q.LocationId IN ('55555555-5555-5555-5555-555555555555', '66666666-6666-6666-6666-666666666666')
AND Q.IsDeleted = 0
AND Q.IsActive = 1;

-- 6. Check if Subscription Plans exist
SELECT '=== SUBSCRIPTION PLANS ===' as Check_Type;
SELECT Id, Name, IsActive
FROM SubscriptionPlans
WHERE Id IN (
  SELECT SubscriptionPlanId FROM Organizations WHERE Id IN (
    SELECT OrganizationId FROM Locations WHERE Id IN ('55555555-5555-5555-5555-555555555555', '66666666-6666-6666-6666-666666666666')
  )
);

-- Summary
SELECT '=== SUMMARY ===' as Check_Type;
SELECT 
  'Elite Barbershop' as Salon,
  (SELECT COUNT(*) FROM ServicesOffered WHERE LocationId = '55555555-5555-5555-5555-555555555555' AND IsDeleted = 0) as Services,
  (SELECT COUNT(*) FROM StaffMembers WHERE LocationId = '55555555-5555-5555-5555-555555555555' AND IsDeleted = 0) as Staff,
  (SELECT COUNT(*) FROM Queues WHERE LocationId = '55555555-5555-5555-5555-555555555555' AND IsActive = 1 AND IsDeleted = 0) as ActiveQueues
UNION ALL
SELECT 
  'Quick Cuts' as Salon,
  (SELECT COUNT(*) FROM ServicesOffered WHERE LocationId = '66666666-6666-6666-6666-666666666666' AND IsDeleted = 0) as Services,
  (SELECT COUNT(*) FROM StaffMembers WHERE LocationId = '66666666-6666-6666-6666-666666666666' AND IsDeleted = 0) as Staff,
  (SELECT COUNT(*) FROM Queues WHERE LocationId = '66666666-6666-6666-6666-666666666666' AND IsActive = 1 AND IsDeleted = 0) as ActiveQueues;

