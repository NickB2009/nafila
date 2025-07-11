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
using Grande.Fila.API.Application.SubscriptionPlans;
using System.Linq;

namespace Grande.Fila.API.Tests.Integration.Controllers
{
    [TestClass]
    [TestCategory("Integration")]
    public class SubscriptionPlansControllerIntegrationTests
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
                        services.AddScoped<CreateSubscriptionPlanService>();
                        services.AddScoped<SubscriptionPlanService>();
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

        // UC-SUBPLAN: PlatformAdmin create subscription plan
        [TestMethod]
        public async Task CreateSubscriptionPlan_ValidRequest_ReturnsSuccess()
        {
            Console.WriteLine("Debug: Starting CreateSubscriptionPlan test");
            var adminToken = await CreateAndAuthenticateUserAsync("PlatformAdmin");
            Console.WriteLine($"Debug: Got admin token: {adminToken?.Substring(0, 20)}...");
            _client.DefaultRequestHeaders.Authorization = new("Bearer", adminToken);

            // Act: Create subscription plan
            var createRequest = new
            {
                Name = "Test Premium Plan",
                Description = "A premium test subscription plan",
                MonthlyPrice = 49.99m,
                YearlyPrice = 499.99m,
                MaxLocations = 5,
                MaxStaffPerLocation = 25,
                IncludesAnalytics = true,
                IncludesAdvancedReporting = true,
                IncludesCustomBranding = true,
                IncludesAdvertising = false,
                IncludesMultipleLocations = true,
                MaxQueueEntriesPerDay = 1000,
                IsFeatured = true,
                IsDefault = false
            };

            var response = await _client.PostAsJsonAsync("/api/subscriptionplans", createRequest);

            // Debug: Check response content
            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.Created && response.StatusCode != HttpStatusCode.OK)
            {
                Assert.Fail($"Expected Created/OK but got {response.StatusCode}. Response: {responseContent}");
            }

            // Debug: Log the actual response before any assertions
            Console.WriteLine($"CreateSubscriptionPlan Status: {response.StatusCode}");
            Console.WriteLine($"CreateSubscriptionPlan Response: {responseContent}");
            
            // Assert
            Assert.IsTrue(response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK);
            
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.IsTrue(result.GetProperty("success").GetBoolean());
            Assert.IsTrue(result.TryGetProperty("subscriptionPlanId", out var planId));
            Assert.AreEqual("Test Premium Plan", result.GetProperty("name").GetString());
        }

        [TestMethod]
        public async Task CreateSubscriptionPlan_NonAdminUser_ReturnsForbidden()
        {
            var client = _factory.CreateClient();
            var barberToken = await CreateAndAuthenticateUserAsync("Barber");

            var createRequest = new
            {
                Name = "Test Plan",
                MonthlyPrice = 29.99m,
                YearlyPrice = 299.99m,
                MaxLocations = 1,
                MaxStaffPerLocation = 5,
                MaxQueueEntriesPerDay = 100
            };

            // Act
            client.DefaultRequestHeaders.Authorization = new("Bearer", barberToken);
            var response = await client.PostAsJsonAsync("/api/subscriptionplans", createRequest);

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [TestMethod]
        public async Task UpdateSubscriptionPlan_ValidRequest_ReturnsSuccess()
        {
            Console.WriteLine("Debug: Starting UpdateSubscriptionPlan test");
            var adminToken = await CreateAndAuthenticateUserAsync("PlatformAdmin");
            Console.WriteLine($"Debug: Got admin token: {adminToken?.Substring(0, 20)}...");
            _client.DefaultRequestHeaders.Authorization = new("Bearer", adminToken);

            // Create a subscription plan first
            using var scope = _factory.Services.CreateScope();
            var subscriptionPlanRepository = scope.ServiceProvider.GetRequiredService<ISubscriptionPlanRepository>();
            var subscriptionPlan = new Domain.Subscriptions.SubscriptionPlan(
                "Original Plan", "Original description", 29.99m, 299.99m, 5, 20, true, true, true, true, true, 500, false, "test");
            await subscriptionPlanRepository.AddAsync(subscriptionPlan, CancellationToken.None);

            // Act: Update subscription plan
            var updateRequest = new
            {
                SubscriptionPlanId = subscriptionPlan.Id.ToString(),
                Name = "Updated Premium Plan",
                Description = "Updated premium subscription plan",
                MonthlyPrice = 59.99m,
                YearlyPrice = 599.99m,
                MaxLocations = 10,
                MaxStaffPerLocation = 30,
                IncludesAnalytics = true,
                IncludesAdvancedReporting = true,
                IncludesCustomBranding = true,
                IncludesAdvertising = true,
                IncludesMultipleLocations = true,
                MaxQueueEntriesPerDay = 1500,
                IsFeatured = true,
                IsDefault = false
            };

            var response = await _client.PutAsJsonAsync($"/api/subscriptionplans/{subscriptionPlan.Id}", updateRequest);

            // Debug: Check response content
            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Assert.Fail($"Expected OK but got {response.StatusCode}. Response: {responseContent}");
            }

            // Debug: Log the actual response before any assertions
            Console.WriteLine($"UpdateSubscriptionPlan Status: {response.StatusCode}");
            Console.WriteLine($"UpdateSubscriptionPlan Response: {responseContent}");
            
            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            
            if (!string.IsNullOrWhiteSpace(responseContent))
            {
                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                Assert.IsTrue(result.GetProperty("success").GetBoolean());
            }
        }

        [TestMethod]
        public async Task GetSubscriptionPlans_ReturnsActiveList()
        {
            Console.WriteLine("Debug: Starting GetSubscriptionPlans test");
            var adminToken = await CreateAndAuthenticateUserAsync("PlatformAdmin");
            Console.WriteLine($"Debug: Got admin token: {adminToken?.Substring(0, 20)}...");
            _client.DefaultRequestHeaders.Authorization = new("Bearer", adminToken);

            // Create some subscription plans first
            using var scope = _factory.Services.CreateScope();
            var subscriptionPlanRepository = scope.ServiceProvider.GetRequiredService<ISubscriptionPlanRepository>();
            var plan1 = new Domain.Subscriptions.SubscriptionPlan(
                "Basic Plan", "Basic subscription", 19.99m, 199.99m, 1, 10, false, false, false, false, false, 200, false, "test");
            var plan2 = new Domain.Subscriptions.SubscriptionPlan(
                "Premium Plan", "Premium subscription", 49.99m, 499.99m, 5, 25, true, true, true, false, true, 1000, true, "test");
            
            await subscriptionPlanRepository.AddAsync(plan1, CancellationToken.None);
            await subscriptionPlanRepository.AddAsync(plan2, CancellationToken.None);

            // Act: Get subscription plans
            var response = await _client.GetAsync("/api/subscriptionplans");

            // Debug: Check response content
            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Assert.Fail($"Expected OK but got {response.StatusCode}. Response: {responseContent}");
            }

            // Debug: Log the actual response before any assertions
            Console.WriteLine($"GetSubscriptionPlans Status: {response.StatusCode}");
            Console.WriteLine($"GetSubscriptionPlans Response: {responseContent}");
            
            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.IsTrue(result.ValueKind == JsonValueKind.Array);
            Assert.IsTrue(result.GetArrayLength() >= 2);
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
                "platformadmin" => UserRoles.PlatformAdmin,
                "owner" => UserRoles.Owner,
                "staff" => UserRoles.Staff,
                "customer" => UserRoles.Customer,
                "serviceaccount" => UserRoles.ServiceAccount,
                // Legacy mappings for backward compatibility
                "admin" => UserRoles.Owner,
                "barber" => UserRoles.Staff,
                "client" => UserRoles.Customer,
                "user" => UserRoles.Customer,
                _ => UserRoles.Customer
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