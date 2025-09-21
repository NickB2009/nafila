using Grande.Fila.API.Application.Auth;
using Grande.Fila.API.Domain.Users;
using Grande.Fila.API.Infrastructure;
using Grande.Fila.API.Infrastructure.Repositories.Bogus;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading;
using System;

namespace Grande.Fila.Tests.Integration.Controllers
{
    [TestClass]
    [TestCategory("Integration")]
    public class AuthControllerIntegrationTests
    {
        private static WebApplicationFactory<Program> _factory;
        private static BogusUserRepository _userRepository;
        private HttpClient _client;

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
                            ["Jwt:Audience"] = "Grande.Fila.API.Test"
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
                        // Use shared repository instance for testing
                        services.AddSingleton<IUserRepository>(_userRepository);
                        services.AddScoped<AuthService>();
                    });
                });
        }

        [TestInitialize]
        public void Setup()
        {
            _client = _factory.CreateClient();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            _factory?.Dispose();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _client?.Dispose();
        }

        [TestMethod]
        public async Task Login_WithValidCredentials_ReturnsSuccessResult()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                FullName = "Test User",
                PhoneNumber = "+12345678901",
                Email = "test@example.com",
                Password = "Password123"
            };

            var registerJson = JsonSerializer.Serialize(registerRequest);
            var registerContent = new StringContent(registerJson, Encoding.UTF8, "application/json");

            // Register the user first
            var registerResponse = await _client.PostAsync("/api/auth/register", registerContent);
            registerResponse.EnsureSuccessStatusCode();

            // Now test login
            var loginRequest = new LoginRequest
            {
                PhoneNumber = "+12345678901",
                Password = "Password123"
            };

            var loginJson = JsonSerializer.Serialize(loginRequest);
            var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");

            // Act
            var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, loginResponse.StatusCode);

            var responseContent = await loginResponse.Content.ReadAsStringAsync();
            var loginResult = JsonSerializer.Deserialize<LoginResult>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(loginResult);
            Assert.IsTrue(loginResult.Success);
            Assert.IsNotNull(loginResult.Token);
            Assert.AreEqual("Test User", loginResult.Username);
            Assert.AreEqual("Customer", loginResult.Role);
        }

        [TestMethod]
        public async Task Login_WithInvalidCredentials_ReturnsBadRequest()
        {
            // Arrange
            Assert.IsNotNull(_client);

            var loginRequest = new LoginRequest
            {
                PhoneNumber = "+12345678902",
                Password = "wrongpassword"
            };

            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/auth/login", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var loginResult = JsonSerializer.Deserialize<LoginResult>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(loginResult);
            Assert.IsFalse(loginResult.Success);
            Assert.IsNotNull(loginResult.Error);
        }

        [TestMethod]
        public async Task Login_WithExistingBogusUser_ReturnsSuccessResult()
        {
            // Arrange - Get a user from the Bogus repository
            Assert.IsNotNull(_factory);
            Assert.IsNotNull(_client);

            using var scope = _factory.Services.CreateScope();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            // Create a test user with known credentials in the repository
            var knownUser = new User("integrationtestuser", "integration@test.com",
                "+12345678903", BCrypt.Net.BCrypt.HashPassword("Password123"), "User");
            await userRepository.AddAsync(knownUser, CancellationToken.None);

            var loginRequest = new LoginRequest
            {
                PhoneNumber = "+12345678903",
                Password = "Password123"
            };

            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/auth/login", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var loginResult = JsonSerializer.Deserialize<LoginResult>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(loginResult);
            Assert.IsTrue(loginResult.Success);
            Assert.IsNotNull(loginResult.Token);
            Assert.AreEqual("integrationtestuser", loginResult.Username);
        }

        [TestMethod]
        public async Task Register_WithValidData_ReturnsSuccessResult()
        {
            // Arrange
            Assert.IsNotNull(_client);

            var registerRequest = new RegisterRequest
            {
                FullName = "New User",
                PhoneNumber = "+12345678904",
                Email = "newuser@example.com",
                Password = "SecurePassword123"
            };

            var json = JsonSerializer.Serialize(registerRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/auth/register", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var registerResult = JsonSerializer.Deserialize<RegisterResult>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(registerResult);
            Assert.IsTrue(registerResult.Success);
        }

        [TestMethod]
        public async Task Register_WithDuplicateUsername_ReturnsBadRequest()
        {
            // First registration
            var firstRequest = new RegisterRequest
            {
                FullName = "First User",
                PhoneNumber = "+12345678905",
                Email = "first@example.com",
                Password = "Password123"
            };

            var firstJson = JsonSerializer.Serialize(firstRequest);
            var firstContent = new StringContent(firstJson, Encoding.UTF8, "application/json");
            var firstResponse = await _client.PostAsync("/api/auth/register", firstContent);
            Assert.AreEqual(HttpStatusCode.OK, firstResponse.StatusCode, 
                $"First registration should succeed but got {firstResponse.StatusCode}: {await firstResponse.Content.ReadAsStringAsync()}");

            // Second registration with same username
            var secondRequest = new RegisterRequest
            {
                FullName = "Second User",
                PhoneNumber = "+12345678905", // Same phone number
                Email = "second@example.com",    // Different email
                Password = "Password123"
            };

            var secondJson = JsonSerializer.Serialize(secondRequest);
            var secondContent = new StringContent(secondJson, Encoding.UTF8, "application/json");
            var secondResponse = await _client.PostAsync("/api/auth/register", secondContent);
            var responseContent = await secondResponse.Content.ReadAsStringAsync();
            
            // Should return BadRequest with Username field error
            Assert.AreEqual(HttpStatusCode.BadRequest, secondResponse.StatusCode, 
                $"Expected BadRequest but got {secondResponse.StatusCode}: {responseContent}");

            var result = JsonSerializer.Deserialize<RegisterResult>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(result, "Response should deserialize to RegisterResult");
            Assert.IsFalse(result.Success, "Second registration should fail");
            Assert.IsNotNull(result.FieldErrors, "Should have field errors");
            Assert.IsTrue(result.FieldErrors.ContainsKey("phoneNumber"), 
                $"Should have phoneNumber field error. Actual errors: {string.Join(", ", result.FieldErrors.Keys)}");
        }

        [TestMethod]
        public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
        {
            // First registration
            var firstRequest = new RegisterRequest
            {
                FullName = "First User",
                PhoneNumber = "+12345678906",
                Email = "duplicate@example.com",
                Password = "Password123"
            };

            var firstJson = JsonSerializer.Serialize(firstRequest);
            var firstContent = new StringContent(firstJson, Encoding.UTF8, "application/json");
            var firstResponse = await _client.PostAsync("/api/auth/register", firstContent);
            Assert.AreEqual(HttpStatusCode.OK, firstResponse.StatusCode, 
                $"First registration should succeed but got {firstResponse.StatusCode}: {await firstResponse.Content.ReadAsStringAsync()}");

            // Second registration with same email
            var secondRequest = new RegisterRequest
            {
                FullName = "Second User",
                PhoneNumber = "+12345678907",        // Different phone number
                Email = "duplicate@example.com", // Same email
                Password = "Password123"
            };

            var secondJson = JsonSerializer.Serialize(secondRequest);
            var secondContent = new StringContent(secondJson, Encoding.UTF8, "application/json");
            var secondResponse = await _client.PostAsync("/api/auth/register", secondContent);
            var responseContent = await secondResponse.Content.ReadAsStringAsync();
            
            // Should return BadRequest with Email field error
            Assert.AreEqual(HttpStatusCode.BadRequest, secondResponse.StatusCode, 
                $"Expected BadRequest but got {secondResponse.StatusCode}: {responseContent}");

            var result = JsonSerializer.Deserialize<RegisterResult>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(result, "Response should deserialize to RegisterResult");
            Assert.IsFalse(result.Success, "Second registration should fail");
            Assert.IsNotNull(result.FieldErrors, "Should have field errors");
            Assert.IsTrue(result.FieldErrors.ContainsKey("email"), 
                $"Should have email field error. Actual errors: {string.Join(", ", result.FieldErrors.Keys)}");
        }


        [TestMethod]
        public async Task Register_WithEmptyFields_ReturnsBadRequest()
        {
            // Arrange
            Assert.IsNotNull(_client);

            var registerRequest = new RegisterRequest
            {
                FullName = "",
                PhoneNumber = "",
                Email = "",
                Password = ""
            };

            var json = JsonSerializer.Serialize(registerRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/auth/register", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var registerResult = JsonSerializer.Deserialize<RegisterResult>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(registerResult);
            Assert.IsFalse(registerResult.Success);
            Assert.IsNotNull(registerResult.FieldErrors);
            Assert.IsTrue(registerResult.FieldErrors.Count >= 3); // Username, Email, Password should all have errors
        }

        [TestMethod]
        public async Task Auth_EndToEndFlow_RegisterThenLogin_WorksCorrectly()
        {
            // Arrange
            Assert.IsNotNull(_client);

            var username = "endtoenduser";
            var email = "endtoend@example.com";
            var password = "SecurePassword123";

            var registerRequest = new RegisterRequest
            {
                FullName = "Test User",
                PhoneNumber = username,
                Email = email,
                Password = password
            };

            // Act 1: Register
            var registerJson = JsonSerializer.Serialize(registerRequest);
            var registerContent = new StringContent(registerJson, Encoding.UTF8, "application/json");
            var registerResponse = await _client.PostAsync("/api/auth/register", registerContent);

            // Assert 1: Registration successful
            Assert.AreEqual(HttpStatusCode.OK, registerResponse.StatusCode);

            // Act 2: Login with the registered credentials
            var loginRequest = new LoginRequest
            {
                PhoneNumber = username,
                Password = password
            };

            var loginJson = JsonSerializer.Serialize(loginRequest);
            var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");
            var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);

            // Assert 2: Login successful
            Assert.AreEqual(HttpStatusCode.OK, loginResponse.StatusCode);

            var responseContent = await loginResponse.Content.ReadAsStringAsync();
            var loginResult = JsonSerializer.Deserialize<LoginResult>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(loginResult);
            Assert.IsTrue(loginResult.Success);
            Assert.IsNotNull(loginResult.Token);
            Assert.AreEqual("Test User", loginResult.Username);
            Assert.AreEqual("Customer", loginResult.Role);
        }

        [TestMethod]
        public async Task Login_AsOwner_WithTwoFactor_ReturnsTwoFactorRequired()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_factory);

            using var scope = _factory.Services.CreateScope();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            // Create an owner user
            var username = $"owner_{Guid.NewGuid():N}";
            var email = $"{username}@test.com";
            var password = "OwnerPass123";

            var user = new User("Test User", email, username, BCrypt.Net.BCrypt.HashPassword(password), UserRoles.Owner);
            user.EnableTwoFactor();
            await userRepository.AddAsync(user, CancellationToken.None);

            // Act
            var loginRequest = new LoginRequest
            {
                PhoneNumber = username,
                Password = password
            };

            var loginJson = JsonSerializer.Serialize(loginRequest);
            var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");

            var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, loginResponse.StatusCode);

            var responseContent = await loginResponse.Content.ReadAsStringAsync();
            var loginResult = JsonSerializer.Deserialize<LoginResult>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(loginResult);
            Assert.IsFalse(loginResult.Success);
            Assert.IsTrue(loginResult.RequiresTwoFactor);
            Assert.IsNotNull(loginResult.TwoFactorToken);
            Assert.IsNull(loginResult.Token);
        }

        [TestMethod]
        public async Task CompleteTwoFactorFlow_WithValidCode_ReturnsSuccess()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_factory);

            using var scope = _factory.Services.CreateScope();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            // Create an owner user
            var username = $"owner_{Guid.NewGuid():N}";
            var email = $"{username}@test.com";
            var password = "OwnerPass123";

            var user = new User("Test User", email, username, BCrypt.Net.BCrypt.HashPassword(password), UserRoles.Owner);
            user.EnableTwoFactor();
            await userRepository.AddAsync(user, CancellationToken.None);

            // Step 1: Initial login to get 2FA token
            var loginRequest = new LoginRequest
            {
                PhoneNumber = username,
                Password = password
            };

            var loginJson = JsonSerializer.Serialize(loginRequest);
            var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");

            var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);
            Assert.AreEqual(HttpStatusCode.BadRequest, loginResponse.StatusCode);

            var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();
            var loginResult = JsonSerializer.Deserialize<LoginResult>(loginResponseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(loginResult);
            Assert.IsTrue(loginResult.RequiresTwoFactor);
            Assert.IsNotNull(loginResult.TwoFactorToken);

            // Step 2: Verify 2FA code
            var verifyRequest = new VerifyTwoFactorRequest
            {
                PhoneNumber = username,
                TwoFactorCode = "123456", // In a real implementation, this would be validated
                TwoFactorToken = loginResult.TwoFactorToken
            };

            var verifyJson = JsonSerializer.Serialize(verifyRequest);
            var verifyContent = new StringContent(verifyJson, Encoding.UTF8, "application/json");

            var verifyResponse = await _client.PostAsync("/api/auth/verify-2fa", verifyContent);

            // Assert final result
            Assert.AreEqual(HttpStatusCode.OK, verifyResponse.StatusCode);

            var verifyResponseContent = await verifyResponse.Content.ReadAsStringAsync();
            var verifyResult = JsonSerializer.Deserialize<LoginResult>(verifyResponseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(verifyResult);
            Assert.IsTrue(verifyResult.Success);
            Assert.IsNotNull(verifyResult.Token);
            Assert.AreEqual("Test User", verifyResult.Username);
            Assert.AreEqual(UserRoles.Owner, verifyResult.Role);
            Assert.IsNotNull(verifyResult.Permissions);
            Assert.IsTrue(verifyResult.Permissions.Length > 0);
        }

        [TestMethod]
        public async Task Login_AsLockedOwner_ReturnsLockedError()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_factory);

            using var scope = _factory.Services.CreateScope();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            // Create a locked owner user
            var username = $"owner_{Guid.NewGuid():N}";
            var email = $"{username}@test.com";
            var password = "OwnerPass123";

            var user = new User("Test User", email, username, BCrypt.Net.BCrypt.HashPassword(password), UserRoles.Owner);
            user.Lock();
            await userRepository.AddAsync(user, CancellationToken.None);

            // Act
            var loginRequest = new LoginRequest
            {
                PhoneNumber = username,
                Password = password
            };

            var loginJson = JsonSerializer.Serialize(loginRequest);
            var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");

            var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, loginResponse.StatusCode);

            var responseContent = await loginResponse.Content.ReadAsStringAsync();
            var loginResult = JsonSerializer.Deserialize<LoginResult>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(loginResult);
            Assert.IsFalse(loginResult.Success);
            Assert.AreEqual("Account is locked. Please contact support.", loginResult.Error);
        }
    }
}
