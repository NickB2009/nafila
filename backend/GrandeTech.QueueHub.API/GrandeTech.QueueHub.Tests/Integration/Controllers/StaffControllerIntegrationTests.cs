using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Application.Auth;
using Grande.Fila.API.Application.Staff;
using Grande.Fila.API.Domain.Users;
using Grande.Fila.API.Domain.Staff;
using Grande.Fila.API.Domain.Locations;
using Grande.Fila.API.Domain.AuditLogs;
using Grande.Fila.API.Domain.Common.ValueObjects;
using Grande.Fila.API.Infrastructure.Repositories.Bogus;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Grande.Fila.API.Tests.Integration;
using BCrypt.Net;
using Grande.Fila.API.Infrastructure;

namespace Grande.Fila.Tests.Integration.Controllers
{
    [TestClass]
    [TestCategory("Integration")]
    public class StaffControllerIntegrationTests
    {
        private static WebApplicationFactory<Program> _factory;
        private static BogusUserRepository _userRepository;
        private static BogusStaffMemberRepository _staffMemberRepository;
        private static BogusLocationRepository _locationRepository;
        private static BogusAuditLogRepository _auditLogRepository;
        private HttpClient _client;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _userRepository = new BogusUserRepository();
            _staffMemberRepository = new BogusStaffMemberRepository();
            _locationRepository = new BogusLocationRepository();
            _auditLogRepository = new BogusAuditLogRepository();
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        var servicesToRemove = new[]
                        {
                            typeof(IUserRepository),
                            typeof(IStaffMemberRepository),
                            typeof(ILocationRepository),
                            typeof(IAuditLogRepository),
                            typeof(AddBarberService),
                            typeof(EditBarberService),
                            typeof(UpdateStaffStatusService),
                            typeof(StartBreakService),
                            typeof(EndBreakService)
                        };
                        var descriptors = services.Where(d => servicesToRemove.Contains(d.ServiceType)).ToList();
                        descriptors.ForEach(d => services.Remove(d));

                        services.AddSingleton<IUserRepository>(_userRepository);
                        services.AddSingleton<IStaffMemberRepository>(_staffMemberRepository);
                        services.AddSingleton<ILocationRepository>(_locationRepository);
                        services.AddSingleton<IAuditLogRepository>(_auditLogRepository);
                        services.AddScoped<AuthService>();
                        services.AddScoped<AddBarberService>();
                        services.AddScoped<EditBarberService>();
                        services.AddScoped<UpdateStaffStatusService>();
                        services.AddScoped<StartBreakService>();
                        services.AddScoped<EndBreakService>();
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
            _factory?.Dispose();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _client?.Dispose();
        }

        [TestMethod]
        public async Task AddBarber_WithValidDataAndAdminRole_ReturnsSuccessResult()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_factory);

            // First, create and authenticate an admin user
            var adminToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "Admin", new[] { Permission.CreateStaff });
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            // Create a location first (required for barber creation)
            var location = new Location(
                "Test Location",
                "test-location",
                "A test location for integration tests.",
                Guid.NewGuid(),
                Address.Create(
                    "123 Test St", // street
                    "1", // number
                    "", // complement
                    "Downtown", // neighborhood
                    "Testville", // city
                    "Test State", // state
                    "USA", // country
                    "12345" // postalCode
                ),
                "+5511999999999",
                "test@location.com",
                new TimeSpan(9, 0, 0),
                new TimeSpan(17, 0, 0),
                100,
                15,
                "test_user"
            );
            await _locationRepository.AddAsync(location);

            var addBarberRequest = new AddBarberRequest
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@barbershop.com",
                PhoneNumber = "+5511999999999",
                Username = "johndoe",
                LocationId = location.Id.ToString(),
                ServiceTypeIds = new List<string> { Guid.NewGuid().ToString() },
                DeactivateOnCreation = false,
                Address = "Rua Exemplo, 123",
                Notes = "Experienced barber"
            };

            var json = JsonSerializer.Serialize(addBarberRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/staff/barbers", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<AddBarberResult>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.BarberId);
            Assert.IsFalse(string.IsNullOrEmpty(result.BarberId));
            Assert.AreEqual("Active", result.Status);
            Assert.AreEqual(0, result.FieldErrors.Count);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public async Task AddBarber_WithValidDataAndOwnerRole_ReturnsSuccessResult()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_factory);

            // First, create and authenticate an owner user
            var ownerToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "Owner", new[] { Permission.CreateStaff });
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);

            // Create a location first
            var location = new Location(
                "Test Location",
                "test-location",
                "A test location for integration tests.",
                Guid.NewGuid(),
                Address.Create(
                    "123 Test St", // street
                    "1", // number
                    "", // complement
                    "Downtown", // neighborhood
                    "Testville", // city
                    "Test State", // state
                    "USA", // country
                    "12345" // postalCode
                ),
                "+5511999999999",
                "test@location.com",
                new TimeSpan(9, 0, 0),
                new TimeSpan(17, 0, 0),
                100,
                15,
                "test_user"
            );
            await _locationRepository.AddAsync(location);

            var addBarberRequest = new AddBarberRequest
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@barbershop.com",
                PhoneNumber = "+5511888888888",
                Username = "janesmith",
                LocationId = location.Id.ToString(),
                ServiceTypeIds = new List<string> { Guid.NewGuid().ToString() },
                DeactivateOnCreation = false,
                Address = "Avenida Teste, 456",
                Notes = "Expert stylist"
            };

            var json = JsonSerializer.Serialize(addBarberRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/staff/barbers", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AddBarberResult>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.BarberId);
            Assert.IsFalse(string.IsNullOrEmpty(result.BarberId));
            Assert.AreEqual("Active", result.Status);
        }

        [TestMethod]
        public async Task AddBarber_WithUnauthorizedRole_ReturnsForbidden()
        {
            // Arrange
            Assert.IsNotNull(_client);

            // Create and authenticate a regular user (not Admin/Owner)
            var userToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "User", new string[0]);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

            var addBarberRequest = new AddBarberRequest
            {
                FirstName = "Bob",
                LastName = "Johnson",
                Email = "bob.johnson@barbershop.com",
                PhoneNumber = "+5511777777777",
                Username = "bobjohnson",
                LocationId = Guid.NewGuid().ToString(),
                ServiceTypeIds = new List<string> { Guid.NewGuid().ToString() }
            };

            var json = JsonSerializer.Serialize(addBarberRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/staff/barbers", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [TestMethod]
        public async Task AddBarber_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Arrange
            Assert.IsNotNull(_client);

            var addBarberRequest = new AddBarberRequest
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test.user@barbershop.com",
                PhoneNumber = "+5511666666666",
                Username = "testuser",
                LocationId = Guid.NewGuid().ToString(),
                ServiceTypeIds = new List<string> { Guid.NewGuid().ToString() }
            };

            var json = JsonSerializer.Serialize(addBarberRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/staff/barbers", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task AddBarber_WithInvalidData_ReturnsBadRequestWithFieldErrors()
        {
            // Arrange
            Assert.IsNotNull(_client);

            var adminToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "Admin", new[] { Permission.CreateStaff });
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            // Invalid request with missing required fields
            var addBarberRequest = new AddBarberRequest
            {
                FirstName = "", // Invalid - empty
                LastName = "", // Invalid - empty
                Email = "invalid-email", // Invalid format
                PhoneNumber = "", // Invalid - empty
                Username = "", // Invalid - empty
                LocationId = "invalid-guid", // Invalid format
                ServiceTypeIds = new List<string>() // Invalid - empty list
            };

            var json = JsonSerializer.Serialize(addBarberRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/staff/barbers", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AddBarberResult>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.Count > 0);
        }

        [TestMethod]
        public async Task AddBarber_WithDuplicateEmail_ReturnsBadRequestWithFieldError()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_factory);

            var adminToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "Admin", new[] { Permission.CreateStaff });
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var locationId = await CreateLocationAsync();

            // Create a barber first with a specific email
            using var scope = _factory.Services.CreateScope();
            var staffRepo = scope.ServiceProvider.GetRequiredService<IStaffMemberRepository>();
            
            var existingBarber = new StaffMember(
                "Existing Barber",
                Guid.Parse(locationId),
                "duplicate@barbershop.com",
                "+5511555555555",
                null,
                "Barber",
                "existingbarber",
                null,
                "testUser"
            );
            await staffRepo.AddAsync(existingBarber, CancellationToken.None);

            // Now try to create another barber with the same email
            var addBarberRequest = new AddBarberRequest
            {
                FirstName = "New",
                LastName = "Barber",
                Email = "duplicate@barbershop.com", // Same email as existing barber
                PhoneNumber = "+5511444444444",
                Username = "newbarber",
                LocationId = locationId,
                ServiceTypeIds = new List<string> { Guid.NewGuid().ToString() }
            };

            var json = JsonSerializer.Serialize(addBarberRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/staff/barbers", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AddBarberResult>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("email"));
            Assert.AreEqual("A barber with this email already exists.", result.FieldErrors["email"]);
        }

        [TestMethod]
        public async Task AddBarber_WithDuplicateUsername_ReturnsBadRequestWithFieldError()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_factory);

            var adminToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "Admin", new[] { Permission.CreateStaff });
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var locationId = await CreateLocationAsync();

            // Create a barber first with a specific username
            using var scope = _factory.Services.CreateScope();
            var staffRepo = scope.ServiceProvider.GetRequiredService<IStaffMemberRepository>();
            
            var existingBarber = new StaffMember(
                "Existing Barber",
                Guid.Parse(locationId),
                "existing@barbershop.com",
                "+5511555555555",
                null,
                "Barber",
                "duplicateuser", // This username will be duplicated
                null,
                "testUser"
            );
            await staffRepo.AddAsync(existingBarber, CancellationToken.None);

            // Now try to create another barber with the same username
            var addBarberRequest = new AddBarberRequest
            {
                FirstName = "New",
                LastName = "Barber",
                Email = "new@barbershop.com",
                PhoneNumber = "+5511444444444",
                Username = "duplicateuser", // Same username as existing barber
                LocationId = locationId,
                ServiceTypeIds = new List<string> { Guid.NewGuid().ToString() }
            };

            var json = JsonSerializer.Serialize(addBarberRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/staff/barbers", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AddBarberResult>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("username"));
            Assert.AreEqual("A barber with this username already exists.", result.FieldErrors["username"]);
        }

        [TestMethod]
        public async Task AddBarber_WithNonExistentLocation_ReturnsBadRequestWithError()
        {
            // Arrange
            Assert.IsNotNull(_client);

            var adminToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "Admin", new[] { Permission.CreateStaff });
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var addBarberRequest = new AddBarberRequest
            {
                FirstName = "Test",
                LastName = "Barber",
                Email = "test@barbershop.com",
                PhoneNumber = "+5511333333333",
                Username = "testbarber",
                LocationId = Guid.NewGuid().ToString(), // Non-existent location
                ServiceTypeIds = new List<string> { Guid.NewGuid().ToString() }
            };

            var json = JsonSerializer.Serialize(addBarberRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/staff/barbers", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AddBarberResult>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Count > 0);
            Assert.IsTrue(result.Errors.Contains("Location not found."));
        }

        [TestMethod]
        public async Task AddBarber_WithDeactivateOnCreation_ReturnsInactiveStatus()
        {
            // Arrange
            var location = new Location(
                "Test Location",
                "test-location",
                "A test location for integration tests.",
                Guid.NewGuid(),
                Address.Create(
                    "123 Test St", // street
                    "1", // number
                    "", // complement
                    "Downtown", // neighborhood
                    "Testville", // city
                    "Test State", // state
                    "USA", // country
                    "12345" // postalCode
                ),
                "+5511999999999",
                "test@location.com",
                new TimeSpan(9, 0, 0),
                new TimeSpan(17, 0, 0),
                100,
                15,
                "test_user"
            );
            await _locationRepository.AddAsync(location);
            var adminToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "Admin", new[] { Permission.CreateStaff });
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var request = new AddBarberRequest
            {
                FirstName = "Inactive",
                LastName = "Barber",
                Email = "inactive@barbershop.com",
                PhoneNumber = "+5511222222222",
                Username = "inactivebarber",
                LocationId = location.Id.ToString(),
                ServiceTypeIds = new List<string> { Guid.NewGuid().ToString() },
                DeactivateOnCreation = true
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/staff/barbers", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AddBarberResult>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.BarberId);
            Assert.AreEqual("Inactive", result.Status);
        }

        // UC-STAFFSTATUS: Barber changes status integration tests
        [TestMethod]
        public async Task UpdateStaffStatus_WithValidRequestAndBarberRole_ReturnsSuccess()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_factory);

            var barberToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "Barber", new string[0]);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", barberToken);

            // Create a staff member first
            var staffMemberId = await CreateStaffMemberAsync();

            var updateStatusRequest = new UpdateStaffStatusRequest
            {
                StaffMemberId = staffMemberId,
                NewStatus = "busy",
                Notes = "Currently serving a client"
            };

            var json = JsonSerializer.Serialize(updateStatusRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/staff/barbers/{staffMemberId}/status", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<UpdateStaffStatusResult>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(staffMemberId, result.StaffMemberId);
            Assert.AreEqual("busy", result.NewStatus);
            Assert.AreEqual("available", result.PreviousStatus); // Default status
        }

        [TestMethod]
        public async Task UpdateStaffStatus_WithValidStatuses_AllReturnSuccess()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_factory);

            var barberToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "Barber", new string[0]);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", barberToken);

            var validStatuses = new[] { "available", "busy", "away", "offline" };

            foreach (var status in validStatuses)
            {
                // Create a new staff member for each test
                var staffMemberId = await CreateStaffMemberAsync();

                var updateStatusRequest = new UpdateStaffStatusRequest
                {
                    StaffMemberId = staffMemberId,
                    NewStatus = status
                };

                var json = JsonSerializer.Serialize(updateStatusRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Act
                var response = await _client.PutAsync($"/api/staff/barbers/{staffMemberId}/status", content);

                // Assert
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, $"Status '{status}' should be valid");

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<UpdateStaffStatusResult>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                Assert.IsNotNull(result);
                Assert.IsTrue(result.Success);
                Assert.AreEqual(status, result.NewStatus);
            }
        }

        [TestMethod]
        public async Task UpdateStaffStatus_WithInvalidStatus_ReturnsBadRequest()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_factory);

            var barberToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "Barber", new string[0]);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", barberToken);

            var staffMemberId = await CreateStaffMemberAsync();

            var updateStatusRequest = new UpdateStaffStatusRequest
            {
                StaffMemberId = staffMemberId,
                NewStatus = "invalid-status"
            };

            var json = JsonSerializer.Serialize(updateStatusRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/staff/barbers/{staffMemberId}/status", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<UpdateStaffStatusResult>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("newStatus"));
            Assert.AreEqual("Invalid status. Must be one of: available, busy, away, offline", result.FieldErrors["newStatus"]);
        }

        [TestMethod]
        public async Task UpdateStaffStatus_WithEmptyStatus_ReturnsBadRequest()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_factory);

            var barberToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "Barber", new string[0]);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", barberToken);

            var staffMemberId = await CreateStaffMemberAsync();

            var updateStatusRequest = new UpdateStaffStatusRequest
            {
                StaffMemberId = staffMemberId,
                NewStatus = ""
            };

            var json = JsonSerializer.Serialize(updateStatusRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/staff/barbers/{staffMemberId}/status", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<UpdateStaffStatusResult>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("newStatus"));
            Assert.AreEqual("Status is required.", result.FieldErrors["newStatus"]);
        }

        [TestMethod]
        public async Task UpdateStaffStatus_WithNonExistentStaffMember_ReturnsBadRequest()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_factory);

            var barberToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "Barber", new string[0]);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", barberToken);

            var nonExistentStaffMemberId = Guid.NewGuid().ToString();

            var updateStatusRequest = new UpdateStaffStatusRequest
            {
                StaffMemberId = nonExistentStaffMemberId,
                NewStatus = "busy"
            };

            var json = JsonSerializer.Serialize(updateStatusRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/staff/barbers/{nonExistentStaffMemberId}/status", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<UpdateStaffStatusResult>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("Staff member not found."));
        }

        [TestMethod]
        public async Task UpdateStaffStatus_WithInvalidStaffMemberId_ReturnsBadRequest()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_factory);

            var barberToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "Barber", new string[0]);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", barberToken);

            var invalidStaffMemberId = "invalid-guid";

            var updateStatusRequest = new UpdateStaffStatusRequest
            {
                StaffMemberId = invalidStaffMemberId,
                NewStatus = "busy"
            };

            var json = JsonSerializer.Serialize(updateStatusRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/staff/barbers/{invalidStaffMemberId}/status", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<UpdateStaffStatusResult>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("staffMemberId"));
            Assert.AreEqual("Invalid staff member ID format.", result.FieldErrors["staffMemberId"]);
        }

        [TestMethod]
        public async Task UpdateStaffStatus_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_factory);

            var staffMemberId = await CreateStaffMemberAsync();

            var updateStatusRequest = new UpdateStaffStatusRequest
            {
                StaffMemberId = staffMemberId,
                NewStatus = "busy"
            };

            var json = JsonSerializer.Serialize(updateStatusRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/staff/barbers/{staffMemberId}/status", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task UpdateStaffStatus_WithUnauthorizedRole_ReturnsForbidden()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_factory);

            // Create and authenticate a regular user (not Barber)
            var userToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "User", new string[0]);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

            var staffMemberId = await CreateStaffMemberAsync();

            var updateStatusRequest = new UpdateStaffStatusRequest
            {
                StaffMemberId = staffMemberId,
                NewStatus = "busy"
            };

            var json = JsonSerializer.Serialize(updateStatusRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/staff/barbers/{staffMemberId}/status", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // UC-STARTBREAK: Barber starts a break integration tests
        [TestMethod]
        public async Task StartBreak_WithValidRequestAndBarberRole_ReturnsSuccess()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_factory);

            var barberToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "Barber", new string[0]);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", barberToken);

            var staffMemberId = await CreateStaffMemberAsync();

            var startBreakRequest = new StartBreakRequest
            {
                StaffMemberId = staffMemberId,
                DurationMinutes = 15,
                Reason = "Lunch break"
            };

            var json = JsonSerializer.Serialize(startBreakRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/staff/barbers/{staffMemberId}/start-break", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<StartBreakResult>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(staffMemberId, result.StaffMemberId);
            Assert.IsNotNull(result.BreakId);
            Assert.AreEqual("on-break", result.NewStatus);
        }

        [TestMethod]
        public async Task StartBreak_WithInvalidStaffMemberId_ReturnsBadRequest()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_factory);

            var barberToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "Barber", new string[0]);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", barberToken);

            var invalidStaffMemberId = "invalid-guid";

            var startBreakRequest = new StartBreakRequest
            {
                StaffMemberId = invalidStaffMemberId,
                DurationMinutes = 10
            };

            var json = JsonSerializer.Serialize(startBreakRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/staff/barbers/{invalidStaffMemberId}/start-break", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<StartBreakResult>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("staffMemberId"));
            Assert.AreEqual("Invalid staff member ID format.", result.FieldErrors["staffMemberId"]);
        }

        [TestMethod]
        public async Task StartBreak_WithZeroDuration_ReturnsBadRequest()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_factory);

            var barberToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "Barber", new string[0]);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", barberToken);

            var staffMemberId = await CreateStaffMemberAsync();

            var startBreakRequest = new StartBreakRequest
            {
                StaffMemberId = staffMemberId,
                DurationMinutes = 0
            };

            var json = JsonSerializer.Serialize(startBreakRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/staff/barbers/{staffMemberId}/start-break", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<StartBreakResult>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("durationMinutes"));
            Assert.AreEqual("Duration must be greater than 0.", result.FieldErrors["durationMinutes"]);
        }

        [TestMethod]
        public async Task StartBreak_WithNonExistentStaffMember_ReturnsBadRequest()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_factory);

            var barberToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "Barber", new string[0]);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", barberToken);

            var nonExistentStaffMemberId = Guid.NewGuid().ToString();

            var startBreakRequest = new StartBreakRequest
            {
                StaffMemberId = nonExistentStaffMemberId,
                DurationMinutes = 10
            };

            var json = JsonSerializer.Serialize(startBreakRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/staff/barbers/{nonExistentStaffMemberId}/start-break", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<StartBreakResult>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("Staff member not found."));
        }

        [TestMethod]
        public async Task StartBreak_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_factory);

            var staffMemberId = await CreateStaffMemberAsync();

            var startBreakRequest = new StartBreakRequest
            {
                StaffMemberId = staffMemberId,
                DurationMinutes = 10
            };

            var json = JsonSerializer.Serialize(startBreakRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/staff/barbers/{staffMemberId}/start-break", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task StartBreak_WithUnauthorizedRole_ReturnsForbidden()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_factory);

            var userToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "User", new string[0]);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

            var staffMemberId = await CreateStaffMemberAsync();

            var startBreakRequest = new StartBreakRequest
            {
                StaffMemberId = staffMemberId,
                DurationMinutes = 10
            };

            var json = JsonSerializer.Serialize(startBreakRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/staff/barbers/{staffMemberId}/start-break", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [TestMethod]
        public async Task EditBarber_ValidRequest_ReturnsOk()
        {
            // Arrange
            var client = _factory.CreateClient();
            var adminToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "Admin", new[] { Permission.UpdateStaff });
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            // First add a barber
            var locationId = await CreateLocationAsync();
            var addRequest = new AddBarberRequest
            {
                LocationId = locationId,
                FirstName = "Original",
                LastName = "Name",
                Email = "original@example.com",
                PhoneNumber = "+1234567890",
                Username = "originaluser",
                ServiceTypeIds = new List<string> { Guid.NewGuid().ToString() }
            };

            var addResponse = await client.PostAsJsonAsync("/api/staff/barbers", addRequest);
            addResponse.EnsureSuccessStatusCode();
            var addResult = await addResponse.Content.ReadFromJsonAsync<AddBarberResult>();
            var staffId = addResult.BarberId;

            // Now edit the barber
            var editRequest = new
            {
                name = "Updated Name",
                email = "updated@example.com",
                phoneNumber = "+0987654321",
                profilePictureUrl = "https://example.com/profile.jpg",
                role = "Senior Barber"
            };

            // Act
            var response = await client.PutAsJsonAsync($"/api/staff/barbers/{staffId}", editRequest);

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<EditBarberResult>();
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.AreEqual("Updated Name", result.Name);
            Assert.AreEqual("updated@example.com", result.Email);
            Assert.AreEqual("+0987654321", result.PhoneNumber);
            Assert.AreEqual("https://example.com/profile.jpg", result.ProfilePictureUrl);
            Assert.AreEqual("Senior Barber", result.Role);
        }

        [TestMethod]
        public async Task EditBarber_WithoutAdminRole_ReturnsForbidden()
        {
            // Arrange
            var client = _factory.CreateClient();
            var userToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "User", new string[0]); // Non-admin token
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

            var request = new
            {
                name = "Updated Name",
                email = "updated@example.com"
            };

            // Act
            var response = await client.PutAsJsonAsync($"/api/staff/barbers/{Guid.NewGuid()}", request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [TestMethod]
        public async Task EditBarber_InvalidStaffId_ReturnsBadRequest()
        {
            // Arrange
            var client = _factory.CreateClient();
            var adminToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "Admin", new[] { Permission.UpdateStaff });
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var request = new
            {
                name = "Updated Name",
                email = "updated@example.com"
            };

            // Act
            var response = await client.PutAsJsonAsync("/api/staff/barbers/invalid-guid", request);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task EditBarber_NonExistentStaff_ReturnsBadRequest()
        {
            // Arrange
            var client = _factory.CreateClient();
            var adminToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "Admin", new[] { Permission.UpdateStaff });
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var request = new
            {
                name = "Updated Name",
                email = "updated@example.com"
            };

            // Act
            var response = await client.PutAsJsonAsync($"/api/staff/barbers/{Guid.NewGuid()}", request);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Helper methods

        private async Task<string> CreateLocationAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var locationRepository = scope.ServiceProvider.GetRequiredService<ILocationRepository>();

            // Create a test organization ID
            var organizationId = Guid.NewGuid();

            var address = Address.Create(
                "123 Test Street",
                "1",
                "",
                "Test Neighborhood",
                "Test City",
                "Test State",
                "US",
                "12345"
            );

            var testLocation = new Location(
                name: "Test Barbershop",
                slug: "test-barbershop",
                description: "A test barbershop for integration tests",
                organizationId: organizationId,
                address: address,
                contactPhone: "+1234567890",
                contactEmail: "test@barbershop.com",
                openingTime: TimeSpan.FromHours(9),
                closingTime: TimeSpan.FromHours(17),
                maxQueueSize: 50,
                lateClientCapTimeInMinutes: 15,
                createdBy: "testUser"
            );

            await locationRepository.AddAsync(testLocation, CancellationToken.None);
            return testLocation.Id.ToString();
        }

        private async Task<string> CreateStaffMemberAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var staffRepository = scope.ServiceProvider.GetRequiredService<IStaffMemberRepository>();
            var locationRepository = scope.ServiceProvider.GetRequiredService<ILocationRepository>();

            // Create a test location first
            var organizationId = Guid.NewGuid();
            var address = Address.Create(
                "123 Test Street",
                "1",
                "",
                "Test Neighborhood",
                "Test City",
                "Test State",
                "US",
                "12345"
            );

            var testLocation = new Location(
                name: "Test Barbershop",
                slug: "test-barbershop",
                description: "A test barbershop for integration tests",
                organizationId: organizationId,
                address: address,
                contactPhone: "+1234567890",
                contactEmail: "test@barbershop.com",
                openingTime: TimeSpan.FromHours(9),
                closingTime: TimeSpan.FromHours(17),
                maxQueueSize: 50,
                lateClientCapTimeInMinutes: 15,
                createdBy: "testUser"
            );

            await locationRepository.AddAsync(testLocation, CancellationToken.None);

            // Create a test staff member
            var testStaffMember = new StaffMember(
                name: "Test Barber",
                locationId: testLocation.Id,
                email: "test.barber@barbershop.com",
                phoneNumber: "+1234567890",
                profilePictureUrl: null,
                role: "Barber",
                username: "testbarber",
                userId: null,
                createdBy: "testUser"
            );

            await staffRepository.AddAsync(testStaffMember, CancellationToken.None);
            return testStaffMember.Id.ToString();
        }
    }
}
