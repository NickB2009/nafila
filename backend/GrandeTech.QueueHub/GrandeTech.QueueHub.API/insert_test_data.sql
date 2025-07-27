-- SIMPLIFIED SEEDING: Only seed tables that exist in the current DbContext
-- (Organizations, Locations, SubscriptionPlans, Customers are disabled due to Slug value object issues)

-- Clear existing data (delete in order due to foreign key constraints)
DELETE FROM QueueEntries;
DELETE FROM Queues;
DELETE FROM Users;

-- Insert test data for Users
INSERT INTO Users (
  Id, Username, Email, PasswordHash, Role, LastLoginAt, IsLocked, RequiresTwoFactor, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy, IsDeleted
)
VALUES
  ('55555555-5555-5555-5555-555555555551', 'platformadmin_test', 'platformadmin@test.com', '$2a$11$8qkrKVPZzSuQGKYvHJgRMuZRvXJgKZ1K8XJJzT4yE.FQtq8WzYkO6', 'PlatformAdmin', NULL, 0, 0, GETDATE(), 'seed', GETDATE(), 'seed', 0),
  ('55555555-5555-5555-5555-555555555552', 'owner_test', 'owner@test.com', '$2a$11$7nKLTUVsN1WqP2rJrJ5R1e8D8lxqJ2zQ5JjF5sN8xQ5xJ8xN5sQ8x', 'Owner', NULL, 0, 0, GETDATE(), 'seed', GETDATE(), 'seed', 0),
  ('55555555-5555-5555-5555-555555555553', 'staff_test', 'staff@test.com', '$2a$11$9mJsY3xP5vJ2mJ8mJ5sN8xQ5xJ8xN5sQ8x2zQ5JjF5sN8xQ5xJ8xN', 'Staff', NULL, 0, 0, GETDATE(), 'seed', GETDATE(), 'seed', 0),
  ('55555555-5555-5555-5555-555555555554', 'customer_test', 'customer@test.com', '$2a$11$6lIsX2yO4wI1lI7lI4rM7yP4yI7yM4rP7y1zP4IiE4rM7yP4yI7yM', 'Customer', NULL, 0, 0, GETDATE(), 'seed', GETDATE(), 'seed', 0),
  ('55555555-5555-5555-5555-555555555555', 'service_test', 'service@test.com', '$2a$11$5kHrW1zN3vH0kH6kH3qL6zO3zH6zL3qO6z0yO3HhD3qL6zO3zH6zL', 'ServiceAccount', NULL, 0, 0, GETDATE(), 'seed', GETDATE(), 'seed', 0);

-- Insert test data for Queues (using different placeholder LocationIds since Locations are disabled)
INSERT INTO Queues (
  Id, LocationId, QueueDate, IsActive, MaxSize, LateClientCapTimeInMinutes, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy, IsDeleted
)
VALUES
  ('11111111-1111-1111-1111-111111111111', '99999999-9999-9999-9999-999999999991', CAST(GETDATE() AS DATE), 1, 50, 15, GETDATE(), 'seed', GETDATE(), 'seed', 0),
  ('11111111-1111-1111-1111-111111111112', '99999999-9999-9999-9999-999999999992', CAST(GETDATE() AS DATE), 1, 10, 5, GETDATE(), 'seed', GETDATE(), 'seed', 0),
  ('11111111-1111-1111-1111-111111111113', '99999999-9999-9999-9999-999999999993', CAST(GETDATE() AS DATE), 1, 20, 10, GETDATE(), 'seed', GETDATE(), 'seed', 0);

-- Insert test data for QueueEntries (using placeholder CustomerId since Customers are disabled)
INSERT INTO QueueEntries (
  Id, QueueId, CustomerId, CustomerName, Position, Status, StaffMemberId, ServiceTypeId, Notes, EnteredAt, CalledAt, CheckedInAt, CompletedAt, CancelledAt, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy, IsDeleted
)
VALUES
  ('22222222-2222-2222-2222-222222222221', '11111111-1111-1111-1111-111111111111', '88888888-8888-8888-8888-888888888881', 'João Silva', 1, 'Waiting', NULL, NULL, 'Regular haircut', GETDATE(), NULL, NULL, NULL, NULL, GETDATE(), 'seed', GETDATE(), 'seed', 0),
  ('22222222-2222-2222-2222-222222222222', '11111111-1111-1111-1111-111111111111', '88888888-8888-8888-8888-888888888882', 'Maria Santos', 2, 'Waiting', NULL, NULL, 'Wash and cut', GETDATE(), NULL, NULL, NULL, NULL, GETDATE(), 'seed', GETDATE(), 'seed', 0),
  ('22222222-2222-2222-2222-222222222223', '11111111-1111-1111-1111-111111111112', '88888888-8888-8888-8888-888888888883', 'Carlos Oliveira', 1, 'CheckedIn', '55555555-5555-5555-5555-555555555553', NULL, 'VIP service', DATEADD(MINUTE, -10, GETDATE()), DATEADD(MINUTE, -8, GETDATE()), DATEADD(MINUTE, -5, GETDATE()), NULL, NULL, GETDATE(), 'seed', GETDATE(), 'seed', 0),
  ('22222222-2222-2222-2222-222222222224', '11111111-1111-1111-1111-111111111113', '88888888-8888-8888-8888-888888888884', 'Ana Costa', 1, 'Waiting', NULL, NULL, 'Appointment scheduled', GETDATE(), NULL, NULL, NULL, NULL, GETDATE(), 'seed', GETDATE(), 'seed', 0);

-- Note: Organizations, Locations, SubscriptionPlans, and Customers are not included
-- because they are disabled in QueueHubDbContext.cs due to Slug value object issues.
-- These can be added back once the value object issues are resolved. 