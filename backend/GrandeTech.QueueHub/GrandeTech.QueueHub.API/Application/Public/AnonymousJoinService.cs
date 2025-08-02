using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.Customers;
using Grande.Fila.API.Domain.Locations;
using Grande.Fila.API.Domain.Staff;
using Grande.Fila.API.Domain.Common;
using Grande.Fila.API.Application.Services.Cache;
using Grande.Fila.API.Infrastructure.Data;

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
        private readonly IUnitOfWork _unitOfWork;
        private readonly QueueHubDbContext _context;

        public AnonymousJoinService(
            IQueueRepository queueRepository,
            ICustomerRepository customerRepository,
            ILocationRepository locationRepository,
            IStaffMemberRepository staffMemberRepository,
            IAverageWaitTimeCache cache,
            IUnitOfWork unitOfWork,
            QueueHubDbContext context)
        {
            _queueRepository = queueRepository ?? throw new ArgumentNullException(nameof(queueRepository));
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
            _staffMemberRepository = staffMemberRepository ?? throw new ArgumentNullException(nameof(staffMemberRepository));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _context = context ?? throw new ArgumentNullException(nameof(context));
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

            // Retry logic for concurrency issues
            const int maxRetries = 3;
            var retryCount = 0;

            while (retryCount < maxRetries)
            {
                try
                {
                    return await TryJoinQueueAsync(request, salonId, cancellationToken);
                }
                catch (Exception ex) when (IsConcurrencyException(ex) && retryCount < maxRetries - 1)
                {
                    retryCount++;
                    // Wait a bit before retrying (exponential backoff)
                    await Task.Delay(100 * retryCount, cancellationToken);
                    continue;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"An error occurred while joining the queue: {ex.Message}");
                    return result;
                }
            }

            result.Errors.Add("Failed to join queue after multiple attempts due to concurrent modifications");
            return result;
        }

        private async Task<AnonymousJoinResult> TryJoinQueueAsync(AnonymousJoinRequest request, Guid salonId, CancellationToken cancellationToken)
        {
            var result = new AnonymousJoinResult();

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
                if (queue.Entries.Any(e => e.CustomerId == existingCustomer.Id && e.Status != QueueEntryStatus.Completed && e.Status != QueueEntryStatus.Cancelled))
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

            // Add customer to repository
            customer = await _customerRepository.AddAsync(customer, cancellationToken);

            // Get next position for the queue
            var nextPosition = await _queueRepository.GetNextPositionAsync(queue.Id, cancellationToken);

            // Create queue entry directly
            var queueEntry = new QueueEntry(
                queueId: queue.Id,
                customerId: customer.Id,
                customerName: customer.Name,
                position: nextPosition,
                staffMemberId: null,
                serviceTypeId: null,
                notes: $"Service: {request.ServiceRequested}"
            );

            // Add queue entry to context
            await _context.QueueEntries.AddAsync(queueEntry, cancellationToken);

            // Calculate estimated wait time
            var averageTimeMinutes = location.AverageServiceTimeInMinutes;
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

            // Save all changes (Entity Framework handles transaction automatically)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Return success result
            result.Success = true;
            result.Id = queueEntry.Id.ToString();
            result.Position = queueEntry.Position;
            result.EstimatedWaitMinutes = estimatedWaitTime;
            result.JoinedAt = queueEntry.EnteredAt;
            result.Status = "waiting";

            return result;
        }

        private bool IsConcurrencyException(Exception ex)
        {
            return ex.Message.Contains("database operation was expected to affect 1 row") ||
                   ex.Message.Contains("optimistic concurrency") ||
                   ex.Message.Contains("may have been modified or deleted") ||
                   ex.Message.Contains("concurrency");
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