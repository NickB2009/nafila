using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Grande.Fila.API.Application.Auth;
using Grande.Fila.API.Domain.Users;
using Grande.Fila.API.Infrastructure.Repositories.Bogus;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Grande.Fila.API.Tests.Integration;
using Grande.Fila.API.Domain.Locations;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Microsoft.AspNetCore.Hosting;

namespace Grande.Fila.API.Tests.Integration.Controllers
{
    [TestClass]
    [TestCategory("Integration")]
    public class MaintenanceControllerIntegrationTests
    {
        private WebApplicationFactory<Program> _factory;
        private HttpClient _client;
        private static BogusUserRepository _userRepository;
        private static BogusLocationRepository _locationRepository;

        [TestInitialize]
        public void TestInitialize()
        {
            if (_userRepository == null)
                _userRepository = new BogusUserRepository();
            if (_locationRepository == null)
                _locationRepository = new BogusLocationRepository();
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
                            ["Database:UseSqlDatabase"] = "false",
                            ["Database:UseInMemoryDatabase"] = "false",
                            ["Database:UseBogusRepositories"] = "true",
                            ["Database:AutoMigrate"] = "false",
                            ["Database:SeedData"] = "false"
                        });
                    });
                    builder.ConfigureServices(services =>
                    {
                        // Remove existing repository registrations
                        var servicesToRemove = new[]
                        {
                            typeof(IUserRepository),
                            typeof(ILocationRepository)
                        };
                        var descriptors = services.Where(d => servicesToRemove.Contains(d.ServiceType)).ToList();
                        foreach (var descriptor in descriptors)
                        {
                            services.Remove(descriptor);
                        }
                        services.AddSingleton<IUserRepository>(_userRepository);
                        services.AddSingleton<ILocationRepository>(_locationRepository);
                        services.AddScoped<AuthService>();
                    });
                });
            _client = _factory.CreateClient();
        }

        [TestMethod]
        public async Task ResetAverages_AsAdmin_ReturnsOk()
        {
            var adminToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "Admin", new string[0]);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var response = await _client.PostAsync("/api/maintenance/reset-averages", null);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResetAvgResponseDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public async Task ResetAverages_AsUser_ReturnsForbidden()
        {
            var userToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "User", new string[0]);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

            var response = await _client.PostAsync("/api/maintenance/reset-averages", null);

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        private class ResetAvgResponseDto
        {
            public bool Success { get; set; }
            public int ResetCount { get; set; }
        }
    }
} 