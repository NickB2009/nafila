using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.Staff;
using Grande.Fila.API.Application.Queues;

namespace Grande.Fila.API.Application.Queues
{
    /// <summary>
    /// Service for handling UC-CALLNEXT: Barber calls next client use case
    /// Implements TDD-driven business logic for calling next customer from queue
    /// </summary>
    public class CallNextService
    {
        private readonly IQueueRepository _queueRepository;
        private readonly IStaffMemberRepository _staffRepository;

        public CallNextService(IQueueRepository queueRepository, IStaffMemberRepository staffRepository)
        {
            _queueRepository = queueRepository;
            _staffRepository = staffRepository;
        }

        /// <summary>
        /// Calls the next customer in the queue
        /// </summary>
        public async Task<CallNextResult> CallNextAsync(CallNextRequest request, string userId, CancellationToken cancellationToken = default)
        {
            var fieldErrors = new Dictionary<string, string>();
            var errors = new List<string>();

            // Validate StaffMemberId
            if (string.IsNullOrWhiteSpace(request.StaffMemberId))
            {
                fieldErrors["StaffMemberId"] = "Staff member ID is required.";
            }
            else if (!Guid.TryParse(request.StaffMemberId, out _))
            {
                fieldErrors["StaffMemberId"] = "Invalid staff member ID format.";
            }

            // Validate QueueId
            if (string.IsNullOrWhiteSpace(request.QueueId))
            {
                fieldErrors["QueueId"] = "Queue ID is required.";
            }
            else if (!Guid.TryParse(request.QueueId, out _))
            {
                fieldErrors["QueueId"] = "Invalid queue ID format.";
            }

            if (fieldErrors.Count > 0)
            {
                return new CallNextResult
                {
                    Success = false,
                    StaffMemberId = request.StaffMemberId,
                    QueueId = request.QueueId,
                    CalledCustomerId = string.Empty,
                    FieldErrors = fieldErrors,
                    Errors = errors
                };
            }

            var staffMemberId = Guid.Parse(request.StaffMemberId);
            var queueId = Guid.Parse(request.QueueId);

            // Get staff member by ID
            var staffMember = await _staffRepository.GetByIdAsync(staffMemberId, cancellationToken);
            if (staffMember == null)
            {
                errors.Add("Staff member not found.");
                return new CallNextResult
                {
                    Success = false,
                    StaffMemberId = request.StaffMemberId,
                    QueueId = request.QueueId,
                    CalledCustomerId = string.Empty,
                    FieldErrors = fieldErrors,
                    Errors = errors
                };
            }

            // Check if staff member is active
            if (!staffMember.IsActive)
            {
                errors.Add("Cannot call next for inactive staff member.");
                return new CallNextResult
                {
                    Success = false,
                    StaffMemberId = request.StaffMemberId,
                    QueueId = request.QueueId,
                    CalledCustomerId = string.Empty,
                    FieldErrors = fieldErrors,
                    Errors = errors
                };
            }

            // Check if staff member is on break
            if (staffMember.IsOnBreak())
            {
                errors.Add("Cannot call next while on break.");
                return new CallNextResult
                {
                    Success = false,
                    StaffMemberId = request.StaffMemberId,
                    QueueId = request.QueueId,
                    CalledCustomerId = string.Empty,
                    FieldErrors = fieldErrors,
                    Errors = errors
                };
            }

            // Get queue by ID
            var queue = await _queueRepository.GetByIdAsync(queueId, cancellationToken);
            if (queue == null)
            {
                errors.Add("Queue not found.");
                return new CallNextResult
                {
                    Success = false,
                    StaffMemberId = request.StaffMemberId,
                    QueueId = request.QueueId,
                    CalledCustomerId = string.Empty,
                    FieldErrors = fieldErrors,
                    Errors = errors
                };
            }

            // Check if queue is empty
            if (queue.Entries.Count == 0)
            {
                errors.Add("Queue is empty. No customers to call.");
                return new CallNextResult
                {
                    Success = false,
                    StaffMemberId = request.StaffMemberId,
                    QueueId = request.QueueId,
                    CalledCustomerId = string.Empty,
                    FieldErrors = fieldErrors,
                    Errors = errors
                };
            }

            // Call next customer from queue
            var calledEntry = queue.CallNextCustomer(staffMemberId);

            // Update queue in repository
            await _queueRepository.UpdateAsync(queue, cancellationToken);

            return new CallNextResult
            {
                Success = true,
                StaffMemberId = request.StaffMemberId,
                QueueId = request.QueueId,
                CalledCustomerId = calledEntry.CustomerId.ToString(),
                FieldErrors = fieldErrors,
                Errors = errors
            };
        }
    }
} 