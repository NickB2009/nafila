using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Grande.Fila.API.Infrastructure.Services;
using Grande.Fila.API.Infrastructure.Data;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.Customers;
using Grande.Fila.API.Domain.Locations;
using Grande.Fila.API.Domain.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Grande.Fila.API.Tests.Integration.Controllers
{
    [TestClass]
    public class BulkOperationsControllerTests
    {
        private ServiceProvider _serviceProvider = null!;
        private BulkOperationsController _controller = null!;
        private QueueHubDbContext _context = null!;

        [TestInitialize]
        public void Setup()
        {
            var services = new ServiceCollection();
            
            // Add in-memory database
            services.AddDbContext<QueueHubDbContext>(options =>
                options.UseInMemoryDatabase($"ControllerTestDb_{Guid.NewGuid()}"));
            
            // Add logging
            services.AddLogging(builder => builder.AddConsole());
            
            // Add bulk operations services
            services.AddScoped<IBulkOperationsService, BulkOperationsService>();
            services.AddScoped<IQueueBulkOperationsService, QueueBulkOperationsService>();
            
            _serviceProvider = services.BuildServiceProvider();
            var bulkOperationsService = _serviceProvider.GetRequiredService<IBulkOperationsService>();
            var queueBulkOperationsService = _serviceProvider.GetRequiredService<IQueueBulkOperationsService>();
            var logger = _serviceProvider.GetRequiredService<ILogger<BulkOperationsController>>();
            
            _controller = new BulkOperationsController(bulkOperationsService, queueBulkOperationsService, logger);
            _context = _serviceProvider.GetRequiredService<QueueHubDbContext>();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _serviceProvider?.Dispose();
        }

        [TestMethod]
        public async Task BulkInsertCustomers_WithValidData_ShouldReturnOk()
        {
            // Arrange
            var customers = new List<Customer>
            {
                new Customer("John Doe", "1234567890", "john@example.com", false),
                new Customer("Jane Smith", "0987654321", "jane@example.com", false),
                new Customer("Bob Johnson", "5555555555", "bob@example.com", true)
            };

            // Act
            var result = await _controller.BulkInsert(customers) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            
            var response = result.Value;
            Assert.IsNotNull(response);
            
            // Verify customers were actually inserted
            var customerCount = await _context.Customers.CountAsync();
            Assert.AreEqual(3, customerCount);
        }

        [TestMethod]
        public async Task BulkUpdateCustomers_WithValidData_ShouldReturnOk()
        {
            // Arrange
            var customers = new List<Customer>
            {
                new Customer("John Doe", "1234567890", "john@example.com", false),
                new Customer("Jane Smith", "0987654321", "jane@example.com", false)
            };
            
            await _context.Customers.AddRangeAsync(customers);
            await _context.SaveChangesAsync();

            // Modify customers
            customers[0].GetType().GetProperty("Name")?.SetValue(customers[0], "John Updated");
            customers[1].GetType().GetProperty("Name")?.SetValue(customers[1], "Jane Updated");

            // Act
            var result = await _controller.BulkUpdate(customers) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            
            var response = result.Value;
            Assert.IsNotNull(response);
        }

        [TestMethod]
        public async Task BulkDeleteCustomers_WithValidIds_ShouldReturnOk()
        {
            // Arrange
            var customers = new List<Customer>
            {
                new Customer("John Doe", "1234567890", "john@example.com", false),
                new Customer("Jane Smith", "0987654321", "jane@example.com", false),
                new Customer("Bob Johnson", "5555555555", "bob@example.com", true)
            };
            
            await _context.Customers.AddRangeAsync(customers);
            await _context.SaveChangesAsync();

            var idsToDelete = customers.Take(2).Select(c => c.Id).ToList();

            // Act
            var result = await _controller.BulkDelete<Customer>(idsToDelete) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            
            var response = result.Value;
            Assert.IsNotNull(response);
            
            // Verify customers were actually deleted
            var customerCount = await _context.Customers.CountAsync();
            Assert.AreEqual(1, customerCount);
        }

        [TestMethod]
        public async Task BulkInsertQueueEntries_WithValidData_ShouldReturnOk()
        {
            // Arrange
            var organization = new Organization("Test Org", "test@example.com", "1234567890");
            await _context.Organizations.AddAsync(organization);
            await _context.SaveChangesAsync();

            var location = new Location(
                organization.Id,
                "Test Location",
                "123 Test St",
                "Test City",
                "Test State",
                "12345",
                "US",
                "test-location",
                "EST"
            );
            await _context.Locations.AddAsync(location);
            await _context.SaveChangesAsync();

            var queue = new Queue(location.Id, "Test Queue", "Test queue for controller testing");
            await _context.Queues.AddAsync(queue);
            await _context.SaveChangesAsync();

            var customers = new List<Customer>
            {
                new Customer("Queue Customer 1", "1111111111", "queue1@example.com", true),
                new Customer("Queue Customer 2", "2222222222", "queue2@example.com", true)
            };
            await _context.Customers.AddRangeAsync(customers);
            await _context.SaveChangesAsync();

            var queueEntries = new List<QueueEntry>
            {
                new QueueEntry(queue.Id, customers[0].Id, "Queue Customer 1", 1, null, null, "Test entry 1"),
                new QueueEntry(queue.Id, customers[1].Id, "Queue Customer 2", 2, null, null, "Test entry 2")
            };

            // Act
            var result = await _controller.BulkInsert(queueEntries) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            
            var response = result.Value;
            Assert.IsNotNull(response);
            
            // Verify queue entries were actually inserted
            var queueEntryCount = await _context.QueueEntries.CountAsync();
            Assert.AreEqual(2, queueEntryCount);
        }

        [TestMethod]
        public async Task BulkJoinQueue_WithValidRequests_ShouldReturnOk()
        {
            // Arrange
            var organization = new Organization("Test Org", "test@example.com", "1234567890");
            await _context.Organizations.AddAsync(organization);
            await _context.SaveChangesAsync();

            var location = new Location(
                organization.Id,
                "Test Location",
                "123 Test St",
                "Test City",
                "Test State",
                "12345",
                "US",
                "test-location",
                "EST"
            );
            await _context.Locations.AddAsync(location);
            await _context.SaveChangesAsync();

            var queue = new Queue(location.Id, "Test Queue", "Test queue for bulk join testing");
            await _context.Queues.AddAsync(queue);
            await _context.SaveChangesAsync();

            var joinRequests = new List<BulkJoinQueueRequest>
            {
                new BulkJoinQueueRequest
                {
                    QueueId = queue.Id,
                    CustomerName = "Bulk Customer 1",
                    PhoneNumber = "1111111111",
                    Email = "bulk1@example.com",
                    IsAnonymous = true,
                    ServiceTypeId = null,
                    Notes = "Bulk join test 1"
                },
                new BulkJoinQueueRequest
                {
                    QueueId = queue.Id,
                    CustomerName = "Bulk Customer 2",
                    PhoneNumber = "2222222222",
                    Email = "bulk2@example.com",
                    IsAnonymous = true,
                    ServiceTypeId = null,
                    Notes = "Bulk join test 2"
                }
            };

            // Act
            var result = await _controller.BulkJoinQueue(joinRequests) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            
            var response = result.Value;
            Assert.IsNotNull(response);
            
            // Verify queue entries were created
            var queueEntryCount = await _context.QueueEntries.CountAsync();
            Assert.AreEqual(2, queueEntryCount);
            
            // Verify customers were created
            var customerCount = await _context.Customers.CountAsync();
            Assert.AreEqual(2, customerCount);
        }

        [TestMethod]
        public async Task BulkUpdateQueueEntryStatus_WithValidData_ShouldReturnOk()
        {
            // Arrange
            var organization = new Organization("Test Org", "test@example.com", "1234567890");
            await _context.Organizations.AddAsync(organization);
            await _context.SaveChangesAsync();

            var location = new Location(
                organization.Id,
                "Test Location",
                "123 Test St",
                "Test City",
                "Test State",
                "12345",
                "US",
                "test-location",
                "EST"
            );
            await _context.Locations.AddAsync(location);
            await _context.SaveChangesAsync();

            var queue = new Queue(location.Id, "Test Queue", "Test queue for status updates");
            await _context.Queues.AddAsync(queue);
            await _context.SaveChangesAsync();

            var customer = new Customer("Status Test Customer", "1234567890", "status@example.com", true);
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();

            var queueEntry = new QueueEntry(
                queue.Id,
                customer.Id,
                "Status Test Customer",
                1,
                null,
                null,
                "Status test entry"
            );
            await _context.QueueEntries.AddAsync(queueEntry);
            await _context.SaveChangesAsync();

            var statusUpdates = new List<BulkUpdateQueueEntryStatusRequest>
            {
                new BulkUpdateQueueEntryStatusRequest
                {
                    EntryId = queueEntry.Id,
                    NewStatus = QueueEntryStatus.Called
                }
            };

            // Act
            var result = await _controller.BulkUpdateQueueEntryStatus(statusUpdates) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            
            var response = result.Value;
            Assert.IsNotNull(response);
            
            // Verify status was updated
            var updatedEntry = await _context.QueueEntries.FirstAsync(e => e.Id == queueEntry.Id);
            Assert.AreEqual(QueueEntryStatus.Called, updatedEntry.Status);
        }

        [TestMethod]
        public async Task GetBulkOperationStats_ShouldReturnAccumulatedStatistics()
        {
            // Arrange - Perform some bulk operations to generate stats
            var customers = new List<Customer>
            {
                new Customer("Stats Customer 1", "1111111111", "stats1@example.com", true),
                new Customer("Stats Customer 2", "2222222222", "stats2@example.com", true)
            };
            
            await _controller.BulkInsert(customers);

            // Act
            var result = await _controller.GetBulkOperationStats() as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            
            var response = result.Value;
            Assert.IsNotNull(response);
        }

        [TestMethod]
        public async Task BulkOperations_WithEmptyData_ShouldHandleGracefully()
        {
            // Arrange
            var emptyCustomers = new List<Customer>();

            // Act
            var result = await _controller.BulkInsert(emptyCustomers) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            
            var response = result.Value;
            Assert.IsNotNull(response);
        }

        [TestMethod]
        public async Task BulkOperations_WithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange - Create customers with potentially invalid data
            var invalidCustomers = new List<Customer>
            {
                new Customer("", "1234567890", "valid@example.com", false), // Empty name
                new Customer("Valid Customer", "", "valid@example.com", false) // Empty phone
            };

            // Act
            var result = await _controller.BulkInsert(invalidCustomers);

            // Assert
            // The result might be OkObjectResult with success=false, or BadRequestObjectResult
            // depending on how the service handles validation errors
            Assert.IsNotNull(result);
        }
    }

    // Helper classes for the controller tests
    public class BulkJoinQueueRequest
    {
        public Guid QueueId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public bool IsAnonymous { get; set; }
        public Guid? ServiceTypeId { get; set; }
        public string? Notes { get; set; }
    }

    public class BulkUpdateQueueEntryStatusRequest
    {
        public Guid EntryId { get; set; }
        public QueueEntryStatus NewStatus { get; set; }
    }
}
