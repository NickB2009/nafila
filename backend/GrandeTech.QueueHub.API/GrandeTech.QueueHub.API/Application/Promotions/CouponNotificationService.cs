using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Grande.Fila.API.Domain.Customers;
using Grande.Fila.API.Domain.Promotions;
using Grande.Fila.API.Application.Notifications.Services;

namespace Grande.Fila.API.Application.Promotions
{
    public class CouponNotificationService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ICouponRepository _couponRepository;
        private readonly ISmsProvider _smsProvider;
        private readonly ILogger<CouponNotificationService> _logger;

        public CouponNotificationService(
            ICustomerRepository customerRepository,
            ICouponRepository couponRepository,
            ISmsProvider smsProvider,
            ILogger<CouponNotificationService> logger)
        {
            _customerRepository = customerRepository;
            _couponRepository = couponRepository;
            _smsProvider = smsProvider;
            _logger = logger;
        }

        public async Task<CouponNotificationResult> ExecuteAsync(CouponNotificationRequest request, string updatedBy, CancellationToken cancellationToken = default)
        {
            var result = new CouponNotificationResult { Success = false };

            // Validate CustomerId
            if (!Guid.TryParse(request.CustomerId, out var customerId))
            {
                result.FieldErrors["CustomerId"] = "Invalid GUID format";
                return result;
            }

            // Validate CouponId
            if (!Guid.TryParse(request.CouponId, out var couponId))
            {
                result.FieldErrors["CouponId"] = "Invalid GUID format";
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
                // Get customer
                var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
                if (customer == null)
                {
                    result.Errors.Add("Customer not found");
                    return result;
                }

                // Get coupon
                var coupon = await _couponRepository.GetByIdAsync(couponId, cancellationToken);
                if (coupon == null)
                {
                    result.Errors.Add("Coupon not found");
                    return result;
                }

                // Check if coupon is still active
                if (!coupon.IsActive || coupon.EndDate <= DateTime.UtcNow)
                {
                    result.Errors.Add("Coupon is not active or has expired");
                    return result;
                }

                // Get customer phone number
                var phoneNumber = customer.PhoneNumber?.Value;
                if (string.IsNullOrWhiteSpace(phoneNumber))
                {
                    phoneNumber = "+1234567890"; // Mock phone for testing
                }

                // Send notification based on channel
                bool sent = false;
                if (request.NotificationChannel.Equals("SMS", StringComparison.OrdinalIgnoreCase))
                {
                    sent = await _smsProvider.SendAsync(phoneNumber, request.Message, cancellationToken);
                }
                else
                {
                    // Future: Add email, push notification support
                    result.Errors.Add($"Notification channel '{request.NotificationChannel}' not supported");
                    return result;
                }

                if (sent)
                {
                    result.Success = true;
                    result.NotificationId = Guid.NewGuid().ToString();
                    _logger.LogInformation("Coupon notification sent to customer {CustomerId} for coupon {CouponId}", customerId, couponId);
                }
                else
                {
                    result.Errors.Add("Failed to send notification");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending coupon notification to customer {CustomerId}", customerId);
                result.Errors.Add("Unable to send coupon notification");
            }

            return result;
        }
    }
} 