using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Grande.Fila.API.Application.Promotions;
using Grande.Fila.API.Domain.Customers;
using Grande.Fila.API.Domain.Promotions;
using Grande.Fila.API.Application.Notifications.Services;
using Grande.Fila.API.Domain.Common.ValueObjects;

namespace Grande.Fila.API.Tests.Application.Promotions
{
    [TestClass]
    public class CouponNotificationServiceTests
    {
        private Mock<ICustomerRepository> _mockCustomerRepo = null!;
        private Mock<ICouponRepository> _mockCouponRepo = null!;
        private Mock<ISmsProvider> _mockSmsProvider = null!;
        private Mock<ILogger<CouponNotificationService>> _mockLogger = null!;
        private CouponNotificationService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockCustomerRepo = new Mock<ICustomerRepository>();
            _mockCouponRepo = new Mock<ICouponRepository>();
            _mockSmsProvider = new Mock<ISmsProvider>();
            _mockLogger = new Mock<ILogger<CouponNotificationService>>();
            _service = new CouponNotificationService(_mockCustomerRepo.Object, _mockCouponRepo.Object, _mockSmsProvider.Object, _mockLogger.Object);
        }

        [TestMethod]
        public async Task ExecuteAsync_ValidRequest_SendsCouponNotification()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var couponId = Guid.NewGuid();
            var locationId = Guid.NewGuid();
            
            var customer = new Customer(
                "John Doe",
                "+1234567890",
                "john@example.com",
                false,
                "system");

            var coupon = new Coupon(
                "SAVE20",
                "Get 20% off your next service",
                locationId,
                20,
                null,
                DateTime.UtcNow,
                DateTime.UtcNow.AddDays(30),
                100,
                false,
                null,
                "system");

            _mockCustomerRepo.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(customer);

            _mockCouponRepo.Setup(r => r.GetByIdAsync(couponId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(coupon);

            _mockSmsProvider.Setup(p => p.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            
            var request = new CouponNotificationRequest
            {
                CustomerId = customerId.ToString(),
                CouponId = couponId.ToString(),
                Message = "You have a new 20% off coupon!"
            };

            // Act
            var result = await _service.ExecuteAsync(request, "system");

            // Assert
            Assert.IsTrue(result.Success);
            _mockSmsProvider.Verify(p => p.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task ExecuteAsync_InvalidCustomerId_ReturnsError()
        {
            // Arrange
            var request = new CouponNotificationRequest
            {
                CustomerId = "invalid-guid",
                CouponId = Guid.NewGuid().ToString(),
                Message = "Test message"
            };

            // Act
            var result = await _service.ExecuteAsync(request, "system");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("CustomerId"));
        }
    }
} 