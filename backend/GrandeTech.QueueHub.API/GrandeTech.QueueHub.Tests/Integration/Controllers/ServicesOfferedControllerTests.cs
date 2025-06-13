using GrandeTech.QueueHub.API.Application.Auth;
using GrandeTech.QueueHub.API.Application.ServicesOffered;
using GrandeTech.QueueHub.API.Domain.Common.ValueObjects;
using GrandeTech.QueueHub.API.Domain.Locations;
using GrandeTech.QueueHub.API.Domain.ServicesOffered;
using GrandeTech.QueueHub.API.Domain.Users;
using GrandeTech.QueueHub.API.Infrastructure.Repositories.Bogus;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GrandeTech.QueueHub.Tests.Integration.Controllers;

[TestClass]
public class ServicesOfferedControllerTests
{
    private static WebApplicationFactory<Program> _factory = null!;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Jwt:Key"] = "your-super-secret-key-with-at-least-32-characters",
                        ["Jwt:Issuer"] = "GrandeTech.QueueHub.API",
                        ["Jwt:Audience"] = "GrandeTech.QueueHub.API"
                    });
                });
                builder.ConfigureServices(services =>
                {
                    // Ensure we're using the Bogus repository for testing
                    services.AddScoped<IUserRepository, BogusUserRepository>();
                    services.AddScoped<IServicesOfferedRepository, BogusServiceTypeRepository>();
                    services.AddScoped<ILocationRepository, BogusLocationRepository>();
                    services.AddScoped<AuthService>();
                });
            });
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        _factory.Dispose();
    }
    [TestMethod]
    public async Task AddServiceType_ValidRequest_ReturnsCreated()
    {
        var client = _factory.CreateClient();
        var adminToken = await CreateAndAuthenticateUserAsync(client, "Admin");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        // Create a test location first
        var testLocationId = await CreateTestLocationAsync(client);

        var request = new AddServiceOfferedRequest
        {
            Name = "Test Service",
            Description = "Test Description",
            LocationId = testLocationId,
            EstimatedDurationMinutes = 30,
            Price = 25.00m
        };
        var response = await client.PostAsJsonAsync("/api/servicesoffered", request);
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        if (!string.IsNullOrEmpty(content))
        {
            var result = JsonSerializer.Deserialize<AddServiceOfferedResult>(content, _jsonOptions);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.ServiceTypeId);
            Assert.AreEqual(0, result.FieldErrors.Count);
            Assert.AreEqual(0, result.Errors.Count);
        }
    }
    [TestMethod]
    public async Task AddServiceType_InvalidRequest_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var adminToken = await CreateAndAuthenticateUserAsync(client, "Admin");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        // Create a test location first
        var testLocationId = await CreateTestLocationAsync(client);

        var request = new AddServiceOfferedRequest
        {
            Name = "", // Invalid empty name
            Description = "Test Description",
            LocationId = testLocationId,
            EstimatedDurationMinutes = 30,
            Price = 25.00m
        };
        var response = await client.PostAsJsonAsync("/api/servicesoffered", request);
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }
    [TestMethod]
    public async Task GetServiceType_ExistingId_ReturnsOk()
    {
        var client = _factory.CreateClient();
        var adminToken = await CreateAndAuthenticateUserAsync(client, "Admin");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        // Create a test location first
        using var scope = _factory.Services.CreateScope();
        var locationRepository = scope.ServiceProvider.GetRequiredService<ILocationRepository>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        // Get the user from the token
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(adminToken);
        var username = jwtToken.Claims.FirstOrDefault(c => c.Type == TenantClaims.Username)?.Value;
        var organizationId = jwtToken.Claims.FirstOrDefault(c => c.Type == TenantClaims.OrganizationId)?.Value;
        Assert.IsNotNull(username);
        Assert.IsNotNull(organizationId);

        var address = Address.Create(
            "123 Test Street",
            "1",
            "",
            "Test Neighborhood",
            "Test City",
            "Test State",
            "US",
            "12345"
        );

        var testLocation = new Location(
            name: "Test Barbershop",
            slug: "test-barbershop",
            description: "A test barbershop for integration tests",
            organizationId: Guid.Parse(organizationId),
            address: address,
            contactPhone: "+1234567890",
            contactEmail: "test@barbershop.com",
            openingTime: TimeSpan.FromHours(9),
            closingTime: TimeSpan.FromHours(17),
            maxQueueSize: 50,
            lateClientCapTimeInMinutes: 15,
            createdBy: username
        );

        await locationRepository.AddAsync(testLocation, CancellationToken.None);
        var testLocationId = testLocation.Id.ToString();

        var addRequest = new AddServiceOfferedRequest
        {
            Name = "Test Service",
            Description = "Test Description",
            LocationId = testLocationId,
            EstimatedDurationMinutes = 30,
            Price = 25.00m
        };
        var addResponse = await client.PostAsJsonAsync("/api/servicesoffered", addRequest);
        var addContent = await addResponse.Content.ReadAsStringAsync();
        var addResult = JsonSerializer.Deserialize<AddServiceOfferedResult>(addContent, _jsonOptions);
        Assert.IsNotNull(addResult);
        Assert.IsTrue(addResult.Success);

        // Now try to get it
        var getResponse = await client.GetAsync($"/api/servicesoffered/{addResult.ServiceTypeId}");
        var getContent = await getResponse.Content.ReadAsStringAsync();
        var getResult = JsonSerializer.Deserialize<ServiceOfferedDto>(getContent, _jsonOptions);

        Assert.AreEqual(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.IsNotNull(getResult);
        Assert.AreEqual(addResult.ServiceTypeId.ToString(), getResult.Id.ToString());
        Assert.AreEqual(addRequest.Name, getResult.Name);
        Assert.AreEqual(addRequest.Description, getResult.Description);
        Assert.AreEqual(addRequest.EstimatedDurationMinutes, getResult.EstimatedDurationMinutes);
        Assert.AreEqual(addRequest.Price, getResult.Price.Amount);
        Assert.AreEqual(testLocationId, getResult.LocationId);
    }

    [TestMethod]
    public async Task GetServiceType_NonExistingId_ReturnsNotFound()
    {
        var client = _factory.CreateClient();
        var adminToken = await CreateAndAuthenticateUserAsync(client, "Client");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var nonExistingId = Guid.NewGuid();
        var response = await client.GetAsync($"/api/servicesoffered/{nonExistingId}");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task GetServiceType_InvalidId_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var adminToken = await CreateAndAuthenticateUserAsync(client, "Client");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var invalidId = "invalid_id";
        var response = await client.GetAsync($"/api/servicesoffered/{invalidId}");
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }
    [TestMethod]
    public async Task AddServiceType_WithoutAuthentication_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        // Create a test location first (we don't need auth for this since it's direct repository access)
        var testLocationId = await CreateTestLocationAsync(client);

        var request = new AddServiceOfferedRequest
        {
            Name = "Test Service",
            Description = "Test Description",
            LocationId = testLocationId,
            EstimatedDurationMinutes = 30,
            Price = 25.00m
        };
        var response = await client.PostAsJsonAsync("/api/servicesoffered", request);
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task GetServiceType_WithoutAuthentication_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/api/servicesoffered/{Guid.NewGuid()}");
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task GetServiceTypes_ReturnsOk()
    {
        var client = _factory.CreateClient();
        var adminToken = await CreateAndAuthenticateUserAsync(client, "PlatformAdmin");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var response = await client.GetAsync("/api/servicesoffered");
        var responseBody = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Status: {response.StatusCode}");
        Console.WriteLine($"Body: {responseBody}");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var result = JsonSerializer.Deserialize<List<ServicesOfferedDto>>(responseBody, _jsonOptions);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task UpdateServiceType_ValidRequest_ReturnsOk()
    {
        var client = _factory.CreateClient();
        var adminToken = await CreateAndAuthenticateUserAsync(client, "Admin");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        // Create a test location first
        var testLocationId = await CreateTestLocationAsync(client);

        var addRequest = new AddServiceOfferedRequest
        {
            Name = "Test Service",
            Description = "Test Description",
            LocationId = testLocationId,
            EstimatedDurationMinutes = 30,
            Price = 25.00m
        };
        var addResponse = await client.PostAsJsonAsync("/api/servicesoffered", addRequest);
        var addContent = await addResponse.Content.ReadAsStringAsync();
        var addResult = JsonSerializer.Deserialize<AddServiceOfferedResult>(addContent, _jsonOptions);
        Assert.IsNotNull(addResult);
        Assert.IsTrue(addResult.Success);

        var updateRequest = new UpdateServicesOfferedRequest
        {
            Name = "Updated Service",
            Description = "Updated Description",
            LocationId = Guid.Parse(testLocationId),
            EstimatedDurationMinutes = 45,
            Price = 30.00m,
            ImageUrl = "http://example.com/image.jpg",
            IsActive = true
        };
        var updateResponse = await client.PutAsJsonAsync($"/api/servicesoffered/{addResult.ServiceTypeId}", updateRequest);
        Assert.AreEqual(HttpStatusCode.OK, updateResponse.StatusCode);
        
        var updateContent = await updateResponse.Content.ReadAsStringAsync();
        if (!string.IsNullOrEmpty(updateContent))
        {
            var updateResult = JsonSerializer.Deserialize<AddServiceOfferedResult>(updateContent, _jsonOptions);
            Assert.IsNotNull(updateResult);
            Assert.IsTrue(updateResult.Success);
            Assert.AreEqual(addResult.ServiceTypeId, updateResult.ServiceTypeId);
            Assert.AreEqual(0, updateResult.FieldErrors.Count);
            Assert.AreEqual(0, updateResult.Errors.Count);

            // Verify the update by getting the service type
            var getResponse = await client.GetAsync($"/api/servicesoffered/{addResult.ServiceTypeId}");
            Assert.AreEqual(HttpStatusCode.OK, getResponse.StatusCode);
            var getContent = await getResponse.Content.ReadAsStringAsync();
            var getResult = JsonSerializer.Deserialize<ServicesOfferedDto>(getContent, _jsonOptions);
            Assert.IsNotNull(getResult);
            Assert.AreEqual(updateRequest.Name, getResult.Name);
            Assert.AreEqual(updateRequest.Description, getResult.Description);
            Assert.AreEqual(updateRequest.LocationId, getResult.LocationId);
            Assert.AreEqual(updateRequest.EstimatedDurationMinutes, getResult.EstimatedDurationMinutes);
            Assert.AreEqual(updateRequest.Price, getResult.Price.Amount);
            Assert.AreEqual(updateRequest.ImageUrl, getResult.ImageUrl);
            Assert.AreEqual(updateRequest.IsActive, getResult.IsActive);
        }
    }
    [TestMethod]
    public async Task DeleteServiceType_ExistingId_ReturnsNoContent()
    {
        var client = _factory.CreateClient();
        var adminToken = await CreateAndAuthenticateUserAsync(client, "Admin");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        // Create a test location first
        var testLocationId = await CreateTestLocationAsync(client);

        var addRequest = new AddServiceOfferedRequest
        {
            Name = "Test Service",
            Description = "Test Description",
            LocationId = testLocationId,
            EstimatedDurationMinutes = 30,
            Price = 25.00m
        };
        var addResponse = await client.PostAsJsonAsync("/api/servicesoffered", addRequest);
        var addContent = await addResponse.Content.ReadAsStringAsync();
        var addResult = JsonSerializer.Deserialize<AddServiceOfferedResult>(addContent, _jsonOptions);
        Assert.IsNotNull(addResult);
        Assert.IsTrue(addResult.Success);

        var deleteResponse = await client.DeleteAsync($"/api/servicesoffered/{addResult.ServiceTypeId}");
        Assert.AreEqual(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }
    [TestMethod]
    public async Task ActivateServiceType_ExistingId_ReturnsOk()
    {
        var client = _factory.CreateClient();
        var adminToken = await CreateAndAuthenticateUserAsync(client, "Admin");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        // Create a test location first
        using var scope = _factory.Services.CreateScope();
        var locationRepository = scope.ServiceProvider.GetRequiredService<ILocationRepository>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        // Get the user from the token
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(adminToken);
        var username = jwtToken.Claims.FirstOrDefault(c => c.Type == TenantClaims.Username)?.Value;
        var organizationId = jwtToken.Claims.FirstOrDefault(c => c.Type == TenantClaims.OrganizationId)?.Value;
        Assert.IsNotNull(username);
        Assert.IsNotNull(organizationId);

        var address = Address.Create(
            "123 Test Street",
            "1",
            "",
            "Test Neighborhood",
            "Test City",
            "Test State",
            "US",
            "12345"
        );

        var testLocation = new Location(
            name: "Test Barbershop",
            slug: "test-barbershop",
            description: "A test barbershop for integration tests",
            organizationId: Guid.Parse(organizationId),
            address: address,
            contactPhone: "+1234567890",
            contactEmail: "test@barbershop.com",
            openingTime: TimeSpan.FromHours(9),
            closingTime: TimeSpan.FromHours(17),
            maxQueueSize: 50,
            lateClientCapTimeInMinutes: 15,
            createdBy: username
        );

        await locationRepository.AddAsync(testLocation, CancellationToken.None);
        var testLocationId = testLocation.Id.ToString();

        var addRequest = new AddServiceOfferedRequest
        {
            Name = "Test Service",
            Description = "Test Description",
            LocationId = testLocationId,
            EstimatedDurationMinutes = 30,
            Price = 25.00m
        };
        var addResponse = await client.PostAsJsonAsync("/api/servicesoffered", addRequest);
        var addContent = await addResponse.Content.ReadAsStringAsync();
        var addResult = JsonSerializer.Deserialize<AddServiceOfferedResult>(addContent, _jsonOptions);
        Assert.IsNotNull(addResult);
        Assert.IsTrue(addResult.Success);

        // First deactivate the service
        var updateRequest = new UpdateServicesOfferedRequest
        {
            Name = "Test Service",
            Description = "Test Description",
            LocationId = Guid.Parse(testLocationId),
            EstimatedDurationMinutes = 30,
            Price = 25.00m,
            IsActive = false
        };
        var updateResponse = await client.PutAsJsonAsync($"/api/servicesoffered/{addResult.ServiceTypeId}", updateRequest);
        Assert.AreEqual(HttpStatusCode.OK, updateResponse.StatusCode);

        // Now try to activate it
        var activateResponse = await client.PutAsJsonAsync($"/api/servicesoffered/{addResult.ServiceTypeId}/activate", new { });
        Assert.AreEqual(HttpStatusCode.OK, activateResponse.StatusCode);

        // Optionally, if a response body is expected, verify its content
        var activateContent = await activateResponse.Content.ReadAsStringAsync();
        if (!string.IsNullOrWhiteSpace(activateContent))
        {
            var activateResult = JsonSerializer.Deserialize<AddServiceOfferedResult>(activateContent, _jsonOptions);
            Assert.IsNotNull(activateResult, "Activation result should not be null");
            Assert.IsTrue(activateResult.Success, "Activation should be successful");
            Assert.AreEqual(addResult.ServiceTypeId, activateResult.ServiceTypeId, "Service type ID should match");
            Assert.AreEqual(0, activateResult.FieldErrors.Count, "There should be no field errors");
            Assert.AreEqual(0, activateResult.Errors.Count, "There should be no general errors");
        }
    }

    private async Task<string> CreateAndAuthenticateUserAsync(HttpClient client, string role = "Owner")
    {
        var username = $"testuser_{Guid.NewGuid():N}";
        var email = $"{username}@test.com";
        var password = "testpassword123";
        var organizationId = Guid.NewGuid();
        var locationId = Guid.NewGuid();

        // Create a test location first
        using var scope = _factory.Services.CreateScope();
        var locationRepository = scope.ServiceProvider.GetRequiredService<ILocationRepository>();
        var address = Address.Create(
            "123 Test Street",
            "1",
            "",
            "Test Neighborhood",
            "Test City",
            "Test State",
            "US",
            "12345"
        );

        var testLocation = new Location(
            name: "Test Barbershop",
            slug: "test-barbershop",
            description: "A test barbershop for integration tests",
            organizationId: organizationId,
            address: address,
            contactPhone: "+1234567890",
            contactEmail: "test@barbershop.com",
            openingTime: TimeSpan.FromHours(9),
            closingTime: TimeSpan.FromHours(17),
            maxQueueSize: 50,
            lateClientCapTimeInMinutes: 15,
            createdBy: username
        );

        await locationRepository.AddAsync(testLocation, CancellationToken.None);
        locationId = testLocation.Id;

        var user = new User(username, email, BCrypt.Net.BCrypt.HashPassword(password), role);
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        await userRepository.AddAsync(user, CancellationToken.None);

        var loginRequest = new LoginRequest
        {
            Username = username,
            Password = password
        };

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResult>();
        Assert.IsNotNull(loginResult);

        // Handle two-factor authentication for admin users
        if (loginResult.RequiresTwoFactor)
        {
            Assert.IsNotNull(loginResult.TwoFactorToken);
            
            // Verify 2FA
            var verifyRequest = new VerifyTwoFactorRequest
            {
                Username = username,
                TwoFactorCode = "123456", // In a real implementation, this would be validated
                TwoFactorToken = loginResult.TwoFactorToken
            };

            var verifyResponse = await client.PostAsJsonAsync("/api/auth/verify-2fa", verifyRequest);
            Assert.AreEqual(HttpStatusCode.OK, verifyResponse.StatusCode);

            var verifyResult = await verifyResponse.Content.ReadFromJsonAsync<LoginResult>();
            Assert.IsNotNull(verifyResult);
            Assert.IsTrue(verifyResult.Success);
            Assert.IsNotNull(verifyResult.Token);
            return verifyResult.Token;
        }
        else
        {
            // Regular user login
            Assert.AreEqual(HttpStatusCode.OK, loginResponse.StatusCode);
            Assert.IsTrue(loginResult.Success);
            Assert.IsNotNull(loginResult.Token);
            return loginResult.Token;
        }
    }
    private async Task<string> CreateTestLocationAsync(HttpClient client)
    {
        using var scope = _factory.Services.CreateScope();
        var locationRepository = scope.ServiceProvider.GetRequiredService<ILocationRepository>();

        // Create a test organization ID
        var organizationId = Guid.NewGuid();

        // Create a test location directly in the repository
        var address = Address.Create(
            "123 Test Street",
            "1",
            "",
            "Test Neighborhood",
            "Test City",
            "Test State",
            "US",
            "12345"
        );

        var testLocation = new Location(
            name: "Test Barbershop",
            slug: "test-barbershop",
            description: "A test barbershop for integration tests",
            organizationId: organizationId, // Use the same organization ID
            address: address,
            contactPhone: "+1234567890",
            contactEmail: "test@barbershop.com",
            openingTime: TimeSpan.FromHours(9),
            closingTime: TimeSpan.FromHours(17),
            maxQueueSize: 50,
            lateClientCapTimeInMinutes: 15,
            createdBy: "test"
        );

        await locationRepository.AddAsync(testLocation, CancellationToken.None);
        return testLocation.Id.ToString();
    }

    // Add a DTO for test deserialization
    public class ServiceOfferedDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string LocationId { get; set; } = string.Empty;
        public int EstimatedDurationMinutes { get; set; }
        public PriceDto Price { get; set; } = new PriceDto();
    }

    public class PriceDto
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
    }
}