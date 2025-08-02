using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Grande.Fila.API.Application.Public;

namespace Grande.Fila.API.Tests.Integration.Controllers
{
    [TestClass]
    [TestCategory("Integration")]
    public class PublicControllerAnonymousJoinIntegrationTest
    {
        private WebApplicationFactory<Program> _factory;
        private HttpClient _client;

        [TestInitialize]
        public void Setup()
        {
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Testing");
                    builder.ConfigureAppConfiguration((context, config) =>
                    {
                        config.AddInMemoryCollection(new Dictionary<string, string?>
                        {
                            ["Jwt:Key"] = "your-super-secret-key-with-at-least-32-characters-for-testing",
                            ["Jwt:Issuer"] = "Grande.Fila.API.Test",
                            ["Jwt:Audience"] = "Grande.Fila.API.Test",
                            ["ConnectionStrings:DefaultConnection"] = "Data Source=:memory:",
                            ["UseInMemoryDatabase"] = "true"
                        });
                    });
                });
            
            _client = _factory.CreateClient();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _client?.Dispose();
            _factory?.Dispose();
        }

        [TestMethod]
        public async Task AnonymousJoin_WithRealDatabase_ShouldNotThrowConcurrencyError()
        {
            // This test specifically targets the concurrency error we're seeing
            
            // Arrange - Use the exact same salon ID from the error logs
            var salonId = "99999999-9999-9999-9999-999999999993";
            
            var request = new
            {
                salonId = salonId,
                name = "Integration Test Customer",
                email = "integration.test@example.com",
                serviceRequested = "Haircut",
                anonymousUserId = "11111111-1111-1111-1111-111111111111"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Public/queue/join", request);

            // Assert
            Console.WriteLine($"Response Status: {response.StatusCode}");
            
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response Content: {content}");

            // We expect either success or a business logic error, but NOT a concurrency exception
            if (!response.IsSuccessStatusCode)
            {
                // Parse the error response
                var errorResult = JsonSerializer.Deserialize<AnonymousJoinResult>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                Assert.IsNotNull(errorResult);
                
                // Check that it's not a concurrency error
                var hasEntityFrameworkError = errorResult.Errors?.Any(e => 
                    e.Contains("database operation was expected to affect 1 row") ||
                    e.Contains("optimistic concurrency") ||
                    e.Contains("may have been modified or deleted")) ?? false;
                
                Assert.IsFalse(hasEntityFrameworkError, 
                    $"Should not have Entity Framework concurrency errors. Errors: {string.Join(", ", errorResult.Errors ?? new List<string>())}");
                
                // Print the actual errors for debugging
                if (errorResult.Errors?.Any() == true)
                {
                    Console.WriteLine($"Business Logic Errors (these are OK): {string.Join(", ", errorResult.Errors)}");
                }
            }
            else
            {
                // Success case - parse and validate
                var result = JsonSerializer.Deserialize<AnonymousJoinResult>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                Assert.IsNotNull(result);
                Assert.IsTrue(result.Success);
                Console.WriteLine($"Successfully joined queue. Position: {result.Position}, Wait Time: {result.EstimatedWaitMinutes}");
            }
        }

        [TestMethod]
        public async Task AnonymousJoin_MultipleSimultaneousRequests_ShouldNotCauseDeadlocks()
        {
            // Test multiple simultaneous requests to the same salon to check for race conditions
            
            var salonId = "99999999-9999-9999-9999-999999999993";
            
            var tasks = new List<Task<HttpResponseMessage>>();
            
            // Create 5 simultaneous requests
            for (int i = 0; i < 5; i++)
            {
                var request = new
                {
                    salonId = salonId,
                    name = $"Concurrent Test Customer {i}",
                    email = $"concurrent.test{i}@example.com",
                    serviceRequested = "Haircut",
                    anonymousUserId = $"22222222-2222-2222-2222-22222222222{i}"
                };
                
                tasks.Add(_client.PostAsJsonAsync("/api/Public/queue/join", request));
            }

            // Wait for all requests to complete
            var responses = await Task.WhenAll(tasks);

            // Check that none have concurrency errors
            int successCount = 0;
            int businessErrorCount = 0;
            int concurrencyErrorCount = 0;

            foreach (var response in responses)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response {Array.IndexOf(responses, response)}: Status={response.StatusCode}, Content={content}");

                if (response.IsSuccessStatusCode)
                {
                    successCount++;
                }
                else
                {
                    var errorResult = JsonSerializer.Deserialize<AnonymousJoinResult>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    var hasEntityFrameworkError = errorResult?.Errors?.Any(e => 
                        e.Contains("database operation was expected to affect 1 row") ||
                        e.Contains("optimistic concurrency") ||
                        e.Contains("may have been modified or deleted")) ?? false;

                    if (hasEntityFrameworkError)
                    {
                        concurrencyErrorCount++;
                    }
                    else
                    {
                        businessErrorCount++;
                    }
                }
            }

            Console.WriteLine($"Results: Success={successCount}, Business Errors={businessErrorCount}, Concurrency Errors={concurrencyErrorCount}");
            
            // Should have no concurrency errors
            Assert.AreEqual(0, concurrencyErrorCount, "Should not have any Entity Framework concurrency errors");
        }
    }
}