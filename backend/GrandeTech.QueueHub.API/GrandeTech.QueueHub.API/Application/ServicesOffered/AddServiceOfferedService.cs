using System;
using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Domain.Locations;
using GrandeTech.QueueHub.API.Domain.ServicesOffered;

namespace GrandeTech.QueueHub.API.Application.ServicesOffered
{
    public class AddServiceOfferedService
    {
        private readonly IServicesOfferedRepository _serviceTypeRepo;
        private readonly ILocationRepository _locationRepo;

        public AddServiceOfferedService(IServicesOfferedRepository serviceTypeRepo, ILocationRepository locationRepo)
        {
            _serviceTypeRepo = serviceTypeRepo;
            _locationRepo = locationRepo;
        }

        public async Task<AddServiceOfferedResult> AddServiceTypeAsync(AddServiceOfferedRequest request, string userId, CancellationToken cancellationToken = default)
        {
            var result = new AddServiceOfferedResult();

            // Validate LocationId format
            if (!Guid.TryParse(request.LocationId, out var locationId))
            {
                result.FieldErrors["LocationId"] = "Invalid location ID format.";
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(request.Name))
                result.FieldErrors["Name"] = "Service name is required.";

            if (request.EstimatedDurationMinutes <= 0)
                result.FieldErrors["EstimatedDurationMinutes"] = "Duration must be greater than 0 minutes.";

            if (request.Price < 0)
                result.FieldErrors["Price"] = "Price cannot be negative.";

            if (result.FieldErrors.Count > 0)
                return result;

            // Check if location exists
            var locationExists = await _locationRepo.ExistsAsync(location => location.Id == locationId, cancellationToken);
            if (!locationExists)
            {
                result.FieldErrors["LocationId"] = "Location not found.";
                return result;
            }

            // Check if service name already exists at this location
            var serviceExists = await _serviceTypeRepo.ExistsAsync(
                service => service.LocationId == locationId && service.Name == request.Name.Trim(), 
                cancellationToken);
            
            if (serviceExists)
            {
                result.FieldErrors["Name"] = "A service with this name already exists at this location.";
                return result;
            }

            try
            {
                var serviceType = new ServiceOffered(
                    name: request.Name.Trim(),
                    description: request.Description?.Trim() ?? "",
                    locationId: locationId,
                    estimatedDurationMinutes: request.EstimatedDurationMinutes,
                    priceValue: request.Price,
                    imageUrl: request.ImageUrl?.Trim(),
                    createdBy: userId
                );

                await _serviceTypeRepo.AddAsync(serviceType, cancellationToken);

                result.Success = true;
                result.ServiceTypeId = serviceType.Id.ToString();
                return result;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"An error occurred while creating the service type: {ex.Message}");
                return result;
            }
        }
    }
}
