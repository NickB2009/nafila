using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.Locations;

namespace Grande.Fila.API.Application.QrCode
{
    public class QrJoinService
    {
        private readonly IQueueRepository _queueRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly IQrCodeGenerator _qrCodeGenerator;
        private readonly ILogger<QrJoinService> _logger;

        public QrJoinService(
            IQueueRepository queueRepository,
            ILocationRepository locationRepository,
            IQrCodeGenerator qrCodeGenerator,
            ILogger<QrJoinService> logger)
        {
            _queueRepository = queueRepository;
            _locationRepository = locationRepository;
            _qrCodeGenerator = qrCodeGenerator;
            _logger = logger;
        }

        public async Task<QrJoinResult> ExecuteAsync(QrJoinRequest request, string updatedBy, CancellationToken cancellationToken = default)
        {
            var result = new QrJoinResult { Success = false };

            // Validate LocationId
            if (!Guid.TryParse(request.LocationId, out var locationId))
            {
                result.FieldErrors["LocationId"] = "Invalid GUID format";
                return result;
            }

            // Validate ServiceTypeId if provided
            Guid? serviceTypeId = null;
            if (!string.IsNullOrWhiteSpace(request.ServiceTypeId))
            {
                if (!Guid.TryParse(request.ServiceTypeId, out var parsedServiceTypeId))
                {
                    result.FieldErrors["ServiceTypeId"] = "Invalid GUID format";
                    return result;
                }
                serviceTypeId = parsedServiceTypeId;
            }

            // Validate ExpiryMinutes
            if (!int.TryParse(request.ExpiryMinutes ?? "60", out var expiryMinutes) || expiryMinutes <= 0)
            {
                result.FieldErrors["ExpiryMinutes"] = "Must be a positive integer";
                return result;
            }

            try
            {
                // Check location exists
                var locationExists = await _locationRepository.ExistsAsync(l => l.Id == locationId, cancellationToken);
                if (!locationExists)
                {
                    result.Errors.Add("Location not found");
                    return result;
                }

                // Generate join URL with parameters
                var baseUrl = "https://app.eutônafila.com"; // In production, this would come from configuration
                var joinUrl = $"{baseUrl}/join?locationId={locationId}";
                
                if (serviceTypeId.HasValue)
                {
                    joinUrl += $"&serviceTypeId={serviceTypeId}";
                }

                // Add expiry timestamp
                var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);
                var expiryUnixTime = new DateTimeOffset(expiresAt).ToUnixTimeSeconds();
                joinUrl += $"&expires={expiryUnixTime}";

                // Generate QR code
                var qrCodeBase64 = await _qrCodeGenerator.GenerateQrCodeAsync(joinUrl, cancellationToken);

                result.Success = true;
                result.QrCodeBase64 = qrCodeBase64;
                result.JoinUrl = joinUrl;
                result.ExpiresAt = expiresAt;

                _logger.LogInformation("Generated QR code for location {LocationId}, expires at {ExpiresAt}", locationId, expiresAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code for location {LocationId}", locationId);
                result.Errors.Add("Unable to generate QR code");
            }

            return result;
        }
    }
} 