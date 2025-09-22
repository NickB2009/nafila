using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Grande.Fila.API.Tests.Integration;
using Grande.Fila.API.Application.Auth;

namespace Grande.Fila.API.Tests.Integration.Controllers
{
    /// <summary>
    /// Production integration tests for Auth Controller endpoints
    /// These tests call actual production API endpoints
    /// </summary>
    [TestClass]
    [TestCategory("Production")]
    [TestCategory("Auth")]
    public class ProductionAuthControllerTests : ProductionIntegrationTestBase
    {
        [TestMethod]
        public async Task Login_Production_WithInvalidCredentials_ShouldReturnProperError()
        {
            // Arrange - Use invalid credentials that should return a proper error response
            var loginRequest = new LoginRequest
            {
                PhoneNumber = "+1234567890",
                Password = "InvalidPassword123!"
            };

            // Act
            var response = await MakeRequestAsync(HttpMethod.Post, "/api/Auth/login", loginRequest);

            // Assert
            Console.WriteLine($"Login Response Status: {response.StatusCode}");
            
            var result = await DeserializeResponseAsync<LoginResult>(response);
            
            Assert.IsNotNull(result, "Response should be deserializable");
            Assert.IsFalse(result.Success, "Login with invalid credentials should fail");
            Assert.IsFalse(string.IsNullOrEmpty(result.Error), "Error message should be provided");
            
            Console.WriteLine($"Login Error (expected): {result.Error}");
            
            // Should not be a 500 error
            Assert.AreNotEqual(500, (int)response.StatusCode, 
                "Should not get 500 Internal Server Error");
        }

        [TestMethod]
        public async Task Register_Production_WithInvalidData_ShouldReturnProperError()
        {
            // Arrange - Use invalid registration data
            var registerRequest = new RegisterRequest
            {
                FullName = "",
                Email = "invalid-email",
                PhoneNumber = "invalid-phone",
                Password = "weak"
            };

            // Act
            var response = await MakeRequestAsync(HttpMethod.Post, "/api/Auth/register", registerRequest);

            // Assert
            Console.WriteLine($"Register Response Status: {response.StatusCode}");
            
            var result = await DeserializeResponseAsync<RegisterResult>(response);
            
            Assert.IsNotNull(result, "Response should be deserializable");
            Assert.IsFalse(result.Success, "Registration with invalid data should fail");
            Assert.IsNotNull(result.FieldErrors, "Validation errors should be provided");
            Assert.IsTrue(result.FieldErrors.Count > 0, "Should have validation errors");
            
            Console.WriteLine($"Registration Errors (expected): {string.Join(", ", result.FieldErrors.Values)}");
            
            // Should not be a 500 error
            Assert.AreNotEqual(500, (int)response.StatusCode, 
                "Should not get 500 Internal Server Error");
        }

        [TestMethod]
        public async Task VerifyTwoFactor_Production_WithInvalidToken_ShouldReturnProperError()
        {
            // Arrange - Use an invalid 2FA token
            var verifyRequest = new VerifyTwoFactorRequest
            {
                PhoneNumber = "+1234567890",
                TwoFactorCode = "123456",
                TwoFactorToken = "invalid-2fa-token"
            };

            // Act
            var response = await MakeRequestAsync(HttpMethod.Post, "/api/Auth/verify-2fa", verifyRequest);

            // Assert
            Console.WriteLine($"VerifyTwoFactor Response Status: {response.StatusCode}");
            
            var result = await DeserializeResponseAsync<LoginResult>(response);
            
            Assert.IsNotNull(result, "Response should be deserializable");
            Assert.IsFalse(result.Success, "Verify 2FA with invalid token should fail");
            Assert.IsFalse(string.IsNullOrEmpty(result.Error), "Error message should be provided");
            
            Console.WriteLine($"VerifyTwoFactor Error (expected): {result.Error}");
            
            // Should not be a 500 error
            Assert.AreNotEqual(500, (int)response.StatusCode, 
                "Should not get 500 Internal Server Error");
        }

        [TestMethod]
        public async Task GetProfile_Production_WithInvalidToken_ShouldReturnProperError()
        {
            // Arrange - Use an invalid JWT token
            var invalidToken = "invalid.jwt.token";

            // Act
            var response = await MakeAuthenticatedRequestAsync(
                HttpMethod.Get, 
                "/api/Auth/profile", 
                authToken: invalidToken);

            // Assert
            Console.WriteLine($"GetProfile Response Status: {response.StatusCode}");
            
            // Should return 401 Unauthorized for invalid tokens
            Assert.AreEqual(401, (int)response.StatusCode, 
                "Invalid token should return 401 Unauthorized");
            
            Console.WriteLine("Profile endpoint correctly rejected invalid token");
        }

        [TestMethod]
        public async Task GetProfile_Production_WithoutToken_ShouldReturnProperError()
        {
            // Act - Try to get profile without providing a token
            var response = await MakeRequestAsync(HttpMethod.Get, "/api/Auth/profile");

            // Assert
            Console.WriteLine($"GetProfile Response Status: {response.StatusCode}");
            
            // Should return 401 Unauthorized when no token is provided
            Assert.AreEqual(401, (int)response.StatusCode, 
                "Profile without token should return 401 Unauthorized");
            
            Console.WriteLine("Profile endpoint correctly rejected request without token");
        }

        [TestMethod]
        public async Task AdminVerify_Production_WithoutAdminToken_ShouldReturnProperError()
        {
            // Arrange - Use an invalid admin verification request
            var adminRequest = new AdminVerificationRequest
            {
                PhoneNumber = "+1234567890",
                TwoFactorCode = "123456",
                TwoFactorToken = "invalid-admin-token"
            };

            // Act
            var response = await MakeRequestAsync(HttpMethod.Post, "/api/Auth/admin/verify", adminRequest);

            // Assert
            Console.WriteLine($"AdminVerify Response Status: {response.StatusCode}");
            
            // Should return 401 Unauthorized for admin endpoints without proper authorization
            Assert.AreEqual(401, (int)response.StatusCode, 
                "Admin endpoint should return 401 Unauthorized without proper token");
            
            // For 401 responses, we don't expect a JSON body, so we don't try to deserialize
            // This is the expected behavior for secured endpoints
            Console.WriteLine("Admin verification endpoint correctly rejected unauthorized request with 401 status");
        }
    }
}
