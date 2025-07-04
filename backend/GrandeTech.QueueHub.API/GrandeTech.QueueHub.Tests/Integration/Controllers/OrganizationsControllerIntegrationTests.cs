using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Grande.Fila.API.Tests.Integration.Controllers
{
    [TestClass]
    public class OrganizationsControllerIntegrationTests
    {
        private static WebApplicationFactory<Program> _factory = null!;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _factory = new WebApplicationFactory<Program>();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            _factory?.Dispose();
        }

        // UC-TRACKQ: Admin/Owner track live activity
        [TestMethod]
        public async Task GetLiveActivity_ValidOrganizationId_ReturnsSuccess()
        {
            var client = _factory.CreateClient();
            var adminToken = await CreateAndAuthenticateUserAsync("Admin", client);

            // Create organization first
            var createOrgRequest = new
            {
                Name = "Test Organization",
                ContactEmail = "test@testorg.com",
                ContactPhone = "+5511999999999",
                WebsiteUrl = "https://testorg.com"
            };

            var createResponse = await client.PostAsJsonAsync("/api/organizations", createOrgRequest);
            var createResult = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
            var organizationId = createResult.GetProperty("organizationId").GetString();

            // Act: Get live activity
            client.DefaultRequestHeaders.Authorization = new("Bearer", adminToken);
            var response = await client.GetAsync($"/api/organizations/{organizationId}/live-activity");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.IsTrue(result.GetProperty("success").GetBoolean());
            Assert.IsTrue(result.TryGetProperty("liveActivity", out var liveActivity));
            Assert.AreEqual(organizationId, liveActivity.GetProperty("organizationId").GetString());
        }

        [TestMethod]
        public async Task GetLiveActivity_NonAdminUser_ReturnsForbidden()
        {
            var client = _factory.CreateClient();
            var clientToken = await CreateAndAuthenticateUserAsync("Client", client);

            var organizationId = Guid.NewGuid().ToString();

            // Act
            client.DefaultRequestHeaders.Authorization = new("Bearer", clientToken);
            var response = await client.GetAsync($"/api/organizations/{organizationId}/live-activity");

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [TestMethod]
        public async Task GetLiveActivity_InvalidOrganizationId_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            var adminToken = await CreateAndAuthenticateUserAsync("Admin", client);

            // Act
            client.DefaultRequestHeaders.Authorization = new("Bearer", adminToken);
            var response = await client.GetAsync("/api/organizations/invalid-guid/live-activity");

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // UC-BRANDING: Admin/Owner customize branding
        [TestMethod]
        public async Task UpdateBranding_ValidRequest_ReturnsSuccess()
        {
            var client = _factory.CreateClient();
            var adminToken = await CreateAndAuthenticateUserAsync("Admin", client);

            // Create organization first
            var createOrgRequest = new
            {
                Name = "Test Organization",
                ContactEmail = "test@testorg.com",
                ContactPhone = "+5511999999999",
                WebsiteUrl = "https://testorg.com"
            };

            var createResponse = await client.PostAsJsonAsync("/api/organizations", createOrgRequest);
            var createResult = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
            var organizationId = createResult.GetProperty("organizationId").GetString();

            // Act: Update branding
            var brandingRequest = new
            {
                PrimaryColor = "#FF0000",
                SecondaryColor = "#00FF00",
                LogoUrl = "https://example.com/logo.png",
                FaviconUrl = "https://example.com/favicon.ico",
                TagLine = "Test tagline"
            };

            client.DefaultRequestHeaders.Authorization = new("Bearer", adminToken);
            var response = await client.PutAsJsonAsync($"/api/organizations/{organizationId}/branding", brandingRequest);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.IsTrue(result.GetProperty("success").GetBoolean());
        }

        [TestMethod]
        public async Task UpdateBranding_NonAdminUser_ReturnsForbidden()
        {
            var client = _factory.CreateClient();
            var clientToken = await CreateAndAuthenticateUserAsync("Client", client);

            var organizationId = Guid.NewGuid().ToString();
            var brandingRequest = new
            {
                PrimaryColor = "#FF0000",
                SecondaryColor = "#00FF00"
            };

            // Act
            client.DefaultRequestHeaders.Authorization = new("Bearer", clientToken);
            var response = await client.PutAsJsonAsync($"/api/organizations/{organizationId}/branding", brandingRequest);

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        private async Task<string> CreateAndAuthenticateUserAsync(string role, HttpClient client)
        {
            var registerRequest = new
            {
                Username = $"testuser_{Guid.NewGuid():N}",
                Email = $"test_{Guid.NewGuid():N}@example.com",
                Password = "TestPassword123!",
                Role = role
            };

            var registerResponse = await client.PostAsJsonAsync("/api/auth/register", registerRequest);
            var registerResult = await registerResponse.Content.ReadFromJsonAsync<JsonElement>();

            if (registerResult.TryGetProperty("token", out var tokenElement))
            {
                return tokenElement.GetString() ?? throw new InvalidOperationException("Token was null");
            }

            // If registration failed, try login instead
            var loginRequest = new
            {
                Username = registerRequest.Username,
                Password = registerRequest.Password
            };

            var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
            var loginResult = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
            return loginResult.GetProperty("token").GetString() ?? throw new InvalidOperationException("Token was null");
        }
    }
}