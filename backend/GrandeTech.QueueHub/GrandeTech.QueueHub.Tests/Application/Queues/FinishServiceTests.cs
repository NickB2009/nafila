using System;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Application.Queues;
using Grande.Fila.API.Domain.Queues;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Grande.Fila.API.Infrastructure.Repositories.Bogus;

namespace Grande.Fila.API.Tests.Application.Queues
{
    [TestClass]
    public class FinishServiceTests
    {
        private Mock<IQueueRepository> _mockQueueRepository;
        private FinishService _finishService;
        private QueueEntryTestDouble _testQueueEntry;

        [TestInitialize]
        public void Setup()
        {
            _mockQueueRepository = new Mock<IQueueRepository>();
            _finishService = new FinishService(_mockQueueRepository.Object);
            
            // Create a test queue entry
            _testQueueEntry = new QueueEntryTestDouble(
                queueId: Guid.NewGuid(),
                customerId: Guid.NewGuid(),
                customerName: "Test Customer",
                position: 1
            );
        }

        [TestMethod]
        public async Task FinishAsync_WithValidRequest_ShouldCompleteService()
        {
            // Arrange
            var request = new FinishRequest
            {
                QueueEntryId = _testQueueEntry.Id.ToString(),
                ServiceDurationMinutes = 30,
                Notes = "Great service!"
            };

            _testQueueEntry.SetStatusForTest(QueueEntryStatus.CheckedIn);
            _mockQueueRepository
                .Setup(r => r.GetQueueEntryById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testQueueEntry);

            // Act
            var result = await _finishService.FinishAsync(request, "test-user");

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(_testQueueEntry.Id.ToString(), result.QueueEntryId);
            Assert.AreEqual("Test Customer", result.CustomerName);
            Assert.AreEqual(30, result.ServiceDurationMinutes);
            Assert.AreEqual("Great service!", result.Notes);
            Assert.IsNotNull(result.CompletedAt);
            Assert.AreEqual(0, result.Errors.Count);
            Assert.AreEqual(0, result.FieldErrors.Count);

            _mockQueueRepository.Verify(r => r.UpdateQueueEntry(It.IsAny<QueueEntry>()), Times.Once);
        }

        [TestMethod]
        public async Task FinishAsync_WithEmptyQueueEntryId_ShouldReturnValidationError()
        {
            // Arrange
            var request = new FinishRequest
            {
                QueueEntryId = "",
                ServiceDurationMinutes = 30
            };

            // Act
            var result = await _finishService.FinishAsync(request, "test-user");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.FieldErrors.Count);
            Assert.IsTrue(result.FieldErrors.ContainsKey("QueueEntryId"));
            Assert.AreEqual("Queue entry ID is required.", result.FieldErrors["QueueEntryId"]);
        }

        [TestMethod]
        public async Task FinishAsync_WithInvalidQueueEntryId_ShouldReturnValidationError()
        {
            // Arrange
            var request = new FinishRequest
            {
                QueueEntryId = "invalid-guid",
                ServiceDurationMinutes = 30
            };

            // Act
            var result = await _finishService.FinishAsync(request, "test-user");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.FieldErrors.Count);
            Assert.IsTrue(result.FieldErrors.ContainsKey("QueueEntryId"));
            Assert.AreEqual("Invalid queue entry ID format.", result.FieldErrors["QueueEntryId"]);
        }

        [TestMethod]
        public async Task FinishAsync_WithZeroServiceDuration_ShouldReturnValidationError()
        {
            // Arrange
            var request = new FinishRequest
            {
                QueueEntryId = _testQueueEntry.Id.ToString(),
                ServiceDurationMinutes = 0
            };

            // Act
            var result = await _finishService.FinishAsync(request, "test-user");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.FieldErrors.Count);
            Assert.IsTrue(result.FieldErrors.ContainsKey("ServiceDurationMinutes"));
            Assert.AreEqual("Service duration must be greater than 0 minutes.", result.FieldErrors["ServiceDurationMinutes"]);
        }

        [TestMethod]
        public async Task FinishAsync_WithNegativeServiceDuration_ShouldReturnValidationError()
        {
            // Arrange
            var request = new FinishRequest
            {
                QueueEntryId = _testQueueEntry.Id.ToString(),
                ServiceDurationMinutes = -5
            };

            // Act
            var result = await _finishService.FinishAsync(request, "test-user");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.FieldErrors.Count);
            Assert.IsTrue(result.FieldErrors.ContainsKey("ServiceDurationMinutes"));
            Assert.AreEqual("Service duration must be greater than 0 minutes.", result.FieldErrors["ServiceDurationMinutes"]);
        }

        [TestMethod]
        public async Task FinishAsync_WithNonExistentQueueEntry_ShouldReturnError()
        {
            // Arrange
            var request = new FinishRequest
            {
                QueueEntryId = Guid.NewGuid().ToString(),
                ServiceDurationMinutes = 30
            };

            _mockQueueRepository
                .Setup(r => r.GetQueueEntryById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((QueueEntry)null);

            // Act
            var result = await _finishService.FinishAsync(request, "test-user");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("Queue entry not found.", result.Errors[0]);
        }

        [TestMethod]
        public async Task FinishAsync_WithWaitingStatus_ShouldReturnError()
        {
            // Arrange
            var request = new FinishRequest
            {
                QueueEntryId = _testQueueEntry.Id.ToString(),
                ServiceDurationMinutes = 30
            };

            _testQueueEntry.SetStatusForTest(QueueEntryStatus.Waiting);
            _mockQueueRepository
                .Setup(r => r.GetQueueEntryById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testQueueEntry);

            // Act
            var result = await _finishService.FinishAsync(request, "test-user");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("Customer is not checked in.", result.Errors[0]);
        }

        [TestMethod]
        public async Task FinishAsync_WithCalledStatus_ShouldReturnError()
        {
            // Arrange
            var request = new FinishRequest
            {
                QueueEntryId = _testQueueEntry.Id.ToString(),
                ServiceDurationMinutes = 30
            };

            _testQueueEntry.SetStatusForTest(QueueEntryStatus.Called);
            _mockQueueRepository
                .Setup(r => r.GetQueueEntryById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testQueueEntry);

            // Act
            var result = await _finishService.FinishAsync(request, "test-user");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("Customer is not checked in.", result.Errors[0]);
        }

        [TestMethod]
        public async Task FinishAsync_WithCompletedStatus_ShouldReturnError()
        {
            // Arrange
            var request = new FinishRequest
            {
                QueueEntryId = _testQueueEntry.Id.ToString(),
                ServiceDurationMinutes = 30
            };

            _testQueueEntry.SetStatusForTest(QueueEntryStatus.Completed);
            _mockQueueRepository
                .Setup(r => r.GetQueueEntryById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testQueueEntry);

            // Act
            var result = await _finishService.FinishAsync(request, "test-user");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("Customer is not checked in.", result.Errors[0]);
        }

        [TestMethod]
        public async Task FinishAsync_WithCancelledStatus_ShouldReturnError()
        {
            // Arrange
            var request = new FinishRequest
            {
                QueueEntryId = _testQueueEntry.Id.ToString(),
                ServiceDurationMinutes = 30
            };

            _testQueueEntry.SetStatusForTest(QueueEntryStatus.Cancelled);
            _mockQueueRepository
                .Setup(r => r.GetQueueEntryById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testQueueEntry);

            // Act
            var result = await _finishService.FinishAsync(request, "test-user");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("Customer is not checked in.", result.Errors[0]);
        }

        [TestMethod]
        public async Task FinishAsync_WithNoShowStatus_ShouldReturnError()
        {
            // Arrange
            var request = new FinishRequest
            {
                QueueEntryId = _testQueueEntry.Id.ToString(),
                ServiceDurationMinutes = 30
            };

            _testQueueEntry.SetStatusForTest(QueueEntryStatus.NoShow);
            _mockQueueRepository
                .Setup(r => r.GetQueueEntryById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testQueueEntry);

            // Act
            var result = await _finishService.FinishAsync(request, "test-user");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("Customer is not checked in.", result.Errors[0]);
        }

        [TestMethod]
        public async Task FinishAsync_WhenRepositoryThrowsException_ShouldReturnError()
        {
            // Arrange
            var request = new FinishRequest
            {
                QueueEntryId = _testQueueEntry.Id.ToString(),
                ServiceDurationMinutes = 30
            };

            _mockQueueRepository
                .Setup(r => r.GetQueueEntryById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _finishService.FinishAsync(request, "test-user");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("An error occurred while completing service: Database connection failed", result.Errors[0]);
        }
    }
} 