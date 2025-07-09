using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Grande.Fila.API.Application.Queues;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Infrastructure.Repositories.Bogus;

namespace Grande.Fila.API.Tests.Application.Queues
{
    [TestClass]
    public class CheckInServiceTests
    {
        private Mock<IQueueRepository> _mockQueueRepository;
        private CheckInService _checkInService;
        private QueueEntryTestDouble _testQueueEntry;

        [TestInitialize]
        public void Setup()
        {
            _mockQueueRepository = new Mock<IQueueRepository>();
            _checkInService = new CheckInService(_mockQueueRepository.Object);
            
            // Create a test queue entry that is called (ready for check-in)
            _testQueueEntry = new QueueEntryTestDouble(
                queueId: Guid.NewGuid(),
                customerId: Guid.NewGuid(),
                customerName: "John Doe",
                position: 1,
                status: QueueEntryStatus.Called
            );
        }

        [TestMethod]
        public async Task CheckIn_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var request = new CheckInRequest
            {
                QueueEntryId = _testQueueEntry.Id.ToString()
            };

            _mockQueueRepository
                .Setup(r => r.GetQueueEntryById(_testQueueEntry.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testQueueEntry);

            // Act
            var result = await _checkInService.CheckInAsync(request, "test-user");

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(_testQueueEntry.Id.ToString(), result.QueueEntryId);
            Assert.AreEqual("John Doe", result.CustomerName);
            Assert.AreEqual(1, result.Position);
            Assert.IsNotNull(result.CheckedInAt);
            Assert.AreEqual(0, result.FieldErrors.Count);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public async Task CheckIn_EmptyQueueEntryId_ReturnsValidationError()
        {
            // Arrange
            var request = new CheckInRequest
            {
                QueueEntryId = ""
            };

            // Act
            var result = await _checkInService.CheckInAsync(request, "test-user");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("QueueEntryId"));
            Assert.AreEqual("Queue entry ID is required.", result.FieldErrors["QueueEntryId"]);
        }

        [TestMethod]
        public async Task CheckIn_InvalidQueueEntryId_ReturnsValidationError()
        {
            // Arrange
            var request = new CheckInRequest
            {
                QueueEntryId = "invalid-guid"
            };

            // Act
            var result = await _checkInService.CheckInAsync(request, "test-user");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("QueueEntryId"));
            Assert.AreEqual("Invalid queue entry ID format.", result.FieldErrors["QueueEntryId"]);
        }

        [TestMethod]
        public async Task CheckIn_QueueEntryNotFound_ReturnsError()
        {
            // Arrange
            var request = new CheckInRequest
            {
                QueueEntryId = Guid.NewGuid().ToString()
            };

            _mockQueueRepository
                .Setup(r => r.GetQueueEntryById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((QueueEntry)null);

            // Act
            var result = await _checkInService.CheckInAsync(request, "test-user");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("Queue entry not found."));
        }

        [TestMethod]
        public async Task CheckIn_CustomerNotCalled_ReturnsError()
        {
            // Arrange
            var waitingQueueEntry = new QueueEntryTestDouble(
                queueId: Guid.NewGuid(),
                customerId: Guid.NewGuid(),
                customerName: "Jane Doe",
                position: 2,
                status: QueueEntryStatus.Waiting
            );

            var request = new CheckInRequest
            {
                QueueEntryId = waitingQueueEntry.Id.ToString()
            };

            _mockQueueRepository
                .Setup(r => r.GetQueueEntryById(waitingQueueEntry.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(waitingQueueEntry);

            // Act
            var result = await _checkInService.CheckInAsync(request, "test-user");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("Customer must be called before they can be checked in."));
        }

        [TestMethod]
        public async Task CheckIn_AlreadyCheckedIn_ReturnsError()
        {
            // Arrange
            var checkedInQueueEntry = new QueueEntryTestDouble(
                queueId: Guid.NewGuid(),
                customerId: Guid.NewGuid(),
                customerName: "Bob Smith",
                position: 3,
                status: QueueEntryStatus.CheckedIn
            );

            var request = new CheckInRequest
            {
                QueueEntryId = checkedInQueueEntry.Id.ToString()
            };

            _mockQueueRepository
                .Setup(r => r.GetQueueEntryById(checkedInQueueEntry.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(checkedInQueueEntry);

            // Act
            var result = await _checkInService.CheckInAsync(request, "test-user");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("Customer must be called before they can be checked in."));
        }

        [TestMethod]
        public async Task CheckIn_CompletedCustomer_ReturnsError()
        {
            // Arrange
            var completedQueueEntry = new QueueEntryTestDouble(
                queueId: Guid.NewGuid(),
                customerId: Guid.NewGuid(),
                customerName: "Alice Johnson",
                position: 4,
                status: QueueEntryStatus.Completed
            );

            var request = new CheckInRequest
            {
                QueueEntryId = completedQueueEntry.Id.ToString()
            };

            _mockQueueRepository
                .Setup(r => r.GetQueueEntryById(completedQueueEntry.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(completedQueueEntry);

            // Act
            var result = await _checkInService.CheckInAsync(request, "test-user");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("Customer must be called before they can be checked in."));
        }

        [TestMethod]
        public async Task CheckIn_RepositoryException_ReturnsError()
        {
            // Arrange
            var request = new CheckInRequest
            {
                QueueEntryId = _testQueueEntry.Id.ToString()
            };

            _mockQueueRepository
                .Setup(r => r.GetQueueEntryById(_testQueueEntry.Id, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Database connection failed"));

            // Act
            var result = await _checkInService.CheckInAsync(request, "test-user");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("Database connection failed"));
        }

        [TestMethod]
        public async Task CheckIn_CheckInThrowsException_ReturnsError()
        {
            // Arrange
            var request = new CheckInRequest
            {
                QueueEntryId = _testQueueEntry.Id.ToString()
            };

            // Change the status to Waiting so that CheckIn will throw an exception
            _testQueueEntry.SetStatusForTest(QueueEntryStatus.Waiting);

            _mockQueueRepository
                .Setup(r => r.GetQueueEntryById(_testQueueEntry.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testQueueEntry);

            // Act
            var result = await _checkInService.CheckInAsync(request, "test-user");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Any(e => e == "Customer must be called before they can be checked in."), 
                $"Expected error message 'Customer must be called before they can be checked in.', but got: {string.Join(", ", result.Errors)}");
        }
    }
} 