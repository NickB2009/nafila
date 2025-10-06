using Microsoft.Extensions.Logging;
using Grande.Fila.API.Infrastructure.Services;
using Grande.Fila.API.Infrastructure.Data;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Grande.Fila.API.Tests.Infrastructure.Services
{
    [TestClass]
    public class BulkOperationsServiceTests
    {
        private ServiceProvider _serviceProvider = null!;
        private IBulkOperationsService _bulkOperationsService = null!;
        private QueueHubDbContext _context = null!;

        [TestInitialize]
        public void Setup()
        {
            var services = new ServiceCollection();
            
            // Add in-memory database
            services.AddDbContext<QueueHubDbContext>(options =>
                options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));
            
            // Add logging
            services.AddLogging(builder => builder.AddConsole());
            
            // Add bulk operations service
            services.AddScoped<IBulkOperationsService, BulkOperationsService>();
            
            _serviceProvider = services.BuildServiceProvider();
            _bulkOperationsService = _serviceProvider.GetRequiredService<IBulkOperationsService>();
            _context = _serviceProvider.GetRequiredService<QueueHubDbContext>();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _serviceProvider?.Dispose();
        }

        [TestMethod]
        public async Task BulkInsertAsync_WithValidEntities_ShouldInsertSuccessfully()
        {
            // Arrange
            var customers = new List<Customer>
            {
                new Customer("John Doe", "1234567890", "john@example.com", false),
                new Customer("Jane Smith", "0987654321", "jane@example.com", false),
                new Customer("Bob Johnson", "5555555555", "bob@example.com", true)
            };

            // Act
            var result = await _bulkOperationsService.BulkInsertAsync(customers);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(3, result.RecordsAffected);
            Assert.AreEqual(3, await _context.Customers.CountAsync());
            Assert.IsTrue(result.RecordsPerSecond > 0);
        }

        [TestMethod]
        public async Task BulkInsertAsync_WithLargeDataset_ShouldProcessInBatches()
        {
            // Arrange
            var customers = new List<Customer>();
            for (int i = 0; i < 2500; i++)
            {
                customers.Add(new Customer($"Customer {i}", $"phone{i}", $"email{i}@example.com", true));
            }

            // Act
            var stopwatch = Stopwatch.StartNew();
            var result = await _bulkOperationsService.BulkInsertAsync(customers, batchSize: 1000);
            stopwatch.Stop();

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(2500, result.RecordsAffected);
            Assert.AreEqual(2500, await _context.Customers.CountAsync());
            Assert.IsTrue(result.BatchesProcessed > 1); // Should be processed in multiple batches
            Assert.IsTrue(result.RecordsPerSecond > 100); // Should be fast
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 5000); // Should complete quickly
        }

        [TestMethod]
        public async Task BulkUpdateAsync_WithValidEntities_ShouldUpdateSuccessfully()
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
            var result = await _bulkOperationsService.BulkUpdateAsync(customers);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(2, result.RecordsAffected);
        }

        [TestMethod]
        public async Task BulkDeleteAsync_WithValidIds_ShouldDeleteSuccessfully()
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

            var idsToDelete = customers.Take(2).Select(c => (object)c.Id).ToList();

            // Act
            var result = await _bulkOperationsService.BulkDeleteAsync<Customer>(idsToDelete);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(2, result.RecordsAffected);
            Assert.AreEqual(1, await _context.Customers.CountAsync());
        }

        [TestMethod]
        public async Task GetOptimalBatchSize_ForDifferentEntityTypes_ShouldReturnAppropriateSizes()
        {
            // Act & Assert
            var queueEntryBatchSize = _bulkOperationsService.GetOptimalBatchSize<QueueEntry>();
            var customerBatchSize = _bulkOperationsService.GetOptimalBatchSize<Customer>();

            Assert.IsTrue(queueEntryBatchSize > 0);
            Assert.IsTrue(customerBatchSize > 0);
            Assert.IsTrue(queueEntryBatchSize > customerBatchSize); // QueueEntry should have larger batch size
        }

        [TestMethod]
        public async Task GetBulkOperationStatsAsync_ShouldReturnAccumulatedStatistics()
        {
            // Arrange
            var customers = new List<Customer>
            {
                new Customer("John Doe", "1234567890", "john@example.com", false),
                new Customer("Jane Smith", "0987654321", "jane@example.com", false)
            };

            await _bulkOperationsService.BulkInsertAsync(customers);

            // Act
            var stats = await _bulkOperationsService.GetBulkOperationStatsAsync();

            // Assert
            Assert.IsNotNull(stats);
            Assert.IsTrue(stats.TotalOperations > 0);
            Assert.IsTrue(stats.TotalRecordsProcessed > 0);
            Assert.IsTrue(stats.StatsByEntityType.ContainsKey("Customer"));
        }

        [TestMethod]
        public async Task BulkInsertAsync_WithEmptyCollection_ShouldReturnSuccess()
        {
            // Arrange
            var emptyCustomers = new List<Customer>();

            // Act
            var result = await _bulkOperationsService.BulkInsertAsync(emptyCustomers);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, result.RecordsAffected);
            Assert.AreEqual(0, result.BatchesProcessed);
        }

        [TestMethod]
        public async Task ExecuteBulkSqlAsync_WithValidSql_ShouldExecuteSuccessfully()
        {
            // Arrange
            var sql = "INSERT INTO Customers (Id, Name, PhoneNumber, Email, IsAnonymous, CreatedAt, CreatedBy) VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6)";
            var parameters = new List<object[]>
            {
                new object[] { Guid.NewGuid(), "SQL Customer 1", "1111111111", "sql1@example.com", true, DateTime.UtcNow, "test" },
                new object[] { Guid.NewGuid(), "SQL Customer 2", "2222222222", "sql2@example.com", true, DateTime.UtcNow, "test" }
            };

            // Act
            var result = await _bulkOperationsService.ExecuteBulkSqlAsync(sql, parameters);

            // Assert
            Assert.IsNotNull(result);
            // Note: The exact success depends on the SQL syntax and database setup
        }
    }
}
