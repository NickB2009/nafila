using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Grande.Fila.API.Application.Services;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.Staff;
using Grande.Fila.API.Domain.Common.ValueObjects;

namespace Grande.Fila.API.Tests.Application.Services
{
    [TestClass]
    public class EstimatedWaitTimeServiceTests
    {
        private Mock<IQueueRepository> _mockQueueRepo = null!;
        private Mock<IStaffMemberRepository> _mockStaffRepo = null!;
        private EstimatedWaitTimeService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockQueueRepo = new Mock<IQueueRepository>();
            _mockStaffRepo = new Mock<IStaffMemberRepository>();
            _service = new EstimatedWaitTimeService(_mockQueueRepo.Object, _mockStaffRepo.Object);
        }

        [TestMethod]
        public async Task CalculateAsync_FirstCustomerInQueue_ReturnsZeroWaitTime()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var queueId = Guid.NewGuid();
            var entryId = Guid.NewGuid();

            var queue = CreateTestQueue(queueId, locationId);
            var entry = queue.AddCustomerToQueue(Guid.NewGuid(), "Customer 1");
            var staffMember = CreateTestStaffMember(locationId);

            _mockQueueRepo.Setup(r => r.GetByIdAsync(queueId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(queue);
            _mockStaffRepo.Setup(r => r.GetByLocationAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StaffMember> { staffMember });

            // Act
            var result = await _service.CalculateAsync(queueId, entry.Id, CancellationToken.None);

            // Assert
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public async Task CalculateAsync_SecondCustomerWithOneStaff_ReturnsThirtyMinutes()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var queueId = Guid.NewGuid();

            var queue = CreateTestQueue(queueId, locationId);
            var entry1 = queue.AddCustomerToQueue(Guid.NewGuid(), "Customer 1");
            var entry2 = queue.AddCustomerToQueue(Guid.NewGuid(), "Customer 2");
            var staffMember = CreateTestStaffMember(locationId);

            _mockQueueRepo.Setup(r => r.GetByIdAsync(queueId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(queue);
            _mockStaffRepo.Setup(r => r.GetByLocationAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StaffMember> { staffMember });

            // Act
            var result = await _service.CalculateAsync(queueId, entry2.Id, CancellationToken.None);

            // Assert
            Assert.AreEqual(30, result); // 1 customer ahead / 1 staff * 30 min = 30 min
        }

        [TestMethod]
        public async Task CalculateAsync_ThirdCustomerWithTwoStaff_ReturnsThirtyMinutes()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var queueId = Guid.NewGuid();

            var queue = CreateTestQueue(queueId, locationId);
            var entry1 = queue.AddCustomerToQueue(Guid.NewGuid(), "Customer 1");
            var entry2 = queue.AddCustomerToQueue(Guid.NewGuid(), "Customer 2");
            var entry3 = queue.AddCustomerToQueue(Guid.NewGuid(), "Customer 3");
            
            var staffMember1 = CreateTestStaffMember(locationId);
            var staffMember2 = CreateTestStaffMember(locationId);

            _mockQueueRepo.Setup(r => r.GetByIdAsync(queueId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(queue);
            _mockStaffRepo.Setup(r => r.GetByLocationAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StaffMember> { staffMember1, staffMember2 });

            // Act
            var result = await _service.CalculateAsync(queueId, entry3.Id, CancellationToken.None);

            // Assert
            Assert.AreEqual(30, result); // Math.Ceiling(2 customers ahead / 2 staff * 30 min) = 30 min
        }

        [TestMethod]
        public async Task CalculateAsync_NoActiveStaff_ReturnsMinusOne()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var queueId = Guid.NewGuid();

            var queue = CreateTestQueue(queueId, locationId);
            var entry = queue.AddCustomerToQueue(Guid.NewGuid(), "Customer 1");

            _mockQueueRepo.Setup(r => r.GetByIdAsync(queueId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(queue);
            _mockStaffRepo.Setup(r => r.GetByLocationAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StaffMember>()); // No staff

            // Act
            var result = await _service.CalculateAsync(queueId, entry.Id, CancellationToken.None);

            // Assert
            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        public async Task CalculateAsync_InactiveStaff_ReturnsMinusOne()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var queueId = Guid.NewGuid();

            var queue = CreateTestQueue(queueId, locationId);
            var entry = queue.AddCustomerToQueue(Guid.NewGuid(), "Customer 1");
            var inactiveStaff = CreateTestStaffMember(locationId);
            inactiveStaff.Deactivate("test");

            _mockQueueRepo.Setup(r => r.GetByIdAsync(queueId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(queue);
            _mockStaffRepo.Setup(r => r.GetByLocationAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StaffMember> { inactiveStaff });

            // Act
            var result = await _service.CalculateAsync(queueId, entry.Id, CancellationToken.None);

            // Assert
            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        public async Task CalculateAsync_QueueNotFound_ReturnsMinusOne()
        {
            // Arrange
            var queueId = Guid.NewGuid();
            var entryId = Guid.NewGuid();

            _mockQueueRepo.Setup(r => r.GetByIdAsync(queueId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Queue?)null);

            // Act
            var result = await _service.CalculateAsync(queueId, entryId, CancellationToken.None);

            // Assert
            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        public async Task CalculateAsync_EntryNotFound_ReturnsMinusOne()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var queueId = Guid.NewGuid();
            var entryId = Guid.NewGuid();

            var queue = CreateTestQueue(queueId, locationId);
            var staffMember = CreateTestStaffMember(locationId);

            _mockQueueRepo.Setup(r => r.GetByIdAsync(queueId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(queue);
            _mockStaffRepo.Setup(r => r.GetByLocationAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StaffMember> { staffMember });

            // Act
            var result = await _service.CalculateAsync(queueId, entryId, CancellationToken.None);

            // Assert
            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        public async Task CalculateAsync_OnlyCountsWaitingAndCalledCustomers()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var queueId = Guid.NewGuid();

            var queue = CreateTestQueue(queueId, locationId);
            var entry1 = queue.AddCustomerToQueue(Guid.NewGuid(), "Customer 1");
            var entry2 = queue.AddCustomerToQueue(Guid.NewGuid(), "Customer 2");
            var entry3 = queue.AddCustomerToQueue(Guid.NewGuid(), "Customer 3");
            var entry4 = queue.AddCustomerToQueue(Guid.NewGuid(), "Customer 4");

            // Simulate different statuses
            entry1.Call(Guid.NewGuid()); // Called first
            entry1.CheckIn(); // CheckedIn - should not count
            entry2.Cancel(); // Cancelled - should not count
            // entry3 is Waiting - should count
            // entry4 is Waiting - should not count (it's the one we're calculating for)

            var staffMember = CreateTestStaffMember(locationId);

            _mockQueueRepo.Setup(r => r.GetByIdAsync(queueId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(queue);
            _mockStaffRepo.Setup(r => r.GetByLocationAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StaffMember> { staffMember });

            // Act
            var result = await _service.CalculateAsync(queueId, entry4.Id, CancellationToken.None);

            // Assert
            Assert.AreEqual(30, result); // Only entry3 (position 3) should count as ahead of entry4 (position 4)
        }

        private Queue CreateTestQueue(Guid queueId, Guid locationId)
        {
            var queue = new Queue(locationId, 50, 15, "test-system");
            
            // Set the queue ID using reflection
            var queueIdProperty = typeof(Queue).GetProperty("Id");
            queueIdProperty?.SetValue(queue, queueId);
            
            return queue;
        }

        private StaffMember CreateTestStaffMember(Guid locationId)
        {
            return new StaffMember(
                "Test Staff",
                locationId,
                "staff@test.com",
                "+1234567890",
                null,
                "Barber",
                "test.staff",
                null,
                "test-system"
            );
        }
    }
} 