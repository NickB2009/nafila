using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using Grande.Fila.API.Application.Auth;
using Grande.Fila.API.Domain.Users;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System;

namespace Grande.Fila.API.Tests.Integration.Controllers
{
    [TestClass]
    public class CacheControllerIntegrationTests
    {
        private static WebApplicationFactory<Program> _factory = null!;
        private static Grande.Fila.API.Infrastructure.Repositories.Bogus.BogusUserRepository _userRepository = null!;
        private static Grande.Fila.API.Infrastructure.Repositories.Bogus.BogusLocationRepository _locationRepository = null!;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _userRepository = new Grande.Fila.API.Infrastructure.Repositories.Bogus.BogusUserRepository();
            _locationRepository = new Grande.Fila.API.Infrastructure.Repositories.Bogus.BogusLocationRepository();
            
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        // Remove any existing repository registrations
                        var servicesToRemove = new[]
                        {
                            typeof(IUserRepository),
                            typeof(Grande.Fila.API.Domain.Locations.ILocationRepository)
                        };
                        var descriptors = services.Where(d => servicesToRemove.Contains(d.ServiceType)).ToList();
                        descriptors.ForEach(d => services.Remove(d));

                        // Use shared singleton repositories for testing
                        services.AddSingleton<IUserRepository>(_userRepository);
                        services.AddSingleton<Grande.Fila.API.Domain.Locations.ILocationRepository>(_locationRepository);
                    });
                });
        }

        [TestMethod]
        public async Task UpdateWaitAverage_AsAdmin_ReturnsOk()
        {
            // Arrange
            var client = _factory.CreateClient();
            var organizationId = Guid.NewGuid();
            var adminToken = await CreateAndAuthenticateUserAsync("Admin", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var locationId = Guid.NewGuid().ToString();
            // Insert dummy location into repo so existence check passes
            using (var scope = _factory.Services.CreateScope())
            {
                var locationRepo = scope.ServiceProvider.GetRequiredService<Grande.Fila.API.Domain.Locations.ILocationRepository>();
                var address = Grande.Fila.API.Domain.Common.ValueObjects.Address.Create("Street", "", "", "", "City", "State", "Country", "12345");
                var loc = new Grande.Fila.API.Domain.Locations.Location("Loc", "loc", "desc", organizationId, address, null, null, TimeSpan.FromHours(8), TimeSpan.FromHours(18), 100, 15, "seed");
                // hack: set id to known
                typeof(Grande.Fila.API.Domain.Common.BaseEntity).GetProperty("Id")!.SetValue(loc, Guid.Parse(locationId));
                await locationRepo.AddAsync(loc, CancellationToken.None);
            }

            var dto = new { averageServiceTimeMinutes = 11.2 };
            var json = JsonSerializer.Serialize(dto);
            var response = await client.PutAsync($"/api/cache/wait-time/locations/{locationId}", new StringContent(json, Encoding.UTF8, "application/json"));
            var responseBody = await response.Content.ReadAsStringAsync();

            // Assert
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Assert.Fail($"Expected 200 OK but got {(int)response.StatusCode}: {response.StatusCode}. Response body: {responseBody}");
            }
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            // Optionally, assert response body structure
            var result = JsonSerializer.Deserialize<ResponseDto>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.IsTrue(result!.Success);
        }

        [TestMethod]
        public async Task UpdateWaitAverage_InvalidId_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            var adminToken = await CreateAndAuthenticateUserAsync("Admin", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var dto = new { averageServiceTimeMinutes = 10 };
            var json = JsonSerializer.Serialize(dto);
            var response = await client.PutAsync($"/api/cache/wait-time/locations/not-a-guid", new StringContent(json, Encoding.UTF8, "application/json"));
            var responseBody = await response.Content.ReadAsStringAsync();

            // Assert
            if (response.StatusCode != HttpStatusCode.BadRequest)
            {
                Assert.Fail($"Expected 400 BadRequest but got {(int)response.StatusCode}: {response.StatusCode}. Response body: {responseBody}");
            }
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            // Optionally, assert response body structure
            var result = JsonSerializer.Deserialize<ResponseDto>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.IsFalse(result!.Success);
            Assert.IsTrue(result.Errors.Length > 0);
        }

        private static async Task<string> CreateAndAuthenticateUserAsync(string role, HttpClient client, Guid? organizationId = null)
        {
            using var scope = _factory.Services.CreateScope();
            var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var username = $"testuser_{role.ToLower()}_{Guid.NewGuid():N}";
            var email = $"{username}@test.com";
            var password = "testpassword123";

            var user = new User(username, email, BCrypt.Net.BCrypt.HashPassword(password), role);
            // Disable 2FA for test users to avoid additional verification steps
            user.DisableTwoFactor();
            
            if (organizationId.HasValue)
            {
                // Set organizationId via reflection if possible
                var orgIdProp = user.GetType().GetProperty("OrganizationId");
                if (orgIdProp != null && orgIdProp.CanWrite)
                {
                    orgIdProp.SetValue(user, organizationId.Value);
                }
            }
            await userRepo.AddAsync(user, CancellationToken.None);

            var loginReq = new LoginRequest { Username = username, Password = password };
            var json = JsonSerializer.Serialize(loginReq);
            var resp = await client.PostAsync("/api/auth/login", new StringContent(json, Encoding.UTF8, "application/json"));
            
            if (!resp.IsSuccessStatusCode)
            {
                var errorContent = await resp.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Login failed with status {resp.StatusCode}: {errorContent}");
            }
            
            var respContent = await resp.Content.ReadAsStringAsync();
            var loginRes = JsonSerializer.Deserialize<LoginResult>(respContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            if (loginRes == null || !loginRes.Success || string.IsNullOrEmpty(loginRes.Token))
            {
                throw new InvalidOperationException($"Login did not return a valid token. Success: {loginRes?.Success}, RequiresTwoFactor: {loginRes?.RequiresTwoFactor}, Error: {loginRes?.Error}");
            }
            
            return loginRes.Token;
        }

        private class ResponseDto
        {
            public bool Success { get; set; }
            public string[] Errors { get; set; } = new string[0];
        }
    }
} 