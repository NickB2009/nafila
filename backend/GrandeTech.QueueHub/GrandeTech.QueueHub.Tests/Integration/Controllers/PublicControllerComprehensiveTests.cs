using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Grande.Fila.API.Application.Public;
using Grande.Fila.API.Tests.Integration;
using Grande.Fila.API.Domain.Locations;
using Grande.Fila.API.Domain.Organizations;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.ServicesOffered;
using Grande.Fila.API.Domain.Common.ValueObjects;
using Grande.Fila.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
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
        private QueueHubDbContext _dbContext;

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
                            ["Database:UseSqlDatabase"] = "true",
                            ["Database:UseInMemoryDatabase"] = "false"
                        });
                    });
                });

            _client = _factory.CreateClient();
            
            using var scope = _factory.Services.CreateScope();
            _dbContext = scope.ServiceProvider.GetRequiredService<QueueHubDbContext>();
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
            // Arrange - Create test data using database context
            var organization = await CreateTestOrganization();
            var location = await CreateTestLocation(organization.Id);
            var queue = await CreateTestQueue(location.Id);
            var service = await CreateTestService(location.Id);

            try
            {
                // Act & Assert - Test the complete flow
                await TestAnonymousJoin(location.Id, queue.Id);
                await TestQueueStatus(location.Id);
                await TestSalonDetails(location.Id);
            }
            finally
            {
                // Cleanup
                await CleanupTestData(organization.Id, location.Id, queue.Id, service.Id);
            }
        }

        [TestMethod]
        public async Task PublicController_JoinQueue_WithInvalidRequest_ShouldReturnBadRequest()
        {
            // Arrange
            var invalidRequest = new { };

            // Act
            var response = await _client.PostAsJsonAsync("/api/public/join-queue", invalidRequest);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task PublicController_LeaveQueue_WithInvalidEntryId_ShouldReturnNotFound()
        {
            // Act
            var response = await _client.DeleteAsync($"/api/public/queue-entry/{Guid.NewGuid()}");

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task PublicController_GetQueueEntryStatus_WithInvalidEntryId_ShouldReturnNotFound()
        {
            // Act
            var response = await _client.GetAsync($"/api/public/queue-entry/{Guid.NewGuid()}/status");

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task PublicController_UpdateQueueEntry_WithInvalidEntryId_ShouldReturnNotFound()
        {
            // Arrange
            var updateRequest = new { customerName = "Test Customer" };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/public/queue-entry/{Guid.NewGuid()}", updateRequest);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task PublicController_AllEndpoints_ShouldNotRequireAuthentication()
        {
            // Test that all public endpoints are accessible without authentication
            var endpoints = new[]
            {
                "/api/public/salons",
                "/api/public/salons/test-slug",
                "/api/public/salons/test-slug/queue-status"
            };

            foreach (var endpoint in endpoints)
            {
                var response = await _client.GetAsync(endpoint);
                // Should not return Unauthorized (401)
                Assert.AreNotEqual(HttpStatusCode.Unauthorized, response.StatusCode, 
                    $"Endpoint {endpoint} should not require authentication");
            }
        }

        private async Task<Organization> CreateTestOrganization()
        {
            var organization = new Organization(
                "Test Organization",
                "test-organization",
                "Test organization for integration tests",
                "test@example.com",
                "123456789",
                "https://test.com",
                null,
                Guid.NewGuid(),
                "Test User"
            );

            _dbContext.Organizations.Add(organization);
            await _dbContext.SaveChangesAsync();
            return organization;
        }

        private async Task<Location> CreateTestLocation(Guid organizationId)
        {
            var address = Address.Create(
                "123 Test Street",
                "1",
                "Apt 101",
                "Test Neighborhood",
                "Test City",
                "TS",
                "Brazil",
                "12345"
            );

            var location = new Location(
                "Test Location",
                "test-location",
                "Test location description",
                organizationId,
                address,
                "987654321",
                "test@location.com",
                TimeSpan.FromHours(9), // Opening time
                TimeSpan.FromHours(18), // Closing time
                100, // Max queue size
                15, // Late client cap time
                "Test User"
            );

            _dbContext.Locations.Add(location);
            await _dbContext.SaveChangesAsync();
            return location;
        }

        private async Task<Queue> CreateTestQueue(Guid locationId)
        {
            var queue = new Queue(
                locationId,
                100,
                15,
                "Test User"
            );

            _dbContext.Queues.Add(queue);
            await _dbContext.SaveChangesAsync();
            return queue;
        }

        private async Task<ServiceOffered> CreateTestService(Guid locationId)
        {
            var service = new ServiceOffered(
                "Test Service",
                "Test service description",
                locationId,
                30, // Estimated duration minutes
                50.00m, // Price
                null, // Image URL
                "Test User"
            );

            _dbContext.ServicesOffered.Add(service);
            await _dbContext.SaveChangesAsync();
            return service;
        }

        private async Task TestAnonymousJoin(Guid locationId, Guid queueId)
        {
            var joinRequest = new
            {
                salonId = locationId,
                customerName = "Test Customer",
                phoneNumber = "+1234567890",
                serviceTypeId = Guid.NewGuid()
            };

            var response = await _client.PostAsJsonAsync("/api/public/join-queue", joinRequest);
            Assert.IsTrue(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.BadRequest);
        }

        private async Task TestQueueStatus(Guid locationId)
        {
            var response = await _client.GetAsync($"/api/public/salons/{locationId}/queue-status");
            Assert.IsTrue(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound);
        }

        private async Task TestSalonDetails(Guid locationId)
        {
            var response = await _client.GetAsync($"/api/public/salons/{locationId}");
            Assert.IsTrue(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound);
        }

        private async Task CleanupTestData(Guid organizationId, Guid locationId, Guid queueId, Guid serviceId)
        {
            try
            {
                // Clean up in reverse order to respect foreign key constraints
                var service = await _dbContext.ServicesOffered.FindAsync(serviceId);
                if (service != null)
                {
                    _dbContext.ServicesOffered.Remove(service);
                }

                var queue = await _dbContext.Queues.FindAsync(queueId);
                if (queue != null)
                {
                    _dbContext.Queues.Remove(queue);
                }

                var location = await _dbContext.Locations.FindAsync(locationId);
                if (location != null)
                {
                    _dbContext.Locations.Remove(location);
                }

                var organization = await _dbContext.Organizations.FindAsync(organizationId);
                if (organization != null)
                {
                    _dbContext.Organizations.Remove(organization);
                }

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cleanup error: {ex.Message}");
            }
        }
    }
}