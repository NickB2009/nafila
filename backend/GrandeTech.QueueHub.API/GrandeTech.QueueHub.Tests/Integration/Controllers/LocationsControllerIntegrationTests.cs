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
using GrandeTech.QueueHub.API.Application.Locations;
using GrandeTech.QueueHub.API.Application.Locations.Requests;
using GrandeTech.QueueHub.API.Application.Locations.Results;
using GrandeTech.QueueHub.API.Domain.Users;
using GrandeTech.QueueHub.API.Infrastructure.Repositories.Bogus;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrandeTech.QueueHub.Tests.Integration.Controllers
{
    [TestClass]
    public class LocationsControllerIntegrationTests
    {
        private WebApplicationFactory<Program>? _factory;
        private HttpClient? _client;

        [TestInitialize]
        public void Setup()
        {
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureAppConfiguration((context, config) =>
                    {
                        config.AddInMemoryCollection(new[]
                        {
                            new KeyValuePair<string, string?>("ConnectionStrings:DefaultConnection", "InMemory"),
                            new KeyValuePair<string, string?>("Jwt:Key", "ThisIsAVeryLongSecretKeyThatShouldBeAtLeast256BitsLong12345678901234567890"),
                            new KeyValuePair<string, string?>("Jwt:Issuer", "TestIssuer"),
                            new KeyValuePair<string, string?>("Jwt:Audience", "TestAudience")
                        });
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
        public async Task CreateLocation_WithValidData_ReturnsSuccessWithLocationSlug()
        {
            // Arrange
            Assert.IsNotNull(_client);

            var adminToken = await CreateAndAuthenticateUserAsync("Admin");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var createRequest = new CreateLocationRequest
            {
                BusinessName = "Barbearia do João",
                ContactEmail = "contato@barbeariajao.com.br",
                ContactPhone = "+551133334444",
                Address = new LocationAddressRequest
                {
                    Street = "Rua das Flores, 123",
                    City = "São Paulo",
                    State = "SP",
                    PostalCode = "01234-567",
                    Country = "Brasil"
                },
                BusinessHours = new Dictionary<string, string>
                {
                    { "Monday", "08:00-18:00" },
                    { "Tuesday", "08:00-18:00" },
                    { "Wednesday", "08:00-18:00" },
                    { "Thursday", "08:00-18:00" },
                    { "Friday", "08:00-18:00" },
                    { "Saturday", "08:00-16:00" }
                },
                MaxQueueCapacity = 20
            };

            var json = JsonSerializer.Serialize(createRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/locations", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<CreateLocationResult>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.LocationId);
            Assert.IsNotNull(result.LocationSlug);
            Assert.AreEqual("barbearia-do-joao-sp", result.LocationSlug);
            Assert.AreEqual("Barbearia do João", result.BusinessName);
        }

        [TestMethod]
        public async Task CreateLocation_WithInvalidData_ReturnsBadRequestWithFieldErrors()
        {
            // Arrange
            Assert.IsNotNull(_client);

            var adminToken = await CreateAndAuthenticateUserAsync("Admin");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var createRequest = new CreateLocationRequest
            {
                BusinessName = "",
                ContactEmail = "invalid-email",
                ContactPhone = "",
                Address = new LocationAddressRequest
                {
                    Street = "",
                    City = "",
                    State = "",
                    PostalCode = "",
                    Country = ""
                },
                MaxQueueCapacity = -1
            };

            var json = JsonSerializer.Serialize(createRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/locations", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<CreateLocationResult>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.Count > 0);
            Assert.IsTrue(result.FieldErrors.ContainsKey("BusinessName"));
            Assert.IsTrue(result.FieldErrors.ContainsKey("ContactEmail"));
        }

        [TestMethod]
        public async Task CreateLocation_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Arrange
            Assert.IsNotNull(_client);

            var createRequest = new CreateLocationRequest
            {
                BusinessName = "Test Barbershop",
                ContactEmail = "test@barbershop.com",
                ContactPhone = "+5511999999999",
                Address = new LocationAddressRequest
                {
                    Street = "Test Street, 123",
                    City = "São Paulo",
                    State = "SP",
                    PostalCode = "01234-567",
                    Country = "Brasil"
                }
            };

            var json = JsonSerializer.Serialize(createRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/locations", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

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

            return loginResult?.Token ?? throw new InvalidOperationException("Failed to get authentication token");
        }
    }
}
