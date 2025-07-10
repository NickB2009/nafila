using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Domain.Locations;
using Grande.Fila.API.Domain.ServicesOffered;
using Grande.Fila.API.Domain.Common.ValueObjects;

namespace Grande.Fila.API.Application.ServicesOffered
{
    public class AddServiceOfferedService
    {
        private readonly IServicesOfferedRepository _serviceTypeRepository;
        private readonly ILocationRepository _locationRepo;

        public AddServiceOfferedService(IServicesOfferedRepository serviceTypeRepository, ILocationRepository locationRepo)
        {
            _serviceTypeRepository = serviceTypeRepository ?? throw new ArgumentNullException(nameof(serviceTypeRepository));
            _locationRepo = locationRepo ?? throw new ArgumentNullException(nameof(locationRepo));
        }

        public async Task<AddServiceOfferedResult> AddServiceTypeAsync(AddServiceOfferedRequest request, string userId, CancellationToken cancellationToken)
        {
            var result = new AddServiceOfferedResult
            {
                Success = false,
                FieldErrors = new Dictionary<string, string>(),
                Errors = new List<string>()
            };

            // Validation
            if (string.IsNullOrWhiteSpace(request.Name))
                result.FieldErrors["Name"] = "Service name is required.";

            // LocationId validation (guid format)
            if (!Guid.TryParse(request.LocationId, out var locationGuid))
            {
                result.FieldErrors["LocationId"] = "Invalid location ID format.";
            }
            else
            {
                // Check if location exists (repository is non-null due to constructor guard)
                var locationExists = await _locationRepo!.ExistsAsync(l => l.Id == locationGuid, cancellationToken);
                if (!locationExists)
                {
                    result.FieldErrors["LocationId"] = "Location not found.";
                }
            }

            if (request.EstimatedDurationMinutes <= 0)
                result.FieldErrors["EstimatedDurationMinutes"] = "Duration must be greater than 0 minutes.";

            if (request.Price < 0)
                result.FieldErrors["Price"] = "Price cannot be negative.";

            // Duplicate name validation (within same location)
            if (string.IsNullOrWhiteSpace(request.Name) == false && result.FieldErrors.ContainsKey("LocationId") == false)
            {
                var duplicateExists = await _serviceTypeRepository!.ExistsAsync(s => s.LocationId == locationGuid && s.Name.ToLower() == request.Name.ToLower(), cancellationToken);
                if (duplicateExists)
                    result.FieldErrors["Name"] = "A service with this name already exists at this location.";
            }

            if (result.FieldErrors.Count > 0)
                return result;

            try
            {
                var serviceType = new ServiceOffered(
                    request.Name,
                    request.Description,
                    locationGuid,
                    request.EstimatedDurationMinutes,
                    request.Price,
                    request.ImageUrl,
                    userId
                );

                await _serviceTypeRepository.AddAsync(serviceType, cancellationToken);
                result.Success = true;
                result.ServiceTypeId = serviceType?.Id.ToString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Failed to add service type: {ex.Message}");
            }

            return result;
        }

        public async Task<AddServiceOfferedResult> UpdateServiceTypeAsync(Guid serviceTypeId, UpdateServiceOfferedRequest request, string userId, CancellationToken cancellationToken)
        {
            var result = new AddServiceOfferedResult
            {
                Success = false,
                FieldErrors = new Dictionary<string, string>(),
                Errors = new List<string>()
            };

            // Validation
            if (string.IsNullOrWhiteSpace(request.Name))
                result.FieldErrors["Name"] = "Service name is required.";

            if (request.EstimatedDurationMinutes <= 0)
                result.FieldErrors["EstimatedDurationMinutes"] = "Duration must be greater than 0 minutes.";

            if (request.Price < 0)
                result.FieldErrors["Price"] = "Price cannot be negative.";

            if (result.FieldErrors.Count > 0)
                return result;

            var serviceType = await _serviceTypeRepository.GetByIdAsync(serviceTypeId, cancellationToken);
            if (serviceType == null)
            {
                result.Errors.Add("Service type not found.");
                return result;
            }

            try
            {
                serviceType.UpdateDetails(
                    request.Name,
                    request.Description,
                    request.EstimatedDurationMinutes,
                    request.Price,
                    request.ImageUrl,
                    userId
                );

                await _serviceTypeRepository.UpdateAsync(serviceType, cancellationToken);
                result.Success = true;
                result.ServiceTypeId = serviceType.Id.ToString();
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Failed to update service type: {ex.Message}");
            }

            return result;
        }

        public async Task<AddServiceOfferedResult> DeleteServiceTypeAsync(Guid serviceTypeId, string userId, CancellationToken cancellationToken)
        {
            var result = new AddServiceOfferedResult
            {
                Success = false,
                FieldErrors = new Dictionary<string, string>(),
                Errors = new List<string>()
            };

            var serviceType = await _serviceTypeRepository.GetByIdAsync(serviceTypeId, cancellationToken);
            if (serviceType == null)
            {
                result.Errors.Add("Service type not found.");
                return result;
            }

            try
            {
                await _serviceTypeRepository.DeleteAsync(serviceType, cancellationToken);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Failed to delete service type: {ex.Message}");
            }

            return result;
        }

        public async Task<AddServiceOfferedResult> ActivateServiceTypeAsync(Guid serviceTypeId, string userId, CancellationToken cancellationToken)
        {
            var result = new AddServiceOfferedResult
            {
                Success = false,
                FieldErrors = new Dictionary<string, string>(),
                Errors = new List<string>()
            };

            var serviceType = await _serviceTypeRepository.GetByIdAsync(serviceTypeId, cancellationToken);
            if (serviceType == null)
            {
                result.Errors.Add("Service type not found.");
                return result;
            }

            try
            {
                serviceType.Activate(userId);
                await _serviceTypeRepository.UpdateAsync(serviceType, cancellationToken);
                result.Success = true;
                result.ServiceTypeId = serviceType.Id.ToString();
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Failed to activate service type: {ex.Message}");
            }

            return result;
        }
    }
}
