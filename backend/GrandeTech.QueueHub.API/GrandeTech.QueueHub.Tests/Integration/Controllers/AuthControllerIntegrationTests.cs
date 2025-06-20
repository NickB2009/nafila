using Grande.Fila.API.Application.Auth;
using Grande.Fila.API.Domain.Users;
using Grande.Fila.API.Infrastructure.Repositories.Bogus;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;
using System.Text.Json;

namespace GrandeTech.QueueHub.Tests.Integration.Controllers
{
    [TestClass]
    public class AuthControllerIntegrationTests
    {
        private WebApplicationFactory<Program>? _factory;
        private HttpClient? _client; [TestInitialize]
        public void Setup()
        {
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
                        // Ensure we're using the Bogus repository for testing
                        services.AddScoped<IUserRepository, BogusUserRepository>();
                        services.AddScoped<AuthService>();
                    });
                });
            _client = _factory.CreateClient();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _client?.Dispose();
            _factory?.Dispose();
        }
        [TestMethod]
        public async Task Login_WithValidCredentials_ReturnsSuccessResult()
        {
            // Arrange - First register a user to ensure we have valid credentials
            Assert.IsNotNull(_client);

            var registerRequest = new RegisterRequest
            {
                Username = "testuser123",
                Email = "test@example.com",
                Password = "password123",
                ConfirmPassword = "password123"
            };

            var registerJson = JsonSerializer.Serialize(registerRequest);
            var registerContent = new StringContent(registerJson, Encoding.UTF8, "application/json");

            // Register the user first
            var registerResponse = await _client.PostAsync("/api/auth/register", registerContent);

            // Now test login
            var loginRequest = new LoginRequest
            {
                Username = "testuser123",
                Password = "password123"
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
            Assert.AreEqual("testuser123", loginResult.Username);
            Assert.AreEqual("User", loginResult.Role);
        }
        [TestMethod]
        public async Task Login_WithInvalidCredentials_ReturnsBadRequest()
        {
            // Arrange
            Assert.IsNotNull(_client);

            var loginRequest = new LoginRequest
            {
                Username = "nonexistentuser",
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
                BCrypt.Net.BCrypt.HashPassword("password123"), "User");
            await userRepository.AddAsync(knownUser, CancellationToken.None);

            var loginRequest = new LoginRequest
            {
                Username = "integrationtestuser",
                Password = "password123"
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
                Username = "newuser123",
                Email = "newuser@example.com",
                Password = "securepassword123",
                ConfirmPassword = "securepassword123"
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
            // Arrange - First register a user
            Assert.IsNotNull(_client);

            var firstRegisterRequest = new RegisterRequest
            {
                Username = "duplicateuser",
                Email = "first@example.com",
                Password = "password123",
                ConfirmPassword = "password123"
            };

            var firstJson = JsonSerializer.Serialize(firstRegisterRequest);
            var firstContent = new StringContent(firstJson, Encoding.UTF8, "application/json");

            await _client.PostAsync("/api/auth/register", firstContent);

            // Now try to register with the same username
            var secondRegisterRequest = new RegisterRequest
            {
                Username = "duplicateuser",
                Email = "second@example.com",
                Password = "password123",
                ConfirmPassword = "password123"
            };

            var secondJson = JsonSerializer.Serialize(secondRegisterRequest);
            var secondContent = new StringContent(secondJson, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/auth/register", secondContent);

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
            Assert.IsTrue(registerResult.FieldErrors.ContainsKey("Username"));
        }

        [TestMethod]
        public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
        {
            // Arrange - First register a user
            Assert.IsNotNull(_client);

            var firstRegisterRequest = new RegisterRequest
            {
                Username = "user1",
                Email = "duplicate@example.com",
                Password = "password123",
                ConfirmPassword = "password123"
            };

            var firstJson = JsonSerializer.Serialize(firstRegisterRequest);
            var firstContent = new StringContent(firstJson, Encoding.UTF8, "application/json");

            await _client.PostAsync("/api/auth/register", firstContent);

            // Now try to register with the same email
            var secondRegisterRequest = new RegisterRequest
            {
                Username = "user2",
                Email = "duplicate@example.com",
                Password = "password123",
                ConfirmPassword = "password123"
            };

            var secondJson = JsonSerializer.Serialize(secondRegisterRequest);
            var secondContent = new StringContent(secondJson, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/auth/register", secondContent);

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
            Assert.IsTrue(registerResult.FieldErrors.ContainsKey("Email"));
        }

        [TestMethod]
        public async Task Register_WithMismatchedPasswords_ReturnsBadRequest()
        {
            // Arrange
            Assert.IsNotNull(_client);

            var registerRequest = new RegisterRequest
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "password123",
                ConfirmPassword = "differentpassword"
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
            Assert.IsTrue(registerResult.FieldErrors.ContainsKey("ConfirmPassword"));
        }

        [TestMethod]
        public async Task Register_WithEmptyFields_ReturnsBadRequest()
        {
            // Arrange
            Assert.IsNotNull(_client);

            var registerRequest = new RegisterRequest
            {
                Username = "",
                Email = "",
                Password = "",
                ConfirmPassword = ""
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
            var password = "securepassword123";

            var registerRequest = new RegisterRequest
            {
                Username = username,
                Email = email,
                Password = password,
                ConfirmPassword = password
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
                Username = username,
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
            Assert.AreEqual(username, loginResult.Username);
            Assert.AreEqual("User", loginResult.Role);
        }

        [TestMethod]
        public async Task Login_AsAdmin_WithTwoFactor_ReturnsTwoFactorRequired()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_factory);

            using var scope = _factory.Services.CreateScope();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            // Create an admin user
            var username = $"admin_{Guid.NewGuid():N}";
            var email = $"{username}@test.com";
            var password = "adminpass123";

            var user = new User(username, email, BCrypt.Net.BCrypt.HashPassword(password), UserRoles.Admin);
            user.EnableTwoFactor();
            await userRepository.AddAsync(user, CancellationToken.None);

            // Act
            var loginRequest = new LoginRequest
            {
                Username = username,
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

            // Create an admin user
            var username = $"admin_{Guid.NewGuid():N}";
            var email = $"{username}@test.com";
            var password = "adminpass123";

            var user = new User(username, email, BCrypt.Net.BCrypt.HashPassword(password), UserRoles.Admin);
            user.EnableTwoFactor();
            await userRepository.AddAsync(user, CancellationToken.None);

            // Step 1: Initial login to get 2FA token
            var loginRequest = new LoginRequest
            {
                Username = username,
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
                Username = username,
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
            Assert.AreEqual(username, verifyResult.Username);
            Assert.AreEqual(UserRoles.Admin, verifyResult.Role);
            Assert.IsNotNull(verifyResult.Permissions);
            Assert.IsTrue(verifyResult.Permissions.Length > 0);
        }

        [TestMethod]
        public async Task Login_AsLockedAdmin_ReturnsLockedError()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_factory);

            using var scope = _factory.Services.CreateScope();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            // Create a locked admin user
            var username = $"admin_{Guid.NewGuid():N}";
            var email = $"{username}@test.com";
            var password = "adminpass123";

            var user = new User(username, email, BCrypt.Net.BCrypt.HashPassword(password), UserRoles.Admin);
            user.Lock();
            await userRepository.AddAsync(user, CancellationToken.None);

            // Act
            var loginRequest = new LoginRequest
            {
                Username = username,
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
            Assert.AreEqual("Admin account is locked. Please contact support.", loginResult.Error);
        }
    }
}
