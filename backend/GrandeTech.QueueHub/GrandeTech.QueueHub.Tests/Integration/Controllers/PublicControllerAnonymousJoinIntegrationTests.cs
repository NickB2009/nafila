using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Grande.Fila.API.Application.Public;

namespace Grande.Fila.API.Tests.Integration.Controllers
{
    [TestClass]
    public class PublicControllerAnonymousJoinIntegrationTests
    {
        private HttpClient _client;
        private IntegrationTestHelper _testHelper;

        [TestInitialize]
        public async Task TestInitialize()
        {
            _testHelper = new IntegrationTestHelper();
            _client = _testHelper.CreateClient();
            
            // Initialize test data
            await _testHelper.InitializeTestDataAsync();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _client?.Dispose();
            _testHelper?.Dispose();
        }

        [TestMethod]
        public async Task AnonymousJoin_WithValidRequest_ShouldReturnSuccess()
        {
            // Arrange
            var request = new AnonymousJoinRequest
            {
                SalonId = "99999999-9999-9999-9999-999999999993", // Test salon ID from seeded data
                Name = "Test User",
                Email = "test@example.com",
                AnonymousUserId = Guid.NewGuid().ToString(),
                ServiceRequested = "Haircut",
                EmailNotifications = true,
                BrowserNotifications = true
            };

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/Public/queue/join", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AnonymousJoinResult>(responseContent, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Id);
            Assert.IsTrue(result.Position > 0);
            Assert.IsNotNull(result.JoinedAt);
            Assert.AreEqual("waiting", result.Status);
        }

        [TestMethod]
        public async Task AnonymousJoin_WithInvalidSalonId_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new AnonymousJoinRequest
            {
                SalonId = "invalid-guid",
                Name = "Test User",
                Email = "test@example.com",
                AnonymousUserId = Guid.NewGuid().ToString(),
                ServiceRequested = "Haircut"
            };

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/Public/queue/join", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task AnonymousJoin_WithEmptyName_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new AnonymousJoinRequest
            {
                SalonId = "99999999-9999-9999-9999-999999999993",
                Name = "",
                Email = "test@example.com",
                AnonymousUserId = Guid.NewGuid().ToString(),
                ServiceRequested = "Haircut"
            };

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/Public/queue/join", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task AnonymousJoin_WithInvalidEmail_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new AnonymousJoinRequest
            {
                SalonId = "99999999-9999-9999-9999-999999999993",
                Name = "Test User",
                Email = "invalid-email",
                AnonymousUserId = Guid.NewGuid().ToString(),
                ServiceRequested = "Haircut"
            };

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/Public/queue/join", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task AnonymousJoin_WithNonExistentSalon_ShouldReturnNotFound()
        {
            // Arrange
            var request = new AnonymousJoinRequest
            {
                SalonId = Guid.NewGuid().ToString(),
                Name = "Test User",
                Email = "test@example.com",
                AnonymousUserId = Guid.NewGuid().ToString(),
                ServiceRequested = "Haircut"
            };

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/Public/queue/join", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task AnonymousJoin_AllowsCorsOrigins()
        {
            // Arrange - test CORS preflight request for the anonymous join endpoint
            var request = new HttpRequestMessage(HttpMethod.Options, "/api/Public/queue/join");
            request.Headers.Add("Origin", "http://localhost:3000");
            request.Headers.Add("Access-Control-Request-Method", "POST");
            request.Headers.Add("Access-Control-Request-Headers", "Content-Type");

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent);

            // Check CORS headers are present
            Assert.IsTrue(response.Headers.Contains("Access-Control-Allow-Origin") ||
                         response.Headers.Contains("access-control-allow-origin"));
        }

        [TestMethod]
        public async Task AnonymousJoin_DoesNotRequireAuthentication()
        {
            // Arrange - No authentication headers
            var request = new AnonymousJoinRequest
            {
                SalonId = "99999999-9999-9999-9999-999999999993",
                Name = "Anonymous User",
                Email = "anon@example.com",
                AnonymousUserId = Guid.NewGuid().ToString(),
                ServiceRequested = "Haircut"
            };

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/Public/queue/join", content);

            // Assert
            Assert.AreNotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.AreNotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}