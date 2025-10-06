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

namespace Grande.Fila.API.Tests.Integration.LoadTests
{
    [TestClass]
    public class BulkOperationsLoadTests
    {
        private ServiceProvider _serviceProvider = null!;
        private IBulkOperationsService _bulkOperationsService = null!;
        private IQueueBulkOperationsService _queueBulkOperationsService = null!;
        private QueueHubDbContext _context = null!;

        [TestInitialize]
        public void Setup()
        {
            var services = new ServiceCollection();
            
            // Add in-memory database
            services.AddDbContext<QueueHubDbContext>(options =>
                options.UseInMemoryDatabase($"LoadTestDb_{Guid.NewGuid()}"));
            
            // Add logging
            services.AddLogging(builder => builder.AddConsole());
            
            // Add bulk operations services
            services.AddScoped<IBulkOperationsService, BulkOperationsService>();
            services.AddScoped<IQueueBulkOperationsService, QueueBulkOperationsService>();
            
            _serviceProvider = services.BuildServiceProvider();
            _bulkOperationsService = _serviceProvider.GetRequiredService<IBulkOperationsService>();
            _queueBulkOperationsService = _serviceProvider.GetRequiredService<IQueueBulkOperationsService>();
            _context = _serviceProvider.GetRequiredService<QueueHubDbContext>();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _serviceProvider?.Dispose();
        }

        [TestMethod]
        public async Task BulkInsertQueueEntries_WithHighVolume_ShouldMaintainPerformance()
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

            var queue = new Queue(location.Id, "Test Queue", "Test queue for load testing");
            await _context.Queues.AddAsync(queue);
            await _context.SaveChangesAsync();

            // Create 10,000 queue entries for load testing
            var queueEntries = new List<QueueEntry>();
            for (int i = 0; i < 10000; i++)
            {
                var customer = new Customer($"Customer {i}", $"phone{i}", $"email{i}@example.com", true);
                await _context.Customers.AddAsync(customer);
                await _context.SaveChangesAsync();

                var queueEntry = new QueueEntry(
                    queue.Id,
                    customer.Id,
                    $"Customer {i}",
                    i + 1,
                    null, // staffMemberId
                    null, // serviceTypeId
                    $"Load test entry {i}"
                );
                queueEntries.Add(queueEntry);
            }

            // Act
            var stopwatch = Stopwatch.StartNew();
            var result = await _bulkOperationsService.BulkInsertAsync(queueEntries, batchSize: 1000);
            stopwatch.Stop();

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(10000, result.RecordsAffected);
            Assert.IsTrue(result.RecordsPerSecond > 1000); // Should process at least 1000 records/second
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 30000); // Should complete within 30 seconds
            Assert.IsTrue(result.BatchesProcessed > 1); // Should use multiple batches
            
            // Verify data integrity
            var actualCount = await _context.QueueEntries.CountAsync();
            Assert.AreEqual(10000, actualCount);
        }

        [TestMethod]
        public async Task BulkUpdateStatus_WithConcurrentOperations_ShouldHandleCorrectly()
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

            var queue = new Queue(location.Id, "Test Queue", "Test queue for concurrent testing");
            await _context.Queues.AddAsync(queue);
            await _context.SaveChangesAsync();

            // Create 1000 queue entries
            var queueEntries = new List<QueueEntry>();
            for (int i = 0; i < 1000; i++)
            {
                var customer = new Customer($"Customer {i}", $"phone{i}", $"email{i}@example.com", true);
                await _context.Customers.AddAsync(customer);
                await _context.SaveChangesAsync();

                var queueEntry = new QueueEntry(
                    queue.Id,
                    customer.Id,
                    $"Customer {i}",
                    i + 1,
                    null,
                    null,
                    $"Concurrent test entry {i}"
                );
                queueEntries.Add(queueEntry);
            }
            await _context.QueueEntries.AddRangeAsync(queueEntries);
            await _context.SaveChangesAsync();

            // Act - Simulate concurrent bulk operations
            var tasks = new List<Task>();
            var batchSize = 200;
            
            for (int i = 0; i < queueEntries.Count; i += batchSize)
            {
                var batch = queueEntries.Skip(i).Take(batchSize).ToList();
                var task = Task.Run(async () =>
                {
                    var result = await _queueBulkOperationsService.BulkUpdateQueueEntryStatusAsync(
                        batch.Select(e => e.Id).ToList(),
                        QueueEntryStatus.Called
                    );
                    Assert.IsTrue(result.Success);
                });
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            // Assert - Verify all entries were updated correctly
            var updatedEntries = await _context.QueueEntries
                .Where(e => e.Status == QueueEntryStatus.Called)
                .CountAsync();
            Assert.AreEqual(1000, updatedEntries);
        }

        [TestMethod]
        public async Task BulkOperations_MemoryUsage_ShouldRemainStable()
        {
            // Arrange
            var initialMemory = GC.GetTotalMemory(true);
            
            var customers = new List<Customer>();
            for (int i = 0; i < 5000; i++)
            {
                customers.Add(new Customer($"Customer {i}", $"phone{i}", $"email{i}@example.com", true));
            }

            // Act
            var result = await _bulkOperationsService.BulkInsertAsync(customers, batchSize: 500);
            
            // Force garbage collection
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            var finalMemory = GC.GetTotalMemory(false);
            var memoryIncrease = finalMemory - initialMemory;

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(5000, result.RecordsAffected);
            
            // Memory increase should be reasonable (less than 100MB for 5000 records)
            Assert.IsTrue(memoryIncrease < 100 * 1024 * 1024, $"Memory increase too high: {memoryIncrease / (1024 * 1024)}MB");
        }

        [TestMethod]
        public async Task BulkOperations_WithDifferentBatchSizes_ShouldOptimizePerformance()
        {
            // Arrange
            var testData = new List<(int batchSize, string description)>
            {
                (100, "Small batches"),
                (500, "Medium batches"),
                (1000, "Large batches"),
                (2000, "Extra large batches")
            };

            var results = new List<(int batchSize, double recordsPerSecond, long duration)>();

            foreach (var (batchSize, description) in testData)
            {
                // Clear database for each test
                _context.Customers.RemoveRange(await _context.Customers.ToListAsync());
                await _context.SaveChangesAsync();

                var customers = new List<Customer>();
                for (int i = 0; i < 2000; i++)
                {
                    customers.Add(new Customer($"Customer {batchSize}_{i}", $"phone{i}", $"email{i}@example.com", true));
                }

                // Act
                var stopwatch = Stopwatch.StartNew();
                var result = await _bulkOperationsService.BulkInsertAsync(customers, batchSize: batchSize);
                stopwatch.Stop();

                // Record results
                results.Add((batchSize, result.RecordsPerSecond, stopwatch.ElapsedMilliseconds));
                
                // Assert basic success
                Assert.IsTrue(result.Success, $"Failed with batch size {batchSize}");
                Assert.AreEqual(2000, result.RecordsAffected);
            }

            // Assert performance characteristics
            Assert.IsTrue(results.Count == 4);
            
            // Log results for analysis
            Console.WriteLine("\nBatch Size Performance Analysis:");
            foreach (var (batchSize, recordsPerSecond, duration) in results)
            {
                Console.WriteLine($"Batch Size: {batchSize}, Records/sec: {recordsPerSecond:F0}, Duration: {duration}ms");
            }
        }

        [TestMethod]
        public async Task QueueBulkOperations_ComplexScenario_ShouldHandleCorrectly()
        {
            // Arrange - Create a realistic scenario with multiple queues and staff
            var organization = new Organization("Load Test Org", "loadtest@example.com", "1234567890");
            await _context.Organizations.AddAsync(organization);
            await _context.SaveChangesAsync();

            var location = new Location(
                organization.Id,
                "Load Test Location",
                "456 Load St",
                "Load City",
                "Load State",
                "54321",
                "US",
                "load-test-location",
                "EST"
            );
            await _context.Locations.AddAsync(location);
            await _context.SaveChangesAsync();

            var queue = new Queue(location.Id, "Load Test Queue", "Queue for complex load testing");
            await _context.Queues.AddAsync(queue);
            await _context.SaveChangesAsync();

            // Create 2000 queue entries
            var queueEntries = new List<QueueEntry>();
            var customers = new List<Customer>();
            
            for (int i = 0; i < 2000; i++)
            {
                var customer = new Customer($"LoadCustomer {i}", $"phone{i}", $"email{i}@example.com", true);
                customers.Add(customer);
                
                var queueEntry = new QueueEntry(
                    queue.Id,
                    customer.Id,
                    $"LoadCustomer {i}",
                    i + 1,
                    null,
                    null,
                    $"Complex load test entry {i}"
                );
                queueEntries.Add(queueEntry);
            }

            // Act - Bulk insert customers first
            var customerResult = await _bulkOperationsService.BulkInsertAsync(customers);
            Assert.IsTrue(customerResult.Success);

            // Bulk insert queue entries
            var queueEntryResult = await _bulkOperationsService.BulkInsertAsync(queueEntries);
            Assert.IsTrue(queueEntryResult.Success);

            // Bulk update statuses
            var statusUpdateResult = await _queueBulkOperationsService.BulkUpdateQueueEntryStatusAsync(
                queueEntries.Take(500).Select(e => e.Id).ToList(),
                QueueEntryStatus.Called
            );
            Assert.IsTrue(statusUpdateResult.Success);

            // Bulk update positions
            var positionUpdates = queueEntries.Skip(500).Take(300).Select((entry, index) => new
            {
                EntryId = entry.Id,
                NewPosition = index + 1
            }).ToList();

            var positionUpdateResult = await _queueBulkOperationsService.BulkUpdateEntryPositionsAsync(
                positionUpdates.ToDictionary(p => p.EntryId, p => p.NewPosition)
            );
            Assert.IsTrue(positionUpdateResult.Success);

            // Assert - Verify all operations completed successfully
            var totalQueueEntries = await _context.QueueEntries.CountAsync();
            var calledEntries = await _context.QueueEntries.CountAsync(e => e.Status == QueueEntryStatus.Called);
            var totalCustomers = await _context.Customers.CountAsync();

            Assert.AreEqual(2000, totalQueueEntries);
            Assert.AreEqual(500, calledEntries);
            Assert.AreEqual(2000, totalCustomers);
        }
    }
}
