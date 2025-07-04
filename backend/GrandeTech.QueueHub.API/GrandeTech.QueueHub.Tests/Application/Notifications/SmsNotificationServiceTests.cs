using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Grande.Fila.API.Application.Notifications;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.Customers;
using Grande.Fila.API.Application.Notifications.Services;
using Grande.Fila.API.Domain.Common.ValueObjects;

namespace Grande.Fila.API.Tests.Application.Notifications
{
    [TestClass]
    public class SmsNotificationServiceTests
    {
        private Mock<IQueueRepository> _mockQueueRepo = null!;
        private Mock<ICustomerRepository> _mockCustomerRepo = null!;
        private Mock<ISmsProvider> _mockSmsProvider = null!;
        private Mock<ILogger<SmsNotificationService>> _mockLogger = null!;
        private SmsNotificationService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockQueueRepo = new Mock<IQueueRepository>();
            _mockCustomerRepo = new Mock<ICustomerRepository>();
            _mockSmsProvider = new Mock<ISmsProvider>();
            _mockLogger = new Mock<ILogger<SmsNotificationService>>();
            _service = new SmsNotificationService(_mockQueueRepo.Object, _mockCustomerRepo.Object, _mockSmsProvider.Object, _mockLogger.Object);
        }

        [TestMethod]
        public async Task ExecuteAsync_ValidRequest_SendsNotification()
        {
            // Arrange
            var queueEntryId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var locationId = Guid.NewGuid();
            
            var queue = new Queue(locationId, 50, 15, "admin");
            var queueEntry = queue.AddCustomerToQueue(customerId, "John Doe", null, null, "admin");

            var customer = new Customer(
                "John Doe",
                "+1234567890",
                "john@example.com",
                false,
                "system");

            _mockQueueRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Queue> { queue });

            _mockCustomerRepo.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(customer);

            _mockSmsProvider.Setup(p => p.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            
            var request = new SmsNotificationRequest
            {
                QueueEntryId = queueEntry.Id.ToString(),
                Message = "Your turn is coming up!",
                NotificationType = "TurnReminder"
            };

            // Act
            var result = await _service.ExecuteAsync(request, "system");

            // Assert
            Assert.IsTrue(result.Success);
            _mockSmsProvider.Verify(p => p.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task ExecuteAsync_InvalidQueueEntryId_ReturnsError()
        {
            // Arrange
            var request = new SmsNotificationRequest
            {
                QueueEntryId = "invalid-guid",
                Message = "Test message",
                NotificationType = "TurnReminder"
            };

            // Act
            var result = await _service.ExecuteAsync(request, "system");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("QueueEntryId"));
        }
    }
} 