using Grande.Fila.API.Application.Auth;
using Grande.Fila.API.Application.Locations.Requests;
using Grande.Fila.API.Application.Locations.Results;
using Grande.Fila.API.Domain.Locations;
using Grande.Fila.API.Domain.Users;
using Grande.Fila.API.Infrastructure.Repositories.Bogus;
using Grande.Fila.API.Tests.Integration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Grande.Fila.Tests.Integration.Controllers
{
    [TestClass]
    [TestCategory("Integration")]
    public class LocationsControllerIntegrationTests
    {
        private static WebApplicationFactory<Program> _factory;
        private HttpClient _client;
        private static BogusUserRepository _userRepository;
        private static BogusLocationRepository _locationRepository;

        [TestInitialize]
        public void TestInitialize()
        {
            if (_userRepository == null)
                _userRepository = new BogusUserRepository();
            if (_locationRepository == null)
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
                            ["Jwt:Audience"] = "Grande.Fila.API.Test"
                        });
                    });
                    builder.ConfigureServices(services =>
                    {
                        var descriptors = services.Where(d => 
                            d.ServiceType == typeof(IUserRepository) ||
                            d.ServiceType == typeof(ILocationRepository)).ToList();
                        foreach (var descriptor in descriptors)
                        {
                            services.Remove(descriptor);
                        }
                        services.AddSingleton<IUserRepository>(_userRepository);
                        services.AddSingleton<ILocationRepository>(_locationRepository);
                        services.AddScoped<AuthService>();
                    });
                });
            _client = _factory.CreateClient();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            _factory.Dispose();
        }

        [TestMethod]
        public async Task AddLocation_AsAdmin_ReturnsCreated()
        {
            var adminToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "Admin", new[] { Permission.CreateStaff });
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var request = new CreateLocationRequest
            {
                BusinessName = "Test Location",
                ContactEmail = "test@example.com",
                ContactPhone = "123-456-7890",
                Address = new LocationAddressRequest
                {
                    Street = "123 Test St",
                    City = "Test City",
                    State = "TS",
                    PostalCode = "12345",
                    Country = "US"
                },
                BusinessHours = new Dictionary<string, string>
                {
                    ["Monday"] = "08:00-18:00"
                },
                MaxQueueCapacity = 50,
                Description = "Test location description"
            };

            var response = await _client.PostAsJsonAsync("/api/locations", request);
            var result = await response.Content.ReadFromJsonAsync<CreateLocationResult>();

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.LocationId);
            Assert.IsNotNull(result.LocationSlug);
            Assert.AreEqual(request.BusinessName, result.BusinessName);
            Assert.IsFalse(result.Errors.Any());
            Assert.IsFalse(result.FieldErrors.Any());
        }

        [TestMethod]
        public async Task AddLocation_AsUser_ReturnsForbidden()
        {
            var client = _factory.CreateClient();
            var userToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "User", new string[0]);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

            var request = new CreateLocationRequest
            {
                BusinessName = "Test Location",
                ContactEmail = "test@example.com",
                ContactPhone = "123-456-7890",
                Address = new LocationAddressRequest
                {
                    Street = "123 Test St",
                    City = "Test City",
                    State = "TS",
                    PostalCode = "12345",
                    Country = "US"
                },
                BusinessHours = new Dictionary<string, string>
                {
                    ["Monday"] = "08:00-18:00"
                },
                MaxQueueCapacity = 50,
                Description = "Test location description"
            };

            var response = await client.PostAsJsonAsync("/api/locations", request);
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [TestMethod]
        public async Task AddLocation_WithoutAuth_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var request = new CreateLocationRequest
            {
                BusinessName = "Test Location",
                ContactEmail = "test@example.com",
                ContactPhone = "123-456-7890",
                Address = new LocationAddressRequest
                {
                    Street = "123 Test St",
                    City = "Test City",
                    State = "TS",
                    PostalCode = "12345",
                    Country = "US"
                },
                BusinessHours = new Dictionary<string, string>
                {
                    ["Monday"] = "08:00-18:00"
                },
                MaxQueueCapacity = 50,
                Description = "Test location description"
            };

            var response = await client.PostAsJsonAsync("/api/locations", request);
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task AddLocation_WithInvalidData_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            var adminToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "Admin", new[] { Permission.CreateStaff });
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var request = new CreateLocationRequest
            {
                BusinessName = "", // Invalid empty name
                ContactEmail = "invalid-email", // Invalid email format
                ContactPhone = "", // Invalid empty phone
                Address = new LocationAddressRequest
                {
                    Street = "", // Invalid empty street
                    City = "", // Invalid empty city
                    State = "", // Invalid empty state
                    PostalCode = "", // Invalid empty postal code
                    Country = "" // Invalid empty country
                },
                MaxQueueCapacity = 0 // Invalid capacity
            };

            var response = await client.PostAsJsonAsync("/api/locations", request);
            var result = await response.Content.ReadFromJsonAsync<CreateLocationResult>();

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.Any(), "No field errors found");
            Assert.IsTrue(result.FieldErrors.ContainsKey("businessName"), "Missing businessName error");
            Assert.IsTrue(result.FieldErrors.ContainsKey("contactEmail"), "Missing contactEmail error");
            Assert.IsTrue(result.FieldErrors.ContainsKey("contactPhone"), "Missing contactPhone error");
            Assert.IsTrue(result.FieldErrors.ContainsKey("address.Street"), "Missing address.Street error");
            Assert.IsTrue(result.FieldErrors.ContainsKey("address.City"), "Missing address.City error");
            Assert.IsTrue(result.FieldErrors.ContainsKey("address.State"), "Missing address.State error");
            Assert.IsTrue(result.FieldErrors.ContainsKey("address.PostalCode"), "Missing address.PostalCode error");
            Assert.IsTrue(result.FieldErrors.ContainsKey("address.Country"), "Missing address.Country error");
            Assert.IsTrue(result.FieldErrors.ContainsKey("maxQueueCapacity"), "Missing maxQueueCapacity error");
        }

        [TestMethod]
        public async Task GetLocation_AsAdmin_ReturnsOk()
        {
            var client = _factory.CreateClient();
            var adminToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "Admin", new[] { Permission.ReadStaff });
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            // First create a location
            var addRequest = new CreateLocationRequest
            {
                BusinessName = "Test Location",
                ContactEmail = "test@example.com",
                ContactPhone = "123-456-7890",
                Address = new LocationAddressRequest
                {
                    Street = "123 Test St",
                    City = "Test City",
                    State = "TS",
                    PostalCode = "12345",
                    Country = "US"
                },
                BusinessHours = new Dictionary<string, string>
                {
                    ["Monday"] = "08:00-18:00"
                },
                MaxQueueCapacity = 50,
                Description = "Test location description"
            };

            var addResponse = await client.PostAsJsonAsync("/api/locations", addRequest);
            var addResult = await addResponse.Content.ReadFromJsonAsync<CreateLocationResult>();

            Assert.IsNotNull(addResult);
            Assert.IsTrue(addResult.Success);

            // Then try to get it
            var getResponse = await client.GetAsync($"/api/locations/{addResult.LocationId}");
            var getResult = await getResponse.Content.ReadFromJsonAsync<LocationDto>();

            Assert.AreEqual(HttpStatusCode.OK, getResponse.StatusCode);
            Assert.IsNotNull(getResult);
            Assert.AreEqual<Guid>(Guid.Parse(addResult.LocationId!), getResult.Id);
            Assert.AreEqual(addRequest.BusinessName, getResult.BusinessName);
            Assert.AreEqual(addRequest.Address.Street, getResult.Address.Street);
            Assert.AreEqual(addRequest.Address.City, getResult.Address.City);
            Assert.AreEqual(addRequest.Address.State, getResult.Address.State);
            Assert.AreEqual(addRequest.Address.PostalCode, getResult.Address.PostalCode);
            Assert.AreEqual(addRequest.Address.Country, getResult.Address.Country);
            Assert.AreEqual<int>(addRequest.BusinessHours.Count, getResult.BusinessHours.Count);
            Assert.AreEqual<int>(addRequest.MaxQueueCapacity, getResult.MaxQueueCapacity);
            Assert.AreEqual(addRequest.Description, getResult.Description);
        }

        [TestMethod]
        public async Task GetLocation_AsUser_ReturnsOk()
        {
            var client = _factory.CreateClient();
            var adminToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "Admin", new[] { Permission.CreateStaff });
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            // First create a location as admin
            var addRequest = new CreateLocationRequest
            {
                BusinessName = "Test Location",
                ContactEmail = "test@example.com",
                ContactPhone = "123-456-7890",
                Address = new LocationAddressRequest
                {
                    Street = "123 Test St",
                    City = "Test City",
                    State = "TS",
                    PostalCode = "12345",
                    Country = "US"
                },
                BusinessHours = new Dictionary<string, string>
                {
                    ["Monday"] = "08:00-18:00"
                },
                MaxQueueCapacity = 50,
                Description = "Test location description"
            };

            var addResponse = await client.PostAsJsonAsync("/api/locations", addRequest);
            var addResult = await addResponse.Content.ReadFromJsonAsync<CreateLocationResult>();

            Assert.IsNotNull(addResult);
            Assert.IsTrue(addResult.Success);

            // Then try to get it as a regular user
            var userToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "User", new string[0]);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

            var getResponse = await client.GetAsync($"/api/locations/{addResult.LocationId}");
            var getResult = await getResponse.Content.ReadFromJsonAsync<LocationDto>();

            Assert.AreEqual(HttpStatusCode.OK, getResponse.StatusCode);
            Assert.IsNotNull(getResult);
            Assert.AreEqual<Guid>(Guid.Parse(addResult.LocationId!), getResult.Id);
        }

        [TestMethod]
        public async Task GetLocation_NonExistingId_ReturnsNotFound()
        {
            var client = _factory.CreateClient();
            var adminToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "Admin", new[] { Permission.ReadStaff });
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var nonExistingId = Guid.NewGuid();
            var response = await client.GetAsync($"/api/locations/{nonExistingId}");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task UpdateLocation_AsAdmin_ReturnsOk()
        {
            var client = _factory.CreateClient();
            var adminToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "Admin", new[] { Permission.UpdateStaff });
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            // First create a location
            var addRequest = new CreateLocationRequest
            {
                BusinessName = "Test Location",
                ContactEmail = "test@example.com",
                ContactPhone = "123-456-7890",
                Address = new LocationAddressRequest
                {
                    Street = "123 Test St",
                    City = "Test City",
                    State = "TS",
                    PostalCode = "12345",
                    Country = "US"
                },
                BusinessHours = new Dictionary<string, string>
                {
                    ["Monday"] = "08:00-18:00"
                },
                MaxQueueCapacity = 50,
                Description = "Test location description"
            };

            var addResponse = await client.PostAsJsonAsync("/api/locations", addRequest);
            var addResult = await addResponse.Content.ReadFromJsonAsync<CreateLocationResult>();

            Assert.IsNotNull(addResult);
            Assert.IsTrue(addResult.Success);

            // Then update it
            var updateRequest = new UpdateLocationRequest
            {
                BusinessName = "Updated Location",
                Address = new LocationAddressRequest
                {
                    Street = "456 New St",
                    City = "New City",
                    State = "NS",
                    PostalCode = "54321",
                    Country = "US"
                },
                BusinessHours = new Dictionary<string, string>
                {
                    ["Monday"] = "09:00-19:00"
                },
                MaxQueueCapacity = 60,
                Description = "Updated location description"
            };

            var updateResponse = await client.PutAsJsonAsync($"/api/locations/{addResult.LocationId}", updateRequest);
            Assert.AreEqual(HttpStatusCode.OK, updateResponse.StatusCode);

            // Verify the update
            var getResponse = await client.GetAsync($"/api/locations/{addResult.LocationId}");
            var getResult = await getResponse.Content.ReadFromJsonAsync<LocationDto>();

            Assert.AreEqual(HttpStatusCode.OK, getResponse.StatusCode);
            Assert.IsNotNull(getResult);
            Assert.AreEqual<Guid>(Guid.Parse(addResult.LocationId!), getResult.Id);
            Assert.AreEqual(updateRequest.BusinessName, getResult.BusinessName);
            Assert.AreEqual(updateRequest.Address.Street, getResult.Address.Street);
            Assert.AreEqual(updateRequest.Address.City, getResult.Address.City);
            Assert.AreEqual(updateRequest.Address.State, getResult.Address.State);
            Assert.AreEqual(updateRequest.Address.PostalCode, getResult.Address.PostalCode);
            Assert.AreEqual(updateRequest.Address.Country, getResult.Address.Country);
            Assert.AreEqual<int>(updateRequest.BusinessHours.Count, getResult.BusinessHours.Count);
            Assert.AreEqual<int>(updateRequest.MaxQueueCapacity, getResult.MaxQueueCapacity);
            Assert.AreEqual(updateRequest.Description, getResult.Description);
        }

        [TestMethod]
        public async Task UpdateLocation_AsUser_ReturnsForbidden()
        {
            var client = _factory.CreateClient();
            var adminToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "Admin", new[] { Permission.CreateStaff });
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            // First create a location as admin
            var addRequest = new CreateLocationRequest
            {
                BusinessName = "Test Location",
                ContactEmail = "test@example.com",
                ContactPhone = "123-456-7890",
                Address = new LocationAddressRequest
                {
                    Street = "123 Test St",
                    City = "Test City",
                    State = "TS",
                    PostalCode = "12345",
                    Country = "US"
                },
                BusinessHours = new Dictionary<string, string>
                {
                    ["Monday"] = "08:00-18:00"
                },
                MaxQueueCapacity = 50,
                Description = "Test location description"
            };

            var addResponse = await client.PostAsJsonAsync("/api/locations", addRequest);
            var addResult = await addResponse.Content.ReadFromJsonAsync<CreateLocationResult>();

            Assert.IsNotNull(addResult);
            Assert.IsTrue(addResult.Success);

            // Try to update as regular user
            var userToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "User", new string[0]);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

            var updateRequest = new UpdateLocationRequest
            {
                BusinessName = "Updated Location",
                Address = new LocationAddressRequest
                {
                    Street = "456 New St",
                    City = "New City",
                    State = "NS",
                    PostalCode = "54321",
                    Country = "US"
                },
                BusinessHours = new Dictionary<string, string>
                {
                    ["Monday"] = "09:00-19:00"
                },
                MaxQueueCapacity = 60,
                Description = "Updated location description"
            };

            var updateResponse = await client.PutAsJsonAsync($"/api/locations/{addResult.LocationId}", updateRequest);
            Assert.AreEqual(HttpStatusCode.Forbidden, updateResponse.StatusCode);
        }

        [TestMethod]
        public async Task DeleteLocation_AsAdmin_ReturnsOk()
        {
            var client = _factory.CreateClient();
            var adminToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "Admin", new[] { Permission.DeleteStaff });
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            // First create a location
            var addRequest = new CreateLocationRequest
            {
                BusinessName = "Test Location",
                ContactEmail = "test@example.com",
                ContactPhone = "123-456-7890",
                Address = new LocationAddressRequest
                {
                    Street = "123 Test St",
                    City = "Test City",
                    State = "TS",
                    PostalCode = "12345",
                    Country = "US"
                },
                BusinessHours = new Dictionary<string, string>
                {
                    ["Monday"] = "08:00-18:00"
                },
                MaxQueueCapacity = 50,
                Description = "Test location description"
            };

            var addResponse = await client.PostAsJsonAsync("/api/locations", addRequest);
            var addResult = await addResponse.Content.ReadFromJsonAsync<CreateLocationResult>();

            Assert.IsNotNull(addResult);
            Assert.IsTrue(addResult.Success);

            // Then delete it
            var deleteResponse = await client.DeleteAsync($"/api/locations/{addResult.LocationId}");
            Assert.AreEqual(HttpStatusCode.OK, deleteResponse.StatusCode);

            // Verify it's deleted
            var getResponse = await client.GetAsync($"/api/locations/{addResult.LocationId}");
            Assert.AreEqual(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [TestMethod]
        public async Task DeleteLocation_AsUser_ReturnsForbidden()
        {
            var client = _factory.CreateClient();
            var adminToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "Admin", new[] { Permission.CreateStaff });
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            // First create a location as admin
            var addRequest = new CreateLocationRequest
            {
                BusinessName = "Test Location",
                ContactEmail = "test@example.com",
                ContactPhone = "123-456-7890",
                Address = new LocationAddressRequest
                {
                    Street = "123 Test St",
                    City = "Test City",
                    State = "TS",
                    PostalCode = "12345",
                    Country = "US"
                },
                BusinessHours = new Dictionary<string, string>
                {
                    ["Monday"] = "08:00-18:00"
                },
                MaxQueueCapacity = 50,
                Description = "Test location description"
            };

            var addResponse = await client.PostAsJsonAsync("/api/locations", addRequest);
            var addResult = await addResponse.Content.ReadFromJsonAsync<CreateLocationResult>();

            Assert.IsNotNull(addResult);
            Assert.IsTrue(addResult.Success);

            // Try to delete as regular user
            var userToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "User", new string[0]);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

            var deleteResponse = await client.DeleteAsync($"/api/locations/{addResult.LocationId}");
            Assert.AreEqual(HttpStatusCode.Forbidden, deleteResponse.StatusCode);
        }

        [TestMethod]
        public async Task ToggleQueueStatus_EnableQueue_ReturnsOk()
        {
            // Arrange
            var client = _factory.CreateClient();
            var adminToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "Admin", new[] { Permission.UpdateStaff });
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            // First create a location
            var createRequest = new
            {
                organizationId = Guid.NewGuid().ToString(),
                businessName = "Test Location",
                contactEmail = "test@test.com",
                contactPhone = "123-456-7890",
                address = new
                {
                    street = "123 Main St",
                    city = "Test City",
                    state = "State",
                    postalCode = "12345",
                    country = "Country"
                },
                businessHours = new Dictionary<string, string>
                {
                    ["Monday"] = "08:00-18:00"
                },
                maxQueueCapacity = 100,
                description = "Test Description"
            };

            var createResponse = await client.PostAsJsonAsync("/api/locations", createRequest);
            
            // Debug: Check if location creation failed
            if (!createResponse.IsSuccessStatusCode)
            {
                var errorContent = await createResponse.Content.ReadAsStringAsync();
                Assert.Fail($"Location creation failed with {createResponse.StatusCode}: {errorContent}");
            }
            
            createResponse.EnsureSuccessStatusCode();
            var createResult = await createResponse.Content.ReadFromJsonAsync<CreateLocationResult>();
            var locationId = createResult?.LocationId;
            
            // Debug: Check if locationId is null
            if (string.IsNullOrEmpty(locationId))
            {
                var responseContent = await createResponse.Content.ReadAsStringAsync();
                Assert.Fail($"LocationId is null or empty. Response: {responseContent}");
            }

            // Now toggle queue status
            var toggleRequest = new
            {
                EnableQueue = false // Disable first
            };

            var disableResponse = await client.PutAsJsonAsync($"/api/locations/{locationId}/queue-status", toggleRequest);
            disableResponse.EnsureSuccessStatusCode();

            // Enable queue
            toggleRequest = new
            {
                EnableQueue = true
            };

            // Act
            var response = await client.PutAsJsonAsync($"/api/locations/{locationId}/queue-status", toggleRequest);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<ToggleQueueResult>();
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.IsQueueEnabled);
        }

        [TestMethod]
        public async Task ToggleQueueStatus_DisableQueue_ReturnsOk()
        {
            // Arrange
            var client = _factory.CreateClient();
            var adminToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "Admin", new[] { Permission.UpdateStaff });
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            // First create a location
            var createRequest = new
            {
                organizationId = Guid.NewGuid().ToString(),
                businessName = "Test Location",
                contactEmail = "test@test.com",
                contactPhone = "123-456-7890",
                address = new
                {
                    street = "123 Main St",
                    city = "Test City",
                    state = "State",
                    postalCode = "12345",
                    country = "Country"
                },
                businessHours = new Dictionary<string, string>
                {
                    ["Monday"] = "08:00-18:00"
                },
                maxQueueCapacity = 100,
                description = "Test Description"
            };

            var createResponse = await client.PostAsJsonAsync("/api/locations", createRequest);
            
            // Debug: Check if location creation failed
            if (!createResponse.IsSuccessStatusCode)
            {
                var errorContent = await createResponse.Content.ReadAsStringAsync();
                Assert.Fail($"Location creation failed with {createResponse.StatusCode}: {errorContent}");
            }
            
            createResponse.EnsureSuccessStatusCode();
            var createResult = await createResponse.Content.ReadFromJsonAsync<CreateLocationResult>();
            var locationId = createResult?.LocationId;
            
            // Debug: Check if locationId is null
            if (string.IsNullOrEmpty(locationId))
            {
                var responseContent = await createResponse.Content.ReadAsStringAsync();
                Assert.Fail($"LocationId is null or empty. Response: {responseContent}");
            }

            // Now disable queue
            var toggleRequest = new
            {
                EnableQueue = false
            };

            // Act
            var response = await client.PutAsJsonAsync($"/api/locations/{locationId}/queue-status", toggleRequest);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<ToggleQueueResult>();
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsFalse(result.IsQueueEnabled);
        }

        [TestMethod]
        public async Task ToggleQueueStatus_WithoutOwnerRole_ReturnsForbidden()
        {
            // Arrange
            var client = _factory.CreateClient();
            var userToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "User", new string[0]);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

            var request = new
            {
                EnableQueue = false
            };

            // Act
            var response = await client.PutAsJsonAsync($"/api/locations/{Guid.NewGuid()}/queue-status", request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [TestMethod]
        public async Task ToggleQueueStatus_InvalidLocationId_ReturnsBadRequest()
        {
            // Arrange
            var client = _factory.CreateClient();
            var adminToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "Admin", new[] { Permission.UpdateStaff });
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var request = new
            {
                EnableQueue = true
            };

            // Act
            var response = await client.PutAsJsonAsync("/api/locations/invalid-guid/queue-status", request);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task ToggleQueueStatus_NonExistentLocation_ReturnsNotFound()
        {
            // Arrange
            var client = _factory.CreateClient();
            var adminToken = await IntegrationTestHelper.CreateAndAuthenticateUserAsync(_userRepository, _factory.Services, "Admin", new[] { Permission.UpdateStaff });
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var request = new
            {
                EnableQueue = true
            };

            // Act
            var response = await client.PutAsJsonAsync($"/api/locations/{Guid.NewGuid()}/queue-status", request);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        // DTOs for test deserialization
        public class UpdateLocationRequest
        {
            public string BusinessName { get; set; } = string.Empty;
            public LocationAddressRequest Address { get; set; } = new LocationAddressRequest();
            public Dictionary<string, string> BusinessHours { get; set; } = new Dictionary<string, string>();
            public int MaxQueueCapacity { get; set; }
            public string Description { get; set; } = string.Empty;
        }

        public class LocationDto
        {
            public Guid Id { get; set; }
            public string BusinessName { get; set; } = string.Empty;
            public LocationAddressRequest Address { get; set; } = new LocationAddressRequest();
            public Dictionary<string, string> BusinessHours { get; set; } = new Dictionary<string, string>();
            public int MaxQueueCapacity { get; set; }
            public string Description { get; set; } = string.Empty;
        }
    }
}