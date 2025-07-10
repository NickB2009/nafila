using System;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Application.Queues;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.Staff;
using Grande.Fila.API.Domain.Customers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Grande.Fila.Tests.Application.Queues
{
    [TestClass]
    public class CallNextServiceTests
    {
        private Mock<IQueueRepository> _queueRepoMock = null!;
        private Mock<IStaffMemberRepository> _staffRepoMock = null!;
        private CallNextService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _queueRepoMock = new Mock<IQueueRepository>();
            _staffRepoMock = new Mock<IStaffMemberRepository>();
            _service = new CallNextService(_queueRepoMock.Object, _staffRepoMock.Object);
        }

        [TestMethod]
        public async Task CallNextAsync_WithValidRequest_ReturnsSuccess()
        {
            // Arrange
            var staffMemberId = Guid.NewGuid();
            var queueId = Guid.NewGuid();
            var request = new CallNextRequest
            {
                StaffMemberId = staffMemberId.ToString(),
                QueueId = queueId.ToString()
            };

            var staffMember = new StaffMember(
                "John Doe",
                Guid.NewGuid(),
                "john@barbershop.com",
                "+5511999999999",
                null,
                "Barber",
                "johndoe",
                null,
                "system"
            );

            var queue = new Queue(
                Guid.NewGuid(),
                50,
                15,
                "system"
            );

            // Add a queue entry
            var customer = new Customer(
                "Jane Smith",
                "+5511888888888",
                "jane@email.com",
                false
            );
            var queueEntry = queue.AddCustomerToQueue(customer.Id, customer.Name);

            _staffRepoMock.Setup(r => r.GetByIdAsync(staffMemberId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(staffMember);
            _queueRepoMock.Setup(r => r.GetByIdAsync(queueId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(queue);
            _queueRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Queue>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((Queue q, CancellationToken _) => q);

            // Act
            var result = await _service.CallNextAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(request.StaffMemberId, result.StaffMemberId);
            Assert.AreEqual(request.QueueId, result.QueueId);
            Assert.IsNotNull(result.CalledCustomerId);
            Assert.AreEqual(0, result.FieldErrors.Count);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public async Task CallNextAsync_WithInvalidStaffMemberId_ReturnsFieldError()
        {
            // Arrange
            var request = new CallNextRequest
            {
                StaffMemberId = "invalid-guid",
                QueueId = Guid.NewGuid().ToString()
            };

            // Act
            var result = await _service.CallNextAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("StaffMemberId"));
            Assert.AreEqual("Invalid staff member ID format.", result.FieldErrors["StaffMemberId"]);
        }

        [TestMethod]
        public async Task CallNextAsync_WithEmptyStaffMemberId_ReturnsFieldError()
        {
            // Arrange
            var request = new CallNextRequest
            {
                StaffMemberId = "",
                QueueId = Guid.NewGuid().ToString()
            };

            // Act
            var result = await _service.CallNextAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("StaffMemberId"));
            Assert.AreEqual("Staff member ID is required.", result.FieldErrors["StaffMemberId"]);
        }

        [TestMethod]
        public async Task CallNextAsync_WithInvalidQueueId_ReturnsFieldError()
        {
            // Arrange
            var request = new CallNextRequest
            {
                StaffMemberId = Guid.NewGuid().ToString(),
                QueueId = "invalid-guid"
            };

            // Act
            var result = await _service.CallNextAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("QueueId"));
            Assert.AreEqual("Invalid queue ID format.", result.FieldErrors["QueueId"]);
        }

        [TestMethod]
        public async Task CallNextAsync_WithEmptyQueueId_ReturnsFieldError()
        {
            // Arrange
            var request = new CallNextRequest
            {
                StaffMemberId = Guid.NewGuid().ToString(),
                QueueId = ""
            };

            // Act
            var result = await _service.CallNextAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("QueueId"));
            Assert.AreEqual("Queue ID is required.", result.FieldErrors["QueueId"]);
        }

        [TestMethod]
        public async Task CallNextAsync_WithNonExistentStaffMember_ReturnsError()
        {
            // Arrange
            var staffMemberId = Guid.NewGuid();
            var request = new CallNextRequest
            {
                StaffMemberId = staffMemberId.ToString(),
                QueueId = Guid.NewGuid().ToString()
            };

            _staffRepoMock.Setup(r => r.GetByIdAsync(staffMemberId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync((StaffMember?)null);

            // Act
            var result = await _service.CallNextAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("Staff member not found."));
        }

        [TestMethod]
        public async Task CallNextAsync_WithInactiveStaffMember_ReturnsError()
        {
            // Arrange
            var staffMemberId = Guid.NewGuid();
            var request = new CallNextRequest
            {
                StaffMemberId = staffMemberId.ToString(),
                QueueId = Guid.NewGuid().ToString()
            };

            var staffMember = new StaffMember(
                "John Doe",
                Guid.NewGuid(),
                "john@barbershop.com",
                "+5511999999999",
                null,
                "Barber",
                "johndoe",
                null,
                "system"
            );
            staffMember.Deactivate("admin");

            _staffRepoMock.Setup(r => r.GetByIdAsync(staffMemberId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(staffMember);

            // Act
            var result = await _service.CallNextAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("Cannot call next for inactive staff member."));
        }

        [TestMethod]
        public async Task CallNextAsync_WithNonExistentQueue_ReturnsError()
        {
            // Arrange
            var staffMemberId = Guid.NewGuid();
            var queueId = Guid.NewGuid();
            var request = new CallNextRequest
            {
                StaffMemberId = staffMemberId.ToString(),
                QueueId = queueId.ToString()
            };

            var staffMember = new StaffMember(
                "John Doe",
                Guid.NewGuid(),
                "john@barbershop.com",
                "+5511999999999",
                null,
                "Barber",
                "johndoe",
                null,
                "system"
            );

            _staffRepoMock.Setup(r => r.GetByIdAsync(staffMemberId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(staffMember);
            _queueRepoMock.Setup(r => r.GetByIdAsync(queueId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync((Queue?)null);

            // Act
            var result = await _service.CallNextAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("Queue not found."));
        }

        [TestMethod]
        public async Task CallNextAsync_WithEmptyQueue_ReturnsError()
        {
            // Arrange
            var staffMemberId = Guid.NewGuid();
            var queueId = Guid.NewGuid();
            var request = new CallNextRequest
            {
                StaffMemberId = staffMemberId.ToString(),
                QueueId = queueId.ToString()
            };

            var staffMember = new StaffMember(
                "John Doe",
                Guid.NewGuid(),
                "john@barbershop.com",
                "+5511999999999",
                null,
                "Barber",
                "johndoe",
                null,
                "system"
            );

            var queue = new Queue(
                Guid.NewGuid(),
                50,
                15,
                "system"
            );
            // Queue is empty (no entries)

            _staffRepoMock.Setup(r => r.GetByIdAsync(staffMemberId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(staffMember);
            _queueRepoMock.Setup(r => r.GetByIdAsync(queueId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(queue);

            // Act
            var result = await _service.CallNextAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("Queue is empty. No customers to call."));
        }

        [TestMethod]
        public async Task CallNextAsync_WithStaffMemberOnBreak_ReturnsError()
        {
            // Arrange
            var staffMemberId = Guid.NewGuid();
            var queueId = Guid.NewGuid();
            var request = new CallNextRequest
            {
                StaffMemberId = staffMemberId.ToString(),
                QueueId = queueId.ToString()
            };

            var staffMember = new StaffMember(
                "John Doe",
                Guid.NewGuid(),
                "john@barbershop.com",
                "+5511999999999",
                null,
                "Barber",
                "johndoe",
                null,
                "system"
            );
            // Start a break
            staffMember.StartBreak(TimeSpan.FromMinutes(15), "Lunch break", "admin");

            var queue = new Queue(
                Guid.NewGuid(),
                50,
                15,
                "system"
            );

            var customer = new Customer(
                "Jane Smith",
                "+5511888888888",
                "jane@email.com",
                false
            );
            var queueEntry = queue.AddCustomerToQueue(customer.Id, customer.Name);

            _staffRepoMock.Setup(r => r.GetByIdAsync(staffMemberId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(staffMember);
            _queueRepoMock.Setup(r => r.GetByIdAsync(queueId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(queue);

            // Act
            var result = await _service.CallNextAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("Cannot call next while on break."));
        }
    }
} 