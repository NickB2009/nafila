using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http.Json;
using System.Text.Json;

namespace Grande.Fila.API.Tests.Integration
{
    /// <summary>
    /// Base class for production integration tests that call actual API endpoints
    /// </summary>
    [TestClass]
    [TestCategory("Production")]
    public abstract class ProductionIntegrationTestBase
    {
        protected HttpClient _client;
        protected string _baseUrl;
        protected JsonSerializerOptions _jsonOptions;

        [TestInitialize]
        public virtual void Setup()
        {
            // Get the environment from environment variables set by the test runner
            var environment = Environment.GetEnvironmentVariable("TEST_ENVIRONMENT") ?? "Local";
            var prodUrl = Environment.GetEnvironmentVariable("PROD_API_URL") ?? "https://api.eutonafila.com.br";

            // Configure the base URL based on environment
            _baseUrl = environment switch
            {
                "Prod" => prodUrl,
                "BoaHost" => prodUrl,
                _ => "http://localhost:5098"
            };

            _client = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };

            // Add common headers
            _client.DefaultRequestHeaders.Add("User-Agent", "QueueHub-Production-Tests/1.0");

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };

            Console.WriteLine($"Production Integration Test Setup - Environment: {environment}, Base URL: {_baseUrl}");
        }

        [TestCleanup]
        public virtual void Cleanup()
        {
            _client?.Dispose();
        }

        /// <summary>
        /// Helper method to make authenticated requests to production endpoints
        /// </summary>
        protected async Task<HttpResponseMessage> MakeAuthenticatedRequestAsync(
            HttpMethod method, 
            string endpoint, 
            object? content = null, 
            string? authToken = null)
        {
            var request = new HttpRequestMessage(method, endpoint);

            if (!string.IsNullOrEmpty(authToken))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
            }

            if (content != null)
            {
                request.Content = JsonContent.Create(content, options: _jsonOptions);
            }

            return await _client.SendAsync(request);
        }

        /// <summary>
        /// Helper method to make unauthenticated requests to production endpoints
        /// </summary>
        protected async Task<HttpResponseMessage> MakeRequestAsync(
            HttpMethod method, 
            string endpoint, 
            object? content = null)
        {
            return await MakeAuthenticatedRequestAsync(method, endpoint, content, null);
        }

        /// <summary>
        /// Helper method to deserialize response content
        /// </summary>
        protected async Task<T?> DeserializeResponseAsync<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response Status: {response.StatusCode}");
            Console.WriteLine($"Response Content: {content}");

            if (string.IsNullOrEmpty(content))
                return default(T);

            try
            {
                return JsonSerializer.Deserialize<T>(content, _jsonOptions);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Failed to deserialize response: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Verify that the production API is accessible
        /// </summary>
        protected async Task<bool> VerifyProductionApiAccessAsync()
        {
            try
            {
                var response = await _client.GetAsync("/api/Health");
                var isHealthy = response.IsSuccessStatusCode;
                
                Console.WriteLine($"Production API Health Check: {(isHealthy ? "✅ Healthy" : "❌ Unhealthy")} - Status: {response.StatusCode}");
                return isHealthy;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to access production API: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get authentication token for production testing (if needed)
        /// This would need to be implemented based on your authentication flow
        /// </summary>
        protected virtual async Task<string?> GetProductionAuthTokenAsync()
        {
            // For now, return null to test unauthenticated endpoints
            // In the future, this could authenticate with production using test credentials
            return null;
        }
    }
}
