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
    public class PublicControllerComprehensiveTests
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
                            ["ConnectionStrings:DefaultConnection"] = "Data Source=:memory:",
                            ["UseInMemoryDatabase"] = "true"
                        });
                    });
                });

            _client = _factory.CreateClient();
            
            using var scope = _factory.Services.CreateScope();
            _locationRepository = scope.ServiceProvider.GetRequiredService<BogusLocationRepository>();
            _organizationRepository = scope.ServiceProvider.GetRequiredService<BogusOrganizationRepository>();
            _queueRepository = scope.ServiceProvider.GetRequiredService<BogusQueueRepository>();
            _serviceRepository = scope.ServiceProvider.GetRequiredService<BogusServiceTypeRepository>();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _client?.Dispose();
            _factory?.Dispose();
        }

        [TestMethod]
        public async Task PublicController_CompleteAnonymousUserFlow_ShouldWorkEndToEnd()
        {
            // Arrange - Create test data
            var organization = await CreateTestOrganization();
            var location = await CreateTestLocation(organization.Id);
            var queue = await CreateTestQueue(location.Id);
            var service = await CreateTestService(location.Id);

            // Act & Assert - Step 1: Get all salons
            var salonsResponse = await _client.GetAsync("/api/Public/salons");
            Assert.AreEqual(HttpStatusCode.OK, salonsResponse.StatusCode);
            
            var salonsContent = await salonsResponse.Content.ReadAsStringAsync();
            var salons = JsonSerializer.Deserialize<PublicSalonDto[]>(salonsContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            Assert.IsNotNull(salons);
            Assert.IsTrue(salons.Length > 0);

            // Act & Assert - Step 2: Get specific salon details
            var salonDetailResponse = await _client.GetAsync($"/api/Public/salons/{location.Id}");
            Assert.AreEqual(HttpStatusCode.OK, salonDetailResponse.StatusCode);
            
            var salonDetailContent = await salonDetailResponse.Content.ReadAsStringAsync();
            var salonDetail = JsonSerializer.Deserialize<PublicSalonDto>(salonDetailContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            Assert.IsNotNull(salonDetail);
            Assert.AreEqual(location.Id.ToString(), salonDetail.Id);

            // Act & Assert - Step 3: Get queue status
            var queueStatusResponse = await _client.GetAsync($"/api/Public/queue-status/{location.Id}");
            Assert.AreEqual(HttpStatusCode.OK, queueStatusResponse.StatusCode);
            
            var queueStatusContent = await queueStatusResponse.Content.ReadAsStringAsync();
            var queueStatus = JsonSerializer.Deserialize<PublicQueueStatusDto>(queueStatusContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            Assert.IsNotNull(queueStatus);

            // Act & Assert - Step 4: Join queue anonymously
            var joinRequest = new
            {
                salonId = location.Id.ToString(),
                name = "Test Customer",
                email = "test@example.com",
                serviceRequested = "Haircut",
                anonymousUserId = Guid.NewGuid().ToString(),
                emailNotifications = true,
                browserNotifications = false
            };

            var joinResponse = await _client.PostAsJsonAsync("/api/Public/queue/join", joinRequest);
            Assert.AreEqual(HttpStatusCode.OK, joinResponse.StatusCode);
            
            var joinContent = await joinResponse.Content.ReadAsStringAsync();
            var joinResult = JsonSerializer.Deserialize<AnonymousJoinResult>(joinContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            Assert.IsNotNull(joinResult);
            Assert.IsTrue(joinResult.Success);
            Assert.IsNotNull(joinResult.Id);
            Assert.IsTrue(joinResult.Position > 0);

            // Act & Assert - Step 5: Check queue entry status
            var entryStatusResponse = await _client.GetAsync($"/api/Public/queue/entry-status/{joinResult.Id}");
            Assert.AreEqual(HttpStatusCode.OK, entryStatusResponse.StatusCode);
            
            var entryStatusContent = await entryStatusResponse.Content.ReadAsStringAsync();
            var entryStatus = JsonSerializer.Deserialize<QueueEntryStatusDto>(entryStatusContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            Assert.IsNotNull(entryStatus);
            Assert.AreEqual(joinResult.Id, entryStatus.Id);
            Assert.AreEqual("waiting", entryStatus.Status.ToLower());

            // Act & Assert - Step 6: Update queue entry
            var updateRequest = new
            {
                name = "Updated Customer Name",
                email = "updated@example.com",
                emailNotifications = false,
                browserNotifications = true
            };

            var updateResponse = await _client.PutAsJsonAsync($"/api/Public/queue/update/{joinResult.Id}", updateRequest);
            Assert.AreEqual(HttpStatusCode.OK, updateResponse.StatusCode);
            
            var updateContent = await updateResponse.Content.ReadAsStringAsync();
            var updateResult = JsonSerializer.Deserialize<UpdateQueueEntryResult>(updateContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            Assert.IsNotNull(updateResult);
            Assert.IsTrue(updateResult.Success);

            // Act & Assert - Step 7: Leave queue
            var leaveResponse = await _client.PostAsync($"/api/Public/queue/leave/{joinResult.Id}", null);
            Assert.AreEqual(HttpStatusCode.OK, leaveResponse.StatusCode);
            
            var leaveContent = await leaveResponse.Content.ReadAsStringAsync();
            var leaveResult = JsonSerializer.Deserialize<LeaveQueueResult>(leaveContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            Assert.IsNotNull(leaveResult);
            Assert.IsTrue(leaveResult.Success);
        }

        [TestMethod]
        public async Task PublicController_JoinQueue_WithInvalidSalonId_ShouldReturnNotFound()
        {
            // Arrange
            var invalidSalonId = Guid.NewGuid().ToString();
            var joinRequest = new
            {
                salonId = invalidSalonId,
                name = "Test Customer",
                email = "test@example.com",
                serviceRequested = "Haircut",
                anonymousUserId = Guid.NewGuid().ToString()
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Public/queue/join", joinRequest);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task PublicController_JoinQueue_WithInvalidRequest_ShouldReturnBadRequest()
        {
            // Arrange
            var joinRequest = new
            {
                salonId = "", // Invalid empty salon ID
                name = "", // Invalid empty name
                email = "test@example.com",
                serviceRequested = "Haircut",
                anonymousUserId = Guid.NewGuid().ToString()
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Public/queue/join", joinRequest);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task PublicController_GetQueueEntryStatus_WithInvalidEntryId_ShouldReturnNotFound()
        {
            // Arrange
            var invalidEntryId = Guid.NewGuid().ToString();

            // Act
            var response = await _client.GetAsync($"/api/Public/queue/entry-status/{invalidEntryId}");

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task PublicController_LeaveQueue_WithInvalidEntryId_ShouldReturnNotFound()
        {
            // Arrange
            var invalidEntryId = Guid.NewGuid().ToString();

            // Act
            var response = await _client.PostAsync($"/api/Public/queue/leave/{invalidEntryId}", null);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task PublicController_UpdateQueueEntry_WithInvalidEntryId_ShouldReturnNotFound()
        {
            // Arrange
            var invalidEntryId = Guid.NewGuid().ToString();
            var updateRequest = new
            {
                name = "Updated Name",
                email = "updated@example.com"
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/Public/queue/update/{invalidEntryId}", updateRequest);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task PublicController_AllEndpoints_ShouldNotRequireAuthentication()
        {
            // Arrange - Create test data
            var organization = await CreateTestOrganization();
            var location = await CreateTestLocation(organization.Id);
            var queue = await CreateTestQueue(location.Id);

            // Act & Assert - Test all public endpoints without authentication
            var endpoints = new[]
            {
                "/api/Public/salons",
                $"/api/Public/salons/{location.Id}",
                $"/api/Public/queue-status/{location.Id}"
            };

            foreach (var endpoint in endpoints)
            {
                var response = await _client.GetAsync(endpoint);
                Assert.AreNotEqual(HttpStatusCode.Unauthorized, response.StatusCode, 
                    $"Endpoint {endpoint} should not require authentication");
                Assert.AreNotEqual(HttpStatusCode.Forbidden, response.StatusCode, 
                    $"Endpoint {endpoint} should not require authentication");
            }
        }

        // Helper methods
        private async Task<Organization> CreateTestOrganization()
        {
            var organization = new Organization(
                "Test Organization",
                "Test organization for integration tests",
                "test@example.com",
                "123456789",
                "Test User"
            );
            
            await _organizationRepository.AddAsync(organization, CancellationToken.None);
            return organization;
        }

        private async Task<Location> CreateTestLocation(Guid organizationId)
        {
            var address = Address.Create(
                "123 Test Street",
                "Test City",
                "TS",
                "12345",
                "Brazil"
            );

            var location = new Location(
                "Test Location",
                "test-location",
                "Test location for integration tests",
                organizationId,
                address,
                "123456789",
                "test@example.com",
                TimeSpan.FromHours(9),
                TimeSpan.FromHours(17),
                100,
                15,
                "Test User"
            );
            
            await _locationRepository.AddAsync(location, CancellationToken.None);
            return location;
        }

        private async Task<Queue> CreateTestQueue(Guid locationId)
        {
            var queue = new Queue(
                locationId,
                DateTime.Today,
                "Test User"
            );
            
            await _queueRepository.AddAsync(queue, CancellationToken.None);
            return queue;
        }

        private async Task<ServiceOffered> CreateTestService(Guid locationId)
        {
            var service = new ServiceOffered(
                "Test Service",
                "Test service for integration tests",
                locationId,
                30, // 30 minutes
                50.00m, // $50
                null,
                "Test User"
            );
            
            await _serviceRepository.AddAsync(service, CancellationToken.None);
            return service;
        }
    }

    // DTOs for testing
    public class QueueEntryStatusDto
    {
        public string? Id { get; set; }
        public int Position { get; set; }
        public int EstimatedWaitMinutes { get; set; }
        public string? Status { get; set; }
        public DateTime? JoinedAt { get; set; }
        public string? ServiceRequested { get; set; }
        public string? CustomerName { get; set; }
    }

    public class UpdateQueueEntryResult
    {
        public bool Success { get; set; }
        public string? QueueEntryId { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
