-- Check if subscription plans exist
SELECT Id, Name, IsActive FROM SubscriptionPlans;

-- Check what subscription plans our test organizations are using
SELECT O.Id, O.Name, O.SubscriptionPlanId, SP.Name as PlanName
FROM Organizations O
LEFT JOIN SubscriptionPlans SP ON O.SubscriptionPlanId = SP.Id
WHERE O.Id IN ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', 'ffffffff-ffff-ffff-ffff-ffffffffffff');

