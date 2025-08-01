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
using Grande.Fila.API.Infrastructure.Repositories.Bogus;
using System.Linq;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.Locations;
using Grande.Fila.API.Domain.Common.ValueObjects;
using Grande.Fila.API.Application.QrCode;
using Grande.Fila.API.Infrastructure;
using System.Net.Http.Json;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;

namespace Grande.Fila.API.Tests.Integration.Controllers
{
    [TestClass]
    public class QrCodeControllerIntegrationTests
    {
        private static WebApplicationFactory<Program> _factory = null!;
        private static BogusUserRepository _userRepository = null!;
        private static BogusQueueRepository _queueRepository = null!;
        private static BogusLocationRepository _locationRepository = null!;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _userRepository = new BogusUserRepository();
            _queueRepository = new BogusQueueRepository();
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
                        // Remove existing registrations
                        var servicesToRemove = new[] { typeof(IUserRepository), typeof(IQueueRepository), typeof(ILocationRepository), typeof(IQrCodeGenerator) };
                        var descriptors = services.Where(d => servicesToRemove.Contains(d.ServiceType)).ToList();
                        descriptors.ForEach(d => services.Remove(d));
                        
                        // Add test-specific services
                        services.AddSingleton<IUserRepository>(_userRepository);
                        services.AddSingleton<IQueueRepository>(_queueRepository);
                        services.AddSingleton<ILocationRepository>(_locationRepository);
                        services.AddSingleton<IQrCodeGenerator, MockQrCodeGenerator>();
                        services.AddScoped<AuthService>();
                        services.AddScoped<QrJoinService>();
                        
                        // Add logging
                        services.AddLogging();
                        
                        // Add missing authorization policies
                        services.AddAuthorization(options =>
                        {
                            options.AddPolicy("RequireStaff", policy =>
                                policy.Requirements.Add(new Grande.Fila.API.Infrastructure.Authorization.TenantRequirement(UserRoles.Staff, requireLocationContext: true)));
                        });
                        
                        // Add authorization handler and context service
                        services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, Grande.Fila.API.Infrastructure.Authorization.TenantAuthorizationHandler>();
                        services.AddScoped<Grande.Fila.API.Infrastructure.Authorization.ITenantContextService, Grande.Fila.API.Infrastructure.Authorization.TenantContextService>();
                    });
                });
        }

        [TestMethod]
        public async Task GenerateQrCode_AsStaff_ReturnsOk()
        {
            // Arrange
            var client = _factory.CreateClient();
            var staffToken = await CreateAndAuthenticateUserAsync("Staff", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", staffToken);

            var location = new Location(
                "Test Location",
                "test-location",
                "A test location for integration tests.",
                Guid.NewGuid(),
                Address.Create(
                    "123 Test St",
                    "1",
                    "",
                    "Downtown",
                    "Testville",
                    "Test State",
                    "USA",
                    "12345"
                ),
                "+5511999999999",
                "test@location.com",
                new TimeSpan(9, 0, 0),
                new TimeSpan(17, 0, 0),
                100,
                15,
                "test_user"
            );
            await _locationRepository.AddAsync(location, CancellationToken.None);

            var dto = new
            {
                locationId = location.Id.ToString(),
                serviceTypeId = Guid.NewGuid().ToString(),
                expiryMinutes = "30"
            };

            var json = JsonSerializer.Serialize(dto);

            // Act
            var response = await client.PostAsync("/api/qrcode/generate", new StringContent(json, Encoding.UTF8, "application/json"));



            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<GenerateQrResponseDto>();
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.QrCodeBase64);
        }

        [TestMethod]
        public async Task GenerateQrCode_AsUser_ReturnsForbidden()
        {
            // Arrange
            var client = _factory.CreateClient();
            var userToken = await CreateAndAuthenticateUserAsync("User", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

            var dto = new
            {
                locationId = Guid.NewGuid().ToString(),
                expiryMinutes = "60"
            };

            var json = JsonSerializer.Serialize(dto);

            // Act
            var response = await client.PostAsync("/api/qrcode/generate", new StringContent(json, Encoding.UTF8, "application/json"));

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [TestMethod]
        public async Task GenerateQrCode_InvalidLocationId_ReturnsBadRequest()
        {
            // Arrange
            var client = _factory.CreateClient();
            var staffToken = await CreateAndAuthenticateUserAsync("Staff", client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", staffToken);

            var dto = new
            {
                locationId = "invalid-guid",
                expiryMinutes = "60"
            };

            var json = JsonSerializer.Serialize(dto);

            // Act
            var response = await client.PostAsync("/api/qrcode/generate", new StringContent(json, Encoding.UTF8, "application/json"));



            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        private static async Task<string> CreateAndAuthenticateUserAsync(string role, HttpClient client)
        {
            using var scope = _factory.Services.CreateScope();
            var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var authService = scope.ServiceProvider.GetRequiredService<AuthService>();
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
            var loginResult = await authService.LoginAsync(loginReq);
            
            Assert.IsTrue(loginResult.Success, $"Login failed: {loginResult.Error}");
            Assert.IsNotNull(loginResult.Token);

            return loginResult.Token!;
        }

        private class GenerateQrResponseDto
        {
            public bool Success { get; set; }
            public string? QrCodeBase64 { get; set; }
        }
    }
} 