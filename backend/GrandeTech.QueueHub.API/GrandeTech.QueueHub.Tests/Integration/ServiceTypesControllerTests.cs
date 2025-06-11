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

namespace GrandeTech.QueueHub.Tests.Integration;

[TestClass]
public class ServiceTypesControllerTests
{
    private static WebApplicationFactory<Program> _factory = null!;

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
                        ["Jwt:Key"] = "your-super-secret-key-with-at-least-32-characters-for-testing",
                        ["Jwt:Issuer"] = "GrandeTech.QueueHub.API.Test",
                        ["Jwt:Audience"] = "GrandeTech.QueueHub.API.Test"
                    });
                }); builder.ConfigureServices(services =>
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
        var adminToken = await CreateAndAuthenticateUserAsync("Admin", client);
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
        var response = await client.PostAsJsonAsync("/api/servicetypes", request);
        var result = await response.Content.ReadFromJsonAsync<AddServiceOfferedResult>();
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.ServiceTypeId);
        Assert.AreEqual(0, result.FieldErrors.Count);
        Assert.AreEqual(0, result.Errors.Count);
    }
    [TestMethod]
    public async Task AddServiceType_InvalidRequest_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var adminToken = await CreateAndAuthenticateUserAsync("Admin", client);
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
        var response = await client.PostAsJsonAsync("/api/servicetypes", request);
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }
    [TestMethod]
    public async Task GetServiceType_ExistingId_ReturnsOk()
    {
        var client = _factory.CreateClient();
        var adminToken = await CreateAndAuthenticateUserAsync("Admin", client);
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
        var addResponse = await client.PostAsJsonAsync("/api/servicetypes", addRequest);
        var addResult = await addResponse.Content.ReadFromJsonAsync<AddServiceOfferedResult>();
        Assert.IsNotNull(addResult);
        Assert.IsTrue(addResult.Success);
        var getResponse = await client.GetAsync($"/api/servicetypes/{addResult.ServiceTypeId}");
        var getResult = await getResponse.Content.ReadFromJsonAsync<ServicesOfferedDto>();
        Assert.AreEqual(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.IsNotNull(getResult);
        Assert.AreEqual(addResult.ServiceTypeId, getResult.Id.ToString());
        Assert.AreEqual(addRequest.Name, getResult.Name);
        Assert.AreEqual(addRequest.Description, getResult.Description);
        Assert.AreEqual(Guid.Parse(addRequest.LocationId), getResult.LocationId);
        Assert.AreEqual(addRequest.EstimatedDurationMinutes, getResult.EstimatedDurationMinutes);
        Assert.AreEqual(addRequest.Price, getResult.Price);
    }

    [TestMethod]
    public async Task GetServiceType_NonExistingId_ReturnsNotFound()
    {
        var client = _factory.CreateClient();
        var adminToken = await CreateAndAuthenticateUserAsync("Admin", client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var nonExistingId = Guid.NewGuid();
        var response = await client.GetAsync($"/api/servicetypes/{nonExistingId}");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task GetServiceType_InvalidId_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var adminToken = await CreateAndAuthenticateUserAsync("Admin", client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var invalidId = "invalid_id";
        var response = await client.GetAsync($"/api/servicetypes/{invalidId}");
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
        var response = await client.PostAsJsonAsync("/api/servicetypes", request);
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task GetServiceType_WithoutAuthentication_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/api/servicetypes/{Guid.NewGuid()}");
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task GetServiceTypes_ReturnsOk()
    {
        var client = _factory.CreateClient();
        var adminToken = await CreateAndAuthenticateUserAsync("Admin", client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var response = await client.GetAsync("/api/servicetypes");
        var result = await response.Content.ReadFromJsonAsync<List<ServicesOfferedDto>>();
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task UpdateServiceType_ValidRequest_ReturnsOk()
    {
        var client = _factory.CreateClient();
        var adminToken = await CreateAndAuthenticateUserAsync("Admin", client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken); var addRequest = new AddServiceOfferedRequest
        {
            Name = "Test Service",
            Description = "Test Description",
            LocationId = "12345678-1234-1234-1234-123456789012",
            EstimatedDurationMinutes = 30,
            Price = 25.00m
        };
        var addResponse = await client.PostAsJsonAsync("/api/servicetypes", addRequest);
        var addResult = await addResponse.Content.ReadFromJsonAsync<AddServiceOfferedResult>();
        Assert.IsNotNull(addResult);
        Assert.IsTrue(addResult.Success);
        var updateRequest = new UpdateServicesOfferedRequest
        {
            Name = "Updated Service",
            Description = "Updated Description",
            LocationId = Guid.Parse("12345678-1234-1234-1234-123456789012"),
            EstimatedDurationMinutes = 45,
            Price = 30.00m,
            ImageUrl = "http://example.com/image.jpg",
            IsActive = true
        };
        var updateResponse = await client.PutAsJsonAsync($"/api/servicetypes/{addResult.ServiceTypeId}", updateRequest);
        var updateResult = await updateResponse.Content.ReadFromJsonAsync<ServicesOfferedDto>();
        Assert.AreEqual(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.IsNotNull(updateResult);
        Assert.AreEqual(updateRequest.Name, updateResult.Name);
        Assert.AreEqual(updateRequest.Description, updateResult.Description);
        Assert.AreEqual(updateRequest.LocationId, updateResult.LocationId);
        Assert.AreEqual(updateRequest.EstimatedDurationMinutes, updateResult.EstimatedDurationMinutes);
        Assert.AreEqual(updateRequest.Price, updateResult.Price);
        Assert.AreEqual(updateRequest.ImageUrl, updateResult.ImageUrl);
        Assert.AreEqual(updateRequest.IsActive, updateResult.IsActive);
    }
    [TestMethod]
    public async Task DeleteServiceType_ExistingId_ReturnsNoContent()
    {
        var client = _factory.CreateClient();
        var adminToken = await CreateAndAuthenticateUserAsync("Admin", client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var addRequest = new AddServiceOfferedRequest
        {
            Name = "Test Service",
            Description = "Test Description",
            LocationId = "12345678-1234-1234-1234-123456789012",
            EstimatedDurationMinutes = 30,
            Price = 25.00m
        };
        var addResponse = await client.PostAsJsonAsync("/api/servicetypes", addRequest);
        var addResult = await addResponse.Content.ReadFromJsonAsync<AddServiceOfferedResult>();
        Assert.IsNotNull(addResult);
        Assert.IsTrue(addResult.Success);
        var deleteResponse = await client.DeleteAsync($"/api/servicetypes/{addResult.ServiceTypeId}");
        Assert.AreEqual(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }
    [TestMethod]
    public async Task ActivateServiceType_ExistingId_ReturnsOk()
    {
        var client = _factory.CreateClient();
        var adminToken = await CreateAndAuthenticateUserAsync("Admin", client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var addRequest = new AddServiceOfferedRequest
        {
            Name = "Test Service",
            Description = "Test Description",
            LocationId = "12345678-1234-1234-1234-123456789012",
            EstimatedDurationMinutes = 30,
            Price = 25.00m
        };
        var addResponse = await client.PostAsJsonAsync("/api/servicetypes", addRequest);
        var addResult = await addResponse.Content.ReadFromJsonAsync<AddServiceOfferedResult>();
        Assert.IsNotNull(addResult);
        Assert.IsTrue(addResult.Success);
        var activateResponse = await client.PostAsync($"/api/servicetypes/{addResult.ServiceTypeId}/activate", null);
        var activateResult = await activateResponse.Content.ReadFromJsonAsync<ServicesOfferedDto>();
        Assert.AreEqual(HttpStatusCode.OK, activateResponse.StatusCode);
        Assert.IsNotNull(activateResult);
        Assert.IsTrue(activateResult.IsActive);
    }

    private async Task<string> CreateAndAuthenticateUserAsync(string role, HttpClient client)
    {
        using var scope = _factory.Services.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        // Create a user with the specified role
        var username = $"testuser_{role.ToLower()}_{Guid.NewGuid():N}";
        var email = $"{username}@test.com";
        var password = "testpassword123";

        var user = new User(username, email, BCrypt.Net.BCrypt.HashPassword(password), role);
        await userRepository.AddAsync(user, CancellationToken.None);

        // Login to get the token
        var loginRequest = new LoginRequest
        {
            Username = username,
            Password = password
        };

        var loginJson = JsonSerializer.Serialize(loginRequest);
        var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");

        var loginResponse = await client.PostAsync("/api/auth/login", loginContent);
        Assert.AreEqual(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();
        var loginResult = JsonSerializer.Deserialize<LoginResult>(loginResponseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.IsNotNull(loginResult);
        Assert.IsTrue(loginResult.Success);
        Assert.IsNotNull(loginResult.Token); return loginResult.Token;
    }
    private async Task<string> CreateTestLocationAsync(HttpClient client)
    {
        using var scope = _factory.Services.CreateScope();
        var locationRepository = scope.ServiceProvider.GetRequiredService<ILocationRepository>();

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
            organizationId: Guid.NewGuid(),
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
}