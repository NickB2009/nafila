using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using GrandeTech.QueueHub.API.Application.ServiceProviders.Requests;
using GrandeTech.QueueHub.API.Application.ServiceProviders.Results;

namespace GrandeTech.QueueHub.Tests.Integration.Controllers
{
    [TestClass]
    public class ServiceProvidersControllerIntegrationTests
    {
        private WebApplicationFactory<Program>? _factory;
        private HttpClient? _client;

        [TestInitialize]
        public void Setup()
        {
            _factory = new WebApplicationFactory<Program>();
            _client = _factory.CreateClient();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _client?.Dispose();
            _factory?.Dispose();
        }

        [TestMethod]
        public async Task CreateServiceProvider_WithValidData_ReturnsSuccessWithLocationSlug()
        {
            // Arrange
            Assert.IsNotNull(_client);

            var adminToken = await CreateAndAuthenticateUserAsync("Admin");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var createRequest = new CreateServiceProviderRequest
            {
                BusinessName = "Barbearia do João",
                ContactEmail = "contato@barbeariajao.com.br",
                ContactPhone = "+551133334444",
                Address = new ServiceProviderAddressRequest
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
            var result = JsonSerializer.Deserialize<CreateServiceProviderResult>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.ServiceProviderId);
            Assert.IsNotNull(result.LocationSlug);
            Assert.AreEqual("barbearia-do-joao-sp", result.LocationSlug);
            Assert.AreEqual("Barbearia do João", result.BusinessName);
        }

        [TestMethod]
        public async Task CreateServiceProvider_WithInvalidData_ReturnsBadRequestWithFieldErrors()
        {
            // Arrange
            Assert.IsNotNull(_client);

            var adminToken = await CreateAndAuthenticateUserAsync("Admin");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var createRequest = new CreateServiceProviderRequest
            {
                BusinessName = "", // Invalid - empty
                ContactEmail = "invalid-email", // Invalid format
                ContactPhone = "", // Invalid - empty
                Address = new ServiceProviderAddressRequest
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
            var result = JsonSerializer.Deserialize<CreateServiceProviderResult>(responseContent, new JsonSerializerOptions
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
        public async Task CreateServiceProvider_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Arrange
            Assert.IsNotNull(_client);

            var createRequest = new CreateServiceProviderRequest
            {
                BusinessName = "Test Barbershop",
                ContactEmail = "test@barbershop.com",
                ContactPhone = "+5511999999999",
                Address = new ServiceProviderAddressRequest
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
        }        private Task<string> CreateAndAuthenticateUserAsync(string role)
        {
            // Mock implementation for now - will be replaced with real auth
            return Task.FromResult("mock-admin-token");
        }
    }
}
