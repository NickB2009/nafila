using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.Customers;
using Grande.Fila.API.Domain.Staff;

namespace Grande.Fila.API.Application.Queues
{
    /// <summary>
    /// Service for handling UC-BARBERADD: Barber adds client to queue use case
    /// Implements TDD-driven business logic for barbers adding customers to queues
    /// </summary>
    public class BarberAddService
    {
        private readonly IQueueRepository _queueRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IStaffMemberRepository _staffRepository;

        public BarberAddService(
            IQueueRepository queueRepository, 
            ICustomerRepository customerRepository,
            IStaffMemberRepository staffRepository)
        {
            _queueRepository = queueRepository;
            _customerRepository = customerRepository;
            _staffRepository = staffRepository;
        }

        /// <summary>
        /// Allows a barber to add a client to the end of the present pool (queue)
        /// </summary>
        public async Task<BarberAddResult> BarberAddAsync(BarberAddRequest request, string userId, CancellationToken cancellationToken = default)
        {
            var result = new BarberAddResult();

            // Input validation
            if (!Guid.TryParse(request.QueueId, out var queueId))
            {
                result.FieldErrors["QueueId"] = "Invalid queue ID format.";
            }

            if (!Guid.TryParse(request.StaffMemberId, out var staffMemberId))
            {
                result.FieldErrors["StaffMemberId"] = "Invalid staff member ID format.";
            }

            if (string.IsNullOrWhiteSpace(request.CustomerName))
            {
                result.FieldErrors["CustomerName"] = "Customer name is required.";
            }

            if (result.FieldErrors.Count > 0)
                return result;

            try
            {
                // Get staff member
                var staffMember = await _staffRepository.GetByIdAsync(staffMemberId, cancellationToken);
                if (staffMember == null)
                {
                    result.Errors.Add("Staff member not found.");
                    return result;
                }

                // Check if staff member is active
                if (!staffMember.IsActive)
                {
                    result.Errors.Add("Cannot add customers for inactive staff member.");
                    return result;
                }

                // Check if staff member is on break
                if (staffMember.IsOnBreak())
                {
                    result.Errors.Add("Cannot add customers while on break.");
                    return result;
                }

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

                // Check if queue is full
                var activeEntriesCount = queue.Entries.Count(e => e.Status == QueueEntryStatus.Waiting || e.Status == QueueEntryStatus.Called);
                if (activeEntriesCount >= queue.MaxSize)
                {
                    result.Errors.Add("Queue has reached its maximum size.");
                    return result;
                }

                // Get or create customer
                Customer customer;
                if (string.IsNullOrWhiteSpace(request.PhoneNumber) && string.IsNullOrWhiteSpace(request.Email))
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
                    staffMemberId: staffMemberId, // Assign the barber who added the customer
                    serviceTypeId: serviceTypeId,
                    notes: request.Notes?.Trim()
                );

                // Update queue
                await _queueRepository.UpdateAsync(queue, cancellationToken);

                // Populate the result with success data
                result.Success = true;
                result.QueueEntryId = queueEntry.Id.ToString();
                result.CustomerName = queueEntry.CustomerName;
                result.Position = queueEntry.Position;
                result.AddedAt = queueEntry.EnteredAt;
                result.QueueId = queue.Id.ToString();
                result.StaffMemberId = staffMemberId.ToString();

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
                result.Errors.Add($"An error occurred while adding customer to queue: {ex.Message}");
                return result;
            }
        }
    }
} 