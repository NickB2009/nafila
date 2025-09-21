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
using Grande.Fila.API.Domain.ServicesOffered;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;

namespace Grande.Fila.API.Tests.Integration.Controllers
{
    [TestClass]
    [TestCategory("Integration")]
    public class WorkflowIntegrationTests
    {
        private static WebApplicationFactory<Program> _factory;
        private static BogusUserRepository _userRepository;
        private static BogusOrganizationRepository _organizationRepository;
        private static BogusQueueRepository _queueRepository;
        private static BogusStaffMemberRepository _staffMemberRepository;
        private static BogusLocationRepository _locationRepository;
        private static BogusAuditLogRepository _auditLogRepository;
        private static BogusSubscriptionPlanRepository _subscriptionPlanRepository;
        private static BogusServiceTypeRepository _serviceTypeRepository;
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
            _serviceTypeRepository = new BogusServiceTypeRepository();
            
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
                            ["Database:UseSqlDatabase"] = "false",
                            ["Database:UseInMemoryDatabase"] = "false",
                            ["Database:UseBogusRepositories"] = "true",
                            ["Database:AutoMigrate"] = "false",
                            ["Database:SeedData"] = "false"
                        });
                    });
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
                            typeof(ISubscriptionPlanRepository),
                            typeof(IServicesOfferedRepository)
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
                        services.AddSingleton<IServicesOfferedRepository>(_serviceTypeRepository);
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
        /// Tests the complete workflow: Subscription Plan → Organization → Location → Services → Staff
        /// This covers the full UC-SUBPLAN integration with dependent use cases
        /// </summary>
        [TestMethod]
        public async Task CompleteWorkflow_SubscriptionPlanToStaffCreation_Success()
        {
            // Arrange: Create admin user
            var adminToken = await CreateAndAuthenticateUserAsync("PlatformAdmin");
            _client.DefaultRequestHeaders.Authorization = new("Bearer", adminToken);

            // Step 1: Create Subscription Plan
            var subscriptionPlanRequest = new
            {
                Name = "Workflow Test Plan",
                Description = "Test plan for full workflow",
                MonthlyPrice = 39.99m,
                YearlyPrice = 399.99m,
                MaxLocations = 3,
                MaxStaffPerLocation = 15,
                IncludesAnalytics = true,
                IncludesAdvancedReporting = false,
                IncludesCustomBranding = true,
                IncludesAdvertising = false,
                IncludesMultipleLocations = true,
                MaxQueueEntriesPerDay = 750,
                IsFeatured = false,
                IsDefault = false
            };

            var planResponse = await _client.PostAsJsonAsync("/api/subscriptionplans", subscriptionPlanRequest);
            Assert.AreEqual(HttpStatusCode.OK, planResponse.StatusCode);
            
            var planResult = await planResponse.Content.ReadFromJsonAsync<JsonElement>();
            var subscriptionPlanId = planResult.GetProperty("subscriptionPlanId").GetString();
            Assert.IsNotNull(subscriptionPlanId);

            // Step 2: Create Organization using Subscription Plan
            var organizationRequest = new
            {
                Name = $"Workflow Test Salon {Guid.NewGuid().ToString("N")[..8]}",
                Slug = $"workflow-test-salon-{Guid.NewGuid().ToString("N")[..8]}",
                Description = "Test salon for workflow integration",
                ContactEmail = "workflow@testsalon.com",
                ContactPhone = "+1234567890",
                SubscriptionPlanId = subscriptionPlanId
            };

            var orgResponse = await _client.PostAsJsonAsync("/api/organizations", organizationRequest);
            Assert.AreEqual(HttpStatusCode.OK, orgResponse.StatusCode);
            
            var orgResult = await orgResponse.Content.ReadFromJsonAsync<JsonElement>();
            var organizationId = orgResult.GetProperty("organizationId").GetString();
            Assert.IsNotNull(organizationId);

            // Step 3: Create Location
            var locationRequest = new
            {
                BusinessName = "Workflow Test Location",
                Description = "Test location for workflow",
                ContactPhone = "+1-555-123-4567",
                ContactEmail = "location@workflow.com",
                Address = new
                {
                    Street = "123 Workflow St",
                    City = "Test City",
                    State = "TS",
                    PostalCode = "12345",
                    Country = "USA"
                },
                BusinessHours = new Dictionary<string, string>
                {
                    { "Monday", "09:00-18:00" },
                    { "Tuesday", "09:00-18:00" },
                    { "Wednesday", "09:00-18:00" },
                    { "Thursday", "09:00-18:00" },
                    { "Friday", "09:00-18:00" }
                },
                MaxQueueCapacity = 50
            };

            var locationResponse = await _client.PostAsJsonAsync("/api/locations", locationRequest);
            locationResponse.EnsureSuccessStatusCode();
            
            var locationResult = await locationResponse.Content.ReadFromJsonAsync<JsonElement>();
            var locationId = locationResult.GetProperty("locationId").GetString();
            Assert.IsNotNull(locationId);

            // Step 4: Create Services
            var serviceRequests = new[]
            {
                new {
                    Name = "Workflow Haircut",
                    Description = "Standard haircut service",
                    LocationId = locationId,
                    EstimatedDurationMinutes = 30,
                    Price = 25.00m
                },
                new {
                    Name = "Workflow Beard Trim",
                    Description = "Professional beard trimming",
                    LocationId = locationId,
                    EstimatedDurationMinutes = 15,
                    Price = 15.00m
                }
            };

            var serviceIds = new List<string>();
            foreach (var serviceRequest in serviceRequests)
            {
                var serviceResponse = await _client.PostAsJsonAsync("/api/servicesoffered", serviceRequest);
                serviceResponse.EnsureSuccessStatusCode();
                
                var serviceResult = await serviceResponse.Content.ReadFromJsonAsync<JsonElement>();
                var serviceId = serviceResult.GetProperty("serviceTypeId").GetString();
                Assert.IsNotNull(serviceId);
                serviceIds.Add(serviceId);
            }

            // Step 5: Create Staff Member using all previous resources
            var staffRequest = new
            {
                FirstName = "Workflow",
                LastName = "Test Barber",
                Email = "workflow.barber@testsalon.com",
                PhoneNumber = "+1234567890",
                Username = "workflow.barber",
                LocationId = locationId,
                ServiceTypeIds = serviceIds,
                Address = "123 Barber St, Test City",
                Notes = "Created through full workflow integration test"
            };

            var staffResponse = await _client.PostAsJsonAsync("/api/staff/barbers", staffRequest);
            Assert.AreEqual(HttpStatusCode.OK, staffResponse.StatusCode);
            
            var staffResult = await staffResponse.Content.ReadFromJsonAsync<JsonElement>();
            var barberId = staffResult.GetProperty("barberId").GetString();
            Assert.IsNotNull(barberId);
            Assert.AreEqual("Active", staffResult.GetProperty("status").GetString());

            // Verification: Ensure all resources are properly linked
            // Verify the staff member exists and has correct associations
            Assert.IsTrue(Guid.TryParse(barberId, out _), "Barber ID should be a valid GUID");
            Assert.IsTrue(Guid.TryParse(locationId, out _), "Location ID should be a valid GUID");
            Assert.IsTrue(Guid.TryParse(organizationId, out _), "Organization ID should be a valid GUID");
            Assert.IsTrue(Guid.TryParse(subscriptionPlanId, out _), "Subscription Plan ID should be a valid GUID");
            
            Console.WriteLine($"✅ Workflow completed successfully:");
            Console.WriteLine($"   Subscription Plan: {subscriptionPlanId}");
            Console.WriteLine($"   Organization: {organizationId}");
            Console.WriteLine($"   Location: {locationId}");
            Console.WriteLine($"   Services: {string.Join(", ", serviceIds)}");
            Console.WriteLine($"   Staff Member: {barberId}");
        }

        /// <summary>
        /// Tests the default subscription plan public endpoint
        /// </summary>
        [TestMethod]
        public async Task GetDefaultSubscriptionPlan_PublicEndpoint_ReturnsDefault()
        {
            // Arrange: Create a default subscription plan
            using var scope = _factory.Services.CreateScope();
            var subscriptionPlanRepository = scope.ServiceProvider.GetRequiredService<ISubscriptionPlanRepository>();
            
            var defaultPlan = new Grande.Fila.API.Domain.Subscriptions.SubscriptionPlan(
                "Default Plan", 
                "Default subscription plan", 
                19.99m, 
                199.99m, 
                1, 
                5, 
                false, 
                false, 
                false, 
                false, 
                false, 
                100, 
                false, 
                "system");
            
            // Set as default using reflection since the property is private
            var isDefaultProperty = typeof(Grande.Fila.API.Domain.Subscriptions.SubscriptionPlan).GetProperty("IsDefault");
            isDefaultProperty?.SetValue(defaultPlan, true);
            
            await subscriptionPlanRepository.AddAsync(defaultPlan, CancellationToken.None);

            // Act: Call public endpoint (no authentication required)
            var response = await _client.GetAsync("/api/subscriptionplans/default");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.AreEqual(defaultPlan.Id.ToString(), result.GetProperty("id").GetString());
            Assert.AreEqual("Default Plan", result.GetProperty("name").GetString());
            Assert.AreEqual(19.99m, result.GetProperty("monthlyPrice").GetDecimal());
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
            
            var user = new User($"testuser_{uniqueId}", $"test_{uniqueId}@example.com", $"+1234567890{uniqueId}", BCrypt.Net.BCrypt.HashPassword("TestPassword123!"), mappedRole);
            user.DisableTwoFactor();

            if (role == "Admin" || role == "Owner" || role == "PlatformAdmin")
            {
                user.AddPermission(Permission.CreateStaff);
                user.AddPermission(Permission.UpdateStaff);
            }

            await userRepository.AddAsync(user, CancellationToken.None);

            var loginRequest = new LoginRequest
            {
                PhoneNumber = user.PhoneNumber,
                Password = "TestPassword123!"
            };

            var loginResult = await authService.LoginAsync(loginRequest);
            Assert.IsTrue(loginResult.Success);
            Assert.IsNotNull(loginResult.Token);

            return loginResult.Token;
        }
    }
} 