using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Grande.Fila.API.Domain.Users;
using Grande.Fila.API.Application.Auth;
using System.Threading;
using Grande.Fila.API.Tests.Integration;
using Grande.Fila.API.Infrastructure.Repositories.Bogus;
using Grande.Fila.API.Infrastructure;
using Grande.Fila.API.Domain.Organizations;
using Grande.Fila.API.Application.Organizations;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.Staff;
using Grande.Fila.API.Domain.Locations;
using Grande.Fila.API.Domain.AuditLogs;
using Grande.Fila.API.Domain.Subscriptions;
using System.Linq;

namespace Grande.Fila.API.Tests.Integration.Controllers
{
    [TestClass]
    [TestCategory("Integration")]
    public class OrganizationsControllerIntegrationTests
    {
        private static WebApplicationFactory<Program> _factory;
        private HttpClient _client;
        private static BogusUserRepository _userRepository;
        private static BogusOrganizationRepository _organizationRepository;
        private static BogusQueueRepository _queueRepository;
        private static BogusStaffMemberRepository _staffMemberRepository;
        private static BogusLocationRepository _locationRepository;
        private static BogusAuditLogRepository _auditLogRepository;
        private static BogusSubscriptionPlanRepository _subscriptionPlanRepository;

        [TestInitialize]
        public void TestInitialize()
        {
            if (_userRepository == null)
                _userRepository = new BogusUserRepository();
            if (_organizationRepository == null)
                _organizationRepository = new BogusOrganizationRepository();
            if (_queueRepository == null)
                _queueRepository = new BogusQueueRepository();
            if (_staffMemberRepository == null)
                _staffMemberRepository = new BogusStaffMemberRepository();
            if (_locationRepository == null)
                _locationRepository = new BogusLocationRepository();
            if (_auditLogRepository == null)
                _auditLogRepository = new BogusAuditLogRepository();
            if (_subscriptionPlanRepository == null)
                _subscriptionPlanRepository = new BogusSubscriptionPlanRepository();
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        // Remove existing repository registrations
                        var descriptors = services.Where(d => 
                            d.ServiceType == typeof(IUserRepository) ||
                            d.ServiceType == typeof(IOrganizationRepository) ||
                            d.ServiceType == typeof(IQueueRepository) ||
                            d.ServiceType == typeof(IStaffMemberRepository) ||
                            d.ServiceType == typeof(ILocationRepository) ||
                            d.ServiceType == typeof(IAuditLogRepository) ||
                            d.ServiceType == typeof(ISubscriptionPlanRepository)).ToList();
                        foreach (var descriptor in descriptors)
                        {
                            services.Remove(descriptor);
                        }
                        services.AddSingleton<IUserRepository>(_userRepository);
                        services.AddSingleton<IOrganizationRepository>(_organizationRepository);
                        services.AddSingleton<IQueueRepository>(_queueRepository);
                        services.AddSingleton<IStaffMemberRepository>(_staffMemberRepository);
                        services.AddSingleton<ILocationRepository>(_locationRepository);
                        services.AddSingleton<IAuditLogRepository>(_auditLogRepository);
                        services.AddSingleton<ISubscriptionPlanRepository>(_subscriptionPlanRepository);
                        services.AddScoped<AuthService>();
                        services.AddScoped<TrackLiveActivityService>();
                        services.AddScoped<OrganizationService>();
                        services.AddScoped<CreateOrganizationService>();
                    });
                });
            _client = _factory.CreateClient();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            _factory.Dispose();
        }

        // UC-TRACKQ: Admin/Owner track live activity
        [TestMethod]
        public async Task GetLiveActivity_ValidOrganizationId_ReturnsSuccess()
        {
            Console.WriteLine("Debug: Starting GetLiveActivity test");
            var adminToken = await CreateAndAuthenticateUserAsync("Admin");
            Console.WriteLine($"Debug: Got admin token: {adminToken?.Substring(0, 20)}...");
            _client.DefaultRequestHeaders.Authorization = new("Bearer", adminToken);

            // Create organization first
            var createOrgRequest = new
            {
                Name = "Test Organization",
                ContactEmail = "test@testorg.com",
                ContactPhone = "+5511999999999",
                WebsiteUrl = "https://testorg.com",
                SubscriptionPlanId = Guid.NewGuid().ToString()
            };

            var createResponse = await _client.PostAsJsonAsync("/api/organizations", createOrgRequest);
            Console.WriteLine($"Debug: Create org response status: {createResponse.StatusCode}");
            var createResult = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
            var organizationId = createResult.GetProperty("organizationId").GetString();
            Console.WriteLine($"Debug: Created organization ID: {organizationId}");

            // Act: Get live activity
            var response = await _client.GetAsync($"/api/organizations/{organizationId}/live-activity");

            // Debug: Check response content
            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Assert.Fail($"Expected OK but got {response.StatusCode}. Response: {responseContent}");
            }

            // Debug: Log the actual response before any assertions
            Console.WriteLine($"GetLiveActivity Status: {response.StatusCode}");
            Console.WriteLine($"GetLiveActivity Response: {responseContent}");
            
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
            var clientToken = await CreateAndAuthenticateUserAsync("Client");

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
            var adminToken = await CreateAndAuthenticateUserAsync("Admin");

            // Act
            client.DefaultRequestHeaders.Authorization = new("Bearer", adminToken);
            var response = await client.GetAsync("/api/organizations/invalid-guid/live-activity");

            // Debug: Check response content
            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.BadRequest)
            {
                Assert.Fail($"Expected BadRequest but got {response.StatusCode}. Response: {responseContent}");
            }

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // UC-BRANDING: Admin/Owner customize branding
        [TestMethod]
        public async Task UpdateBranding_ValidRequest_ReturnsSuccess()
        {
            Console.WriteLine("Debug: Starting UpdateBranding test");
            var client = _factory.CreateClient();
            var adminToken = await CreateAndAuthenticateUserAsync("Admin");
            Console.WriteLine($"Debug: Got admin token: {adminToken?.Substring(0, 20)}...");

            // Create organization first
            var createOrgRequest = new
            {
                Name = "Test Organization",
                ContactEmail = "test@testorg.com",
                ContactPhone = "+5511999999999",
                WebsiteUrl = "https://testorg.com",
                SubscriptionPlanId = Guid.NewGuid().ToString()
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

            // Debug: Check response content
            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Assert.Fail($"Expected OK but got {response.StatusCode}. Response: {responseContent}");
            }

            // Debug: Log the actual response before any assertions
            Console.WriteLine($"UpdateBranding Status: {response.StatusCode}");
            Console.WriteLine($"UpdateBranding Response: {responseContent}");
            
            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.IsTrue(result.GetProperty("success").GetBoolean());
        }

        [TestMethod]
        public async Task UpdateBranding_NonAdminUser_ReturnsForbidden()
        {
            var client = _factory.CreateClient();
            var clientToken = await CreateAndAuthenticateUserAsync("Client");

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

        private async Task<string> CreateAndAuthenticateUserAsync(string role)
        {
            Assert.IsNotNull(_client);

            using var scope = _factory.Services.CreateScope();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var authService = scope.ServiceProvider.GetRequiredService<AuthService>();

            var uniqueId = Guid.NewGuid().ToString("N");
            // Map old test roles to new roles
            var mappedRole = role.ToLower() switch
            {
                "admin" => UserRoles.Owner, // Admin tests should use Owner role for RequireOwner endpoints
                "owner" => UserRoles.Owner,
                "barber" => UserRoles.Barber,
                "client" => UserRoles.Client,
                "user" => UserRoles.Client,
                "system" => UserRoles.ServiceAccount,
                _ => UserRoles.Client
            };
            
            var user = new User($"testuser_{uniqueId}", $"test_{uniqueId}@example.com", BCrypt.Net.BCrypt.HashPassword("TestPassword123!"), mappedRole);

            // Disable 2FA for test users to simplify integration tests
            user.DisableTwoFactor();

            if (role == "Admin" || role == "Owner")
            {
                user.AddPermission(Permission.CreateStaff);
                user.AddPermission(Permission.UpdateStaff);
            }

            await userRepository.AddAsync(user, CancellationToken.None);

            var loginRequest = new LoginRequest
            {
                Username = user.Username,
                Password = "TestPassword123!"
            };

            var loginResult = await authService.LoginAsync(loginRequest);

            Assert.IsTrue(loginResult.Success, $"Login failed for user {user.Username}");
            Assert.IsNotNull(loginResult.Token, $"Token was null for user {user.Username}");

            return loginResult.Token;
        }
    }
}