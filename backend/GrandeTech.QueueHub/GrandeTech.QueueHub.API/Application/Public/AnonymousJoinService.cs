using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.Customers;
using Grande.Fila.API.Domain.Locations;
using Grande.Fila.API.Domain.Staff;
using Grande.Fila.API.Application.Services.Cache;

namespace Grande.Fila.API.Application.Public
{
    /// <summary>
    /// Service for handling anonymous users joining queues
    /// </summary>
    public class AnonymousJoinService
    {
        private readonly IQueueRepository _queueRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly IStaffMemberRepository _staffMemberRepository;
        private readonly IAverageWaitTimeCache _cache;

        public AnonymousJoinService(
            IQueueRepository queueRepository,
            ICustomerRepository customerRepository,
            ILocationRepository locationRepository,
            IStaffMemberRepository staffMemberRepository,
            IAverageWaitTimeCache cache)
        {
            _queueRepository = queueRepository ?? throw new ArgumentNullException(nameof(queueRepository));
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
            _staffMemberRepository = staffMemberRepository ?? throw new ArgumentNullException(nameof(staffMemberRepository));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        /// <summary>
        /// Processes an anonymous user's request to join a queue
        /// </summary>
        public async Task<AnonymousJoinResult> ExecuteAsync(AnonymousJoinRequest request, CancellationToken cancellationToken = default)
        {
            var result = new AnonymousJoinResult();

            // Input validation
            if (!Guid.TryParse(request.SalonId, out var salonId))
            {
                result.FieldErrors["SalonId"] = "Invalid salon ID format.";
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                result.FieldErrors["Name"] = "Name is required.";
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                result.FieldErrors["Email"] = "Email is required.";
            }
            else if (!IsValidEmail(request.Email))
            {
                result.FieldErrors["Email"] = "Invalid email format.";
            }

            if (string.IsNullOrWhiteSpace(request.AnonymousUserId) || !Guid.TryParse(request.AnonymousUserId, out _))
            {
                result.FieldErrors["AnonymousUserId"] = "Valid anonymous user ID is required.";
            }

            if (string.IsNullOrWhiteSpace(request.ServiceRequested))
            {
                result.FieldErrors["ServiceRequested"] = "Service requested is required.";
            }

            if (result.FieldErrors.Count > 0)
                return result;

            try
            {
                // Check if salon exists
                var location = await _locationRepository.GetByIdAsync(salonId, cancellationToken);
                if (location == null)
                {
                    result.Errors.Add("Salon not found");
                    return result;
                }

                // Get active queue for the salon
                var queue = await _queueRepository.GetActiveQueueByLocationIdAsync(salonId, cancellationToken);
                if (queue == null)
                {
                    result.Errors.Add("Salon not found or not accepting customers");
                    return result;
                }

                // Check if customer already exists by email
                var existingCustomer = await _customerRepository.GetByEmailAsync(request.Email.Trim(), cancellationToken);
                if (existingCustomer != null)
                {
                    // Check if customer is already in this queue
                    if (queue.Entries.Any(e => e.CustomerId == existingCustomer.Id && e.Status != "completed" && e.Status != "cancelled"))
                    {
                        result.Errors.Add("User already in queue for this salon");
                        return result;
                    }
                }

                // Create anonymous customer
                var customer = new Customer(
                    name: request.Name.Trim(),
                    phoneNumber: null,
                    email: request.Email.Trim(),
                    isAnonymous: true
                );

                customer = await _customerRepository.AddAsync(customer, cancellationToken);

                // Add customer to queue
                var queueEntry = queue.AddCustomerToQueue(
                    customerId: customer.Id,
                    customerName: customer.Name,
                    staffMemberId: null,
                    serviceTypeId: null,
                    notes: $"Service: {request.ServiceRequested}"
                );

                // Calculate estimated wait time
                var averageTimeMinutes = location.AverageServiceTimeInMinutes ?? 30.0;
                if (_cache.TryGetAverage(salonId, out var cachedAverage))
                {
                    averageTimeMinutes = cachedAverage;
                }

                // Get active staff count
                var staffMembers = await _staffMemberRepository.GetByLocationAsync(salonId, cancellationToken);
                var activeStaffCount = staffMembers.Count(s => s.IsActive && !s.IsOnBreak());

                var estimatedWaitTime = activeStaffCount > 0
                    ? queue.CalculateEstimatedWaitTimeInMinutes(queueEntry.Id, averageTimeMinutes, activeStaffCount)
                    : -1; // No staff available

                // Update queue
                await _queueRepository.UpdateAsync(queue, cancellationToken);

                // Return success result
                result.Success = true;
                result.Id = queueEntry.Id.ToString();
                result.Position = queueEntry.Position;
                result.EstimatedWaitMinutes = estimatedWaitTime;
                result.JoinedAt = queueEntry.JoinedAt;
                result.Status = "waiting";

                return result;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"An error occurred while joining the queue: {ex.Message}");
                return result;
            }
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Use a simple regex pattern for email validation
                var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                return Regex.IsMatch(email, emailPattern, RegexOptions.IgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }
}