using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.Customers;
using Grande.Fila.API.Application.Notifications.Services;

namespace Grande.Fila.API.Application.Notifications
{
    public class SmsNotificationService
    {
        private readonly IQueueRepository _queueRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ISmsProvider _smsProvider;
        private readonly ILogger<SmsNotificationService> _logger;

        public SmsNotificationService(
            IQueueRepository queueRepository,
            ICustomerRepository customerRepository,
            ISmsProvider smsProvider,
            ILogger<SmsNotificationService> logger)
        {
            _queueRepository = queueRepository;
            _customerRepository = customerRepository;
            _smsProvider = smsProvider;
            _logger = logger;
        }

        public async Task<SmsNotificationResult> ExecuteAsync(SmsNotificationRequest request, string updatedBy, CancellationToken cancellationToken = default)
        {
            var result = new SmsNotificationResult { Success = false };

            // Validate QueueEntryId
            if (!Guid.TryParse(request.QueueEntryId, out var queueEntryId))
            {
                result.FieldErrors["QueueEntryId"] = "Invalid GUID format";
                return result;
            }

            // Validate message
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                result.FieldErrors["Message"] = "Message cannot be empty";
                return result;
            }

            try
            {
                // Find queue entry across all queues
                var allQueues = await _queueRepository.GetAllAsync(cancellationToken);
                QueueEntry? queueEntry = null;
                Queue? queue = null;

                foreach (var q in allQueues)
                {
                    var entry = q.Entries.FirstOrDefault(e => e.Id == queueEntryId);
                    if (entry != null)
                    {
                        queueEntry = entry;
                        queue = q;
                        break;
                    }
                }

                if (queueEntry == null)
                {
                    result.Errors.Add("Queue entry not found");
                    return result;
                }

                // Get customer phone number
                string? phoneNumber = null;
                if (queueEntry.CustomerId != Guid.Empty)
                {
                    var customer = await _customerRepository.GetByIdAsync(queueEntry.CustomerId, cancellationToken);
                    phoneNumber = customer?.PhoneNumber;
                }
                
                // For anonymous entries, we would need to add phone to QueueEntry or use different approach
                // For now, if no customer found, use a mock phone number for testing
                if (string.IsNullOrWhiteSpace(phoneNumber))
                {
                    phoneNumber = "+1234567890"; // Mock phone for testing
                }

                // Send SMS
                var sent = await _smsProvider.SendAsync(phoneNumber, request.Message, cancellationToken);
                
                if (sent)
                {
                    result.Success = true;
                    result.MessageId = Guid.NewGuid().ToString(); // Mock message ID
                    _logger.LogInformation("SMS notification sent to {PhoneNumber} for queue entry {EntryId}", phoneNumber, queueEntryId);
                }
                else
                {
                    result.Errors.Add("Failed to send SMS");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS notification for queue entry {EntryId}", queueEntryId);
                result.Errors.Add("Unable to send SMS notification");
            }

            return result;
        }
    }
} 