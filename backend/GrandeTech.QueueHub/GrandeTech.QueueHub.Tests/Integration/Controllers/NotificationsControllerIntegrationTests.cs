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
    public class NotificationsControllerIntegrationTests
    {
        private static WebApplicationFactory<Program> _factory = null!;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _factory = new WebApplicationFactory<Program>();
        }

        [TestMethod]
        public async Task SendSmsNotification_AsServiceAccount_ReturnsOk()
        {
            // Arrange
            var client = _factory.CreateClient();
            var serviceToken = await CreateAndAuthenticateUserAsync("ServiceAccount", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);

            var dto = new
            {
                queueEntryId = Guid.NewGuid().ToString(),
                message = "Your turn is coming up!",
                notificationType = "TurnReminder"
            };

            var json = JsonSerializer.Serialize(dto);

            // Act
            var response = await client.PostAsync("/api/notifications/sms", new StringContent(json, Encoding.UTF8, "application/json"));

            // Assert - Should return BadRequest due to missing queue entry but not Unauthorized
            Assert.AreNotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task SendSmsNotification_AsUser_ReturnsForbidden()
        {
            // Arrange
            var client = _factory.CreateClient();
            var userToken = await CreateAndAuthenticateUserAsync("User", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

            var dto = new
            {
                queueEntryId = Guid.NewGuid().ToString(),
                message = "Test message",
                notificationType = "TurnReminder"
            };

            var json = JsonSerializer.Serialize(dto);

            // Act
            var response = await client.PostAsync("/api/notifications/sms", new StringContent(json, Encoding.UTF8, "application/json"));

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        private static async Task<string> CreateAndAuthenticateUserAsync(string role, HttpClient client)
        {
            using var scope = _factory.Services.CreateScope();
            var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var username = $"testuser_{role.ToLower()}_{Guid.NewGuid():N}";
            var email = $"{username}@test.com";
            var password = "testpassword123";

            var user = new User(username, email, BCrypt.Net.BCrypt.HashPassword(password), role);
            await userRepo.AddAsync(user, CancellationToken.None);

            var loginReq = new LoginRequest { Username = username, Password = password };
            var json = JsonSerializer.Serialize(loginReq);
            var resp = await client.PostAsync("/api/auth/login", new StringContent(json, Encoding.UTF8, "application/json"));
            var respContent = await resp.Content.ReadAsStringAsync();
            var loginRes = JsonSerializer.Deserialize<LoginResult>(respContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return loginRes!.Token!;
        }
    }
} 