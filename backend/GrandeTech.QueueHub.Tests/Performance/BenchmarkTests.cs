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

namespace Grande.Fila.API.Tests.Performance
{
    [TestClass]
    public class BenchmarkTests
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
                options.UseInMemoryDatabase($"BenchmarkDb_{Guid.NewGuid()}"));
            
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
        public async Task BulkOperations_Benchmark_PerformanceComparison()
        {
            // Arrange
            var testSizes = new[] { 100, 500, 1000, 2000 };
            var results = new List<(int size, long individualMs, long bulkMs, double improvement)>();

            foreach (var size in testSizes)
            {
                // Clear database
                _context.Customers.RemoveRange(await _context.Customers.ToListAsync());
                await _context.SaveChangesAsync();

                var customers = new List<Customer>();
                for (int i = 0; i < size; i++)
                {
                    customers.Add(new Customer($"Benchmark Customer {size}_{i}", $"phone{i}", $"email{i}@example.com", true));
                }

                // Test individual inserts
                var individualStopwatch = Stopwatch.StartNew();
                foreach (var customer in customers)
                {
                    _context.Customers.Add(customer);
                    await _context.SaveChangesAsync();
                }
                individualStopwatch.Stop();

                // Clear database
                _context.Customers.RemoveRange(await _context.Customers.ToListAsync());
                await _context.SaveChangesAsync();

                // Test bulk insert
                var bulkStopwatch = Stopwatch.StartNew();
                await _bulkOperationsService.BulkInsertAsync(customers);
                bulkStopwatch.Stop();

                var improvement = (double)individualStopwatch.ElapsedMilliseconds / bulkStopwatch.ElapsedMilliseconds;
                results.Add((size, individualStopwatch.ElapsedMilliseconds, bulkStopwatch.ElapsedMilliseconds, improvement));

                Console.WriteLine($"Size: {size}, Individual: {individualStopwatch.ElapsedMilliseconds}ms, Bulk: {bulkStopwatch.ElapsedMilliseconds}ms, Improvement: {improvement:F2}x");
            }

            // Assert - Bulk operations should be significantly faster
            foreach (var (size, individualMs, bulkMs, improvement) in results)
            {
                Assert.IsTrue(improvement > 5, $"Bulk operations should be at least 5x faster than individual operations for size {size}. Improvement: {improvement:F2}x");
                Assert.IsTrue(bulkMs < individualMs, $"Bulk operations should be faster than individual operations for size {size}");
            }

            // Log summary
            var avgImprovement = results.Average(r => r.improvement);
            Console.WriteLine($"\nAverage improvement: {avgImprovement:F2}x");
            Assert.IsTrue(avgImprovement > 10, $"Average improvement should be at least 10x. Actual: {avgImprovement:F2}x");
        }

        [TestMethod]
        public async Task QueryOptimization_Benchmark_IndexPerformance()
        {
            // Arrange - Create test data
            var customers = new List<Customer>();
            for (int i = 0; i < 1000; i++)
            {
                customers.Add(new Customer($"Index Customer {i}", $"phone{i}", $"email{i}@example.com", i % 2 == 0));
            }
            await _context.Customers.AddRangeAsync(customers);
            await _context.SaveChangesAsync();

            // Test queries that would benefit from indexes
            var testQueries = new[]
            {
                "Find by name",
                "Find by phone",
                "Find by email",
                "Find by anonymous status"
            };

            var queryTimes = new Dictionary<string, List<long>>();

            // Run each query multiple times to get average performance
            foreach (var queryName in testQueries)
            {
                var times = new List<long>();
                
                for (int i = 0; i < 10; i++)
                {
                    var stopwatch = Stopwatch.StartNew();
                    
                    switch (queryName)
                    {
                        case "Find by name":
                            await _context.Customers.Where(c => c.Name.Contains("Index Customer")).ToListAsync();
                            break;
                        case "Find by phone":
                            await _context.Customers.Where(c => c.PhoneNumber!.Contains("phone")).ToListAsync();
                            break;
                        case "Find by email":
                            await _context.Customers.Where(c => c.Email!.Contains("email")).ToListAsync();
                            break;
                        case "Find by anonymous status":
                            await _context.Customers.Where(c => c.IsAnonymous == true).ToListAsync();
                            break;
                    }
                    
                    stopwatch.Stop();
                    times.Add(stopwatch.ElapsedMilliseconds);
                }
                
                queryTimes[queryName] = times;
                var avgTime = times.Average();
                Console.WriteLine($"{queryName}: Average {avgTime:F2}ms (Min: {times.Min()}ms, Max: {times.Max()}ms)");
            }

            // Assert - All queries should complete quickly (under 100ms for in-memory DB)
            foreach (var (queryName, times) in queryTimes)
            {
                var avgTime = times.Average();
                Assert.IsTrue(avgTime < 100, $"Query '{queryName}' should complete in under 100ms. Average: {avgTime:F2}ms");
            }
        }

        [TestMethod]
        public async Task MemoryUsage_Benchmark_ShouldRemainStable()
        {
            // Arrange
            var initialMemory = GC.GetTotalMemory(true);
            var memorySnapshots = new List<long> { initialMemory };

            // Act - Perform bulk operations of increasing sizes
            var testSizes = new[] { 500, 1000, 2000, 3000 };
            
            foreach (var size in testSizes)
            {
                // Clear database
                _context.Customers.RemoveRange(await _context.Customers.ToListAsync());
                await _context.SaveChangesAsync();

                var customers = new List<Customer>();
                for (int i = 0; i < size; i++)
                {
                    customers.Add(new Customer($"Memory Customer {size}_{i}", $"phone{i}", $"email{i}@example.com", true));
                }

                // Perform bulk insert
                await _bulkOperationsService.BulkInsertAsync(customers, batchSize: 500);

                // Force garbage collection and measure memory
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                
                var currentMemory = GC.GetTotalMemory(false);
                memorySnapshots.Add(currentMemory);
                
                var memoryIncrease = currentMemory - initialMemory;
                Console.WriteLine($"Size: {size}, Memory: {currentMemory / (1024 * 1024)}MB, Increase: {memoryIncrease / (1024 * 1024)}MB");
            }

            // Assert - Memory usage should be reasonable
            var maxMemoryIncrease = memorySnapshots.Max() - initialMemory;
            Assert.IsTrue(maxMemoryIncrease < 200 * 1024 * 1024, $"Memory increase should be less than 200MB. Actual: {maxMemoryIncrease / (1024 * 1024)}MB");

            // Memory should not grow linearly with data size (should be bounded)
            var memoryGrowthRate = (memorySnapshots.Last() - memorySnapshots.First()) / (double)testSizes.Last();
            Assert.IsTrue(memoryGrowthRate < 50 * 1024, $"Memory growth rate should be less than 50KB per record. Actual: {memoryGrowthRate / 1024:F2}KB per record");
        }

        [TestMethod]
        public async Task ConcurrentOperations_Benchmark_ShouldMaintainPerformance()
        {
            // Arrange
            var concurrentTasks = 5;
            var recordsPerTask = 200;
            var tasks = new List<Task<BulkOperationResult>>();

            // Act - Run concurrent bulk operations
            for (int taskId = 0; taskId < concurrentTasks; taskId++)
            {
                var task = Task.Run(async () =>
                {
                    var customers = new List<Customer>();
                    for (int i = 0; i < recordsPerTask; i++)
                    {
                        customers.Add(new Customer($"Concurrent Customer {taskId}_{i}", $"phone{taskId}_{i}", $"email{taskId}_{i}@example.com", true));
                    }
                    return await _bulkOperationsService.BulkInsertAsync(customers);
                });
                tasks.Add(task);
            }

            var stopwatch = Stopwatch.StartNew();
            var results = await Task.WhenAll(tasks);
            stopwatch.Stop();

            // Assert - All operations should succeed
            foreach (var result in results)
            {
                Assert.IsTrue(result.Success);
                Assert.AreEqual(recordsPerTask, result.RecordsAffected);
            }

            // Total records should be correct
            var totalRecords = await _context.Customers.CountAsync();
            Assert.AreEqual(concurrentTasks * recordsPerTask, totalRecords);

            // Performance should be reasonable even under concurrency
            var totalRecordsProcessed = results.Sum(r => r.RecordsAffected);
            var recordsPerSecond = totalRecordsProcessed / (stopwatch.ElapsedMilliseconds / 1000.0);
            
            Console.WriteLine($"Concurrent operations: {totalRecordsProcessed} records in {stopwatch.ElapsedMilliseconds}ms ({recordsPerSecond:F0} records/sec)");
            Assert.IsTrue(recordsPerSecond > 500, $"Concurrent operations should maintain at least 500 records/sec. Actual: {recordsPerSecond:F0} records/sec");
        }

        [TestMethod]
        public async Task BulkOperations_Benchmark_DifferentEntityTypes()
        {
            // Arrange
            var testResults = new Dictionary<string, (int count, long duration, double recordsPerSecond)>();

            // Test different entity types
            var entityTests = new[]
            {
                ("Customers", GenerateCustomers(1000)),
                ("QueueEntries", await GenerateQueueEntries(500))
            };

            foreach (var (entityType, entities) in entityTests)
            {
                // Clear relevant tables
                if (entityType == "Customers")
                {
                    _context.Customers.RemoveRange(await _context.Customers.ToListAsync());
                    await _context.SaveChangesAsync();
                }
                else if (entityType == "QueueEntries")
                {
                    _context.QueueEntries.RemoveRange(await _context.QueueEntries.ToListAsync());
                    await _context.SaveChangesAsync();
                }

                // Measure bulk insert performance
                var stopwatch = Stopwatch.StartNew();
                BulkOperationResult result;
                
                if (entities is List<Customer> customers)
                {
                    result = await _bulkOperationsService.BulkInsertAsync(customers);
                }
                else if (entities is List<QueueEntry> queueEntries)
                {
                    result = await _bulkOperationsService.BulkInsertAsync(queueEntries);
                }
                else
                {
                    throw new InvalidOperationException($"Unsupported entity type: {entityType}");
                }
                
                stopwatch.Stop();

                var recordsPerSecond = result.RecordsAffected / (stopwatch.ElapsedMilliseconds / 1000.0);
                testResults[entityType] = (result.RecordsAffected, stopwatch.ElapsedMilliseconds, recordsPerSecond);

                Console.WriteLine($"{entityType}: {result.RecordsAffected} records in {stopwatch.ElapsedMilliseconds}ms ({recordsPerSecond:F0} records/sec)");
                
                Assert.IsTrue(result.Success);
            }

            // Assert - All entity types should perform reasonably well
            foreach (var (entityType, (count, duration, recordsPerSecond)) in testResults)
            {
                Assert.IsTrue(recordsPerSecond > 200, $"{entityType} bulk operations should achieve at least 200 records/sec. Actual: {recordsPerSecond:F0} records/sec");
                Assert.IsTrue(duration < 10000, $"{entityType} bulk operations should complete within 10 seconds. Actual: {duration}ms");
            }
        }

        private List<Customer> GenerateCustomers(int count)
        {
            var customers = new List<Customer>();
            for (int i = 0; i < count; i++)
            {
                customers.Add(new Customer($"Benchmark Customer {i}", $"phone{i}", $"email{i}@example.com", i % 2 == 0));
            }
            return customers;
        }

        private async Task<List<QueueEntry>> GenerateQueueEntries(int count)
        {
            // Create required dependencies
            var organization = new Organization("Benchmark Org", "benchmark@example.com", "1234567890");
            await _context.Organizations.AddAsync(organization);
            await _context.SaveChangesAsync();

            var location = new Location(
                organization.Id,
                "Benchmark Location",
                "123 Benchmark St",
                "Benchmark City",
                "Benchmark State",
                "12345",
                "US",
                "benchmark-location",
                "EST"
            );
            await _context.Locations.AddAsync(location);
            await _context.SaveChangesAsync();

            var queue = new Queue(location.Id, "Benchmark Queue", "Queue for benchmark testing");
            await _context.Queues.AddAsync(queue);
            await _context.SaveChangesAsync();

            // Create customers
            var customers = new List<Customer>();
            for (int i = 0; i < count; i++)
            {
                customers.Add(new Customer($"Queue Customer {i}", $"phone{i}", $"email{i}@example.com", true));
            }
            await _context.Customers.AddRangeAsync(customers);
            await _context.SaveChangesAsync();

            // Create queue entries
            var queueEntries = new List<QueueEntry>();
            for (int i = 0; i < count; i++)
            {
                var queueEntry = new QueueEntry(
                    queue.Id,
                    customers[i].Id,
                    $"Queue Customer {i}",
                    i + 1,
                    null,
                    null,
                    $"Benchmark queue entry {i}"
                );
                queueEntries.Add(queueEntry);
            }

            return queueEntries;
        }
    }
}
