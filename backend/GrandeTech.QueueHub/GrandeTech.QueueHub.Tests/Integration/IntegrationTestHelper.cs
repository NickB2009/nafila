using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Application.Auth;
using Grande.Fila.API.Domain.Users;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Grande.Fila.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Grande.Fila.API.Tests.Integration
{
    public static class IntegrationTestHelper
    {
        public static async Task<string> CreateAndAuthenticateUserAsync(
            IServiceProvider services, string role, string[] permissions)
        {
            using var scope = services.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<AuthService>();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var dbContext = scope.ServiceProvider.GetRequiredService<QueueHubDbContext>();
            
            var uniqueId = Guid.NewGuid().ToString("N");
            
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
                "user" => UserRoles.Customer, // Default user becomes Customer
                _ => UserRoles.Customer // Default fallback
            };
            
            var user = new User($"testuser_{uniqueId}", $"test_{uniqueId}@example.com", $"+1234567890{uniqueId}", BCrypt.Net.BCrypt.HashPassword("TestPassword123!"), mappedRole);
            
            // Disable 2FA for test users to simplify integration tests
            user.DisableTwoFactor();
            
            // Add permissions if specified (legacy support)
            foreach (var permission in permissions)
                user.AddPermission(permission);
                
            await userRepository.AddAsync(user, CancellationToken.None);
            await dbContext.SaveChangesAsync();
            await Task.Delay(100);
            
            var loginRequest = new LoginRequest { PhoneNumber = user.PhoneNumber, Password = "TestPassword123!" };
            try
            {
                var loginResult = await authService.LoginAsync(loginRequest, CancellationToken.None);
                
                if (!loginResult.Success || string.IsNullOrEmpty(loginResult.Token))
                {
                    Console.WriteLine($"Login failed for user {user.FullName} with error: {loginResult.Error}");
                    throw new InvalidOperationException($"Authentication failed for user {user.FullName}: {loginResult.Error}");
                }
                
                Assert.IsNotNull(loginResult.Token, $"Token was null for user {user.FullName}");
                return loginResult.Token;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Authentication error for user {user.FullName}: {ex.Message}");
                throw;
            }
        }

        public static async Task<HttpClient> CreateAuthenticatedClientAsync(
            Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> factory, string role, string[] permissions)
        {
            var token = await CreateAndAuthenticateUserAsync(factory.Services, role, permissions);
            
            // Create a new HTTP client with the authentication header
            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            return client;
        }

        public static async Task<HttpClient> CreateAuthenticatedClientAsync(Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> factory, string role)
        {
            return await CreateAuthenticatedClientAsync(factory, role, Array.Empty<string>());
        }

        public static async Task<string> CreateAndAuthenticateUserAsync(IServiceProvider services, string role)
        {
            return await CreateAndAuthenticateUserAsync(services, role, Array.Empty<string>());
        }

        public static async Task<string> CreateAndAuthenticateUserAsync(
            IServiceProvider services, string role, string[] permissions, string? username = null)
        {
            return await CreateAndAuthenticateUserAsync(services, role, permissions);
        }

        public static async Task CleanupTestUserAsync(IServiceProvider services, string phoneNumber)
        {
            using var scope = services.CreateScope();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var dbContext = scope.ServiceProvider.GetRequiredService<QueueHubDbContext>();
            
            try
            {
                var user = await userRepository.GetByPhoneNumberAsync(phoneNumber, CancellationToken.None);
                if (user != null)
                {
                    dbContext.Users.Remove(user);
                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cleaning up test user {phoneNumber}: {ex.Message}");
            }
        }
    }
}