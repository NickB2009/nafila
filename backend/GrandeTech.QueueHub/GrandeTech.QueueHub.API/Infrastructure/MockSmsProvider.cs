using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Grande.Fila.API.Application.Notifications.Services;

namespace Grande.Fila.API.Infrastructure
{
    public class MockSmsProvider : ISmsProvider
    {
        private readonly ILogger<MockSmsProvider> _logger;

        public MockSmsProvider(ILogger<MockSmsProvider> logger)
        {
            _logger = logger;
        }

        public async Task<bool> SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
        {
            // Mock implementation - just log the message
            _logger.LogInformation("SMS would be sent to {PhoneNumber}: {Message}", phoneNumber, message);
            
            // Simulate async operation
            await Task.Delay(100, cancellationToken);
            
            return true;
        }
    }
} 