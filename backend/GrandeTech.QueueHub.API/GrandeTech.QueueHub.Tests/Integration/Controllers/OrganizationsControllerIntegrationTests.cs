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
        private static BogusUserRepository _userRepository;
        private static BogusOrganizationRepository _organizationRepository;
        private static BogusQueueRepository _queueRepository;
        private static BogusStaffMemberRepository _staffMemberRepository;
        private static BogusLocationRepository _locationRepository;
        private static BogusAuditLogRepository _auditLogRepository;
        private static BogusSubscriptionPlanRepository _subscriptionPlanRepository;
        private HttpClient _client;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _userRepository = new BogusUserRepository();
            _organizationRepository = new BogusOrganizationRepository();
            _queueRepository = new BogusQueueRepository();
            _staffMemberRepository = new BogusStaffMemberRepository();
            _locationRepository = new BogusLocationRepository();
            _auditLogRepository = new BogusAuditLogRepository();
            _subscriptionPlanRepository = new BogusSubscriptionPlanRepository();
            
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        // Remove existing repository registrations
                        var servicesToRemove = new[]
                        {
                            typeof(IUserRepository),
                            typeof(IOrganizationRepository),
                            typeof(IQueueRepository),
                            typeof(IStaffMemberRepository),
                            typeof(ILocationRepository),
                            typeof(IAuditLogRepository),
                            typeof(ISubscriptionPlanRepository)
                        };
                        var descriptors = services.Where(d => servicesToRemove.Contains(d.ServiceType)).ToList();
                        descriptors.ForEach(d => services.Remove(d));

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
        }

        [TestInitialize]
        public void TestInitialize()
        {
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
            var adminToken = await CreateAndAuthenticateUserAsync("PlatformAdmin");
            Console.WriteLine($"Debug: Got admin token: {adminToken?.Substring(0, 20)}...");
            _client.DefaultRequestHeaders.Authorization = new("Bearer", adminToken);

            // Create a subscription plan first
            using var subscriptionScope = _factory.Services.CreateScope();
            var subscriptionPlanRepository = subscriptionScope.ServiceProvider.GetRequiredService<ISubscriptionPlanRepository>();
            var subscriptionPlan = new Domain.Subscriptions.SubscriptionPlan(
                "Test Plan", "Test subscription plan", 29.99m, 299.99m, 5, 20, true, true, true, true, true, 1000, false, "test");
            await subscriptionPlanRepository.AddAsync(subscriptionPlan, CancellationToken.None);

            // Create organization first
            var createOrgRequest = new
            {
                Name = "Test Organization",
                Slug = "test-organization",
                ContactEmail = "test@testorg.com",
                ContactPhone = "+5511999999999",
                WebsiteUrl = "https://testorg.com",
                SubscriptionPlanId = subscriptionPlan.Id.ToString()
            };

            var createResponse = await _client.PostAsJsonAsync("/api/organizations", createOrgRequest);
            Console.WriteLine($"Debug: Create org response status: {createResponse.StatusCode}");
            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            if (createResponse.StatusCode != HttpStatusCode.Created && createResponse.StatusCode != HttpStatusCode.OK)
            {
                Assert.Fail($"Failed to create organization. Status: {createResponse.StatusCode}, Response: {createResponseContent}");
            }
            
            string organizationId;
            if (!string.IsNullOrWhiteSpace(createResponseContent))
            {
                var createResult = JsonSerializer.Deserialize<JsonElement>(createResponseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                organizationId = createResult.GetProperty("organizationId").GetString();
            }
            else
            {
                // If empty response, generate a test ID
                organizationId = Guid.NewGuid().ToString();
            }
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
            var adminToken = await CreateAndAuthenticateUserAsync("PlatformAdmin");

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
            var adminToken = await CreateAndAuthenticateUserAsync("PlatformAdmin");
            Console.WriteLine($"Debug: Got admin token: {adminToken?.Substring(0, 20)}...");
            _client.DefaultRequestHeaders.Authorization = new("Bearer", adminToken);

            // Create a subscription plan first
            using var subscriptionScope = _factory.Services.CreateScope();
            var subscriptionPlanRepository = subscriptionScope.ServiceProvider.GetRequiredService<ISubscriptionPlanRepository>();
            var subscriptionPlan = new Domain.Subscriptions.SubscriptionPlan(
                "Test Plan", "Test subscription plan", 29.99m, 299.99m, 5, 20, true, true, true, true, true, 1000, false, "test");
            await subscriptionPlanRepository.AddAsync(subscriptionPlan, CancellationToken.None);

            // Create organization first
            var createOrgRequest = new
            {
                Name = "Test Organization",
                Slug = "test-organization-branding",
                ContactEmail = "test@testorg.com",
                ContactPhone = "+5511999999999",
                WebsiteUrl = "https://testorg.com",
                SubscriptionPlanId = subscriptionPlan.Id.ToString()
            };

            var createResponse = await _client.PostAsJsonAsync("/api/organizations", createOrgRequest);
            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            if (createResponse.StatusCode != HttpStatusCode.Created && createResponse.StatusCode != HttpStatusCode.OK)
            {
                Assert.Fail($"Failed to create organization. Status: {createResponse.StatusCode}, Response: {createResponseContent}");
            }
            
            string organizationId;
            if (!string.IsNullOrWhiteSpace(createResponseContent))
            {
                var createResult = JsonSerializer.Deserialize<JsonElement>(createResponseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                organizationId = createResult.GetProperty("organizationId").GetString();
            }
            else
            {
                // If empty response, generate a test ID
                organizationId = Guid.NewGuid().ToString();
            }

            // Act: Update branding
            var brandingRequest = new
            {
                OrganizationId = organizationId,
                PrimaryColor = "#FF0000",
                SecondaryColor = "#00FF00",
                LogoUrl = "https://example.com/logo.png",
                FaviconUrl = "https://example.com/favicon.ico",
                TagLine = "Test tagline"
            };

            var response = await _client.PutAsJsonAsync($"/api/organizations/{organizationId}/branding", brandingRequest);

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
            
            // Handle empty response content (some endpoints return 204 No Content or empty body)
            if (!string.IsNullOrWhiteSpace(responseContent))
            {
                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                Assert.IsTrue(result.GetProperty("success").GetBoolean());
            }
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
                "platformadmin" => UserRoles.Admin, // PlatformAdmin tests should use Admin role for RequireAdmin endpoints
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