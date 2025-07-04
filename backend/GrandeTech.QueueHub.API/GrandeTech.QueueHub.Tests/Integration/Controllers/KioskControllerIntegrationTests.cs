using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Grande.Fila.API.Domain.Queues;

namespace Grande.Fila.API.Tests.Integration.Controllers
{
    [TestClass]
    public class KioskControllerIntegrationTests
    {
        private static WebApplicationFactory<Program> _factory = null!;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _factory = new WebApplicationFactory<Program>();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            _factory?.Dispose();
        }

        public class KioskJoinRequest
        {
            public string QueueId { get; set; }
            public string CustomerName { get; set; }
            public string PhoneNumber { get; set; }
        }

        public class KioskJoinResult
        {
            public bool Success { get; set; }
            public string QueueEntryId { get; set; }
            public string CustomerName { get; set; }
            public int Position { get; set; }
            public int EstimatedWaitTimeMinutes { get; set; }
            public string[] Errors { get; set; } = new string[0];
        }

        public class KioskCancelRequest
        {
            public string QueueEntryId { get; set; }
        }

        public class KioskCancelResult
        {
            public bool Success { get; set; }
            public string CustomerName { get; set; }
            public string[] Errors { get; set; } = new string[0];
        }

        // UC-INPUTDATA: Kiosk allows anonymous entry with basic data
        [TestMethod]
        public async Task KioskJoin_ValidRequest_ReturnsSuccess()
        {
            var client = _factory.CreateClient();
            var queueId = await CreateTestQueueDirectlyAsync();

            var request = new KioskJoinRequest
            {
                QueueId = queueId.ToString(),
                CustomerName = "Kiosk Customer",
                PhoneNumber = "+1234567890"
            };

            var response = await client.PostAsJsonAsync("/api/kiosk/join", request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<KioskJoinResult>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.AreEqual("Kiosk Customer", result.CustomerName);
            Assert.IsTrue(result.Position > 0);
            Assert.IsNotNull(result.QueueEntryId);
        }

        [TestMethod]
        public async Task KioskJoin_EmptyCustomerName_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            var queueId = await CreateTestQueueDirectlyAsync();

            var request = new KioskJoinRequest
            {
                QueueId = queueId.ToString(),
                CustomerName = "",
                PhoneNumber = "+1234567890"
            };

            var response = await client.PostAsJsonAsync("/api/kiosk/join", request);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<KioskJoinResult>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Length > 0);
            Assert.IsTrue(Array.Exists(result.Errors, e => e.Contains("Customer name is required")));
        }

        [TestMethod]
        public async Task KioskJoin_EmptyQueueId_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();

            var request = new KioskJoinRequest
            {
                QueueId = "",
                CustomerName = "Kiosk Customer",
                PhoneNumber = "+1234567890"
            };

            var response = await client.PostAsJsonAsync("/api/kiosk/join", request);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<KioskJoinResult>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Length > 0);
            Assert.IsTrue(Array.Exists(result.Errors, e => e.Contains("Queue ID is required")));
        }

        [TestMethod]
        public async Task KioskJoin_WithoutPhoneNumber_ReturnsSuccess()
        {
            var client = _factory.CreateClient();
            var queueId = await CreateTestQueueDirectlyAsync();

            var request = new KioskJoinRequest
            {
                QueueId = queueId.ToString(),
                CustomerName = "Kiosk Customer No Phone"
            };

            var response = await client.PostAsJsonAsync("/api/kiosk/join", request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<KioskJoinResult>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.AreEqual("Kiosk Customer No Phone", result.CustomerName);
        }

        // UC-KIOSKCANCEL: Kiosk allows cancellation
        [TestMethod]
        public async Task KioskCancel_ValidRequest_ReturnsSuccess()
        {
            var client = _factory.CreateClient();
            var queueId = await CreateTestQueueDirectlyAsync();

            // First join the queue
            var joinRequest = new KioskJoinRequest
            {
                QueueId = queueId.ToString(),
                CustomerName = "Kiosk Customer to Cancel",
                PhoneNumber = "+1234567890"
            };

            var joinResponse = await client.PostAsJsonAsync("/api/kiosk/join", joinRequest);
            joinResponse.EnsureSuccessStatusCode();

            var joinContent = await joinResponse.Content.ReadAsStringAsync();
            var joinResult = JsonSerializer.Deserialize<KioskJoinResult>(joinContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Now cancel the entry
            var cancelRequest = new KioskCancelRequest
            {
                QueueEntryId = joinResult.QueueEntryId
            };

            var cancelResponse = await client.PostAsJsonAsync("/api/kiosk/cancel", cancelRequest);
            cancelResponse.EnsureSuccessStatusCode();

            var cancelContent = await cancelResponse.Content.ReadAsStringAsync();
            var cancelResult = JsonSerializer.Deserialize<KioskCancelResult>(cancelContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(cancelResult);
            Assert.IsTrue(cancelResult.Success);
            Assert.AreEqual("Kiosk Customer to Cancel", cancelResult.CustomerName);
        }

        [TestMethod]
        public async Task KioskCancel_EmptyQueueEntryId_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();

            var request = new KioskCancelRequest
            {
                QueueEntryId = ""
            };

            var response = await client.PostAsJsonAsync("/api/kiosk/cancel", request);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<KioskCancelResult>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Length > 0);
            Assert.IsTrue(Array.Exists(result.Errors, e => e.Contains("Queue entry ID is required")));
        }

        [TestMethod]
        public async Task KioskCancel_NonExistentQueueEntry_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();

            var request = new KioskCancelRequest
            {
                QueueEntryId = Guid.NewGuid().ToString()
            };

            var response = await client.PostAsJsonAsync("/api/kiosk/cancel", request);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        private static async Task<Guid> CreateTestQueueDirectlyAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var queueRepository = scope.ServiceProvider.GetRequiredService<IQueueRepository>();
            
            var queue = new Queue(
                Guid.Parse("12345678-1234-1234-1234-123456789012"),
                50,
                15,
                "test-system"
            );
            
            await queueRepository.AddAsync(queue);
            return queue.Id;
        }
    }
}