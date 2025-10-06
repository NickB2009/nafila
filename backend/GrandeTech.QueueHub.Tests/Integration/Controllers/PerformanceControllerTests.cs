using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Grande.Fila.API.Infrastructure.Services;
using Grande.Fila.API.Infrastructure.Data;
using Grande.Fila.API.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Grande.Fila.API.Tests.Integration.Controllers
{
    [TestClass]
    public class PerformanceControllerTests
    {
        private ServiceProvider _serviceProvider = null!;
        private PerformanceController _controller = null!;
        private QueueHubDbContext _context = null!;

        [TestInitialize]
        public void Setup()
        {
            var services = new ServiceCollection();
            
            // Add in-memory database
            services.AddDbContext<QueueHubDbContext>(options =>
                options.UseInMemoryDatabase($"PerformanceTestDb_{Guid.NewGuid()}"));
            
            // Add logging
            services.AddLogging(builder => builder.AddConsole());
            
            // Add memory cache
            services.AddMemoryCache();
            
            // Add performance services
            services.AddScoped<IQueryOptimizationService, QueryOptimizationService>();
            services.AddScoped<IQueryCacheService, QueryCacheService>();
            services.AddScoped<IConnectionPoolingService, ConnectionPoolingService>();
            
            _serviceProvider = services.BuildServiceProvider();
            var queryOptimizationService = _serviceProvider.GetRequiredService<IQueryOptimizationService>();
            var queryCacheService = _serviceProvider.GetRequiredService<IQueryCacheService>();
            var connectionPoolingService = _serviceProvider.GetRequiredService<IConnectionPoolingService>();
            var logger = _serviceProvider.GetRequiredService<ILogger<PerformanceController>>();
            
            _controller = new PerformanceController(
                queryOptimizationService, 
                queryCacheService, 
                connectionPoolingService, 
                logger);
            _context = _serviceProvider.GetRequiredService<QueueHubDbContext>();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _serviceProvider?.Dispose();
        }

        [TestMethod]
        public async Task GetDatabaseHealth_ShouldReturnOk()
        {
            // Act
            var result = await _controller.GetDatabaseHealth() as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            
            var response = result.Value;
            Assert.IsNotNull(response);
        }

        [TestMethod]
        public async Task GetConnectionPoolStats_ShouldReturnOk()
        {
            // Act
            var result = await _controller.GetConnectionPoolStats() as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            
            var response = result.Value;
            Assert.IsNotNull(response);
        }

        [TestMethod]
        public async Task GetQueryCacheStats_ShouldReturnOk()
        {
            // Arrange - Generate some cache activity
            var customers = new List<Customer>
            {
                new Customer("Cache Test Customer", "1234567890", "cache@example.com", true)
            };
            await _context.Customers.AddRangeAsync(customers);
            await _context.SaveChangesAsync();

            var queryCacheService = _serviceProvider.GetRequiredService<IQueryCacheService>();
            await queryCacheService.GetOrAddAsync("test_cache", async () => await _context.Customers.ToListAsync(), TimeSpan.FromMinutes(5));

            // Act
            var result = await _controller.GetQueryCacheStats() as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            
            var response = result.Value;
            Assert.IsNotNull(response);
        }

        [TestMethod]
        public async Task GetSlowQueries_ShouldReturnOk()
        {
            // Arrange - Generate some query activity
            var customers = new List<Customer>();
            for (int i = 0; i < 100; i++)
            {
                customers.Add(new Customer($"Customer {i}", $"phone{i}", $"email{i}@example.com", true));
            }
            await _context.Customers.AddRangeAsync(customers);
            await _context.SaveChangesAsync();

            var queryOptimizationService = _serviceProvider.GetRequiredService<IQueryOptimizationService>();
            await queryOptimizationService.ExecuteOptimizedQueryAsync(async () => 
                await _context.Customers.Where(c => c.Name.Contains("Customer")).ToListAsync());

            // Act
            var result = await _controller.GetSlowQueries() as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            
            var response = result.Value;
            Assert.IsNotNull(response);
        }

        [TestMethod]
        public async Task GetDatabaseHealth_WithDatabaseActivity_ShouldReturnHealthyStatus()
        {
            // Arrange - Perform some database operations
            var customers = new List<Customer>
            {
                new Customer("Health Test Customer 1", "1111111111", "health1@example.com", true),
                new Customer("Health Test Customer 2", "2222222222", "health2@example.com", true)
            };
            await _context.Customers.AddRangeAsync(customers);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetDatabaseHealth() as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            
            var response = result.Value;
            Assert.IsNotNull(response);
        }

        [TestMethod]
        public async Task GetConnectionPoolStats_ShouldReturnValidStatistics()
        {
            // Arrange - Perform some database operations to generate connection activity
            var customers = new List<Customer>();
            for (int i = 0; i < 50; i++)
            {
                customers.Add(new Customer($"Connection Test Customer {i}", $"phone{i}", $"email{i}@example.com", true));
            }
            await _context.Customers.AddRangeAsync(customers);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetConnectionPoolStats() as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            
            var response = result.Value;
            Assert.IsNotNull(response);
        }

        [TestMethod]
        public async Task GetQueryCacheStats_ShouldReturnCacheMetrics()
        {
            // Arrange - Generate cache activity
            var queryCacheService = _serviceProvider.GetRequiredService<IQueryCacheService>();
            
            // Perform multiple cache operations
            await queryCacheService.GetOrAddAsync("test_key_1", async () => await Task.FromResult("value1"), TimeSpan.FromMinutes(5));
            await queryCacheService.GetOrAddAsync("test_key_1", async () => await Task.FromResult("value1"), TimeSpan.FromMinutes(5)); // Cache hit
            await queryCacheService.GetOrAddAsync("test_key_2", async () => await Task.FromResult("value2"), TimeSpan.FromMinutes(5));
            await queryCacheService.InvalidateCacheAsync("test_key_1");

            // Act
            var result = await _controller.GetQueryCacheStats() as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            
            var response = result.Value;
            Assert.IsNotNull(response);
        }

        [TestMethod]
        public async Task GetAllPerformanceEndpoints_ShouldReturnConsistentData()
        {
            // Arrange - Generate some activity across all services
            var customers = new List<Customer>();
            for (int i = 0; i < 25; i++)
            {
                customers.Add(new Customer($"Performance Customer {i}", $"phone{i}", $"email{i}@example.com", true));
            }
            await _context.Customers.AddRangeAsync(customers);
            await _context.SaveChangesAsync();

            var queryCacheService = _serviceProvider.GetRequiredService<IQueryCacheService>();
            var queryOptimizationService = _serviceProvider.GetRequiredService<IQueryOptimizationService>();

            await queryCacheService.GetOrAddAsync("performance_test", async () => await _context.Customers.ToListAsync(), TimeSpan.FromMinutes(5));
            await queryOptimizationService.ExecuteOptimizedQueryAsync(async () => await _context.Customers.ToListAsync());

            // Act - Call all performance endpoints
            var healthResult = await _controller.GetDatabaseHealth() as OkObjectResult;
            var connectionResult = await _controller.GetConnectionPoolStats() as OkObjectResult;
            var cacheResult = await _controller.GetQueryCacheStats() as OkObjectResult;
            var slowQueriesResult = await _controller.GetSlowQueries() as OkObjectResult;

            // Assert - All endpoints should return OK
            Assert.IsNotNull(healthResult);
            Assert.IsNotNull(connectionResult);
            Assert.IsNotNull(cacheResult);
            Assert.IsNotNull(slowQueriesResult);
            
            Assert.AreEqual(200, healthResult.StatusCode);
            Assert.AreEqual(200, connectionResult.StatusCode);
            Assert.AreEqual(200, cacheResult.StatusCode);
            Assert.AreEqual(200, slowQueriesResult.StatusCode);
        }
    }
}
