using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Grande.Fila.API.Application.QrCode;

namespace Grande.Fila.API.Infrastructure
{
    public class MockQrCodeGenerator : IQrCodeGenerator
    {
        private readonly ILogger<MockQrCodeGenerator> _logger;

        public MockQrCodeGenerator(ILogger<MockQrCodeGenerator> logger)
        {
            _logger = logger;
        }

        public async Task<string> GenerateQrCodeAsync(string data, CancellationToken cancellationToken = default)
        {
            // Mock implementation - generate a base64 encoded string representing a QR code
            _logger.LogInformation("Generating QR code for data: {Data}", data);
            
            // Simulate async operation
            await Task.Delay(50, cancellationToken);
            
            // Create a mock base64 string (in real implementation this would be actual QR code image)
            var mockQrData = $"QR-{Convert.ToBase64String(Encoding.UTF8.GetBytes(data))}";
            
            return mockQrData;
        }
    }
} 