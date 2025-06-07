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
using GrandeTech.QueueHub.API.Application.ServicesProviders;
using GrandeTech.QueueHub.API.Application.ServicesProviders.Requests;
using GrandeTech.QueueHub.API.Application.ServicesProviders.Results;
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
    public class ServicesProvidersControllerIntegrationTests
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
                            d.ServiceType == typeof(IUserRepository)).ToList();
                        
                        foreach (var descriptor in descriptors)
                        {
                            services.Remove(descriptor);
                        }

                        // Use Bogus repositories for testing
                        services.AddScoped<IUserRepository, BogusUserRepository>();
                        
                        // Register CreateServicesProviderService
                        services.AddScoped<CreateServicesProviderService>();
                        
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
        public async Task CreateServicesProvider_WithValidData_ReturnsSuccessWithLocationSlug()
        {
            // Arrange
            Assert.IsNotNull(_client);

            var adminToken = await CreateAndAuthenticateUserAsync("Admin");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var createRequest = new CreateServicesProviderRequest
            {
                BusinessName = "Barbearia do João",
                ContactEmail = "contato@barbeariajao.com.br",
                ContactPhone = "+551133334444",
                Address = new ServicesProviderAddressRequest
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
            var response = await _client.PostAsync("/api/service-providers", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<CreateServicesProviderResult>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.ServicesProviderId);
            Assert.IsNotNull(result.LocationSlug);
            Assert.AreEqual("barbearia-do-joao-sp", result.LocationSlug);
            Assert.AreEqual("Barbearia do João", result.BusinessName);
        }

        [TestMethod]
        public async Task CreateServicesProvider_WithInvalidData_ReturnsBadRequestWithFieldErrors()
        {
            // Arrange
            Assert.IsNotNull(_client);

            var adminToken = await CreateAndAuthenticateUserAsync("Admin");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var createRequest = new CreateServicesProviderRequest
            {
                BusinessName = "", // Invalid - empty
                ContactEmail = "invalid-email", // Invalid format
                ContactPhone = "", // Invalid - empty
                Address = new ServicesProviderAddressRequest
                {
                    Street = "",
                    City = "",
                    State = "",
                    PostalCode = "",
                    Country = ""
                },
                MaxQueueCapacity = -1 // Invalid - negative
            };

            var json = JsonSerializer.Serialize(createRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/service-providers", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<CreateServicesProviderResult>(responseContent, new JsonSerializerOptions
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
        public async Task CreateServicesProvider_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Arrange
            Assert.IsNotNull(_client);

            var createRequest = new CreateServicesProviderRequest
            {
                BusinessName = "Test Barbershop",
                ContactEmail = "test@barbershop.com",
                ContactPhone = "+5511999999999",
                Address = new ServicesProviderAddressRequest
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
            var response = await _client.PostAsync("/api/service-providers", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }        private async Task<string> CreateAndAuthenticateUserAsync(string role)
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
    }
}
