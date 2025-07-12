using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.Customers;
using Grande.Fila.API.Application.Notifications.Services;

namespace Grande.Fila.API.Application.Queues.Handlers
{
    public class SmsNotificationHandler : IQueueMessageHandler<SmsNotificationMessage>
    {
        private readonly IQueueRepository _queueRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ISmsProvider _smsProvider;
        private readonly ILogger<SmsNotificationHandler> _logger;

        public SmsNotificationHandler(
            IQueueRepository queueRepository,
            ICustomerRepository customerRepository,
            ISmsProvider smsProvider,
            ILogger<SmsNotificationHandler> logger)
        {
            _queueRepository = queueRepository;
            _customerRepository = customerRepository;
            _smsProvider = smsProvider;
            _logger = logger;
        }

        public async Task<bool> HandleAsync(SmsNotificationMessage message, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Processing SMS notification for queue entry {QueueEntryId}", message.QueueEntryId);

                // Validate QueueEntryId
                if (!Guid.TryParse(message.QueueEntryId, out var queueEntryId))
                {
                    _logger.LogError("Invalid queue entry ID format: {QueueEntryId}", message.QueueEntryId);
                    return false;
                }

                // Validate message content
                if (string.IsNullOrWhiteSpace(message.Message))
                {
                    _logger.LogError("SMS message content is empty for queue entry {QueueEntryId}", message.QueueEntryId);
                    return false;
                }

                string phoneNumber = message.PhoneNumber;

                // If phone number is not provided in message, try to get it from customer
                if (string.IsNullOrWhiteSpace(phoneNumber))
                {
                    phoneNumber = await GetPhoneNumberFromQueueEntry(queueEntryId, cancellationToken);
                    if (string.IsNullOrWhiteSpace(phoneNumber))
                    {
                        _logger.LogWarning("No phone number found for queue entry {QueueEntryId}", message.QueueEntryId);
                        return false;
                    }
                }

                // Send SMS
                var sent = await _smsProvider.SendAsync(phoneNumber, message.Message, cancellationToken);
                
                if (sent)
                {
                    _logger.LogInformation("SMS notification sent successfully to {PhoneNumber} for queue entry {QueueEntryId}", 
                        MaskPhoneNumber(phoneNumber), message.QueueEntryId);
                    return true;
                }
                else
                {
                    _logger.LogError("Failed to send SMS notification to {PhoneNumber} for queue entry {QueueEntryId}", 
                        MaskPhoneNumber(phoneNumber), message.QueueEntryId);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing SMS notification for queue entry {QueueEntryId}", message.QueueEntryId);
                return false;
            }
        }

        private async Task<string> GetPhoneNumberFromQueueEntry(Guid queueEntryId, CancellationToken cancellationToken)
        {
            try
            {
                // Find queue entry across all queues
                var allQueues = await _queueRepository.GetAllAsync(cancellationToken);
                QueueEntry? queueEntry = null;

                foreach (var queue in allQueues)
                {
                    var entry = queue.Entries.FirstOrDefault(e => e.Id == queueEntryId);
                    if (entry != null)
                    {
                        queueEntry = entry;
                        break;
                    }
                }

                if (queueEntry == null)
                {
                    _logger.LogWarning("Queue entry {QueueEntryId} not found", queueEntryId);
                    return string.Empty;
                }

                // Get customer phone number
                if (queueEntry.CustomerId != Guid.Empty)
                {
                    var customer = await _customerRepository.GetByIdAsync(queueEntry.CustomerId, cancellationToken);
                    return customer?.PhoneNumber?.Value ?? string.Empty;
                }

                // For anonymous entries, we might need to store phone number in QueueEntry
                // For now, return empty string
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting phone number for queue entry {QueueEntryId}", queueEntryId);
                return string.Empty;
            }
        }

        private static string MaskPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || phoneNumber.Length < 4)
                return "****";

            return phoneNumber.Substring(0, 3) + "****" + phoneNumber.Substring(phoneNumber.Length - 2);
        }
    }
} 