using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Application.Auth;
using GrandeTech.QueueHub.API.Application.Staff;
using GrandeTech.QueueHub.API.Domain.Users;
using GrandeTech.QueueHub.API.Domain.Staff;
using GrandeTech.QueueHub.API.Domain.ServicesProviders;
using GrandeTech.QueueHub.API.Domain.AuditLogs;
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
                            d.ServiceType == typeof(IUserRepository) ||
                            d.ServiceType == typeof(IStaffMemberRepository) ||
                            d.ServiceType == typeof(IServicesProviderRepository) ||
                            d.ServiceType == typeof(IAuditLogRepository)).ToList();
                        
                        foreach (var descriptor in descriptors)
                        {
                            services.Remove(descriptor);
                        }                        // Use Bogus repositories for testing
                        services.AddScoped<IUserRepository, BogusUserRepository>();
                        services.AddScoped<IStaffMemberRepository, BogusStaffMemberRepository>();
                        services.AddScoped<IServicesProviderRepository, BogusServicesProviderRepository>();
                        services.AddScoped<IAuditLogRepository, BogusAuditLogRepository>();
                        
                        // Register AddBarberService
                        services.AddScoped<AddBarberService>();
                        
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
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            // Create a service provider first (required for barber creation)
            var ServicesProviderId = await CreateServicesProviderAsync();

            var addBarberRequest = new AddBarberRequest
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@barbershop.com",
                PhoneNumber = "+5511999999999",
                Username = "johndoe",
                ServicesProviderId = ServicesProviderId,
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
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);

            // Create a service provider first
            var ServicesProviderId = await CreateServicesProviderAsync();

            var addBarberRequest = new AddBarberRequest
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@barbershop.com",
                PhoneNumber = "+5511888888888",
                Username = "janesmith",
                ServicesProviderId = ServicesProviderId,
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
                Email = "bob.johnson@barbershop.com",
                PhoneNumber = "+5511777777777",
                Username = "bobjohnson",
                ServicesProviderId = Guid.NewGuid().ToString(),
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
                ServicesProviderId = Guid.NewGuid().ToString(),
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
            {
                FirstName = "", // Invalid - empty
                LastName = "", // Invalid - empty
                Email = "invalid-email", // Invalid format
                PhoneNumber = "", // Invalid - empty
                Username = "", // Invalid - empty
                ServicesProviderId = "invalid-guid", // Invalid format
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
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var ServicesProviderId = await CreateServicesProviderAsync();

            // Create a barber first with a specific email
            using var scope = _factory.Services.CreateScope();
            var staffRepo = scope.ServiceProvider.GetRequiredService<IStaffMemberRepository>();
            
            var existingBarber = new StaffMember(
                "Existing Barber",
                Guid.Parse(ServicesProviderId),
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
                ServicesProviderId = ServicesProviderId,
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
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var ServicesProviderId = await CreateServicesProviderAsync();

            // Create a barber first with a specific username
            using var scope = _factory.Services.CreateScope();
            var staffRepo = scope.ServiceProvider.GetRequiredService<IStaffMemberRepository>();
            
            var existingBarber = new StaffMember(
                "Existing Barber",
                Guid.Parse(ServicesProviderId),
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
                ServicesProviderId = ServicesProviderId,
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
        }

        [TestMethod]
        public async Task AddBarber_WithNonExistentServicesProvider_ReturnsBadRequestWithError()
        {
            // Arrange
            Assert.IsNotNull(_client);

            var adminToken = await CreateAndAuthenticateUserAsync("Admin");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var addBarberRequest = new AddBarberRequest
            {
                FirstName = "Test",
                LastName = "Barber",
                Email = "test@barbershop.com",
                PhoneNumber = "+5511333333333",
                Username = "testbarber",
                ServicesProviderId = Guid.NewGuid().ToString(), // Non-existent service provider
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
            Assert.IsTrue(result.Errors.Contains("Service provider not found."));
        }

        [TestMethod]
        public async Task AddBarber_WithDeactivateOnCreation_ReturnsInactiveStatus()
        {
            // Arrange
            Assert.IsNotNull(_client);

            var adminToken = await CreateAndAuthenticateUserAsync("Admin");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var ServicesProviderId = await CreateServicesProviderAsync();

            var addBarberRequest = new AddBarberRequest
            {
                FirstName = "Inactive",
                LastName = "Barber",
                Email = "inactive@barbershop.com",
                PhoneNumber = "+5511222222222",
                Username = "inactivebarber",
                ServicesProviderId = ServicesProviderId,
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
            Assert.AreEqual(HttpStatusCode.OK, loginResponse.StatusCode);

            var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();
            var loginResult = JsonSerializer.Deserialize<LoginResult>(loginResponseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(loginResult);
            Assert.IsTrue(loginResult.Success);
            Assert.IsNotNull(loginResult.Token);

            return loginResult.Token;
        }

        private async Task<string> CreateServicesProviderAsync()
        {
            Assert.IsNotNull(_factory);

            using var scope = _factory.Services.CreateScope();
            var ServicesProviderRepo = scope.ServiceProvider.GetRequiredService<IServicesProviderRepository>();            // Create a service provider for testing
            var ServicesProvider = new GrandeTech.QueueHub.API.Domain.ServicesProviders.ServicesProvider(
                "Test Barbershop",
                "test-barbershop",
                "Test Description",
                Guid.NewGuid(),
                GrandeTech.QueueHub.API.Domain.Common.ValueObjects.Address.Create(
                    "Test Street", "123", "", "Test Neighborhood", "Test City", "Test State", "Test Country", "12345"),
                "+5511123456789",
                "test@barbershop.com",
                TimeSpan.FromHours(8),
                TimeSpan.FromHours(18),
                50,
                15,
                "testuser"
            );

            await ServicesProviderRepo.AddAsync(ServicesProvider, CancellationToken.None);

            return ServicesProvider.Id.ToString();
        }
    }
}
