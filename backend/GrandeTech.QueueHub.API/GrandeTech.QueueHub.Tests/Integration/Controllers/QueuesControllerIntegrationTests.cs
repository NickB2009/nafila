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
using GrandeTech.QueueHub.API.Application.Auth;
using GrandeTech.QueueHub.API.Application.Queues;
using GrandeTech.QueueHub.API.Domain.Queues;
using GrandeTech.QueueHub.API.Domain.Users;
using GrandeTech.QueueHub.API.Infrastructure.Repositories.Bogus;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Linq;

namespace GrandeTech.QueueHub.Tests.Integration.Controllers
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
                            ["Jwt:Issuer"] = "GrandeTech.QueueHub.API.Test",
                            ["Jwt:Audience"] = "GrandeTech.QueueHub.API.Test"
                        });
                    });
                    builder.ConfigureServices(services =>
                    {
                        // Remove existing IQueueRepository registrations using fully qualified type
                        var queueRepoDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(GrandeTech.QueueHub.API.Domain.Queues.IQueueRepository));
                        if (queueRepoDescriptor != null)
                            services.Remove(queueRepoDescriptor);

                        // Ensure we're using the Bogus repository for testing
                        services.AddScoped<IUserRepository, BogusUserRepository>();
                        services.AddSingleton<GrandeTech.QueueHub.API.Domain.Queues.IQueueRepository, BogusQueueRepository>(); // Use singleton for queue repo
                        services.AddScoped<AddQueueService>();
                        services.AddScoped<AuthService>();
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

        private static async Task<string> CreateAndAuthenticateUserAsync(string role, HttpClient client)
        {
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

        // Add a DTO for test deserialization
        public class QueueDto
        {
            public Guid Id { get; set; }
            public Guid LocationId { get; set; }
            public int MaxSize { get; set; }
            public int LateClientCapTimeInMinutes { get; set; }
        }
    }
} 