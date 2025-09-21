using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
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
    [TestCategory("Environment")]
    public class PublicApiEnvironmentTests
    {
        private static HttpClient _client = null!;
        private static string _baseUrl = null!;
        private static string _testEnvironment = null!;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            // Read test environment from environment variable or test context
            _testEnvironment = Environment.GetEnvironmentVariable("TEST_ENVIRONMENT") ?? "Local";
            
            if (_testEnvironment.Equals("Local", StringComparison.OrdinalIgnoreCase))
            {
                SetupLocalEnvironment();
            }
            else if (_testEnvironment.Equals("Prod", StringComparison.OrdinalIgnoreCase) || 
                     _testEnvironment.Equals("BoaHost", StringComparison.OrdinalIgnoreCase))
            {
                SetupProductionEnvironment();
            }
            else
            {
                throw new InvalidOperationException($"Unknown test environment: {_testEnvironment}. Use 'Local' or 'Prod'.");
            }
        }

        private static void SetupLocalEnvironment()
        {
            _baseUrl = "http://localhost:5098";
            
            // Create HttpClient for local testing
            _client = new HttpClient()
            {
                BaseAddress = new Uri(_baseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
            
            Console.WriteLine($"[TEST] Environment: Local - Testing against {_baseUrl}");
        }

        private static void SetupProductionEnvironment()
        {
            // Get production URL from environment variable or use default BoaHost URL
            _baseUrl = Environment.GetEnvironmentVariable("PROD_API_URL") ?? "https://api.eutonafila.com.br";
            
            // Create HttpClient for production testing
            _client = new HttpClient()
            {
                BaseAddress = new Uri(_baseUrl),
                Timeout = TimeSpan.FromSeconds(60) // Longer timeout for production
            };
            
            // Add any production-specific headers if needed
            _client.DefaultRequestHeaders.Add("User-Agent", "QueueHub-IntegrationTests/1.0");
            
            Console.WriteLine($"[TEST] Environment: Production - Testing against {_baseUrl}");
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            _client?.Dispose();
        }

        [TestMethod]
        [TestCategory("Smoke")]
        public async Task HealthCheck_ShouldReturnHealthy()
        {
            // Act
            var response = await _client.GetAsync("/api/Health");
            
            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, 
                $"Health check failed in {_testEnvironment} environment");
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(content.Contains("\"status\":\"Healthy\""), 
                "Health check should return healthy status");
            
            Console.WriteLine($"[TEST] Health check passed in {_testEnvironment} environment");
        }

        [TestMethod]
        [TestCategory("PublicAPI")]
        public async Task GetPublicSalons_ShouldReturnValidResponse()
        {
            // Act
            var response = await _client.GetAsync("/api/Public/salons");
            
            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode,
                $"Get salons failed in {_testEnvironment} environment");
            
            var content = await response.Content.ReadAsStringAsync();
            
            // Validate JSON structure
            var salons = JsonSerializer.Deserialize<JsonElement>(content);
            Assert.IsTrue(salons.ValueKind == JsonValueKind.Array, 
                "Salons response should be an array");
            
            Console.WriteLine($"[TEST] Get salons passed in {_testEnvironment} environment - {salons.GetArrayLength()} salons returned");
        }

        [TestMethod]
        [TestCategory("PublicAPI")]
        public async Task GetQueueEntryStatus_WithInvalidId_ShouldReturnBadRequest()
        {
            // Act
            var response = await _client.GetAsync("/api/Public/queue/entry-status/invalid-id");
            
            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode,
                $"Invalid entry ID should return BadRequest in {_testEnvironment} environment");
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(content.Contains("Invalid entry ID format") || content.Contains("errors"),
                "Response should indicate invalid ID format");
            
            Console.WriteLine($"[TEST] Invalid entry ID validation passed in {_testEnvironment} environment");
        }

        [TestMethod]
        [TestCategory("PublicAPI")]
        public async Task GetQueueEntryStatus_WithValidNonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            var validGuid = Guid.NewGuid().ToString();
            
            // Act
            var response = await _client.GetAsync($"/api/Public/queue/entry-status/{validGuid}");
            
            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode,
                $"Non-existent entry should return NotFound in {_testEnvironment} environment");
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(content.Contains("Queue entry not found") || content.Contains("message"),
                "Response should indicate entry not found");
            
            Console.WriteLine($"[TEST] Non-existent entry validation passed in {_testEnvironment} environment");
        }

        [TestMethod]
        [TestCategory("PublicAPI")]
        public async Task LeaveQueue_WithValidNonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            var validGuid = Guid.NewGuid().ToString();
            
            // Act
            var response = await _client.PostAsync($"/api/Public/queue/leave/{validGuid}", null);
            
            // Assert - Should return 404 or a structured error response
            var content = await response.Content.ReadAsStringAsync();
            
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                Assert.IsTrue(content.Contains("Queue entry not found"),
                    "Response should indicate entry not found");
            }
            else if (response.StatusCode == HttpStatusCode.OK)
            {
                // Some APIs return 200 with error details in body
                var result = JsonSerializer.Deserialize<JsonElement>(content);
                Assert.IsTrue(result.TryGetProperty("success", out var success) && !success.GetBoolean(),
                    "Response should indicate operation was not successful");
            }
            else
            {
                Assert.Fail($"Unexpected status code: {response.StatusCode}");
            }
            
            Console.WriteLine($"[TEST] Leave queue validation passed in {_testEnvironment} environment");
        }

        [TestMethod]
        [TestCategory("PublicAPI")]
        public async Task UpdateQueueEntry_WithValidJsonAndNonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            var validGuid = Guid.NewGuid().ToString();
            var updateRequest = new
            {
                name = "Test User",
                email = "test@example.com",
                emailNotifications = true,
                browserNotifications = false
            };
            
            var json = JsonSerializer.Serialize(updateRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            // Act
            var response = await _client.PutAsync($"/api/Public/queue/update/{validGuid}", content);
            
            // Assert - Should return 404 or a structured error response
            var responseContent = await response.Content.ReadAsStringAsync();
            
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                Assert.IsTrue(responseContent.Contains("Queue entry not found"),
                    "Response should indicate entry not found");
            }
            else if (response.StatusCode == HttpStatusCode.OK)
            {
                // Some APIs return 200 with error details in body
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                Assert.IsTrue(result.TryGetProperty("success", out var success) && !success.GetBoolean(),
                    "Response should indicate operation was not successful");
            }
            else
            {
                Assert.Fail($"Unexpected status code: {response.StatusCode}");
            }
            
            Console.WriteLine($"[TEST] Update queue entry validation passed in {_testEnvironment} environment");
        }

        [TestMethod]
        [TestCategory("PublicAPI")]
        [TestCategory("Security")]
        public async Task PublicEndpoints_ShouldNotRequireAuthentication()
        {
            // Arrange - List of all public endpoints
            var endpoints = new[]
            {
                "/api/Public/salons",
                $"/api/Public/queue/entry-status/{Guid.NewGuid()}",
                // Note: POST/PUT endpoints tested separately with proper payloads
            };

            // Act & Assert
            foreach (var endpoint in endpoints)
            {
                var response = await _client.GetAsync(endpoint);
                
                // Should not return Unauthorized or Forbidden
                Assert.AreNotEqual(HttpStatusCode.Unauthorized, response.StatusCode,
                    $"Endpoint {endpoint} should not require authentication in {_testEnvironment}");
                Assert.AreNotEqual(HttpStatusCode.Forbidden, response.StatusCode,
                    $"Endpoint {endpoint} should not require authentication in {_testEnvironment}");
                
                Console.WriteLine($"[TEST] Endpoint {endpoint} is properly public in {_testEnvironment} environment");
            }
        }

        [TestMethod]
        [TestCategory("Performance")]
        public async Task PublicEndpoints_ShouldRespondWithinReasonableTime()
        {
            // Arrange
            var timeout = _testEnvironment.Equals("Local", StringComparison.OrdinalIgnoreCase) ? 5000 : 10000; // ms
            var endpoints = new[]
            {
                "/api/Health",
                "/api/Public/salons"
            };

            // Act & Assert
            foreach (var endpoint in endpoints)
            {
                var startTime = DateTime.UtcNow;
                var response = await _client.GetAsync(endpoint);
                var duration = DateTime.UtcNow - startTime;
                
                Assert.IsTrue(duration.TotalMilliseconds < timeout,
                    $"Endpoint {endpoint} took {duration.TotalMilliseconds}ms, expected < {timeout}ms in {_testEnvironment}");
                
                Console.WriteLine($"[TEST] Endpoint {endpoint} responded in {duration.TotalMilliseconds}ms in {_testEnvironment} environment");
            }
        }
    }
}
