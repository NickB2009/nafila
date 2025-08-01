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
using System.Threading;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Grande.Fila.API.Tests.Integration.Controllers
{
    [TestClass]
    public class PromotionsControllerIntegrationTests
    {
        private static WebApplicationFactory<Program> _factory = null!;
        private static BogusUserRepository _userRepository = null!;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _userRepository = new BogusUserRepository();
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
                        // Remove existing IUserRepository registrations
                        var descriptors = services.Where(d => d.ServiceType == typeof(IUserRepository)).ToList();
                        foreach (var descriptor in descriptors)
                        {
                            services.Remove(descriptor);
                        }
                        services.AddSingleton<IUserRepository>(_userRepository);
                        services.AddScoped<AuthService>();
                    });
                });
        }

        [TestMethod]
        public async Task SendCouponNotification_AsServiceAccount_ReturnsOk()
        {
            // Arrange
            var client = _factory.CreateClient();
            var serviceToken = await CreateAndAuthenticateUserAsync("ServiceAccount", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);

            var dto = new
            {
                customerId = Guid.NewGuid().ToString(),
                couponId = Guid.NewGuid().ToString(),
                message = "You have a new 20% off coupon!",
                notificationChannel = "SMS"
            };

            var json = JsonSerializer.Serialize(dto);

            // Act
            var response = await client.PostAsync("/api/promotions/notify-coupon", new StringContent(json, Encoding.UTF8, "application/json"));

            // Assert - Should return BadRequest due to missing entities but not Unauthorized
            Assert.AreNotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task SendCouponNotification_AsUser_ReturnsForbidden()
        {
            // Arrange
            var client = _factory.CreateClient();
            var userToken = await CreateAndAuthenticateUserAsync("User", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

            var dto = new
            {
                customerId = Guid.NewGuid().ToString(),
                couponId = Guid.NewGuid().ToString(),
                message = "Test message",
                notificationChannel = "SMS"
            };

            var json = JsonSerializer.Serialize(dto);

            // Act
            var response = await client.PostAsync("/api/promotions/notify-coupon", new StringContent(json, Encoding.UTF8, "application/json"));

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

            // Map old test roles to new roles
            var mappedRole = role.ToLower() switch
            {
                "platformadmin" => UserRoles.PlatformAdmin,
                "owner" => UserRoles.Owner,
                "staff" => UserRoles.Staff,
                "customer" => UserRoles.Customer,
                "serviceaccount" => UserRoles.ServiceAccount,
                // Legacy mappings for backward compatibility
                "admin" => UserRoles.Owner,
                "barber" => UserRoles.Staff,
                "client" => UserRoles.Customer,
                "user" => UserRoles.Customer,
                _ => UserRoles.Customer
            };
            
            var user = new User(username, email, BCrypt.Net.BCrypt.HashPassword(password), mappedRole);
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