using System;
using System.Threading.Tasks;

namespace GrandeTech.QueueHub.API.Domain.Services
{
    public class AddServiceTypeService
    {
        private readonly IServiceTypeRepository _repository;

        public AddServiceTypeService(IServiceTypeRepository repository)
        {
            _repository = repository;
        }

        public async Task<AddServiceTypeResult> ExecuteAsync(AddServiceTypeRequest request)
        {
            try
            {
                var serviceType = new ServiceType(
                    request.Name,
                    request.Description,
                    request.LocationId,
                    request.EstimatedDurationMinutes,
                    request.Price,
                    request.ImageUrl,
                    request.CreatedBy
                );

                await _repository.AddAsync(serviceType);

                return new AddServiceTypeResult
                {
                    Success = true,
                    ServiceTypeId = serviceType.Id
                };
            }
            catch (Exception ex)
            {
                return new AddServiceTypeResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
} 