using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Application.Auth;
using Grande.Fila.API.Application.Queues;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.Users;
using Grande.Fila.API.Domain.Customers;
using Grande.Fila.API.Infrastructure.Repositories.Bogus;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Linq;
using Grande.Fila.API.Domain.Staff;
using Grande.Fila.API.Domain.Locations;
using Grande.Fila.API.Domain.Organizations;
using Grande.Fila.API.Domain.Common.ValueObjects;
using BCrypt.Net;
using Grande.Fila.API.Application.Staff;
using Grande.Fila.API.Application.ServicesOffered;
using Grande.Fila.API.Domain.ServicesOffered;
using Grande.Fila.API.Application.Services;

namespace Grande.Fila.Tests.Integration.Controllers
{
    [TestClass]
    public class QueuesControllerIntegrationTests
    {
        private static WebApplicationFactory<Program> _factory = null!;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
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
                            ["Jwt:Audience"] = "Grande.Fila.API.Test"
                        });
                    });
                    builder.ConfigureServices(services =>
                    {
                        var queueRepoDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IQueueRepository));
                        if (queueRepoDescriptor != null)
                            services.Remove(queueRepoDescriptor);
                        var servicesOfferedRepoDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IServicesOfferedRepository));
                        if(servicesOfferedRepoDescriptor != null)
                            services.Remove(servicesOfferedRepoDescriptor);


                        services.AddScoped<IUserRepository, BogusUserRepository>();
                        services.AddSingleton<IQueueRepository, BogusQueueRepository>();
                        services.AddScoped<AddQueueService>();
                        services.AddScoped<AuthService>();
                        services.AddScoped<JoinQueueService>();
                        services.AddScoped<ICustomerRepository, BogusCustomerRepository>();
                        services.AddScoped<CallNextService>();
                        services.AddScoped<CheckInService>();
                        services.AddScoped<FinishService>();
                        services.AddScoped<CancelQueueService>();
                        services.AddScoped<EstimatedWaitTimeService>();
                        services.AddScoped<IStaffMemberRepository, BogusStaffMemberRepository>();
                        services.AddScoped<ILocationRepository, BogusLocationRepository>();
                        services.AddScoped<IOrganizationRepository, BogusOrganizationRepository>();
                        services.AddScoped<IServicesOfferedRepository, BogusServiceTypeRepository>();
                        services.AddScoped<AddServiceOfferedService>();
                    });
                });
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            _factory.Dispose();
        }

        [TestMethod]
        public async Task AddQueue_ValidRequest_ReturnsCreated()
        {
            var client = _factory.CreateClient();
            var adminToken = await CreateAndAuthenticateUserAsync("Admin", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var request = new AddQueueRequest
            {
                LocationId = Guid.Parse("12345678-1234-1234-1234-123456789012"),
                MaxSize = 50,
                LateClientCapTimeInMinutes = 15
            };

            var response = await client.PostAsJsonAsync("/api/queues", request);
            var result = await response.Content.ReadFromJsonAsync<AddQueueResult>();

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.AreNotEqual(Guid.Empty, result.QueueId);
            Assert.IsNull(result.ErrorMessage);
        }

        [TestMethod]
        public async Task AddQueue_InvalidRequest_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            var adminToken = await CreateAndAuthenticateUserAsync("Admin", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var request = new AddQueueRequest
            {
                LocationId = Guid.Empty, // Invalid empty GUID
                MaxSize = -1, // Invalid negative size
                LateClientCapTimeInMinutes = -1 // Invalid negative time
            };

            var response = await client.PostAsJsonAsync("/api/queues", request);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task GetQueue_ExistingId_ReturnsOk()
        {
            var client = _factory.CreateClient();
            var adminToken = await CreateAndAuthenticateUserAsync("Admin", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            // First create a queue
            var addRequest = new AddQueueRequest
            {
                LocationId = Guid.Parse("12345678-1234-1234-1234-123456789012"),
                MaxSize = 50,
                LateClientCapTimeInMinutes = 15
            };

            var addResponse = await client.PostAsJsonAsync("/api/queues", addRequest);
            var addResult = await addResponse.Content.ReadFromJsonAsync<AddQueueResult>();

            Assert.IsNotNull(addResult);
            Assert.IsTrue(addResult.Success);

            // Then try to get it
            var getResponse = await client.GetAsync($"/api/queues/{addResult.QueueId}");
            var jsonResponse = await getResponse.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"JSON Response: {jsonResponse}");

            var getResult = await getResponse.Content.ReadFromJsonAsync<QueueDto>();

            Assert.AreEqual(HttpStatusCode.OK, getResponse.StatusCode);
            Assert.IsNotNull(getResult);
            Assert.AreEqual(addResult.QueueId, getResult.Id);
            Assert.AreEqual(addRequest.LocationId, getResult.LocationId);
            Assert.AreEqual(addRequest.MaxSize, getResult.MaxSize);
            Assert.AreEqual(addRequest.LateClientCapTimeInMinutes, getResult.LateClientCapTimeInMinutes);
        }

        [TestMethod]
        public async Task GetQueue_NonExistingId_ReturnsNotFound()
        {
            var client = _factory.CreateClient();
            var adminToken = await CreateAndAuthenticateUserAsync("Admin", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var nonExistingId = Guid.NewGuid();
            var response = await client.GetAsync($"/api/queues/{nonExistingId}");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task GetQueue_InvalidId_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            var adminToken = await CreateAndAuthenticateUserAsync("Admin", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var invalidId = "invalid_id";
            var response = await client.GetAsync($"/api/queues/{invalidId}");

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task AddQueue_WithoutAuthentication_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var request = new AddQueueRequest
            {
                LocationId = Guid.Parse("12345678-1234-1234-1234-123456789012"),
                MaxSize = 50,
                LateClientCapTimeInMinutes = 15
            };

            var response = await client.PostAsJsonAsync("/api/queues", request);
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task GetQueue_WithoutAuthentication_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync($"/api/queues/{Guid.NewGuid()}");
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // UC-ENTRY: Client enters queue integration tests
        [TestMethod]
        public async Task JoinQueue_ValidRequest_ReturnsSuccess()
        {
            var client = _factory.CreateClient();

            // Create queue as admin first
            var adminToken = await CreateAndAuthenticateUserAsync("Admin", client);
            var adminClient = _factory.CreateClient();
            adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var addRequest = new AddQueueRequest
            {
                LocationId = Guid.Parse("12345678-1234-1234-1234-123456789012"),
                MaxSize = 50,
                LateClientCapTimeInMinutes = 15
            };

            var addResponse = await adminClient.PostAsJsonAsync("/api/queues", addRequest);
            var addResult = await addResponse.Content.ReadFromJsonAsync<AddQueueResult>();

            Assert.IsNotNull(addResult);
            Assert.IsTrue(addResult.Success);

            // Join queue with valid request
            var joinRequest = new JoinQueueRequest
            {
                QueueId = addResult.QueueId.ToString(),
                CustomerName = "John Doe",
                IsAnonymous = true
            };

            var joinResponse = await client.PostAsJsonAsync($"/api/queues/{addResult.QueueId}/join", joinRequest);
            var joinResult = await joinResponse.Content.ReadFromJsonAsync<JoinQueueResult>();

            Assert.AreEqual(HttpStatusCode.OK, joinResponse.StatusCode);
            Assert.IsNotNull(joinResult);
            Assert.IsTrue(joinResult.Success);
            Assert.AreNotEqual(Guid.Empty, Guid.Parse(joinResult.QueueEntryId));
            Assert.IsTrue(joinResult.Position > 0);
        }

        [TestMethod]
        public async Task JoinQueue_AnonymousCustomer_ReturnsSuccess()
        {
            var client = _factory.CreateClient();

            // Create queue as admin first
            var adminToken = await CreateAndAuthenticateUserAsync("Admin", client);
            var adminClient = _factory.CreateClient();
            adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var addRequest = new AddQueueRequest
            {
                LocationId = Guid.Parse("12345678-1234-1234-1234-123456789012"),
                MaxSize = 50,
                LateClientCapTimeInMinutes = 15
            };

            var addResponse = await adminClient.PostAsJsonAsync("/api/queues", addRequest);
            var addResult = await addResponse.Content.ReadFromJsonAsync<AddQueueResult>();

            Assert.IsNotNull(addResult);
            Assert.IsTrue(addResult.Success);

            // Join queue as anonymous customer
            var joinRequest = new JoinQueueRequest
            {
                QueueId = addResult.QueueId.ToString(),
                CustomerName = "Anonymous Customer",
                IsAnonymous = true
            };

            var joinResponse = await client.PostAsJsonAsync($"/api/queues/{addResult.QueueId}/join", joinRequest);
            var joinResult = await joinResponse.Content.ReadFromJsonAsync<JoinQueueResult>();

            Assert.AreEqual(HttpStatusCode.OK, joinResponse.StatusCode);
            Assert.IsNotNull(joinResult);
            Assert.IsTrue(joinResult.Success);
            Assert.AreNotEqual(Guid.Empty, Guid.Parse(joinResult.QueueEntryId));
            Assert.IsTrue(joinResult.Position > 0);
        }

        [TestMethod]
        public async Task JoinQueue_InvalidQueueId_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();

            var joinRequest = new JoinQueueRequest
            {
                QueueId = "invalid-guid",
                CustomerName = "John Doe"
            };

            var joinResponse = await client.PostAsJsonAsync("/api/queues/invalid-guid/join", joinRequest);
            Assert.AreEqual(HttpStatusCode.BadRequest, joinResponse.StatusCode);
        }

        [TestMethod]
        public async Task JoinQueue_NonExistentQueue_ReturnsNotFound()
        {
            var client = _factory.CreateClient();

            var nonExistentQueueId = Guid.NewGuid();
            var joinRequest = new JoinQueueRequest
            {
                QueueId = nonExistentQueueId.ToString(),
                CustomerName = "John Doe"
            };

            var joinResponse = await client.PostAsJsonAsync($"/api/queues/{nonExistentQueueId}/join", joinRequest);
            Assert.AreEqual(HttpStatusCode.NotFound, joinResponse.StatusCode);
        }

        [TestMethod]
        public async Task JoinQueue_EmptyCustomerName_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();

            // Create queue as admin first
            var adminToken = await CreateAndAuthenticateUserAsync("Admin", client);
            var adminClient = _factory.CreateClient();
            adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var addRequest = new AddQueueRequest
            {
                LocationId = Guid.Parse("12345678-1234-1234-1234-123456789012"),
                MaxSize = 50,
                LateClientCapTimeInMinutes = 15
            };

            var addResponse = await adminClient.PostAsJsonAsync("/api/queues", addRequest);
            var addResult = await addResponse.Content.ReadFromJsonAsync<AddQueueResult>();

            Assert.IsNotNull(addResult);
            Assert.IsTrue(addResult.Success);

            // Now try to join queue with empty customer name (should fail validation)
            var joinRequest = new JoinQueueRequest
            {
                QueueId = addResult.QueueId.ToString(),
                CustomerName = "" // Empty customer name
            };

            var joinResponse = await client.PostAsJsonAsync($"/api/queues/{addResult.QueueId}/join", joinRequest);
            var joinResult = await joinResponse.Content.ReadFromJsonAsync<JoinQueueResult>();

            Assert.AreEqual(HttpStatusCode.BadRequest, joinResponse.StatusCode);
            Assert.IsNotNull(joinResult);
            Assert.IsFalse(joinResult.Success);
            Assert.IsTrue(joinResult.FieldErrors.ContainsKey("CustomerName"));
        }

        [TestMethod]
        public async Task JoinQueue_WithoutAuthentication_ReturnsSuccess()
        {
            var client = _factory.CreateClient();

            // Create queue as admin first
            var adminToken = await CreateAndAuthenticateUserAsync("Admin", client);
            var adminClient = _factory.CreateClient();
            adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var addRequest = new AddQueueRequest
            {
                LocationId = Guid.Parse("12345678-1234-1234-1234-123456789012"),
                MaxSize = 50,
                LateClientCapTimeInMinutes = 15
            };

            var addResponse = await adminClient.PostAsJsonAsync("/api/queues", addRequest);
            var addResult = await addResponse.Content.ReadFromJsonAsync<AddQueueResult>();

            Assert.IsNotNull(addResult);
            Assert.IsTrue(addResult.Success);

            // Now try to join queue without authentication (should work)
            var joinRequest = new JoinQueueRequest
            {
                QueueId = addResult.QueueId.ToString(),
                CustomerName = "Anonymous Customer",
                IsAnonymous = true
            };

            var joinResponse = await client.PostAsJsonAsync($"/api/queues/{addResult.QueueId}/join", joinRequest);
            var joinResult = await joinResponse.Content.ReadFromJsonAsync<JoinQueueResult>();

            Assert.AreEqual(HttpStatusCode.OK, joinResponse.StatusCode);
            Assert.IsNotNull(joinResult);
            Assert.IsTrue(joinResult.Success);
        }

        [TestMethod]
        public async Task JoinQueue_FullQueue_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();

            // Create queue as admin with size 1
            var adminToken = await CreateAndAuthenticateUserAsync("Admin", client);
            var adminClient = _factory.CreateClient();
            adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var addRequest = new AddQueueRequest
            {
                LocationId = Guid.Parse("12345678-1234-1234-1234-123456789012"),
                MaxSize = 1, // Small queue
                LateClientCapTimeInMinutes = 15
            };

            var addResponse = await adminClient.PostAsJsonAsync("/api/queues", addRequest);
            var addResult = await addResponse.Content.ReadFromJsonAsync<AddQueueResult>();

            Assert.IsNotNull(addResult);
            Assert.IsTrue(addResult.Success);

            // Join first customer (should succeed)
            var joinRequest1 = new JoinQueueRequest
            {
                QueueId = addResult.QueueId.ToString(),
                CustomerName = "First Customer",
                IsAnonymous = true
            };

            var joinResponse1 = await client.PostAsJsonAsync($"/api/queues/{addResult.QueueId}/join", joinRequest1);
            var joinResult1 = await joinResponse1.Content.ReadFromJsonAsync<JoinQueueResult>();

            Assert.AreEqual(HttpStatusCode.OK, joinResponse1.StatusCode);
            Assert.IsNotNull(joinResult1);
            Assert.IsTrue(joinResult1.Success);

            // Try to join second customer (should fail - queue is full)
            var joinRequest2 = new JoinQueueRequest
            {
                QueueId = addResult.QueueId.ToString(),
                CustomerName = "Second Customer",
                IsAnonymous = true
            };

            var joinResponse2 = await client.PostAsJsonAsync($"/api/queues/{addResult.QueueId}/join", joinRequest2);
            var joinResult2 = await joinResponse2.Content.ReadFromJsonAsync<JoinQueueResult>();

            Assert.AreEqual(HttpStatusCode.BadRequest, joinResponse2.StatusCode);
            Assert.IsNotNull(joinResult2);
            Assert.IsFalse(joinResult2.Success);
            Assert.IsTrue(joinResult2.Errors.Any(e => e.Contains("maximum size")));
        }

        // UC-CALLNEXT: Barber calls next client integration tests
        [TestMethod]
        public async Task CallNext_ValidRequest_ReturnsSuccess()
        {
            var client = _factory.CreateClient();
            var barberToken = await CreateAndAuthenticateUserAsync("Barber", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", barberToken);

            // Create queue as admin
            var adminToken = await CreateAndAuthenticateUserAsync("Admin", client);
            var adminClient = _factory.CreateClient();
            adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var addRequest = new AddQueueRequest
            {
                LocationId = Guid.Parse("12345678-1234-1234-1234-123456789012"),
                MaxSize = 50,
                LateClientCapTimeInMinutes = 15
            };

            var addResponse = await adminClient.PostAsJsonAsync("/api/queues", addRequest);
            var addResult = await addResponse.Content.ReadFromJsonAsync<AddQueueResult>();

            // Join queue as client
            var clientToken = await CreateAndAuthenticateUserAsync("Client", client);
            var clientHttpClient = _factory.CreateClient();
            clientHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", clientToken);

            var joinRequest = new JoinQueueRequest
            {
                QueueId = addResult.QueueId.ToString(),
                CustomerName = "John Doe",
                PhoneNumber = "+5511999999999"
            };

            var joinResponse = await clientHttpClient.PostAsJsonAsync($"/api/queues/{addResult.QueueId}/join", joinRequest);
            var joinResult = await joinResponse.Content.ReadFromJsonAsync<JoinQueueResult>();

            Assert.AreEqual(HttpStatusCode.OK, joinResponse.StatusCode);
            Assert.IsTrue(joinResult.Success);

            // Now call next as barber
            var callNextRequest = new CallNextRequest
            {
                StaffMemberId = Guid.NewGuid().ToString() // Barber's ID
            };

            var callNextResponse = await client.PostAsJsonAsync($"/api/queues/{addResult.QueueId}/call-next", callNextRequest);
            var callNextResult = await callNextResponse.Content.ReadFromJsonAsync<CallNextResult>();

            Assert.AreEqual(HttpStatusCode.OK, callNextResponse.StatusCode);
            Assert.IsNotNull(callNextResult);
            Assert.IsTrue(callNextResult.Success);
            Assert.IsNotNull(callNextResult.QueueEntryId);
            Assert.AreEqual(joinResult.QueueEntryId, callNextResult.QueueEntryId);
            Assert.AreEqual("John Doe", callNextResult.CustomerName);
            Assert.AreEqual(1, callNextResult.Position);
        }

        [TestMethod]
        public async Task CallNext_EmptyQueue_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            var barberToken = await CreateAndAuthenticateUserAsync("Barber", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", barberToken);

            // Create empty queue as admin
            var adminToken = await CreateAndAuthenticateUserAsync("Admin", client);
            var adminClient = _factory.CreateClient();
            adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var addRequest = new AddQueueRequest
            {
                LocationId = Guid.Parse("12345678-1234-1234-1234-123456789012"),
                MaxSize = 50,
                LateClientCapTimeInMinutes = 15
            };

            var addResponse = await adminClient.PostAsJsonAsync("/api/queues", addRequest);
            var addResult = await addResponse.Content.ReadFromJsonAsync<AddQueueResult>();

            // Try to call next on empty queue
            var callNextRequest = new CallNextRequest
            {
                StaffMemberId = Guid.NewGuid().ToString()
            };

            var callNextResponse = await client.PostAsJsonAsync($"/api/queues/{addResult.QueueId}/call-next", callNextRequest);
            var callNextResult = await callNextResponse.Content.ReadFromJsonAsync<CallNextResult>();

            Assert.AreEqual(HttpStatusCode.BadRequest, callNextResponse.StatusCode);
            Assert.IsFalse(callNextResult.Success);
            Assert.IsTrue(callNextResult.Errors.Any(e => e.Contains("No customers in queue")));
        }

        [TestMethod]
        public async Task CallNext_InvalidQueueId_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            var barberToken = await CreateAndAuthenticateUserAsync("Barber", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", barberToken);

            var callNextRequest = new CallNextRequest
            {
                StaffMemberId = Guid.NewGuid().ToString()
            };

            var callNextResponse = await client.PostAsJsonAsync("/api/queues/invalid-guid/call-next", callNextRequest);
            Assert.AreEqual(HttpStatusCode.BadRequest, callNextResponse.StatusCode);
        }

        [TestMethod]
        public async Task CallNext_NonExistentQueue_ReturnsNotFound()
        {
            var client = _factory.CreateClient();
            var barberToken = await CreateAndAuthenticateUserAsync("Barber", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", barberToken);

            var nonExistentQueueId = Guid.NewGuid();
            var callNextRequest = new CallNextRequest
            {
                StaffMemberId = Guid.NewGuid().ToString()
            };

            var callNextResponse = await client.PostAsJsonAsync($"/api/queues/{nonExistentQueueId}/call-next", callNextRequest);
            Assert.AreEqual(HttpStatusCode.NotFound, callNextResponse.StatusCode);
        }

        [TestMethod]
        public async Task CallNext_WithoutAuthentication_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var callNextRequest = new CallNextRequest
            {
                StaffMemberId = Guid.NewGuid().ToString()
            };

            var callNextResponse = await client.PostAsJsonAsync($"/api/queues/{Guid.NewGuid()}/call-next", callNextRequest);
            Assert.AreEqual(HttpStatusCode.Unauthorized, callNextResponse.StatusCode);
        }

        [TestMethod]
        public async Task CallNext_WithoutStaffMemberId_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            var barberToken = await CreateAndAuthenticateUserAsync("Barber", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", barberToken);

            // Create queue as admin
            var adminToken = await CreateAndAuthenticateUserAsync("Admin", client);
            var adminClient = _factory.CreateClient();
            adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var addRequest = new AddQueueRequest
            {
                LocationId = Guid.Parse("12345678-1234-1234-1234-123456789012"),
                MaxSize = 50,
                LateClientCapTimeInMinutes = 15
            };

            var addResponse = await adminClient.PostAsJsonAsync("/api/queues", addRequest);
            var addResult = await addResponse.Content.ReadFromJsonAsync<AddQueueResult>();

            // Join queue as client
            var clientToken = await CreateAndAuthenticateUserAsync("Client", client);
            var clientHttpClient = _factory.CreateClient();
            clientHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", clientToken);

            var joinRequest = new JoinQueueRequest
            {
                QueueId = addResult.QueueId.ToString(),
                CustomerName = "John Doe",
                IsAnonymous = true
            };

            var joinResponse = await clientHttpClient.PostAsJsonAsync($"/api/queues/{addResult.QueueId}/join", joinRequest);
            Assert.AreEqual(HttpStatusCode.OK, joinResponse.StatusCode);

            // Try to call next without staff member ID
            var callNextRequest = new CallNextRequest
            {
                StaffMemberId = "" // Empty staff member ID
            };

            var callNextResponse = await client.PostAsJsonAsync($"/api/queues/{addResult.QueueId}/call-next", callNextRequest);
            var contentType = callNextResponse.Content.Headers.ContentType?.MediaType;
            Assert.AreEqual(HttpStatusCode.BadRequest, callNextResponse.StatusCode);
            Assert.IsTrue(contentType == "application/json" || contentType == "text/plain");
            var responseText = await callNextResponse.Content.ReadAsStringAsync();
            Assert.IsTrue(responseText.Contains("Staff member ID is required."));
        }

        // UC-CHECKIN: Check-in client integration tests
        [TestMethod]
        public async Task CheckIn_ValidRequest_ReturnsSuccess()
        {
            var client = _factory.CreateClient();
            var barberToken = await CreateAndAuthenticateUserAsync("Barber", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", barberToken);

            // Create queue as admin
            var adminToken = await CreateAndAuthenticateUserAsync("Admin", client);
            var adminClient = _factory.CreateClient();
            adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var addRequest = new AddQueueRequest
            {
                LocationId = Guid.Parse("12345678-1234-1234-1234-123456789012"),
                MaxSize = 50,
                LateClientCapTimeInMinutes = 15
            };

            var addResponse = await adminClient.PostAsJsonAsync("/api/queues", addRequest);
            var addResult = await addResponse.Content.ReadFromJsonAsync<AddQueueResult>();

            // Join queue as client
            var clientToken = await CreateAndAuthenticateUserAsync("Client", client);
            var clientHttpClient = _factory.CreateClient();
            clientHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", clientToken);

            var joinRequest = new JoinQueueRequest
            {
                QueueId = addResult.QueueId.ToString(),
                CustomerName = "John Doe",
                PhoneNumber = "+5511999999999"
            };

            var joinResponse = await clientHttpClient.PostAsJsonAsync($"/api/queues/{addResult.QueueId}/join", joinRequest);
            var joinResult = await joinResponse.Content.ReadFromJsonAsync<JoinQueueResult>();

            // Call next as barber
            var callNextRequest = new CallNextRequest
            {
                StaffMemberId = Guid.NewGuid().ToString()
            };

            var callNextResponse = await client.PostAsJsonAsync($"/api/queues/{addResult.QueueId}/call-next", callNextRequest);
            var callNextResult = await callNextResponse.Content.ReadFromJsonAsync<CallNextResult>();

            Assert.AreEqual(HttpStatusCode.OK, callNextResponse.StatusCode);
            Assert.IsTrue(callNextResult.Success);

            // Now check in the customer
            var checkInRequest = new CheckInRequest
            {
                QueueEntryId = callNextResult.QueueEntryId
            };

            var checkInResponse = await client.PostAsJsonAsync($"/api/queues/{addResult.QueueId}/check-in", checkInRequest);
            var checkInResult = await checkInResponse.Content.ReadFromJsonAsync<CheckInResult>();

            Assert.AreEqual(HttpStatusCode.OK, checkInResponse.StatusCode);
            Assert.IsNotNull(checkInResult);
            Assert.IsTrue(checkInResult.Success);
            Assert.AreEqual(callNextResult.QueueEntryId, checkInResult.QueueEntryId);
            Assert.AreEqual("John Doe", checkInResult.CustomerName);
            Assert.IsNotNull(checkInResult.CheckedInAt);
        }

        [TestMethod]
        public async Task CheckIn_NotCalledCustomer_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            var barberToken = await CreateAndAuthenticateUserAsync("Barber", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", barberToken);

            // Create queue as admin
            var adminToken = await CreateAndAuthenticateUserAsync("Admin", client);
            var adminClient = _factory.CreateClient();
            adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var addRequest = new AddQueueRequest
            {
                LocationId = Guid.Parse("12345678-1234-1234-1234-123456789012"),
                MaxSize = 50,
                LateClientCapTimeInMinutes = 15
            };

            var addResponse = await adminClient.PostAsJsonAsync("/api/queues", addRequest);
            var addResult = await addResponse.Content.ReadFromJsonAsync<AddQueueResult>();

            // Join queue as client (but don't call next)
            var clientToken = await CreateAndAuthenticateUserAsync("Client", client);
            var clientHttpClient = _factory.CreateClient();
            clientHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", clientToken);

            var joinRequest = new JoinQueueRequest
            {
                QueueId = addResult.QueueId.ToString(),
                CustomerName = "John Doe",
                PhoneNumber = "+5511999999999"
            };

            var joinResponse = await clientHttpClient.PostAsJsonAsync($"/api/queues/{addResult.QueueId}/join", joinRequest);
            var joinResult = await joinResponse.Content.ReadFromJsonAsync<JoinQueueResult>();

            // Try to check in without being called first
            var checkInRequest = new CheckInRequest
            {
                QueueEntryId = joinResult.QueueEntryId
            };

            var checkInResponse = await client.PostAsJsonAsync($"/api/queues/{addResult.QueueId}/check-in", checkInRequest);
            var checkInResult = await checkInResponse.Content.ReadFromJsonAsync<CheckInResult>();

            Assert.AreEqual(HttpStatusCode.BadRequest, checkInResponse.StatusCode);
            Assert.IsFalse(checkInResult.Success);
            Assert.IsTrue(checkInResult.Errors.Any(e => e.Contains("Cannot check in a customer with status")));
        }

        [TestMethod]
        public async Task CheckIn_InvalidQueueEntryId_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            var barberToken = await CreateAndAuthenticateUserAsync("Barber", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", barberToken);

            // Create queue as admin
            var adminToken = await CreateAndAuthenticateUserAsync("Admin", client);
            var adminClient = _factory.CreateClient();
            adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var addRequest = new AddQueueRequest
            {
                LocationId = Guid.Parse("12345678-1234-1234-1234-123456789012"),
                MaxSize = 50,
                LateClientCapTimeInMinutes = 15
            };

            var addResponse = await adminClient.PostAsJsonAsync("/api/queues", addRequest);
            var addResult = await addResponse.Content.ReadFromJsonAsync<AddQueueResult>();

            var checkInRequest = new CheckInRequest
            {
                QueueEntryId = "invalid-guid"
            };

            var checkInResponse = await client.PostAsJsonAsync($"/api/queues/{addResult.QueueId}/check-in", checkInRequest);
            Assert.AreEqual(HttpStatusCode.BadRequest, checkInResponse.StatusCode);
        }

        [TestMethod]
        public async Task CheckIn_NonExistentQueueEntry_ReturnsNotFound()
        {
            var client = _factory.CreateClient();
            var barberToken = await CreateAndAuthenticateUserAsync("Barber", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", barberToken);

            // Create queue as admin
            var adminToken = await CreateAndAuthenticateUserAsync("Admin", client);
            var adminClient = _factory.CreateClient();
            adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var addRequest = new AddQueueRequest
            {
                LocationId = Guid.Parse("12345678-1234-1234-1234-123456789012"),
                MaxSize = 50,
                LateClientCapTimeInMinutes = 15
            };

            var addResponse = await adminClient.PostAsJsonAsync("/api/queues", addRequest);
            var addResult = await addResponse.Content.ReadFromJsonAsync<AddQueueResult>();

            var nonExistentEntryId = Guid.NewGuid();
            var checkInRequest = new CheckInRequest
            {
                QueueEntryId = nonExistentEntryId.ToString()
            };

            var checkInResponse = await client.PostAsJsonAsync($"/api/queues/{addResult.QueueId}/check-in", checkInRequest);
            Assert.AreEqual(HttpStatusCode.NotFound, checkInResponse.StatusCode);
        }

        [TestMethod]
        public async Task CheckIn_WithoutAuthentication_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var checkInRequest = new CheckInRequest
            {
                QueueEntryId = Guid.NewGuid().ToString()
            };

            var checkInResponse = await client.PostAsJsonAsync($"/api/queues/{Guid.NewGuid()}/check-in", checkInRequest);
            Assert.AreEqual(HttpStatusCode.Unauthorized, checkInResponse.StatusCode);
        }

        [TestMethod]
        public async Task CheckIn_EmptyQueueEntryId_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            var barberToken = await CreateAndAuthenticateUserAsync("Barber", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", barberToken);

            // Create queue as admin
            var adminToken = await CreateAndAuthenticateUserAsync("Admin", client);
            var adminClient = _factory.CreateClient();
            adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var addRequest = new AddQueueRequest
            {
                LocationId = Guid.Parse("12345678-1234-1234-1234-123456789012"),
                MaxSize = 50,
                LateClientCapTimeInMinutes = 15
            };

            var addResponse = await adminClient.PostAsJsonAsync("/api/queues", addRequest);
            var addResult = await addResponse.Content.ReadFromJsonAsync<AddQueueResult>();

            var checkInRequest = new CheckInRequest
            {
                QueueEntryId = "" // Empty queue entry ID
            };

            var checkInResponse = await client.PostAsJsonAsync($"/api/queues/{addResult.QueueId}/check-in", checkInRequest);
            var contentType = checkInResponse.Content.Headers.ContentType?.MediaType;
            Assert.AreEqual(HttpStatusCode.BadRequest, checkInResponse.StatusCode);
            Assert.IsTrue(contentType == "application/json" || contentType == "text/plain");
            var responseText = await checkInResponse.Content.ReadAsStringAsync();
            Assert.IsTrue(responseText.Contains("Queue entry ID is required."));
        }

        // UC-FINISH: Barber finishes service integration tests
        [TestMethod]
        public async Task Finish_ValidRequest_ReturnsSuccess()
        {
            var client = _factory.CreateClient();
            var barberToken = await CreateAndAuthenticateUserAsync("Barber", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", barberToken);

            // Create queue as admin
            var adminToken = await CreateAndAuthenticateUserAsync("Admin", client);
            var adminClient = _factory.CreateClient();
            adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var addRequest = new AddQueueRequest
            {
                LocationId = Guid.Parse("12345678-1234-1234-1234-123456789012"),
                MaxSize = 50,
                LateClientCapTimeInMinutes = 15
            };

            var addResponse = await adminClient.PostAsJsonAsync("/api/queues", addRequest);
            var addResult = await addResponse.Content.ReadFromJsonAsync<AddQueueResult>();

            // Join queue as client
            var clientToken = await CreateAndAuthenticateUserAsync("Client", client);
            var clientHttpClient = _factory.CreateClient();
            clientHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", clientToken);

            var joinRequest = new JoinQueueRequest
            {
                QueueId = addResult.QueueId.ToString(),
                CustomerName = "John Doe",
                PhoneNumber = "+5511999999999"
            };

            var joinResponse = await clientHttpClient.PostAsJsonAsync($"/api/queues/{addResult.QueueId}/join", joinRequest);
            var joinResult = await joinResponse.Content.ReadFromJsonAsync<JoinQueueResult>();

            // Call next as barber
            var callNextRequest = new CallNextRequest
            {
                StaffMemberId = Guid.NewGuid().ToString()
            };

            var callNextResponse = await client.PostAsJsonAsync($"/api/queues/{addResult.QueueId}/call-next", callNextRequest);
            var callNextResult = await callNextResponse.Content.ReadFromJsonAsync<CallNextResult>();

            // Check in the customer
            var checkInRequest = new CheckInRequest
            {
                QueueEntryId = callNextResult.QueueEntryId
            };

            var checkInResponse = await client.PostAsJsonAsync($"/api/queues/{addResult.QueueId}/check-in", checkInRequest);
            var checkInResult = await checkInResponse.Content.ReadFromJsonAsync<CheckInResult>();

            // Now finish the service
            var finishRequest = new FinishRequest
            {
                QueueEntryId = checkInResult.QueueEntryId,
                ServiceDurationMinutes = 30,
                Notes = "Great haircut!"
            };

            var finishResponse = await client.PostAsJsonAsync($"/api/queues/{addResult.QueueId}/finish", finishRequest);
            var finishResult = await finishResponse.Content.ReadFromJsonAsync<FinishResult>();

            Assert.AreEqual(HttpStatusCode.OK, finishResponse.StatusCode);
            Assert.IsNotNull(finishResult);
            Assert.IsTrue(finishResult.Success);
            Assert.AreEqual(checkInResult.QueueEntryId, finishResult.QueueEntryId);
            Assert.AreEqual("John Doe", finishResult.CustomerName);
            Assert.AreEqual(30, finishResult.ServiceDurationMinutes);
            Assert.AreEqual("Great haircut!", finishResult.Notes);
            Assert.IsNotNull(finishResult.CompletedAt);
        }

        [TestMethod]
        public async Task Finish_NotCheckedInCustomer_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            var barberToken = await CreateAndAuthenticateUserAsync("Barber", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", barberToken);

            // Create queue as admin
            var adminToken = await CreateAndAuthenticateUserAsync("Admin", client);
            var adminClient = _factory.CreateClient();
            adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var addRequest = new AddQueueRequest
            {
                LocationId = Guid.Parse("12345678-1234-1234-1234-123456789012"),
                MaxSize = 50,
                LateClientCapTimeInMinutes = 15
            };

            var addResponse = await adminClient.PostAsJsonAsync("/api/queues", addRequest);
            var addResult = await addResponse.Content.ReadFromJsonAsync<AddQueueResult>();

            // Join queue as client
            var clientToken = await CreateAndAuthenticateUserAsync("Client", client);
            var clientHttpClient = _factory.CreateClient();
            clientHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", clientToken);

            var joinRequest = new JoinQueueRequest
            {
                QueueId = addResult.QueueId.ToString(),
                CustomerName = "John Doe",
                PhoneNumber = "+5511999999999"
            };

            var joinResponse = await clientHttpClient.PostAsJsonAsync($"/api/queues/{addResult.QueueId}/join", joinRequest);
            var joinResult = await joinResponse.Content.ReadFromJsonAsync<JoinQueueResult>();

            // Try to finish service without checking in (should fail)
            var finishRequest = new FinishRequest
            {
                QueueEntryId = joinResult.QueueEntryId,
                ServiceDurationMinutes = 30
            };

            var finishResponse = await client.PostAsJsonAsync($"/api/queues/{addResult.QueueId}/finish", finishRequest);
            var finishResult = await finishResponse.Content.ReadFromJsonAsync<FinishResult>();

            Assert.AreEqual(HttpStatusCode.BadRequest, finishResponse.StatusCode);
            Assert.IsFalse(finishResult.Success);
            Assert.IsTrue(finishResult.Errors.Any(e => e.Contains("not checked in")));
        }

        [TestMethod]
        public async Task Finish_EmptyQueueEntryId_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            var barberToken = await CreateAndAuthenticateUserAsync("Barber", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", barberToken);

            var queueId = await CreateTestQueueDirectlyAsync();

            var finishRequest = new FinishRequest
            {
                QueueEntryId = "",
                ServiceDurationMinutes = 30
            };

            var response = await client.PostAsJsonAsync($"/api/queues/{queueId}/finish", finishRequest);
            var result = await response.Content.ReadFromJsonAsync<FinishResult>();

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("QueueEntryId"));
        }

        [TestMethod]
        public async Task Finish_InvalidQueueEntryId_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            var barberToken = await CreateAndAuthenticateUserAsync("Barber", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", barberToken);

            var queueId = await CreateTestQueueDirectlyAsync();

            var finishRequest = new FinishRequest
            {
                QueueEntryId = "invalid-guid",
                ServiceDurationMinutes = 30
            };

            var response = await client.PostAsJsonAsync($"/api/queues/{queueId}/finish", finishRequest);
            var result = await response.Content.ReadFromJsonAsync<FinishResult>();

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("QueueEntryId"));
        }

        [TestMethod]
        public async Task Finish_ZeroServiceDuration_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            var barberToken = await CreateAndAuthenticateUserAsync("Barber", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", barberToken);

            var queueId = await CreateTestQueueDirectlyAsync();

            var finishRequest = new FinishRequest
            {
                QueueEntryId = Guid.NewGuid().ToString(),
                ServiceDurationMinutes = 0
            };

            var response = await client.PostAsJsonAsync($"/api/queues/{queueId}/finish", finishRequest);
            var result = await response.Content.ReadFromJsonAsync<FinishResult>();

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("ServiceDurationMinutes"));
        }

        [TestMethod]
        public async Task Finish_NegativeServiceDuration_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            var barberToken = await CreateAndAuthenticateUserAsync("Barber", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", barberToken);

            var queueId = await CreateTestQueueDirectlyAsync();

            var finishRequest = new FinishRequest
            {
                QueueEntryId = Guid.NewGuid().ToString(),
                ServiceDurationMinutes = -5
            };

            var response = await client.PostAsJsonAsync($"/api/queues/{queueId}/finish", finishRequest);
            var result = await response.Content.ReadFromJsonAsync<FinishResult>();

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("ServiceDurationMinutes"));
        }

        [TestMethod]
        public async Task Finish_InvalidQueueId_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            var barberToken = await CreateAndAuthenticateUserAsync("Barber", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", barberToken);

            var finishRequest = new FinishRequest
            {
                QueueEntryId = Guid.NewGuid().ToString(),
                ServiceDurationMinutes = 30
            };

            var response = await client.PostAsJsonAsync("/api/queues/invalid-guid/finish", finishRequest);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task Finish_NonExistentQueueEntry_ReturnsNotFound()
        {
            var client = _factory.CreateClient();
            var barberToken = await CreateAndAuthenticateUserAsync("Barber", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", barberToken);

            // Create queue as admin
            var adminToken = await CreateAndAuthenticateUserAsync("Admin", client);
            var adminClient = _factory.CreateClient();
            adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var addRequest = new AddQueueRequest
            {
                LocationId = Guid.Parse("12345678-1234-1234-1234-123456789012"),
                MaxSize = 50,
                LateClientCapTimeInMinutes = 15
            };

            var addResponse = await adminClient.PostAsJsonAsync("/api/queues", addRequest);
            var addResult = await addResponse.Content.ReadFromJsonAsync<AddQueueResult>();

            var nonExistentEntryId = Guid.NewGuid();
            var finishRequest = new FinishRequest
            {
                QueueEntryId = nonExistentEntryId.ToString(),
                ServiceDurationMinutes = 30
            };

            var finishResponse = await client.PostAsJsonAsync($"/api/queues/{addResult.QueueId}/finish", finishRequest);
            var finishResult = await finishResponse.Content.ReadFromJsonAsync<FinishResult>();

            Assert.AreEqual(HttpStatusCode.BadRequest, finishResponse.StatusCode);
            Assert.IsFalse(finishResult.Success);
            Assert.IsTrue(finishResult.Errors.Any(e => e.Contains("not found")));
        }

        [TestMethod]
        public async Task Finish_WithoutAuthentication_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var finishRequest = new FinishRequest
            {
                QueueEntryId = Guid.NewGuid().ToString(),
                ServiceDurationMinutes = 30
            };

            var finishResponse = await client.PostAsJsonAsync($"/api/queues/{Guid.NewGuid()}/finish", finishRequest);
            Assert.AreEqual(HttpStatusCode.Unauthorized, finishResponse.StatusCode);
        }

        [TestMethod]
        public async Task Finish_WithNonBarberRole_ReturnsForbidden()
        {
            var client = _factory.CreateClient();
            var clientToken = await CreateAndAuthenticateUserAsync("Client", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", clientToken);

            var finishRequest = new FinishRequest
            {
                QueueEntryId = Guid.NewGuid().ToString(),
                ServiceDurationMinutes = 30
            };

            var finishResponse = await client.PostAsJsonAsync($"/api/queues/{Guid.NewGuid()}/finish", finishRequest);
            Assert.AreEqual(HttpStatusCode.Forbidden, finishResponse.StatusCode);
        }

        // UC-CANCEL: Client cancels queue spot
        [TestMethod]
        public async Task CancelQueue_ValidRequest_ReturnsSuccess()
        {
            var client = _factory.CreateClient();
            var customerToken = await CreateAndAuthenticateUserAsync("Client", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", customerToken);

            // First create a queue and join it
            var queueId = await CreateTestQueueDirectlyAsync();
            var joinResult = await JoinTestQueueAsync(client, queueId, "Test Customer");

            // Now cancel the queue entry
            var cancelRequest = new CancelQueueRequest
            {
                QueueEntryId = joinResult.QueueEntryId
            };

            var response = await client.PostAsJsonAsync($"/api/queues/{queueId}/cancel", cancelRequest);
            var result = await response.Content.ReadFromJsonAsync<CancelQueueResult>();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(joinResult.QueueEntryId, result.QueueEntryId);
            Assert.AreEqual("Test Customer", result.CustomerName);
            Assert.IsNotNull(result.CancelledAt);
        }

        [TestMethod]
        public async Task CancelQueue_AlreadyCalledCustomer_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            var customerToken = await CreateAndAuthenticateUserAsync("Client", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", customerToken);

            // First create a queue and join it
            var queueId = await CreateTestQueueDirectlyAsync();
            var joinResult = await JoinTestQueueAsync(client, queueId, "Test Customer");

            // Call next the customer (barber calls them)
            var barberClient = _factory.CreateClient();
            var barberToken = await CreateAndAuthenticateUserAsync("Barber", barberClient);
            barberClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", barberToken);

            var callNextRequest = new CallNextRequest
            {
                StaffMemberId = Guid.NewGuid().ToString()
            };
            var callNextResponse = await barberClient.PostAsJsonAsync($"/api/queues/{queueId}/call-next", callNextRequest);
            var callNextResult = await callNextResponse.Content.ReadFromJsonAsync<CallNextResult>();

            // Now try to cancel the called customer
            var cancelRequest = new CancelQueueRequest
            {
                QueueEntryId = callNextResult.QueueEntryId
            };

            var response = await client.PostAsJsonAsync($"/api/queues/{queueId}/cancel", cancelRequest);
            var result = await response.Content.ReadFromJsonAsync<CancelQueueResult>();

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            if (!result.Errors.Any(e => e.Contains("Only waiting customers can be cancelled.")))
            {
                throw new Exception("Errors: " + string.Join(", ", result.Errors));
            }
            Assert.IsTrue(result.Errors.Any(e => e.Contains("Only waiting customers can be cancelled.")));
        }

        [TestMethod]
        public async Task CancelQueue_AlreadyCheckedInCustomer_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            var customerToken = await CreateAndAuthenticateUserAsync("Client", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", customerToken);

            // First create a queue and join it
            var queueId = await CreateTestQueueDirectlyAsync();
            var joinResult = await JoinTestQueueAsync(client, queueId, "Test Customer");

            // Call next and check in the customer
            var barberClient = _factory.CreateClient();
            var barberToken = await CreateAndAuthenticateUserAsync("Barber", barberClient);
            barberClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", barberToken);

            var callNextRequest = new CallNextRequest
            {
                StaffMemberId = Guid.NewGuid().ToString()
            };
            var callNextResponse = await barberClient.PostAsJsonAsync($"/api/queues/{queueId}/call-next", callNextRequest);
            var callNextResult = await callNextResponse.Content.ReadFromJsonAsync<CallNextResult>();

            var checkInRequest = new CheckInRequest
            {
                QueueEntryId = callNextResult.QueueEntryId
            };
            var checkInResponse = await barberClient.PostAsJsonAsync($"/api/queues/{queueId}/check-in", checkInRequest);

            // Now try to cancel the checked-in customer
            var cancelRequest = new CancelQueueRequest
            {
                QueueEntryId = callNextResult.QueueEntryId
            };

            var response = await client.PostAsJsonAsync($"/api/queues/{queueId}/cancel", cancelRequest);
            var result = await response.Content.ReadFromJsonAsync<CancelQueueResult>();

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            if (!result.Errors.Any(e => e.Contains("Only waiting customers can be cancelled.")))
            {
                throw new Exception("Errors: " + string.Join(", ", result.Errors));
            }
            Assert.IsTrue(result.Errors.Any(e => e.Contains("Only waiting customers can be cancelled.")));
        }

        [TestMethod]
        public async Task CancelQueue_InvalidQueueEntryId_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            var customerToken = await CreateAndAuthenticateUserAsync("Client", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", customerToken);

            var queueId = await CreateTestQueueDirectlyAsync();

            var cancelRequest = new CancelQueueRequest
            {
                QueueEntryId = "invalid-guid"
            };

            var response = await client.PostAsJsonAsync($"/api/queues/{queueId}/cancel", cancelRequest);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<CancelQueueResult>();
                Assert.IsNotNull(result);
                Assert.IsFalse(result.Success);
            }
            else
            {
                var responseText = await response.Content.ReadAsStringAsync();
                Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
                Assert.IsTrue(responseText.Contains("Invalid queue entry ID format") || responseText.Contains("Invalid queue entry id") || responseText.Contains("invalid"));
            }
        }

        [TestMethod]
        public async Task CancelQueue_NonExistentQueueEntry_ReturnsNotFound()
        {
            var client = _factory.CreateClient();
            var customerToken = await CreateAndAuthenticateUserAsync("Client", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", customerToken);

            var queueId = await CreateTestQueueDirectlyAsync();

            var cancelRequest = new CancelQueueRequest
            {
                QueueEntryId = Guid.NewGuid().ToString()
            };

            var response = await client.PostAsJsonAsync($"/api/queues/{queueId}/cancel", cancelRequest);
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task CancelQueue_WithoutAuthentication_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var queueId = await CreateTestQueueDirectlyAsync();

            var cancelRequest = new CancelQueueRequest
            {
                QueueEntryId = Guid.NewGuid().ToString()
            };

            var response = await client.PostAsJsonAsync($"/api/queues/{queueId}/cancel", cancelRequest);
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task CancelQueue_EmptyQueueEntryId_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            var customerToken = await CreateAndAuthenticateUserAsync("Client", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", customerToken);

            var queueId = await CreateTestQueueDirectlyAsync();

            var cancelRequest = new CancelQueueRequest
            {
                QueueEntryId = ""
            };

            var response = await client.PostAsJsonAsync($"/api/queues/{queueId}/cancel", cancelRequest);
            var result = await response.Content.ReadFromJsonAsync<CancelQueueResult>();

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
        }

        private static async Task<string> CreateAndAuthenticateUserAsync(string role, HttpClient client)
        {
            using var scope = _factory.Services.CreateScope();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

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

            var loginResponse = await client.PostAsync("/api/auth/login", loginContent);
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

                var verifyResponse = await client.PostAsync("/api/auth/verify-2fa", verifyContent);
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
        }

        private static async Task<Guid> CreateTestQueueAsync(HttpClient client, Guid? locationId = null)
        {
            var addRequest = new AddQueueRequest
            {
                LocationId = locationId ?? Guid.Parse("12345678-1234-1234-1234-123456789012"),
                MaxSize = 50,
                LateClientCapTimeInMinutes = 15
            };
            var addResponse = await client.PostAsJsonAsync("/api/queues", addRequest);
            
            if (addResponse.IsSuccessStatusCode)
            {
                var addResult = await addResponse.Content.ReadFromJsonAsync<AddQueueResult>();
                Assert.IsNotNull(addResult);
                return addResult.QueueId;
            }
            else
            {
                // If the response is not successful, read as string to get the error message
                var errorContent = await addResponse.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Failed to create test queue. Status: {addResponse.StatusCode}, Content: {errorContent}");
            }
        }

        private async Task<JoinQueueResult> JoinTestQueueAsync(HttpClient client, Guid queueId, string customerName)
        {
            var joinRequest = new JoinQueueRequest
            {
                QueueId = queueId.ToString(),
                CustomerName = customerName,
                IsAnonymous = true
            };
            var joinResponse = await client.PostAsJsonAsync($"/api/queues/{queueId}/join", joinRequest);
            joinResponse.EnsureSuccessStatusCode();
            var joinResult = await joinResponse.Content.ReadFromJsonAsync<JoinQueueResult>();
            Assert.IsNotNull(joinResult);
            return joinResult;
        }
        
        private static async Task<Guid> CreateLocationAsync()
        {
            var locationRepo = _factory!.Services.GetRequiredService<ILocationRepository>();
            var organizationRepo = _factory!.Services.GetRequiredService<IOrganizationRepository>();
            
            var org = (await organizationRepo.GetAllAsync()).FirstOrDefault();
            if (org == null)
            {
                org = new Organization(
                    "Test Org for Location", 
                    Slug.Create("test-org-location"), 
                    "Test description", 
                    Email.Create("contact@test.com"), 
                    PhoneNumber.Create("+1234567890"), 
                    "http://test.com",
                    BrandingConfig.Default,
                    Guid.NewGuid(),
                    "admin-user-id");
                await organizationRepo.AddAsync(org);
            }

            var location = new Location(
                "Test Location",
                Slug.Create("test-location"),
                "Test Description",
                org.Id,
                Address.Create("123 Main St", "100", "", "Downtown", "Anytown", "Anystate", "USA", "12345"),
                PhoneNumber.Create("+1987654321"),
                Email.Create("location@test.com"),
                new TimeSpan(9,0,0),
                new TimeSpan(17,0,0),
                100,
                15,
                "Test Location"
                );
            await locationRepo.AddAsync(location);
            return location.Id;
        }
        
        private static async Task<Guid> CreateStaffMemberAsync(Guid locationId)
        {
            var staffRepo = _factory!.Services.GetRequiredService<IStaffMemberRepository>();
            var staffMember = new StaffMember("Test Barber", locationId, "test.barber@test.com", "+1234567890", null, "Barber", "testbarber", null, "system");
            await staffRepo.AddAsync(staffMember);
            return staffMember.Id;
        }

        [TestMethod]
        public async Task GetEstimatedWaitTime_ValidRequest_ReturnsWaitTime()
        {
            var client = _factory.CreateClient();
            var adminToken = await CreateAndAuthenticateUserAsync("Admin", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var locationId = await CreateLocationAsync();
            var staffId = await CreateStaffMemberAsync(locationId);
            var queueId = await CreateTestQueueAsync(client, locationId);
            var joinResult = await JoinTestQueueAsync(client, queueId, "Test Customer");

            Assert.IsTrue(joinResult.Success);
            Assert.IsNotNull(joinResult.QueueEntryId);
            var queueEntryId = joinResult.QueueEntryId;

            // Act
            var response = await client.GetAsync($"/api/queues/{queueId}/entries/{queueEntryId}/wait-time");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content);
            var waitTime = result.GetProperty("estimatedWaitTimeInMinutes").GetInt32();
            
            Assert.IsTrue(waitTime >= 0);
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

        public class QueueDto
        {
            public Guid Id { get; set; }
            public Guid LocationId { get; set; }
            public int MaxSize { get; set; }
            public int LateClientCapTimeInMinutes { get; set; }
        }
    }
} 