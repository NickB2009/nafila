using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Grande.Fila.API.Application.Queues;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.Customers;

namespace Grande.Fila.API.Tests.Application.Queues
{
    [TestClass]
    public class SaveHaircutDetailsServiceTests
    {
        private Mock<IQueueRepository> _mockQueueRepository;
        private Mock<ICustomerRepository> _mockCustomerRepository;
        private Mock<ILogger<SaveHaircutDetailsService>> _mockLogger;
        private SaveHaircutDetailsService _service;

        [TestInitialize]
        public void Setup()
        {
            _mockQueueRepository = new Mock<IQueueRepository>();
            _mockCustomerRepository = new Mock<ICustomerRepository>();
            _mockLogger = new Mock<ILogger<SaveHaircutDetailsService>>();
            _service = new SaveHaircutDetailsService(_mockQueueRepository.Object, _mockCustomerRepository.Object, _mockLogger.Object);
        }

        [TestMethod]
        public async Task ExecuteAsync_ValidRequest_SavesHaircutDetailsSuccessfully()
        {
            // Arrange
            var queueId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var queueEntryId = Guid.NewGuid();
            var locationId = Guid.NewGuid();
            var staffMemberId = Guid.NewGuid();
            var serviceTypeId = Guid.NewGuid();

            var queue = new Queue(locationId, 100, 15, "admin");
            var queueEntry = queue.AddCustomerToQueue(customerId, "Test Customer", staffMemberId, serviceTypeId);
            
            // Set queue entry to completed state
            queueEntry.SetStatusForTest(QueueEntryStatus.Called);
            queueEntry.CheckIn();
            queueEntry.Complete(30);

            var customer = new Customer("Test Customer", "+1234567890", "test@example.com", false);

            _mockQueueRepository
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { queue });

            _mockCustomerRepository
                .Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(customer);

            _mockCustomerRepository
                .Setup(r => r.UpdateAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(customer);

            _mockQueueRepository
                .Setup(r => r.UpdateAsync(It.IsAny<Queue>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queue);

            var request = new SaveHaircutDetailsRequest
            {
                QueueEntryId = queueEntry.Id.ToString(),
                HaircutDetails = "Fade haircut with line design",
                AdditionalNotes = "Customer prefers short on sides",
                PhotoUrl = "https://example.com/haircut.jpg"
            };

            // Act
            var result = await _service.ExecuteAsync(request, "barber123");

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.ServiceHistoryId);
            Assert.AreEqual(customer.Id.ToString(), result.CustomerId);

            // Verify customer service history was updated
            Assert.AreEqual(1, customer.ServiceHistory.Count);
            var historyItem = customer.ServiceHistory.First();
            Assert.IsTrue(historyItem.Notes!.Contains("Fade haircut with line design"));
            Assert.IsTrue(historyItem.Notes!.Contains("Customer prefers short on sides"));
            Assert.IsTrue(historyItem.Notes!.Contains("https://example.com/haircut.jpg"));
        }

        [TestMethod]
        public async Task ExecuteAsync_InvalidQueueEntryId_ReturnsError()
        {
            // Arrange
            var request = new SaveHaircutDetailsRequest
            {
                QueueEntryId = "invalid-guid",
                HaircutDetails = "Fade haircut"
            };

            // Act
            var result = await _service.ExecuteAsync(request, "barber123");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("QueueEntryId"));
            Assert.AreEqual("Invalid queue entry ID format", result.FieldErrors["QueueEntryId"]);
        }

        [TestMethod]
        public async Task ExecuteAsync_EmptyHaircutDetails_ReturnsError()
        {
            // Arrange
            var request = new SaveHaircutDetailsRequest
            {
                QueueEntryId = Guid.NewGuid().ToString(),
                HaircutDetails = ""
            };

            // Act
            var result = await _service.ExecuteAsync(request, "barber123");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("HaircutDetails"));
            Assert.AreEqual("Haircut details are required", result.FieldErrors["HaircutDetails"]);
        }

        [TestMethod]
        public async Task ExecuteAsync_QueueEntryNotFound_ReturnsError()
        {
            // Arrange
            _mockQueueRepository
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<Queue>());

            var request = new SaveHaircutDetailsRequest
            {
                QueueEntryId = Guid.NewGuid().ToString(),
                HaircutDetails = "Fade haircut"
            };

            // Act
            var result = await _service.ExecuteAsync(request, "barber123");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("Queue entry not found"));
        }

        [TestMethod]
        public async Task ExecuteAsync_QueueEntryNotCompleted_ReturnsError()
        {
            // Arrange
            var queueId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var locationId = Guid.NewGuid();

            var queue = new Queue(locationId, 100, 15, "admin");
            var queueEntry = queue.AddCustomerToQueue(customerId, "Test Customer");
            
            // Queue entry is still in Waiting status

            _mockQueueRepository
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { queue });

            var request = new SaveHaircutDetailsRequest
            {
                QueueEntryId = queueEntry.Id.ToString(),
                HaircutDetails = "Fade haircut"
            };

            // Act
            var result = await _service.ExecuteAsync(request, "barber123");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("Haircut details can only be saved for completed services"));
        }

        [TestMethod]
        public async Task ExecuteAsync_CustomerNotFound_ReturnsError()
        {
            // Arrange
            var queueId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var locationId = Guid.NewGuid();

            var queue = new Queue(locationId, 100, 15, "admin");
            var queueEntry = queue.AddCustomerToQueue(customerId, "Test Customer");
            
            // Set queue entry to completed state
            queueEntry.SetStatusForTest(QueueEntryStatus.Called);
            queueEntry.CheckIn();
            queueEntry.Complete(30);

            _mockQueueRepository
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { queue });

            _mockCustomerRepository
                .Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Customer?)null);

            var request = new SaveHaircutDetailsRequest
            {
                QueueEntryId = queueEntry.Id.ToString(),
                HaircutDetails = "Fade haircut"
            };

            // Act
            var result = await _service.ExecuteAsync(request, "barber123");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("Customer not found"));
        }
    }
}