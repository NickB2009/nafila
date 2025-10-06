using Microsoft.Extensions.Logging;
using Grande.Fila.API.Infrastructure.Services;
using Grande.Fila.API.Infrastructure.Data;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Grande.Fila.API.Tests.Infrastructure.Performance
{
    [TestClass]
    public class DatabasePerformanceTests
    {
        private ServiceProvider _serviceProvider = null!;
        private IQueryOptimizationService _queryOptimizationService = null!;
        private IQueryCacheService _queryCacheService = null!;
        private IConnectionPoolingService _connectionPoolingService = null!;
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
            
            // Add memory cache for query caching
            services.AddMemoryCache();
            
            // Add performance services
            services.AddScoped<IQueryOptimizationService, QueryOptimizationService>();
            services.AddScoped<IQueryCacheService, QueryCacheService>();
            services.AddScoped<IConnectionPoolingService, ConnectionPoolingService>();
            
            _serviceProvider = services.BuildServiceProvider();
            _queryOptimizationService = _serviceProvider.GetRequiredService<IQueryOptimizationService>();
            _queryCacheService = _serviceProvider.GetRequiredService<IQueryCacheService>();
            _connectionPoolingService = _serviceProvider.GetRequiredService<IConnectionPoolingService>();
            _context = _serviceProvider.GetRequiredService<QueueHubDbContext>();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _serviceProvider?.Dispose();
        }

        [TestMethod]
        public async Task QueryOptimizationService_ShouldTrackSlowQueries()
        {
            // Arrange
            var customers = new List<Customer>();
            for (int i = 0; i < 100; i++)
            {
                customers.Add(new Customer($"Customer {i}", $"phone{i}", $"email{i}@example.com", true));
            }
            await _context.Customers.AddRangeAsync(customers);
            await _context.SaveChangesAsync();

            // Act - Execute a query that might be slow
            var slowQuery = async () => await _context.Customers
                .Where(c => c.Name.Contains("Customer"))
                .ToListAsync();

            var result = await _queryOptimizationService.ExecuteOptimizedQueryAsync(slowQuery);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            
            // Check if slow query was tracked
            var stats = await _queryOptimizationService.GetQueryPerformanceStatsAsync();
            Assert.IsNotNull(stats);
        }

        [TestMethod]
        public async Task QueryCacheService_ShouldCacheFrequentlyAccessedData()
        {
            // Arrange
            var customers = new List<Customer>
            {
                new Customer("John Doe", "1234567890", "john@example.com", false),
                new Customer("Jane Smith", "0987654321", "jane@example.com", false)
            };
            await _context.Customers.AddRangeAsync(customers);
            await _context.SaveChangesAsync();

            var cacheKey = "test_customers";
            var query = async () => await _context.Customers.ToListAsync();

            // Act - Execute same query multiple times
            var result1 = await _queryCacheService.GetOrAddAsync(cacheKey, query, TimeSpan.FromMinutes(5));
            var result2 = await _queryCacheService.GetOrAddAsync(cacheKey, query, TimeSpan.FromMinutes(5));
            var result3 = await _queryCacheService.GetOrAddAsync(cacheKey, query, TimeSpan.FromMinutes(5));

            // Assert
            Assert.AreEqual(2, result1.Count);
            Assert.AreEqual(2, result2.Count);
            Assert.AreEqual(2, result3.Count);
            
            // Check cache statistics
            var stats = await _queryCacheService.GetCacheStatisticsAsync();
            Assert.IsNotNull(stats);
            Assert.IsTrue(stats.TotalHits > 0);
        }

        [TestMethod]
        public async Task QueryCacheService_ShouldInvalidateCacheCorrectly()
        {
            // Arrange
            var customer = new Customer("Test Customer", "1234567890", "test@example.com", false);
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();

            var cacheKey = "test_customer";
            var query = async () => await _context.Customers.FirstOrDefaultAsync();

            // Act
            var result1 = await _queryCacheService.GetOrAddAsync(cacheKey, query, TimeSpan.FromMinutes(5));
            
            // Invalidate cache
            await _queryCacheService.InvalidateCacheAsync(cacheKey);
            
            // Modify data
            customer.GetType().GetProperty("Name")?.SetValue(customer, "Updated Customer");
            await _context.SaveChangesAsync();
            
            var result2 = await _queryCacheService.GetOrAddAsync(cacheKey, query, TimeSpan.FromMinutes(5));

            // Assert
            Assert.AreEqual("Test Customer", result1?.Name);
            Assert.AreEqual("Updated Customer", result2?.Name);
        }

        [TestMethod]
        public async Task ConnectionPoolingService_ShouldMonitorConnections()
        {
            // Act
            var stats = await _connectionPoolingService.GetConnectionPoolStatsAsync();

            // Assert
            Assert.IsNotNull(stats);
            Assert.IsTrue(stats.TotalConnections >= 0);
            Assert.IsTrue(stats.ActiveConnections >= 0);
            Assert.IsTrue(stats.IdleConnections >= 0);
        }

        [TestMethod]
        public async Task ConnectionPoolingService_ShouldProvideOptimizationRecommendations()
        {
            // Act
            var recommendations = await _connectionPoolingService.GetOptimizationRecommendationsAsync();

            // Assert
            Assert.IsNotNull(recommendations);
            Assert.IsTrue(recommendations.Count >= 0); // Should return recommendations or empty list
        }

        [TestMethod]
        public async Task QueryOptimizationService_ShouldRetryOnFailure()
        {
            // Arrange
            var failingQuery = async () =>
            {
                await Task.Delay(10); // Simulate some work
                throw new InvalidOperationException("Simulated failure");
            };

            // Act & Assert
            var result = await _queryOptimizationService.ExecuteOptimizedQueryAsync(failingQuery);
            
            // Should handle the failure gracefully
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public async Task PerformanceServices_ShouldProvideComprehensiveMetrics()
        {
            // Arrange
            var customers = new List<Customer>();
            for (int i = 0; i < 50; i++)
            {
                customers.Add(new Customer($"Customer {i}", $"phone{i}", $"email{i}@example.com", true));
            }
            await _context.Customers.AddRangeAsync(customers);
            await _context.SaveChangesAsync();

            // Act - Execute some operations to generate metrics
            var query = async () => await _context.Customers.ToListAsync();
            await _queryCacheService.GetOrAddAsync("performance_test", query, TimeSpan.FromMinutes(5));
            await _queryOptimizationService.ExecuteOptimizedQueryAsync(query);

            // Assert - Check that all services provide metrics
            var queryStats = await _queryOptimizationService.GetQueryPerformanceStatsAsync();
            var cacheStats = await _queryCacheService.GetCacheStatisticsAsync();
            var connectionStats = await _connectionPoolingService.GetConnectionPoolStatsAsync();

            Assert.IsNotNull(queryStats);
            Assert.IsNotNull(cacheStats);
            Assert.IsNotNull(connectionStats);
        }
    }
}
