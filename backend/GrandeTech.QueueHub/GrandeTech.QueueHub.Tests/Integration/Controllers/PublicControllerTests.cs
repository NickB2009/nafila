using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Grande.Fila.API.Application.Public;
using Grande.Fila.API.Tests.Integration;
using Grande.Fila.API.Infrastructure.Repositories.Bogus;
using Grande.Fila.API.Domain.Locations;
using Grande.Fila.API.Domain.Organizations;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.ServicesOffered;
using Grande.Fila.API.Domain.Common.ValueObjects;
using System.Linq;
using System.Threading;

namespace Grande.Fila.API.Tests.Integration.Controllers
{
    [TestClass]
    [TestCategory("Integration")]
    public class PublicControllerTests
    {
        private WebApplicationFactory<Program> _factory;
        private HttpClient _client;
        private BogusLocationRepository _locationRepository;
        private BogusOrganizationRepository _organizationRepository;
        private BogusQueueRepository _queueRepository;
        private BogusServiceTypeRepository _serviceRepository;

        [TestInitialize]
        public void Setup()
        {
            _locationRepository = new BogusLocationRepository();
            _organizationRepository = new BogusOrganizationRepository();
            _queueRepository = new BogusQueueRepository();
            _serviceRepository = new BogusServiceTypeRepository();
            
            // Add some test data
            SetupTestData();
            
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
                            ["Jwt:Audience"] = "Grande.Fila.API.Test"
                        });
                    });
                    builder.ConfigureServices(services =>
                    {
                        var descriptors = services.Where(d => 
                            d.ServiceType == typeof(ILocationRepository) ||
                            d.ServiceType == typeof(IOrganizationRepository) ||
                            d.ServiceType == typeof(IQueueRepository) ||
                            d.ServiceType == typeof(IServicesOfferedRepository)).ToList();
                        foreach (var descriptor in descriptors)
                        {
                            services.Remove(descriptor);
                        }
                        services.AddSingleton<ILocationRepository>(_locationRepository);
                        services.AddSingleton<IOrganizationRepository>(_organizationRepository);
                        services.AddSingleton<IQueueRepository>(_queueRepository);
                        services.AddSingleton<IServicesOfferedRepository>(_serviceRepository);
                    });
                });
            
            _client = _factory.CreateClient();
        }

        private void SetupTestData()
        {
            // Create test organization
            var organization = new Organization(
                "Test Barbershop Chain",
                "test-chain",
                "A test barbershop chain",
                "test@example.com",
                "+1234567890",
                "https://testchain.com",
                null,
                Guid.NewGuid(),
                "system");

            _organizationRepository.AddAsync(organization, CancellationToken.None).Wait();

            // Create test location
            var address = Address.Create("123 Test St", "100", "", "Downtown", "Test City", "TC", "Test Country", "12345");
            var location = new Location(
                "Test Barbershop",
                "test-barbershop",
                "A test barbershop",
                organization.Id,
                address,
                "+1234567890",
                "test@barbershop.com",
                TimeSpan.FromHours(8),
                TimeSpan.FromHours(18),
                50,
                15,
                "system");

            _locationRepository.AddAsync(location, CancellationToken.None).Wait();

            // Create test queue with some entries
            var queue = new Queue(location.Id, 50, 15, "system");
            queue.AddCustomerToQueue(Guid.NewGuid(), "Test Customer 1");
            queue.AddCustomerToQueue(Guid.NewGuid(), "Test Customer 2");

            _queueRepository.AddAsync(queue, CancellationToken.None).Wait();

            // Create test services
            var service1 = new ServiceOffered("Corte Masculino", "Traditional haircut", location.Id, 30, 25.00m, null, "system");
            var service2 = new ServiceOffered("Barba", "Beard trim", location.Id, 15, 15.00m, null, "system");

            _serviceRepository.AddAsync(service1, CancellationToken.None).Wait();
            _serviceRepository.AddAsync(service2, CancellationToken.None).Wait();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _client?.Dispose();
            _factory?.Dispose();
        }

        [TestMethod]
        public async Task GetSalons_ShouldReturnPublicSalons()
        {
            // Act
            var response = await _client.GetAsync("/api/Public/salons");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.IsNotNull(content);
            
            var salons = JsonSerializer.Deserialize<List<PublicSalonDto>>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            Assert.IsNotNull(salons);
            Assert.IsTrue(salons.Count >= 1); // Should return at least one test salon
        }

        [TestMethod]
        public async Task GetSalonById_WithValidId_ShouldReturnSalon()
        {
            // First get all salons to get a valid ID
            var allSalonsResponse = await _client.GetAsync("/api/Public/salons");
            Assert.AreEqual(HttpStatusCode.OK, allSalonsResponse.StatusCode);
            
            var allSalonsContent = await allSalonsResponse.Content.ReadAsStringAsync();
            var salons = JsonSerializer.Deserialize<List<PublicSalonDto>>(allSalonsContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            if (salons?.Count > 0)
            {
                var salonId = salons[0].Id;
                
                // Act
                var response = await _client.GetAsync($"/api/Public/salons/{salonId}");
                
                // Assert
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                
                var content = await response.Content.ReadAsStringAsync();
                var salon = JsonSerializer.Deserialize<PublicSalonDto>(content, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                
                Assert.IsNotNull(salon);
                Assert.AreEqual(salonId, salon.Id);
                Assert.IsNotNull(salon.Name);
                Assert.IsNotNull(salon.Address);
                Assert.IsNotNull(salon.BusinessHours);
                Assert.IsNotNull(salon.Services);
            }
            else
            {
                Assert.Inconclusive("No salons available for testing");
            }
        }

        [TestMethod]
        public async Task GetSalonById_WithInvalidId_ShouldReturnBadRequest()
        {
            // Act
            var response = await _client.GetAsync("/api/Public/salons/invalid-guid");

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task GetSalonById_WithNonExistentId_ShouldReturnNotFound()
        {
            // Act
            var nonExistentId = Guid.NewGuid().ToString();
            var response = await _client.GetAsync($"/api/Public/salons/{nonExistentId}");

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task GetQueueStatus_WithValidId_ShouldReturnQueueStatus()
        {
            // First get all salons to get a valid ID
            var allSalonsResponse = await _client.GetAsync("/api/Public/salons");
            Assert.AreEqual(HttpStatusCode.OK, allSalonsResponse.StatusCode);
            
            var allSalonsContent = await allSalonsResponse.Content.ReadAsStringAsync();
            var salons = JsonSerializer.Deserialize<List<PublicSalonDto>>(allSalonsContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            if (salons?.Count > 0)
            {
                var salonId = salons[0].Id;
                
                // Act
                var response = await _client.GetAsync($"/api/Public/queue-status/{salonId}");
                
                // Assert
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                
                var content = await response.Content.ReadAsStringAsync();
                var queueStatus = JsonSerializer.Deserialize<PublicQueueStatusDto>(content, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                
                Assert.IsNotNull(queueStatus);
                Assert.AreEqual(salonId, queueStatus.SalonId);
                Assert.IsNotNull(queueStatus.SalonName);
                Assert.IsTrue(queueStatus.QueueLength >= 0);
                Assert.IsTrue(queueStatus.EstimatedWaitTimeMinutes >= 0);
                Assert.IsNotNull(queueStatus.AvailableServices);
            }
            else
            {
                Assert.Inconclusive("No salons available for testing");
            }
        }

        [TestMethod]
        public async Task GetQueueStatus_WithInvalidId_ShouldReturnBadRequest()
        {
            // Act
            var response = await _client.GetAsync("/api/Public/queue-status/invalid-guid");

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task GetQueueStatus_WithNonExistentId_ShouldReturnNotFound()
        {
            // Act
            var nonExistentId = Guid.NewGuid().ToString();
            var response = await _client.GetAsync($"/api/Public/queue-status/{nonExistentId}");

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task PublicEndpoints_ShouldAllowCorsOrigins()
        {
            // Act - test CORS preflight request
            var request = new HttpRequestMessage(HttpMethod.Options, "/api/Public/salons");
            request.Headers.Add("Origin", "http://localhost:3000");
            request.Headers.Add("Access-Control-Request-Method", "GET");
            request.Headers.Add("Access-Control-Request-Headers", "Content-Type");
            
            var response = await _client.SendAsync(request);

            // Assert
            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent);
            
            // Check CORS headers are present
            Assert.IsTrue(response.Headers.Contains("Access-Control-Allow-Origin") || 
                         response.Headers.Contains("access-control-allow-origin"));
        }

        [TestMethod]
        public async Task PublicEndpoints_ShouldNotRequireAuthentication()
        {
            // Act - test all public endpoints without any authentication headers
            var responses = new[]
            {
                await _client.GetAsync("/api/Public/salons"),
                // We'll skip specific ID tests since we need actual data
            };

            // Assert - all should be accessible (either OK or have valid responses)
            foreach (var response in responses)
            {
                Assert.AreNotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
                Assert.AreNotEqual(HttpStatusCode.Forbidden, response.StatusCode);
            }
        }
    }
}