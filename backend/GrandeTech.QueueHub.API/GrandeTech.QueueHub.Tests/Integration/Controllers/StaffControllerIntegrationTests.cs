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
using GrandeTech.QueueHub.API.Application.Auth;
using GrandeTech.QueueHub.API.Application.Staff;
using GrandeTech.QueueHub.API.Domain.Users;
using GrandeTech.QueueHub.API.Domain.Staff;
using GrandeTech.QueueHub.API.Domain.Locations;
using GrandeTech.QueueHub.API.Domain.AuditLogs;
using GrandeTech.QueueHub.API.Domain.Common.ValueObjects;
using GrandeTech.QueueHub.API.Infrastructure.Repositories.Bogus;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrandeTech.QueueHub.Tests.Integration.Controllers
{
    [TestClass]
    public class StaffControllerIntegrationTests
    {
        private WebApplicationFactory<Program>? _factory;
        private HttpClient? _client;        [TestInitialize]
        public void Setup()
        {
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Testing");
                    builder.ConfigureAppConfiguration((context, config) =>
                    {
                        config.AddInMemoryCollection(new Dictionary<string, string?>
                        {
                            ["Jwt:Key"] = "your-super-secret-key-with-at-least-32-characters-for-testing",
                            ["Jwt:Issuer"] = "GrandeTech.QueueHub.API.Test",
                            ["Jwt:Audience"] = "GrandeTech.QueueHub.API.Test"
                        });
                    });
                    builder.ConfigureServices(services =>
                    {
                        // Remove existing repositories if any
                        var descriptors = services.Where(d => 
                            d.ServiceType == typeof(IUserRepository) ||                            d.ServiceType == typeof(IStaffMemberRepository) ||
                            d.ServiceType == typeof(ILocationRepository) ||
                            d.ServiceType == typeof(IAuditLogRepository)).ToList();
                        
                        foreach (var descriptor in descriptors)
                        {
                            services.Remove(descriptor);
                        }                        // Use Bogus repositories for testing
                        services.AddScoped<IUserRepository, BogusUserRepository>();
                        services.AddScoped<IStaffMemberRepository, BogusStaffMemberRepository>();
                        services.AddScoped<ILocationRepository, BogusLocationRepository>();
                        services.AddScoped<IAuditLogRepository, BogusAuditLogRepository>();
                        
                        // Register AddBarberService
                        services.AddScoped<AddBarberService>();
                        
                        // Register UpdateStaffStatusService
                        services.AddScoped<UpdateStaffStatusService>();
                        
                        // Register AuthService for JWT token generation
                        services.AddScoped<AuthService>();
                    });
                });
            _client = _factory.CreateClient();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _client?.Dispose();
            _factory?.Dispose();
        }

        [TestMethod]
        public async Task AddBarber_WithValidDataAndAdminRole_ReturnsSuccessResult()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_factory);

            // First, create and authenticate an admin user
            var adminToken = await CreateAndAuthenticateUserAsync("Admin");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);            // Create a location first (required for barber creation)
            var locationId = await CreateLocationAsync();

            var addBarberRequest = new AddBarberRequest
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@barbershop.com",
                PhoneNumber = "+5511999999999",                Username = "johndoe",
                LocationId = locationId,
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
            var ownerToken = await CreateAndAuthenticateUserAsync("Owner");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);            // Create a location first
            var locationId = await CreateLocationAsync();

            var addBarberRequest = new AddBarberRequest
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@barbershop.com",
                PhoneNumber = "+5511888888888",
                Username = "janesmith",
                LocationId = locationId,
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
            var userToken = await CreateAndAuthenticateUserAsync("User");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

            var addBarberRequest = new AddBarberRequest
            {
                FirstName = "Bob",
                LastName = "Johnson",
                Email = "bob.johnson@barbershop.com",                PhoneNumber = "+5511777777777",
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
                Email = "test.user@barbershop.com",                PhoneNumber = "+5511666666666",
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

            var adminToken = await CreateAndAuthenticateUserAsync("Admin");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            // Invalid request with missing required fields
            var addBarberRequest = new AddBarberRequest
            {                FirstName = "", // Invalid - empty
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

            var adminToken = await CreateAndAuthenticateUserAsync("Admin");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);            var locationId = await CreateLocationAsync();

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
                LastName = "Barber",                Email = "duplicate@barbershop.com", // Same email as existing barber
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
            Assert.IsTrue(result.FieldErrors.ContainsKey("Email"));
            Assert.AreEqual("A barber with this email already exists.", result.FieldErrors["Email"]);
        }

        [TestMethod]
        public async Task AddBarber_WithDuplicateUsername_ReturnsBadRequestWithFieldError()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_factory);

            var adminToken = await CreateAndAuthenticateUserAsync("Admin");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);            var locationId = await CreateLocationAsync();

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
                LastName = "Barber",                Email = "new@barbershop.com",
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
            Assert.IsTrue(result.FieldErrors.ContainsKey("Username"));
            Assert.AreEqual("A barber with this username already exists.", result.FieldErrors["Username"]);
        }        [TestMethod]
        public async Task AddBarber_WithNonExistentLocation_ReturnsBadRequestWithError()
        {
            // Arrange
            Assert.IsNotNull(_client);

            var adminToken = await CreateAndAuthenticateUserAsync("Admin");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var addBarberRequest = new AddBarberRequest
            {
                FirstName = "Test",
                LastName = "Barber",
                Email = "test@barbershop.com",                PhoneNumber = "+5511333333333",
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
            Assert.IsNotNull(_client);

            var adminToken = await CreateAndAuthenticateUserAsync("Admin");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);            var locationId = await CreateLocationAsync();

            var addBarberRequest = new AddBarberRequest
            {
                FirstName = "Inactive",
                LastName = "Barber",
                Email = "inactive@barbershop.com",
                PhoneNumber = "+5511222222222",
                Username = "inactivebarber",
                LocationId = locationId,
                ServiceTypeIds = new List<string> { Guid.NewGuid().ToString() },
                DeactivateOnCreation = true // This should make the barber inactive
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
            Assert.AreEqual("Inactive", result.Status);
        }

        // UC-STAFFSTATUS: Barber changes status integration tests
        [TestMethod]
        public async Task UpdateStaffStatus_WithValidRequestAndBarberRole_ReturnsSuccess()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_factory);

            var barberToken = await CreateAndAuthenticateUserAsync("Barber");
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

            var barberToken = await CreateAndAuthenticateUserAsync("Barber");
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

            var barberToken = await CreateAndAuthenticateUserAsync("Barber");
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
            Assert.IsTrue(result.FieldErrors.ContainsKey("NewStatus"));
            Assert.AreEqual("Invalid status. Must be one of: available, busy, away, offline", result.FieldErrors["NewStatus"]);
        }

        [TestMethod]
        public async Task UpdateStaffStatus_WithEmptyStatus_ReturnsBadRequest()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_factory);

            var barberToken = await CreateAndAuthenticateUserAsync("Barber");
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
            Assert.IsTrue(result.FieldErrors.ContainsKey("NewStatus"));
            Assert.AreEqual("Status is required.", result.FieldErrors["NewStatus"]);
        }

        [TestMethod]
        public async Task UpdateStaffStatus_WithNonExistentStaffMember_ReturnsBadRequest()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_factory);

            var barberToken = await CreateAndAuthenticateUserAsync("Barber");
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

            var barberToken = await CreateAndAuthenticateUserAsync("Barber");
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
            Assert.IsTrue(result.FieldErrors.ContainsKey("StaffMemberId"));
            Assert.AreEqual("Invalid staff member ID format.", result.FieldErrors["StaffMemberId"]);
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
            var userToken = await CreateAndAuthenticateUserAsync("User");
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

            var barberToken = await CreateAndAuthenticateUserAsync("Barber");
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

            var barberToken = await CreateAndAuthenticateUserAsync("Barber");
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
            Assert.IsTrue(result.FieldErrors.ContainsKey("StaffMemberId"));
            Assert.AreEqual("Invalid staff member ID format.", result.FieldErrors["StaffMemberId"]);
        }

        [TestMethod]
        public async Task StartBreak_WithZeroDuration_ReturnsBadRequest()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_factory);

            var barberToken = await CreateAndAuthenticateUserAsync("Barber");
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
            Assert.IsTrue(result.FieldErrors.ContainsKey("DurationMinutes"));
            Assert.AreEqual("Duration must be greater than 0.", result.FieldErrors["DurationMinutes"]);
        }

        [TestMethod]
        public async Task StartBreak_WithNonExistentStaffMember_ReturnsBadRequest()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_factory);

            var barberToken = await CreateAndAuthenticateUserAsync("Barber");
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

            var userToken = await CreateAndAuthenticateUserAsync("User");
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

        // Helper methods

        private async Task<string> CreateAndAuthenticateUserAsync(string role)
        {
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_factory);

            using var scope = _factory.Services.CreateScope();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            // Create a user with the specified role
            var username = $"testuser_{role.ToLower()}_{Guid.NewGuid():N}";
            var email = $"{username}@test.com";
            var password = "testpassword123";

            var user = new User(username, email, BCrypt.Net.BCrypt.HashPassword(password), role);
            await userRepository.AddAsync(user, CancellationToken.None);

            // Login to get the token
            var loginRequest = new LoginRequest
            {
                Username = username,
                Password = password
            };

            var loginJson = JsonSerializer.Serialize(loginRequest);
            var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");

            var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);
            var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();
            var loginResult = JsonSerializer.Deserialize<LoginResult>(loginResponseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(loginResult);

            // Handle two-factor authentication for admin users
            if (loginResult.RequiresTwoFactor)
            {
                Assert.IsNotNull(loginResult.TwoFactorToken);
                
                // Verify 2FA
                var verifyRequest = new VerifyTwoFactorRequest
                {
                    Username = username,
                    TwoFactorCode = "123456", // In a real implementation, this would be validated
                    TwoFactorToken = loginResult.TwoFactorToken
                };

                var verifyJson = JsonSerializer.Serialize(verifyRequest);
                var verifyContent = new StringContent(verifyJson, Encoding.UTF8, "application/json");

                var verifyResponse = await _client.PostAsync("/api/auth/verify-2fa", verifyContent);
                Assert.AreEqual(HttpStatusCode.OK, verifyResponse.StatusCode);

                var verifyResponseContent = await verifyResponse.Content.ReadAsStringAsync();
                var verifyResult = JsonSerializer.Deserialize<LoginResult>(verifyResponseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                Assert.IsNotNull(verifyResult);
                Assert.IsTrue(verifyResult.Success);
                Assert.IsNotNull(verifyResult.Token);
                return verifyResult.Token;
            }
            else
            {
                // Regular user login
                Assert.AreEqual(HttpStatusCode.OK, loginResponse.StatusCode);
                Assert.IsTrue(loginResult.Success);
                Assert.IsNotNull(loginResult.Token);
                return loginResult.Token;
            }
        }        private async Task<string> CreateLocationAsync()
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
