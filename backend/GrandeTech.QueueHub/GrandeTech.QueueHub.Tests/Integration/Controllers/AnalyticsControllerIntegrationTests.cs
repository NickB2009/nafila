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
using Grande.Fila.API.Application.Services;

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
                        services.AddScoped<CalculateWaitService>();
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

        [TestMethod]
        public async Task Debug_ServiceAccountTokenGeneration()
        {
            // Arrange: Create service account user
            using var scope = _factory.Services.CreateScope();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var authService = scope.ServiceProvider.GetRequiredService<AuthService>();

            var uniqueId = Guid.NewGuid().ToString("N");
            var user = new User($"debug_service_{uniqueId}", $"debug_service_{uniqueId}@example.com", 
                BCrypt.Net.BCrypt.HashPassword("TestPassword123!"), UserRoles.ServiceAccount);
            user.DisableTwoFactor();

            await userRepository.AddAsync(user, CancellationToken.None);

            var loginRequest = new LoginRequest
            {
                Username = user.Username,
                Password = "TestPassword123!"
            };

            // Act: Login and get token
            var loginResult = await authService.LoginAsync(loginRequest);

            // Assert: Check login result
            Assert.IsTrue(loginResult.Success, $"Login failed: {loginResult.Error}");
            Assert.IsNotNull(loginResult.Token);
            Assert.AreEqual(UserRoles.ServiceAccount, loginResult.Role);

            // Decode and examine the JWT token
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(loginResult.Token);

            // Check claims
            var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == TenantClaims.Role);
            var isServiceAccountClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == TenantClaims.IsServiceAccount);
            var orgIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == TenantClaims.OrganizationId);

            Assert.IsNotNull(roleClaim, "Role claim is missing");
            Assert.AreEqual(UserRoles.ServiceAccount, roleClaim.Value, "Role claim has wrong value");
            Assert.IsNotNull(isServiceAccountClaim, "IsServiceAccount claim is missing");
            Assert.AreEqual("true", isServiceAccountClaim.Value, "IsServiceAccount claim should be true");
            Assert.IsNull(orgIdClaim, "ServiceAccount should not have organization context");

            // Test the token with the endpoint
            _client.DefaultRequestHeaders.Authorization = new("Bearer", loginResult.Token);

            var testData = await SetupTestWaitTimeDataAsync();
            var request = new
            {
                LocationId = testData.LocationId.ToString(),
                QueueId = testData.QueueId.ToString(),
                EntryId = testData.EntryId.ToString()
            };

            var response = await _client.PostAsJsonAsync("/api/analytics/calculate-wait", request);
            
            // Debug: Print response details
            var responseContent = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"Response Status: {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"Response Content: {responseContent}");
            
            // This should work now
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, 
                $"Expected OK but got {response.StatusCode}. Response: {responseContent}");
        }

        /// <summary>
        /// UC-CALCWAIT: Calculate estimated wait time with valid service account
        /// </summary>
        [TestMethod]
        public async Task CalculateWaitTime_ValidServiceAccount_ReturnsSuccess()
        {
            // Arrange: Create service account user
            var serviceAccountToken = await CreateAndAuthenticateUserAsync("ServiceAccount");
            _client.DefaultRequestHeaders.Authorization = new("Bearer", serviceAccountToken);

            // Create test data
            var testData = await SetupTestWaitTimeDataAsync();

            var request = new
            {
                LocationId = testData.LocationId.ToString(),
                QueueId = testData.QueueId.ToString(),
                EntryId = testData.EntryId.ToString()
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/analytics/calculate-wait", request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            Assert.IsTrue(result.GetProperty("success").GetBoolean());
            Assert.IsTrue(result.GetProperty("estimatedWaitMinutes").GetInt32() >= 0);
        }

        [TestMethod]
        public async Task CalculateWaitTime_InvalidLocationId_ReturnsBadRequest()
        {
            // Arrange
            var serviceAccountToken = await CreateAndAuthenticateUserAsync("ServiceAccount");
            _client.DefaultRequestHeaders.Authorization = new("Bearer", serviceAccountToken);

            var request = new
            {
                LocationId = "invalid-guid",
                QueueId = Guid.NewGuid().ToString(),
                EntryId = Guid.NewGuid().ToString()
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/analytics/calculate-wait", request);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            Assert.IsFalse(result.GetProperty("success").GetBoolean());
            Assert.IsTrue(result.GetProperty("errors").GetArrayLength() > 0);
        }

        [TestMethod]
        public async Task CalculateWaitTime_InvalidQueueId_ReturnsBadRequest()
        {
            // Arrange
            var serviceAccountToken = await CreateAndAuthenticateUserAsync("ServiceAccount");
            _client.DefaultRequestHeaders.Authorization = new("Bearer", serviceAccountToken);

            var request = new
            {
                LocationId = Guid.NewGuid().ToString(),
                QueueId = "invalid-guid",
                EntryId = Guid.NewGuid().ToString()
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/analytics/calculate-wait", request);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            Assert.IsFalse(result.GetProperty("success").GetBoolean());
            Assert.IsTrue(result.GetProperty("errors").GetArrayLength() > 0);
        }

        [TestMethod]
        public async Task CalculateWaitTime_InvalidEntryId_ReturnsBadRequest()
        {
            // Arrange
            var serviceAccountToken = await CreateAndAuthenticateUserAsync("ServiceAccount");
            _client.DefaultRequestHeaders.Authorization = new("Bearer", serviceAccountToken);

            var request = new
            {
                LocationId = Guid.NewGuid().ToString(),
                QueueId = Guid.NewGuid().ToString(),
                EntryId = "invalid-guid"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/analytics/calculate-wait", request);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            Assert.IsFalse(result.GetProperty("success").GetBoolean());
            Assert.IsTrue(result.GetProperty("errors").GetArrayLength() > 0);
        }

        [TestMethod]
        public async Task CalculateWaitTime_NonExistentQueue_ReturnsBadRequest()
        {
            // Arrange
            var serviceAccountToken = await CreateAndAuthenticateUserAsync("ServiceAccount");
            _client.DefaultRequestHeaders.Authorization = new("Bearer", serviceAccountToken);

            var request = new
            {
                LocationId = Guid.NewGuid().ToString(),
                QueueId = Guid.NewGuid().ToString(),
                EntryId = Guid.NewGuid().ToString()
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/analytics/calculate-wait", request);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            Assert.IsFalse(result.GetProperty("success").GetBoolean());
            Assert.IsTrue(result.GetProperty("errors").GetArrayLength() > 0);
        }

        [TestMethod]
        public async Task CalculateWaitTime_NonServiceAccount_ReturnsForbidden()
        {
            // Arrange: Create non-service account user
            var userToken = await CreateAndAuthenticateUserAsync("Barber");
            _client.DefaultRequestHeaders.Authorization = new("Bearer", userToken);

            var request = new
            {
                LocationId = Guid.NewGuid().ToString(),
                QueueId = Guid.NewGuid().ToString(),
                EntryId = Guid.NewGuid().ToString()
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/analytics/calculate-wait", request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [TestMethod]
        public async Task CalculateWaitTime_NoAuthentication_ReturnsUnauthorized()
        {
            // Arrange: No authentication
            var request = new
            {
                LocationId = Guid.NewGuid().ToString(),
                QueueId = Guid.NewGuid().ToString(),
                EntryId = Guid.NewGuid().ToString()
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/analytics/calculate-wait", request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task CalculateWaitTime_EmptyLocationId_ReturnsBadRequest()
        {
            // Arrange
            var serviceAccountToken = await CreateAndAuthenticateUserAsync("ServiceAccount");
            _client.DefaultRequestHeaders.Authorization = new("Bearer", serviceAccountToken);

            var request = new
            {
                LocationId = "",
                QueueId = Guid.NewGuid().ToString(),
                EntryId = Guid.NewGuid().ToString()
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/analytics/calculate-wait", request);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task CalculateWaitTime_EmptyQueueId_ReturnsBadRequest()
        {
            // Arrange
            var serviceAccountToken = await CreateAndAuthenticateUserAsync("ServiceAccount");
            _client.DefaultRequestHeaders.Authorization = new("Bearer", serviceAccountToken);

            var request = new
            {
                LocationId = Guid.NewGuid().ToString(),
                QueueId = "",
                EntryId = Guid.NewGuid().ToString()
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/analytics/calculate-wait", request);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task CalculateWaitTime_EmptyEntryId_ReturnsBadRequest()
        {
            // Arrange
            var serviceAccountToken = await CreateAndAuthenticateUserAsync("ServiceAccount");
            _client.DefaultRequestHeaders.Authorization = new("Bearer", serviceAccountToken);

            var request = new
            {
                LocationId = Guid.NewGuid().ToString(),
                QueueId = Guid.NewGuid().ToString(),
                EntryId = ""
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/analytics/calculate-wait", request);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        /// <summary>
        /// UC-ANALYTICS: Platform admin views cross-barbershop analytics
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
            // Arrange: Create non-platform-admin user
            var userToken = await CreateAndAuthenticateUserAsync("Staff");
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
        /// UC-ANALYTICS: Platform admin views top performing organizations
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

        [TestMethod]
        public async Task Debug_PlatformAdminTokenGeneration()
        {
            // Arrange: Create platform admin user
            using var scope = _factory.Services.CreateScope();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var authService = scope.ServiceProvider.GetRequiredService<AuthService>();

            var uniqueId = Guid.NewGuid().ToString("N");
            var user = new User($"debug_admin_{uniqueId}", $"debug_admin_{uniqueId}@example.com", 
                BCrypt.Net.BCrypt.HashPassword("TestPassword123!"), UserRoles.PlatformAdmin);
            user.DisableTwoFactor();

            await userRepository.AddAsync(user, CancellationToken.None);

            var loginRequest = new LoginRequest
            {
                Username = user.Username,
                Password = "TestPassword123!"
            };

            // Act: Login and get token
            var loginResult = await authService.LoginAsync(loginRequest);

            // Assert: Check login result
            Assert.IsTrue(loginResult.Success, $"Login failed: {loginResult.Error}");
            Assert.IsNotNull(loginResult.Token);
            Assert.AreEqual(UserRoles.PlatformAdmin, loginResult.Role);

            // Decode and examine the JWT token
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(loginResult.Token);

            // Check claims
            var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == TenantClaims.Role);
            var isServiceAccountClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == TenantClaims.IsServiceAccount);
            var orgIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == TenantClaims.OrganizationId);

            Assert.IsNotNull(roleClaim, "Role claim is missing");
            Assert.AreEqual(UserRoles.PlatformAdmin, roleClaim.Value, "Role claim has wrong value");
            Assert.IsNotNull(isServiceAccountClaim, "IsServiceAccount claim is missing");
            Assert.AreEqual("false", isServiceAccountClaim.Value, "IsServiceAccount claim should be false for PlatformAdmin");
            Assert.IsNull(orgIdClaim, "PlatformAdmin should not have organization context");

            // Test the token with the admin endpoint
            _client.DefaultRequestHeaders.Authorization = new("Bearer", loginResult.Token);

            await SetupTestAnalyticsDataAsync();
            var request = new
            {
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow,
                MetricType = "CompletedServices",
                MaxResults = 10,
                IncludeDetails = false
            };

            var response = await _client.PostAsJsonAsync("/api/analytics/top-organizations", request);
            
            // Debug: Print response details
            var responseContent = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"Response Status: {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"Response Content: {responseContent}");
            
            // This should work now
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, 
                $"Expected OK but got {response.StatusCode}. Response: {responseContent}");
        }

        private async Task<(Guid LocationId, Guid QueueId, Guid EntryId)> SetupTestWaitTimeDataAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var subscriptionPlanRepository = scope.ServiceProvider.GetRequiredService<ISubscriptionPlanRepository>();
            var organizationRepository = scope.ServiceProvider.GetRequiredService<IOrganizationRepository>();
            var locationRepository = scope.ServiceProvider.GetRequiredService<ILocationRepository>();
            var queueRepository = scope.ServiceProvider.GetRequiredService<IQueueRepository>();
            var staffMemberRepository = scope.ServiceProvider.GetRequiredService<IStaffMemberRepository>();

            // Create subscription plan
            var subscriptionPlan = new Grande.Fila.API.Domain.Subscriptions.SubscriptionPlan(
                "Wait Time Test Plan", "Plan for wait time testing", 49.99m, 499.99m, 3, 15, 
                true, true, true, false, true, 1000, false, "test");
            await subscriptionPlanRepository.AddAsync(subscriptionPlan, CancellationToken.None);

            // Create organization
            var uniqueId = Guid.NewGuid().ToString("N")[..8];
            var organization = new Organization($"Wait Time Test Org {uniqueId}", $"wait-time-test-org-{uniqueId}", "Test organization", 
                "waittime@test.com", "+1234567890", "https://waittime-test.com", null, subscriptionPlan.Id, "test");
            await organizationRepository.AddAsync(organization, CancellationToken.None);

            // Create location
            var address = Grande.Fila.API.Domain.Common.ValueObjects.Address.Create(
                "123 Wait Time St", "1", "", "Downtown", "Wait Time City", "WTC", "USA", "12345");
            var location = new Location("Wait Time Location", "wait-time-location", "Test location", 
                organization.Id, address, "+1234567890", "location@waittime.com", 
                TimeSpan.FromHours(9), TimeSpan.FromHours(17), 50, 30, "test");
            await locationRepository.AddAsync(location, CancellationToken.None);

            // Create staff member
            var staffMember = new StaffMember("Wait Time Barber", location.Id, "barber@waittime.com", 
                "+1234567890", null, "Barber", "waittime.barber", null, "test");
            await staffMemberRepository.AddAsync(staffMember, CancellationToken.None);

            // Create queue with entries
            var queue = new Queue(location.Id, 50, 30, "test");
            var customerId = Guid.NewGuid();
            queue.AddCustomerToQueue(customerId, "Test Customer for Wait Time", staffMember.Id);
            var queueEntry = queue.Entries.First();
            await queueRepository.AddAsync(queue, CancellationToken.None);

            return (location.Id, queue.Id, queueEntry.Id);
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
                "platformadmin" => UserRoles.PlatformAdmin,
                "owner" => UserRoles.Owner,
                "staff" => UserRoles.Staff,
                "customer" => UserRoles.Customer,
                "serviceaccount" => UserRoles.ServiceAccount,
                // Legacy mappings for backward compatibility
                "admin" => UserRoles.Owner,
                "barber" => UserRoles.Staff,
                "client" => UserRoles.Customer,
                _ => UserRoles.Customer
            };
            
            var user = new User($"testuser_{uniqueId}", $"test_{uniqueId}@example.com", BCrypt.Net.BCrypt.HashPassword("TestPassword123!"), mappedRole);
            user.DisableTwoFactor();

            if (role == "Owner" || role == "PlatformAdmin" || role == "Admin")
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
            Assert.IsTrue(loginResult.Success, $"Login failed for role {role}: {loginResult.Error}");
            Assert.IsNotNull(loginResult.Token);
            
            // Debug: Check the role in the login result
            Assert.AreEqual(mappedRole, loginResult.Role, $"Expected role {mappedRole} but got {loginResult.Role}");

            return loginResult.Token;
        }
    }
} 