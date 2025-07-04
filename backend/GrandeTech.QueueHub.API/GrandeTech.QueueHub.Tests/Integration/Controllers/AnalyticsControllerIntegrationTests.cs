using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using Grande.Fila.API.Application.Auth;
using Grande.Fila.API.Domain.Users;
using Grande.Fila.API.Infrastructure.Repositories.Bogus;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using System;
using Grande.Fila.API.Tests.Integration;

namespace Grande.Fila.API.Tests.Integration.Controllers
{
    [TestClass]
    public class AnalyticsControllerIntegrationTests
    {
        private static WebApplicationFactory<Program> _factory;
        private HttpClient _client;
        private static BogusUserRepository _userRepository;

        [TestInitialize]
        public void TestInitialize()
        {
            if (_userRepository == null)
                _userRepository = new BogusUserRepository();

            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        var descriptors = services.Where(d => d.ServiceType == typeof(IUserRepository)).ToList();
                        foreach (var descriptor in descriptors)
                            services.Remove(descriptor);
                        services.AddSingleton<IUserRepository>(_userRepository);
                        services.AddScoped<AuthService>();
                    });
                });
            _client = _factory.CreateClient();
        }

        [TestMethod]
        public async Task CalculateWaitTime_AsUser_ReturnsForbidden()
        {
            var userToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "User", new string[0]);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

            var dto = new
            {
                locationId = Guid.NewGuid().ToString(),
                queueId = Guid.NewGuid().ToString(),
                entryId = Guid.NewGuid().ToString()
            };

            var json = JsonSerializer.Serialize(dto);

            // Act
            var response = await _client.PostAsync("/api/analytics/calculate-wait", new StringContent(json, Encoding.UTF8, "application/json"));

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
} 