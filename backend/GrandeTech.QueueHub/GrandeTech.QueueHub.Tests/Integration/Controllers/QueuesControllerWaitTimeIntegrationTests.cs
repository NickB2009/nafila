using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Grande.Fila.API.Domain.Locations;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.Staff;
using Grande.Fila.API.Domain.Organizations;
using Grande.Fila.API.Domain.Subscriptions;
using Grande.Fila.API.Domain.Common.ValueObjects;
using Grande.Fila.API.Infrastructure.Repositories.Bogus;
using Grande.Fila.API.Tests.Integration;
using System.Threading;

namespace Grande.Fila.API.Tests.Integration.Controllers
{
    [TestClass]
    [TestCategory("Integration")]
    public class QueuesControllerWaitTimeIntegrationTests
    {
        private static WebApplicationFactory<Program> _factory;
        private static BogusUserRepository _userRepository;
        private static BogusOrganizationRepository _organizationRepository;
        private static BogusQueueRepository _queueRepository;
        private static BogusStaffMemberRepository _staffMemberRepository;
        private static BogusLocationRepository _locationRepository;
        private static BogusSubscriptionPlanRepository _subscriptionPlanRepository;
        private HttpClient _client;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _userRepository = new BogusUserRepository();
            _organizationRepository = new BogusOrganizationRepository();
            _queueRepository = new BogusQueueRepository();
            _staffMemberRepository = new BogusStaffMemberRepository();
            _locationRepository = new BogusLocationRepository();
            _subscriptionPlanRepository = new BogusSubscriptionPlanRepository();

            _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(_userRepository);
                    services.AddSingleton(_organizationRepository);
                    services.AddSingleton(_queueRepository);
                    services.AddSingleton(_staffMemberRepository);
                    services.AddSingleton(_locationRepository);
                    services.AddSingleton(_subscriptionPlanRepository);
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

        [TestMethod]
        public async Task GetEstimatedWaitTime_FirstCustomerInQueue_ReturnsZeroWaitTime()
        {
            // Arrange
            var testData = await SetupTestWaitTimeDataAsync();
            var adminToken = await CreateAndAuthenticateUserAsync("Admin");
            _client.DefaultRequestHeaders.Authorization = new("Bearer", adminToken);

            // Act
            var response = await _client.GetAsync($"/api/queues/{testData.QueueId}/entries/{testData.EntryId}/wait-time");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
            Assert.AreEqual(0, result.GetProperty("estimatedWaitTimeInMinutes").GetInt32());
        }

        [TestMethod]
        public async Task GetEstimatedWaitTime_SecondCustomerInQueue_ReturnsThirtyMinutes()
        {
            // Arrange
            var testData = await SetupTestWaitTimeDataWithMultipleCustomersAsync();
            var adminToken = await CreateAndAuthenticateUserAsync("Admin");
            _client.DefaultRequestHeaders.Authorization = new("Bearer", adminToken);

            // Act
            var response = await _client.GetAsync($"/api/queues/{testData.QueueId}/entries/{testData.SecondEntryId}/wait-time");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
            Assert.AreEqual(30, result.GetProperty("estimatedWaitTimeInMinutes").GetInt32());
        }

        [TestMethod]
        public async Task GetEstimatedWaitTime_NoActiveStaff_ReturnsNotFound()
        {
            // Arrange
            var testData = await SetupTestWaitTimeDataWithNoStaffAsync();
            var adminToken = await CreateAndAuthenticateUserAsync("Admin");
            _client.DefaultRequestHeaders.Authorization = new("Bearer", adminToken);

            // Act
            var response = await _client.GetAsync($"/api/queues/{testData.QueueId}/entries/{testData.EntryId}/wait-time");

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task GetEstimatedWaitTime_InvalidQueueId_ReturnsNotFound()
        {
            // Arrange
            var invalidQueueId = Guid.NewGuid();
            var invalidEntryId = Guid.NewGuid();
            var adminToken = await CreateAndAuthenticateUserAsync("Admin");
            _client.DefaultRequestHeaders.Authorization = new("Bearer", adminToken);

            // Act
            var response = await _client.GetAsync($"/api/queues/{invalidQueueId}/entries/{invalidEntryId}/wait-time");

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task GetEstimatedWaitTime_InvalidEntryId_ReturnsNotFound()
        {
            // Arrange
            var testData = await SetupTestWaitTimeDataAsync();
            var invalidEntryId = Guid.NewGuid();
            var adminToken = await CreateAndAuthenticateUserAsync("Admin");
            _client.DefaultRequestHeaders.Authorization = new("Bearer", adminToken);

            // Act
            var response = await _client.GetAsync($"/api/queues/{testData.QueueId}/entries/{invalidEntryId}/wait-time");

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task GetEstimatedWaitTime_AnonymousUser_ReturnsSuccess()
        {
            // Arrange
            var testData = await SetupTestWaitTimeDataAsync();
            // No authentication token - testing anonymous access

            // Act
            var response = await _client.GetAsync($"/api/queues/{testData.QueueId}/entries/{testData.EntryId}/wait-time");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
            Assert.AreEqual(0, result.GetProperty("estimatedWaitTimeInMinutes").GetInt32());
        }

        private async Task<(Guid LocationId, Guid QueueId, Guid EntryId)> SetupTestWaitTimeDataAsync()
        {
            var uniqueId = Guid.NewGuid().ToString()[..8];
            
            // Create subscription plan
            var subscriptionPlan = new SubscriptionPlan(
                "Wait Time Test Plan", "Plan for wait time testing", 49.99m, 499.99m, 3, 15,
                true, true, true, true, true, 1000, true, "test");
            await _subscriptionPlanRepository.AddAsync(subscriptionPlan, CancellationToken.None);

            // Create organization
            var organization = new Organization($"Wait Time Test Org {uniqueId}", $"wait-time-test-org-{uniqueId}", "Test organization",
                "waittime@test.com", "+1234567890", "https://waittime-test.com", null, subscriptionPlan.Id, "test");
            await _organizationRepository.AddAsync(organization, CancellationToken.None);

            // Create location
            var address = Address.Create(
                "123 Wait Time St", "1", "", "Downtown", "Wait Time City", "WTC", "USA", "12345");
            var location = new Location("Wait Time Location", "wait-time-location", "Test location",
                organization.Id, address, "+1234567890", "location@waittime.com",
                TimeSpan.FromHours(9), TimeSpan.FromHours(17), 50, 15, "test");
            await _locationRepository.AddAsync(location, CancellationToken.None);

            // Create active staff member
            var staffMember = new StaffMember("Wait Time Barber", location.Id, "barber@waittime.com",
                "+1234567890", null, "Barber", "waittime.barber", null, "test");
            await _staffMemberRepository.AddAsync(staffMember, CancellationToken.None);

            // Create queue
            var queue = new Queue(location.Id, 50, 15, "test");
            await _queueRepository.AddAsync(queue, CancellationToken.None);

            // Add customer to queue
            var customerId = Guid.NewGuid();
            var entry = queue.AddCustomerToQueue(customerId, "Test Customer for Wait Time", staffMember.Id);
            await _queueRepository.UpdateAsync(queue, CancellationToken.None);

            return (location.Id, queue.Id, entry.Id);
        }

        private async Task<string> CreateAndAuthenticateUserAsync(string role)
        {
            return await IntegrationTestHelper.CreateAndAuthenticateUserAsync(
                _userRepository, _factory.Services, role, new string[0]);
        }

        private async Task<(Guid LocationId, Guid QueueId, Guid FirstEntryId, Guid SecondEntryId)> SetupTestWaitTimeDataWithMultipleCustomersAsync()
        {
            var uniqueId = Guid.NewGuid().ToString()[..8];
            
            // Create subscription plan
            var subscriptionPlan = new SubscriptionPlan(
                "Wait Time Test Plan", "Plan for wait time testing", 49.99m, 499.99m, 3, 15,
                true, true, true, true, true, 1000, true, "test");
            await _subscriptionPlanRepository.AddAsync(subscriptionPlan, CancellationToken.None);

            // Create organization
            var organization = new Organization($"Wait Time Test Org {uniqueId}", $"wait-time-test-org-{uniqueId}", "Test organization",
                "waittime@test.com", "+1234567890", "https://waittime-test.com", null, subscriptionPlan.Id, "test");
            await _organizationRepository.AddAsync(organization, CancellationToken.None);

            // Create location
            var address = Address.Create(
                "123 Wait Time St", "1", "", "Downtown", "Wait Time City", "WTC", "USA", "12345");
            var location = new Location("Wait Time Location", "wait-time-location", "Test location",
                organization.Id, address, "+1234567890", "location@waittime.com",
                TimeSpan.FromHours(9), TimeSpan.FromHours(17), 50, 15, "test");
            await _locationRepository.AddAsync(location, CancellationToken.None);

            // Create active staff member
            var staffMember = new StaffMember("Wait Time Barber", location.Id, "barber@waittime.com",
                "+1234567890", null, "Barber", "waittime.barber", null, "test");
            await _staffMemberRepository.AddAsync(staffMember, CancellationToken.None);

            // Create queue
            var queue = new Queue(location.Id, 50, 15, "test");
            await _queueRepository.AddAsync(queue, CancellationToken.None);

            // Add two customers to queue
            var firstCustomerId = Guid.NewGuid();
            var secondCustomerId = Guid.NewGuid();
            var firstEntry = queue.AddCustomerToQueue(firstCustomerId, "First Customer", staffMember.Id);
            var secondEntry = queue.AddCustomerToQueue(secondCustomerId, "Second Customer", staffMember.Id);
            await _queueRepository.UpdateAsync(queue, CancellationToken.None);

            return (location.Id, queue.Id, firstEntry.Id, secondEntry.Id);
        }

        private async Task<(Guid LocationId, Guid QueueId, Guid EntryId)> SetupTestWaitTimeDataWithNoStaffAsync()
        {
            var uniqueId = Guid.NewGuid().ToString()[..8];
            
            // Create subscription plan
            var subscriptionPlan = new SubscriptionPlan(
                "Wait Time Test Plan", "Plan for wait time testing", 49.99m, 499.99m, 3, 15,
                true, true, true, true, true, 1000, true, "test");
            await _subscriptionPlanRepository.AddAsync(subscriptionPlan, CancellationToken.None);

            // Create organization
            var organization = new Organization($"Wait Time Test Org {uniqueId}", $"wait-time-test-org-{uniqueId}", "Test organization",
                "waittime@test.com", "+1234567890", "https://waittime-test.com", null, subscriptionPlan.Id, "test");
            await _organizationRepository.AddAsync(organization, CancellationToken.None);

            // Create location
            var address = Address.Create(
                "123 Wait Time St", "1", "", "Downtown", "Wait Time City", "WTC", "USA", "12345");
            var location = new Location("Wait Time Location", "wait-time-location", "Test location",
                organization.Id, address, "+1234567890", "location@waittime.com",
                TimeSpan.FromHours(9), TimeSpan.FromHours(17), 50, 15, "test");
            await _locationRepository.AddAsync(location, CancellationToken.None);

            // Create inactive staff member
            var staffMember = new StaffMember("Inactive Barber", location.Id, "barber@waittime.com",
                "+1234567890", null, "Barber", "waittime.barber", null, "test");
            staffMember.Deactivate("test");
            await _staffMemberRepository.AddAsync(staffMember, CancellationToken.None);

            // Create queue
            var queue = new Queue(location.Id, 50, 15, "test");
            await _queueRepository.AddAsync(queue, CancellationToken.None);

            // Add customer to queue
            var customerId = Guid.NewGuid();
            var entry = queue.AddCustomerToQueue(customerId, "Test Customer", null);
            await _queueRepository.UpdateAsync(queue, CancellationToken.None);

            return (location.Id, queue.Id, entry.Id);
        }
    }
} 