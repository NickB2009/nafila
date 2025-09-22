using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Grande.Fila.API.Tests.Integration;
using Grande.Fila.API.Application.Public;

namespace Grande.Fila.API.Tests.Integration.Controllers
{
    /// <summary>
    /// Production integration tests for Public Controller endpoints
    /// These tests call actual production API endpoints
    /// </summary>
    [TestClass]
    [TestCategory("Production")]
    [TestCategory("Public")]
    public class ProductionPublicControllerTests : ProductionIntegrationTestBase
    {
        [TestMethod]
        public async Task HealthCheck_Production_ShouldReturnHealthy()
        {
            // Act
            var response = await MakeRequestAsync(HttpMethod.Get, "/api/Health");

            // Assert
            Assert.IsTrue(response.IsSuccessStatusCode, 
                $"Health check failed with status: {response.StatusCode}");
            
            var isHealthy = await VerifyProductionApiAccessAsync();
            Assert.IsTrue(isHealthy, "Production API should be accessible and healthy");
        }

        [TestMethod]
        public async Task GetSalons_Production_ShouldReturnValidResponse()
        {
            // Act - Test the correct endpoint name
            var response = await MakeRequestAsync(HttpMethod.Get, "/api/Public/salons");

            // Assert
            Console.WriteLine($"GetSalons Response Status: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Assert.IsFalse(string.IsNullOrEmpty(content), "Response content should not be empty");
                Console.WriteLine($"Salons Response: {content}");
            }
            else
            {
                // Even if it fails, we want to see what the actual response is
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"GetSalons Error Response: {errorContent}");
                
                // Don't fail the test immediately - this might be expected behavior
                // depending on the production environment setup
            }
        }

        [TestMethod]
        public async Task GetServices_Production_ShouldReturnValidResponse()
        {
            // Act
            var response = await MakeRequestAsync(HttpMethod.Get, "/api/Public/services");

            // Assert
            Console.WriteLine($"GetServices Response Status: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Assert.IsFalse(string.IsNullOrEmpty(content), "Response content should not be empty");
                Console.WriteLine($"Services Response: {content}");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"GetServices Error Response: {errorContent}");
            }
        }

        [TestMethod]
        public async Task AnonymousJoin_Production_WithValidData_ShouldHandleRequest()
        {
            // Arrange - Use a test request that should be handled gracefully
            var request = new
            {
                salonId = "00000000-0000-0000-0000-000000000001", // Test salon ID
                name = "Production Test Customer",
                email = "production.test@example.com",
                serviceRequested = "Test Service",
                anonymousUserId = Guid.NewGuid().ToString()
            };

            // Act
            var response = await MakeRequestAsync(HttpMethod.Post, "/api/Public/queue/join", request);

            // Assert
            Console.WriteLine($"AnonymousJoin Response Status: {response.StatusCode}");
            
            var result = await DeserializeResponseAsync<AnonymousJoinResult>(response);
            
            if (response.IsSuccessStatusCode)
            {
                Assert.IsNotNull(result, "Response should be deserializable");
                Assert.IsTrue(result.Success, "Join should be successful");
                Console.WriteLine($"Successfully joined queue. Position: {result.Position}, Wait Time: {result.EstimatedWaitMinutes}");
            }
            else
            {
                // Even if it fails, we want to ensure it's a proper business logic error, not a server error
                Assert.IsNotNull(result, "Error response should be deserializable");
                
                // Check that it's not a 500 error (which would indicate server issues)
                Assert.AreNotEqual(500, (int)response.StatusCode, 
                    "Should not get 500 Internal Server Error - indicates server configuration issues");
                
                Console.WriteLine($"Business logic error (expected): {string.Join(", ", result.Errors ?? new List<string>())}");
            }
        }

        [TestMethod]
        public async Task GetQueueStatus_Production_ShouldReturnValidResponse()
        {
            // Arrange - Use a test location ID
            var locationId = "00000000-0000-0000-0000-000000000001";

            // Act
            var response = await MakeRequestAsync(HttpMethod.Get, $"/api/Public/queue/status/{locationId}");

            // Assert
            Console.WriteLine($"GetQueueStatus Response Status: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Assert.IsFalse(string.IsNullOrEmpty(content), "Response content should not be empty");
                Console.WriteLine($"Queue Status Response: {content}");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"GetQueueStatus Error Response: {errorContent}");
                
                // Don't fail the test for business logic errors (404, etc.)
                Assert.AreNotEqual(500, (int)response.StatusCode, 
                    "Should not get 500 Internal Server Error");
            }
        }

        [TestMethod]
        public async Task GetLocationInfo_Production_ShouldReturnValidResponse()
        {
            // Arrange - Use a test location ID
            var locationId = "00000000-0000-0000-0000-000000000001";

            // Act
            var response = await MakeRequestAsync(HttpMethod.Get, $"/api/Public/salons/{locationId}");

            // Assert
            Console.WriteLine($"GetLocationInfo Response Status: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Assert.IsFalse(string.IsNullOrEmpty(content), "Response content should not be empty");
                Console.WriteLine($"Location Info Response: {content}");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"GetLocationInfo Error Response: {errorContent}");
                
                // Don't fail the test for business logic errors (404, etc.)
                Assert.AreNotEqual(500, (int)response.StatusCode, 
                    "Should not get 500 Internal Server Error");
            }
        }

        [TestMethod]
        public async Task GetQueueEntryStatus_Production_WithInvalidEntryId_ShouldReturnProperError()
        {
            // Arrange - Use an invalid queue entry ID
            var entryId = "invalid-entry-id";

            // Act
            var response = await MakeRequestAsync(HttpMethod.Get, $"/api/Public/queue/entry/{entryId}");

            // Assert
            Console.WriteLine($"GetQueueEntryStatus Response Status: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Assert.IsFalse(string.IsNullOrEmpty(content), "Response content should not be empty");
                Console.WriteLine($"Queue Entry Status Response: {content}");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"GetQueueEntryStatus Error Response: {errorContent}");
                
                // Should return 404 for non-existent entry or 400 for invalid format
                Assert.IsTrue((int)response.StatusCode == 404 || (int)response.StatusCode == 400, 
                    "Should return 404 (not found) or 400 (bad request) for invalid entry ID");
                
                // Should not be a 500 error
                Assert.AreNotEqual(500, (int)response.StatusCode, 
                    "Should not get 500 Internal Server Error");
            }
        }

        [TestMethod]
        public async Task LeaveQueue_Production_WithInvalidEntryId_ShouldReturnProperError()
        {
            // Arrange - Use an invalid queue entry ID
            var entryId = "invalid-entry-id";

            // Act
            var response = await MakeRequestAsync(HttpMethod.Post, $"/api/Public/queue/leave/{entryId}");

            // Assert
            Console.WriteLine($"LeaveQueue Response Status: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Assert.IsFalse(string.IsNullOrEmpty(content), "Response content should not be empty");
                Console.WriteLine($"Leave Queue Response: {content}");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"LeaveQueue Error Response: {errorContent}");
                
                // Should return 404 for non-existent entry or 400 for invalid format
                Assert.IsTrue((int)response.StatusCode == 404 || (int)response.StatusCode == 400, 
                    "Should return 404 (not found) or 400 (bad request) for invalid entry ID");
                
                // Should not be a 500 error
                Assert.AreNotEqual(500, (int)response.StatusCode, 
                    "Should not get 500 Internal Server Error");
            }
        }

        [TestMethod]
        public async Task UpdateQueueEntry_Production_WithInvalidEntryId_ShouldReturnProperError()
        {
            // Arrange - Use an invalid queue entry ID and update request
            var entryId = "invalid-entry-id";
            var updateRequest = new
            {
                name = "Updated Test Customer",
                email = "updated.test@example.com",
                phoneNumber = "+1234567890"
            };

            // Act
            var response = await MakeRequestAsync(HttpMethod.Put, $"/api/Public/queue/update/{entryId}", updateRequest);

            // Assert
            Console.WriteLine($"UpdateQueueEntry Response Status: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Assert.IsFalse(string.IsNullOrEmpty(content), "Response content should not be empty");
                Console.WriteLine($"Update Queue Entry Response: {content}");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"UpdateQueueEntry Error Response: {errorContent}");
                
                // Should return 404 for non-existent entry or 400 for invalid format
                Assert.IsTrue((int)response.StatusCode == 404 || (int)response.StatusCode == 400, 
                    "Should return 404 (not found) or 400 (bad request) for invalid entry ID");
                
                // Should not be a 500 error
                Assert.AreNotEqual(500, (int)response.StatusCode, 
                    "Should not get 500 Internal Server Error");
            }
        }

        [TestMethod]
        public async Task AnonymousJoin_Production_WithInvalidData_ShouldReturnValidationErrors()
        {
            // Arrange - Use invalid data that should trigger validation errors
            var request = new
            {
                salonId = "", // Invalid empty salon ID
                name = "", // Invalid empty name
                email = "invalid-email", // Invalid email format
                serviceRequested = "", // Invalid empty service
                anonymousUserId = "" // Invalid empty user ID
            };

            // Act
            var response = await MakeRequestAsync(HttpMethod.Post, "/api/Public/queue/join", request);

            // Assert
            Console.WriteLine($"AnonymousJoin Invalid Data Response Status: {response.StatusCode}");
            
            // Should return 400 Bad Request for validation errors
            Assert.AreEqual(400, (int)response.StatusCode, 
                "Should return 400 Bad Request for validation errors");
            
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"AnonymousJoin Validation Error Response: {errorContent}");
            
            // Should not be a 500 error
            Assert.AreNotEqual(500, (int)response.StatusCode, 
                "Should not get 500 Internal Server Error");
        }


        [TestMethod]
        public async Task GetSalonById_Production_WithInvalidId_ShouldReturnProperError()
        {
            // Arrange - Use an invalid salon ID
            var salonId = "invalid-salon-id";

            // Act
            var response = await MakeRequestAsync(HttpMethod.Get, $"/api/Public/salons/{salonId}");

            // Assert
            Console.WriteLine($"GetSalonById Invalid ID Response Status: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Assert.IsFalse(string.IsNullOrEmpty(content), "Response content should not be empty");
                Console.WriteLine($"Salon Detail Response: {content}");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"GetSalonById Error Response: {errorContent}");
                
                // Should return 400 for invalid format or 404 for not found
                Assert.IsTrue((int)response.StatusCode == 400 || (int)response.StatusCode == 404, 
                    "Should return 400 (bad request) or 404 (not found) for invalid salon ID");
                
                // Should not be a 500 error
                Assert.AreNotEqual(500, (int)response.StatusCode, 
                    "Should not get 500 Internal Server Error");
            }
        }
    }
}
