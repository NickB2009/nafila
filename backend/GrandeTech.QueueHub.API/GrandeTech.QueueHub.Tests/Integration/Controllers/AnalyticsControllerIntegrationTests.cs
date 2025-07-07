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
using Grande.Fila.API.Domain.Organizations;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.Staff;
using Grande.Fila.API.Domain.Locations;
using Grande.Fila.API.Domain.AuditLogs;
using Grande.Fila.API.Domain.Subscriptions;
using Grande.Fila.API.Application.Analytics;
using System.Linq;
using System.Collections.Generic;

namespace Grande.Fila.API.Tests.Integration.Controllers
{
    [TestClass]
    [TestCategory("Integration")]
    public class AnalyticsControllerIntegrationTests
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
                        services.AddScoped<AnalyticsService>();
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

        /// <summary>
        /// UC-ANALYTICS: Platform administrator views cross-barbershop analytics
        /// </summary>
        [TestMethod]
        public async Task GetCrossBarbershopAnalytics_ValidPlatformAdmin_ReturnsSuccess()
        {
            // Arrange: Create platform admin user
            var adminToken = await CreateAndAuthenticateUserAsync("PlatformAdmin");
            _client.DefaultRequestHeaders.Authorization = new("Bearer", adminToken);

            // Create test data: subscription plans, organizations, locations, queues, staff
            await SetupTestAnalyticsDataAsync();

            var request = new
            {
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow,
                IncludeActiveQueues = true,
                IncludeCompletedServices = true,
                IncludeAverageWaitTimes = true,
                IncludeStaffUtilization = true,
                IncludeLocationMetrics = true
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/analytics/cross-barbershop", request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            Assert.IsTrue(result.GetProperty("success").GetBoolean());
            
            var analyticsData = result.GetProperty("analyticsData");
            Assert.IsTrue(analyticsData.GetProperty("totalOrganizations").GetInt32() >= 0);
            Assert.IsTrue(analyticsData.GetProperty("totalActiveQueues").GetInt32() >= 0);
            Assert.IsTrue(analyticsData.GetProperty("totalCompletedServices").GetInt32() >= 0);
            Assert.IsTrue(analyticsData.GetProperty("averageWaitTimeMinutes").GetDouble() >= 0);
            Assert.IsTrue(analyticsData.GetProperty("staffUtilizationPercentage").GetDouble() >= 0);
        }

        [TestMethod]
        public async Task GetCrossBarbershopAnalytics_NonPlatformAdmin_ReturnsForbidden()
        {
            // Arrange: Create non-admin user
            var userToken = await CreateAndAuthenticateUserAsync("Barber");
            _client.DefaultRequestHeaders.Authorization = new("Bearer", userToken);

            var request = new
            {
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/analytics/cross-barbershop", request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [TestMethod]
        public async Task GetCrossBarbershopAnalytics_InvalidDateRange_ReturnsBadRequest()
        {
            // Arrange
            var adminToken = await CreateAndAuthenticateUserAsync("PlatformAdmin");
            _client.DefaultRequestHeaders.Authorization = new("Bearer", adminToken);

            var request = new
            {
                StartDate = DateTime.UtcNow.AddDays(1), // Future date
                EndDate = DateTime.UtcNow.AddDays(-1)   // Past date
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/analytics/cross-barbershop", request);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            Assert.IsFalse(result.GetProperty("success").GetBoolean());
            Assert.IsTrue(result.GetProperty("fieldErrors").EnumerateObject().Any());
        }

        /// <summary>
        /// UC-ANALYTICS: Admin/Owner views organization-specific analytics
        /// </summary>
        [TestMethod]
        public async Task GetOrganizationAnalytics_ValidOwner_ReturnsSuccess()
        {
            // Arrange: Create owner user
            var ownerToken = await CreateAndAuthenticateUserAsync("Owner");
            _client.DefaultRequestHeaders.Authorization = new("Bearer", ownerToken);

            // Create test organization
            var organizationId = await CreateTestOrganizationAsync();

            var request = new
            {
                OrganizationId = organizationId.ToString(),
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow,
                IncludeQueueMetrics = true,
                IncludeServiceMetrics = true,
                IncludeStaffMetrics = true,
                IncludeLocationMetrics = true
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/analytics/organization", request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            Assert.IsTrue(result.GetProperty("success").GetBoolean());
            
            var analyticsData = result.GetProperty("analyticsData");
            Assert.AreEqual(organizationId.ToString(), analyticsData.GetProperty("organizationId").GetString());
            Assert.IsTrue(analyticsData.GetProperty("totalLocations").GetInt32() >= 0);
            Assert.IsTrue(analyticsData.GetProperty("totalStaffMembers").GetInt32() >= 0);
        }

        [TestMethod]
        public async Task GetOrganizationAnalytics_InvalidOrganizationId_ReturnsBadRequest()
        {
            // Arrange
            var ownerToken = await CreateAndAuthenticateUserAsync("Owner");
            _client.DefaultRequestHeaders.Authorization = new("Bearer", ownerToken);

            var request = new
            {
                OrganizationId = "invalid-guid",
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/analytics/organization", request);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        /// <summary>
        /// UC-ANALYTICS: Platform administrator views top performing organizations
        /// </summary>
        [TestMethod]
        public async Task GetTopPerformingOrganizations_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var adminToken = await CreateAndAuthenticateUserAsync("PlatformAdmin");
            _client.DefaultRequestHeaders.Authorization = new("Bearer", adminToken);

            // Create test data
            await SetupTestAnalyticsDataAsync();

            var request = new
            {
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow,
                MetricType = "CompletedServices",
                MaxResults = 10,
                IncludeDetails = false
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/analytics/top-organizations", request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            Assert.IsTrue(result.GetProperty("success").GetBoolean());
            Assert.AreEqual("CompletedServices", result.GetProperty("metricType").GetString());
            
            var topOrganizations = result.GetProperty("topOrganizations");
            Assert.IsTrue(topOrganizations.GetArrayLength() <= 10);
        }

        [TestMethod]
        public async Task GetTopPerformingOrganizations_ExcessiveMaxResults_ReturnsBadRequest()
        {
            // Arrange
            var adminToken = await CreateAndAuthenticateUserAsync("PlatformAdmin");
            _client.DefaultRequestHeaders.Authorization = new("Bearer", adminToken);

            var request = new
            {
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow,
                MetricType = "CompletedServices",
                MaxResults = 200 // Exceeds allowed limit
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/analytics/top-organizations", request);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task GetTopPerformingOrganizations_NonPlatformAdmin_ReturnsForbidden()
        {
            // Arrange
            var userToken = await CreateAndAuthenticateUserAsync("Owner");
            _client.DefaultRequestHeaders.Authorization = new("Bearer", userToken);

            var request = new
            {
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow,
                MetricType = "CompletedServices",
                MaxResults = 10
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/analytics/top-organizations", request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [TestMethod]
        public async Task Analytics_UnauthorizedAccess_ReturnsUnauthorized()
        {
            // Arrange: No authentication
            var request = new
            {
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/analytics/cross-barbershop", request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        private async Task<Guid> SetupTestAnalyticsDataAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var subscriptionPlanRepository = scope.ServiceProvider.GetRequiredService<ISubscriptionPlanRepository>();
            var organizationRepository = scope.ServiceProvider.GetRequiredService<IOrganizationRepository>();
            var locationRepository = scope.ServiceProvider.GetRequiredService<ILocationRepository>();
            var queueRepository = scope.ServiceProvider.GetRequiredService<IQueueRepository>();
            var staffMemberRepository = scope.ServiceProvider.GetRequiredService<IStaffMemberRepository>();

            // Create subscription plan with analytics
            var subscriptionPlan = new Grande.Fila.API.Domain.Subscriptions.SubscriptionPlan(
                "Analytics Test Plan", "Plan with analytics enabled", 49.99m, 499.99m, 3, 15, 
                true, true, true, false, true, 1000, false, "test");
            await subscriptionPlanRepository.AddAsync(subscriptionPlan, CancellationToken.None);

            // Create organization with analytics sharing enabled
            var uniqueId = Guid.NewGuid().ToString("N")[..8];
            var organization = new Organization($"Analytics Test Org {uniqueId}", $"analytics-test-org-{uniqueId}", "Test organization", 
                "analytics@test.com", "+1234567890", "https://analytics-test.com", null, subscriptionPlan.Id, "test");
            organization.SetAnalyticsSharing(true, "test");
            await organizationRepository.AddAsync(organization, CancellationToken.None);

            // Create location
            var address = Grande.Fila.API.Domain.Common.ValueObjects.Address.Create(
                "123 Analytics St", "1", "", "Downtown", "Analytics City", "AC", "USA", "12345");
            var location = new Location("Analytics Location", "analytics-location", "Test location", 
                organization.Id, address, "+1234567890", "location@analytics.com", 
                TimeSpan.FromHours(9), TimeSpan.FromHours(17), 50, 30, "test");
            await locationRepository.AddAsync(location, CancellationToken.None);

            // Create staff member
            var staffMember = new StaffMember("Analytics Barber", location.Id, "barber@analytics.com", 
                "+1234567890", null, "Barber", "analytics.barber", null, "test");
            await staffMemberRepository.AddAsync(staffMember, CancellationToken.None);

            // Create queue with some entries
            var queue = new Queue(location.Id, 50, 30, "test");
            queue.AddCustomerToQueue(Guid.NewGuid(), "Test Customer 1", staffMember.Id);
            queue.AddCustomerToQueue(Guid.NewGuid(), "Test Customer 2", staffMember.Id);
            await queueRepository.AddAsync(queue, CancellationToken.None);

            return organization.Id;
        }

        private async Task<Guid> CreateTestOrganizationAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var subscriptionPlanRepository = scope.ServiceProvider.GetRequiredService<ISubscriptionPlanRepository>();
            var organizationRepository = scope.ServiceProvider.GetRequiredService<IOrganizationRepository>();

            // Create subscription plan
            var subscriptionPlan = new Grande.Fila.API.Domain.Subscriptions.SubscriptionPlan(
                "Test Plan", "Test plan", 29.99m, 299.99m, 2, 10, true, false, false, false, true, 500, false, "test");
            await subscriptionPlanRepository.AddAsync(subscriptionPlan, CancellationToken.None);

            // Create organization
            var uniqueId = Guid.NewGuid().ToString("N")[..8];
            var organization = new Organization($"Test Organization {uniqueId}", $"test-organization-{uniqueId}", "Test description", 
                "test@org.com", "+1234567890", "https://test-org.com", null, subscriptionPlan.Id, "test");
            await organizationRepository.AddAsync(organization, CancellationToken.None);

            return organization.Id;
        }

        private async Task<string> CreateAndAuthenticateUserAsync(string role)
        {
            using var scope = _factory.Services.CreateScope();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var authService = scope.ServiceProvider.GetRequiredService<AuthService>();

            var uniqueId = Guid.NewGuid().ToString("N");
            var mappedRole = role.ToLower() switch
            {
                "platformadmin" => UserRoles.Admin,
                "admin" => UserRoles.Owner,
                "owner" => UserRoles.Owner,
                "barber" => UserRoles.Barber,
                "client" => UserRoles.Client,
                _ => UserRoles.Client
            };
            
            var user = new User($"testuser_{uniqueId}", $"test_{uniqueId}@example.com", BCrypt.Net.BCrypt.HashPassword("TestPassword123!"), mappedRole);
            user.DisableTwoFactor();

            if (role == "Admin" || role == "Owner" || role == "PlatformAdmin")
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
            Assert.IsTrue(loginResult.Success);
            Assert.IsNotNull(loginResult.Token);

            return loginResult.Token;
        }
    }
} 