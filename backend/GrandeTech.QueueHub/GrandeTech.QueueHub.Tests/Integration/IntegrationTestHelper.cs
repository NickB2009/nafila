using System;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Application.Auth;
using Grande.Fila.API.Domain.Users;
using Grande.Fila.API.Infrastructure.Repositories.Bogus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Grande.Fila.API.Tests.Integration
{
    public static class IntegrationTestHelper
    {
        public static async Task<string> CreateAndAuthenticateUserAsync(
            BogusUserRepository userRepository, IServiceProvider services, string role, string[] permissions)
        {
            using var scope = services.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<AuthService>();
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
            
            var user = new User($"testuser_{uniqueId}", $"test_{uniqueId}@example.com", BCrypt.Net.BCrypt.HashPassword("TestPassword123!"), mappedRole);
            
            // Disable 2FA for test users to simplify integration tests
            user.DisableTwoFactor();
            
            // Add permissions if specified (legacy support)
            foreach (var permission in permissions)
                user.AddPermission(permission);
                
            await userRepository.AddAsync(user, CancellationToken.None);
            await Task.Delay(100);
            
            var loginRequest = new LoginRequest { Username = user.Username, Password = "TestPassword123!" };
            try
            {
                var loginResult = await authService.LoginAsync(loginRequest);
                if (!loginResult.Success)
                {
                    Console.WriteLine($"Login failed for user {user.Username} with error: {loginResult.Error}");
                    Console.WriteLine($"User role: {user.Role}, mapped role: {mappedRole}");
                    Console.WriteLine($"User is active: {user.IsActive}");
                }
                Assert.IsTrue(loginResult.Success, $"Login failed for user {user.Username}. Error: {loginResult.Error}");
                Assert.IsNotNull(loginResult.Token, $"Token was null for user {user.Username}");
                return loginResult.Token;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occurred during login: {ex}");
                throw;
            }
        }
    }
} 