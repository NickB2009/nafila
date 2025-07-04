using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Grande.Fila.API.Application.Auth;
using Grande.Fila.API.Domain.Users;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using System;

namespace Grande.Fila.API.Tests.Integration.Controllers
{
    [TestClass]
    public class MaintenanceControllerIntegrationTests
    {
        private static WebApplicationFactory<Program> _factory = null!;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _factory = new WebApplicationFactory<Program>();
        }

        [TestMethod]
        public async Task ResetAverages_AsAdmin_ReturnsOk()
        {
            // Arrange
            var client = _factory.CreateClient();
            var adminToken = await CreateAndAuthenticateUserAsync("Admin", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            // Act
            var response = await client.PostAsync("/api/maintenance/reset-averages", null);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResetAvgResponseDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public async Task ResetAverages_AsUser_ReturnsForbidden()
        {
            // Arrange
            var client = _factory.CreateClient();
            var userToken = await CreateAndAuthenticateUserAsync("User", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

            // Act
            var response = await client.PostAsync("/api/maintenance/reset-averages", null);

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
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

            var loginRequest = new LoginRequest { Username = username, Password = password };
            var loginJson = JsonSerializer.Serialize(loginRequest);
            var response = await client.PostAsync("/api/auth/login", new StringContent(loginJson, Encoding.UTF8, "application/json"));
            var respContent = await response.Content.ReadAsStringAsync();
            var loginResult = JsonSerializer.Deserialize<LoginResult>(respContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.IsNotNull(loginResult);
            Assert.IsTrue(loginResult.Success);
            return loginResult.Token!;
        }

        private class ResetAvgResponseDto
        {
            public bool Success { get; set; }
            public int ResetCount { get; set; }
        }
    }
} 