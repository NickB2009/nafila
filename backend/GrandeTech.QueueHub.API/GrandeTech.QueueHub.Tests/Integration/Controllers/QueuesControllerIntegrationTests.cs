using System;
using System.Collections.Generic;
using System.Linq;
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
using Grande.Fila.API.Infrastructure.Repositories.Bogus;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Grande.Fila.API.Tests.Integration;
using Grande.Fila.API.Infrastructure;

namespace Grande.Fila.Tests.Integration.Controllers
{
    [TestClass]
    [TestCategory("Integration")]
    public class QueuesControllerIntegrationTests
    {
        private WebApplicationFactory<Program> _factory;
        private HttpClient _client;
        private BogusUserRepository _userRepository;
        private BogusQueueRepository _queueRepository;

        [TestInitialize]
        public void TestInitialize()
        {
            _userRepository = new BogusUserRepository();
            _queueRepository = new BogusQueueRepository();
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
                            d.ServiceType == typeof(IUserRepository) ||
                            d.ServiceType == typeof(IQueueRepository)).ToList();
                        foreach (var descriptor in descriptors)
                        {
                            services.Remove(descriptor);
                        }
                        services.AddSingleton<IUserRepository>(_userRepository);
                        services.AddSingleton<IQueueRepository>(_queueRepository);
                        services.AddScoped<AuthService>();
                        services.AddScoped<AddQueueService>();
                    });
                });
            _client = _factory.CreateClient();
        }

        [TestMethod]
        public async Task AddQueue_ValidRequest_ReturnsCreated()
        {
            var adminToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "Admin", new[] { Permission.CreateStaff });
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var request = new AddQueueRequest
            {
                LocationId = Guid.Parse("12345678-1234-1234-1234-123456789012"),
                MaxSize = 50,
                LateClientCapTimeInMinutes = 15
            };

            var response = await _client.PostAsJsonAsync("/api/queues", request);
            var result = await response.Content.ReadFromJsonAsync<AddQueueResult>();

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.AreNotEqual(Guid.Empty, result.QueueId);
            Assert.IsNull(result.ErrorMessage);
        }
    }
} 