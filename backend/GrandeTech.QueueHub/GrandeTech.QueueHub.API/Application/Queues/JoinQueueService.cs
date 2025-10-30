using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.Customers;
using Grande.Fila.API.Domain.Staff;
using Grande.Fila.API.Domain.Locations;
using Grande.Fila.API.Application.Services.Cache;

namespace Grande.Fila.API.Application.Queues
{
    /// <summary>
    /// Service for handling UC-ENTRY: Client enters queue use case
    /// Implements TDD-driven business logic for joining queues
    /// </summary>
    public class JoinQueueService
    {
        private readonly IQueueRepository _queueRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IStaffMemberRepository _staffMemberRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly IAverageWaitTimeCache _cache;

        public JoinQueueService(
            IQueueRepository queueRepository, 
            ICustomerRepository customerRepository,
            IStaffMemberRepository staffMemberRepository,
            ILocationRepository locationRepository,
            IAverageWaitTimeCache cache)
        {
            _queueRepository = queueRepository;
            _customerRepository = customerRepository;
            _staffMemberRepository = staffMemberRepository;
            _locationRepository = locationRepository;
            _cache = cache;
        }

        /// <summary>
        /// Processes a customer's request to join a queue
        /// Handles both existing and new customers, anonymous and registered
        /// </summary>
        public async Task<JoinQueueResult> JoinQueueAsync(JoinQueueRequest request, string userId, CancellationToken cancellationToken = default)
        {
            var result = new JoinQueueResult();

            // Input validation
            if (!Guid.TryParse(request.QueueId, out var queueId))
            {
                result.FieldErrors["QueueId"] = "Invalid queue ID format.";
            }

            if (string.IsNullOrWhiteSpace(request.CustomerName))
                result.FieldErrors["CustomerName"] = "Customer name is required.";

            // For non-anonymous customers, require contact information
            if (!request.IsAnonymous && string.IsNullOrWhiteSpace(request.PhoneNumber) && string.IsNullOrWhiteSpace(request.Email))
                result.FieldErrors["PhoneNumber"] = "Phone number or email is required for registered customers.";

            if (result.FieldErrors.Count > 0)
                return result;

            try
            {
                // Get queue
                var queue = await _queueRepository.GetByIdAsync(queueId, cancellationToken);
                if (queue == null)
                {
                    result.Errors.Add("Queue not found.");
                    return result;
                }

                // Check if queue is active
                if (!queue.IsActive)
                {
                    result.Errors.Add("Queue is currently inactive.");
                    return result;
                }

                // Get or create customer
                Customer customer;
                if (request.IsAnonymous)
                {
                    // Create anonymous customer
                    customer = new Customer(
                        name: request.CustomerName.Trim(),
                        phoneNumber: null,
                        email: null,
                        isAnonymous: true
                    );
                    customer = await _customerRepository.AddAsync(customer, cancellationToken);
                }
                else
                {
                    // Try to find existing customer by phone number first
                    Customer? existingCustomer = null;
                    if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                    {
                        existingCustomer = await _customerRepository.GetByPhoneNumberAsync(request.PhoneNumber.Trim(), cancellationToken);
                    }

                    // If not found by phone, try by email
                    if (existingCustomer == null && !string.IsNullOrWhiteSpace(request.Email))
                    {
                        existingCustomer = await _customerRepository.GetByEmailAsync(request.Email.Trim(), cancellationToken);
                    }

                    // Create new customer if not found
                    if (existingCustomer == null)
                    {
                        customer = new Customer(
                            name: request.CustomerName.Trim(),
                            phoneNumber: request.PhoneNumber?.Trim(),
                            email: request.Email?.Trim(),
                            isAnonymous: false
                        );
                        customer = await _customerRepository.AddAsync(customer, cancellationToken);
                    }
                    else
                    {
                        customer = existingCustomer;
                        // Update existing customer's name if different
                        if (customer.Name != request.CustomerName.Trim())
                        {
                            customer.Name = request.CustomerName.Trim();
                            // UpdateProfile method removed during simplification - direct property assignment
                            await _customerRepository.UpdateAsync(customer, cancellationToken);
                        }
                    }
                }

                // Parse service type if provided
                Guid? serviceTypeId = null;
                if (!string.IsNullOrWhiteSpace(request.ServiceTypeId) && Guid.TryParse(request.ServiceTypeId, out var parsedServiceTypeId))
                {
                    serviceTypeId = parsedServiceTypeId;
                }

                // Add customer to queue
                var queueEntry = queue.AddCustomerToQueue(
                    customerId: customer.Id,
                    customerName: customer.Name,
                    staffMemberId: null, // Will be assigned when called
                    serviceTypeId: serviceTypeId,
                    notes: request.Notes?.Trim()
                );

                // Calculate estimated wait time using real data
                var location = await _locationRepository.GetByIdAsync(queue.LocationId, cancellationToken);
                
                // Get average service time from cache or use location default
                var averageTimeMinutes = location?.AverageServiceTimeInMinutes ?? 30.0;
                if (_cache.TryGetAverage(queue.LocationId, out var cachedAverage))
                {
                    averageTimeMinutes = cachedAverage;
                }

                // Get active staff count
                var staffMembers = await _staffMemberRepository.GetByLocationAsync(queue.LocationId, cancellationToken);
                var activeStaffCount = staffMembers.Count(s => s.IsActive && !s.IsOnBreak());

                // Use real values for calculation
                var estimatedWaitTime = activeStaffCount > 0 
                    ? queue.CalculateEstimatedWaitTimeInMinutes(queueEntry.Id, averageTimeMinutes, activeStaffCount)
                    : -1; // No staff available

                // Update queue
                await _queueRepository.UpdateAsync(queue, cancellationToken);

                // Return success result
                result.Success = true;
                result.QueueEntryId = queueEntry.Id.ToString();
                result.Position = queueEntry.Position;
                result.EstimatedWaitTimeMinutes = estimatedWaitTime;
                
                return result;
            }
            catch (InvalidOperationException ex)
            {
                result.Errors.Add(ex.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                result.Errors.Add(ex.Message);
                return result;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"An error occurred while joining the queue: {ex.Message}");
                return result;
            }
        }
    }
}
