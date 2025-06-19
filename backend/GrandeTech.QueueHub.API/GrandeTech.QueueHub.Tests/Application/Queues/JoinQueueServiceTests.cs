using System;
using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Application.Queues;
using GrandeTech.QueueHub.API.Domain.Queues;
using GrandeTech.QueueHub.API.Domain.Customers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GrandeTech.QueueHub.Tests.Application.Queues
{
    [TestClass]
    public class JoinQueueServiceTests
    {
        private Mock<IQueueRepository> _queueRepoMock = null!;
        private Mock<ICustomerRepository> _customerRepoMock = null!;
        private JoinQueueService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _queueRepoMock = new Mock<IQueueRepository>();
            _customerRepoMock = new Mock<ICustomerRepository>();
            _service = new JoinQueueService(_queueRepoMock.Object, _customerRepoMock.Object);
        }

        [TestMethod]
        public async Task JoinQueueAsync_WithValidRequest_ReturnsSuccess()
        {
            // Arrange
            var queueId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var locationId = Guid.NewGuid();
            
            var request = new JoinQueueRequest
            {
                QueueId = queueId.ToString(),
                CustomerName = "John Doe",
                PhoneNumber = "+5511999999999",
                Email = null,
                IsAnonymous = false,
                Notes = "Regular customer"
            };

            var existingQueue = new Queue(locationId, 50, 15, "system");
            var existingCustomer = new Customer("John Doe", "+5511999999999", null, false);            _queueRepoMock.Setup(r => r.GetByIdAsync(queueId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(existingQueue);
              _customerRepoMock.Setup(r => r.GetByPhoneNumberAsync("+5511999999999", It.IsAny<CancellationToken>()))
                            .ReturnsAsync(existingCustomer);

            _queueRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Queue>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((Queue q, CancellationToken _) => q);

            // Act
            var result = await _service.JoinQueueAsync(request, "user123");

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.QueueEntryId);
            Assert.AreEqual(1, result.Position);
            Assert.IsTrue(result.EstimatedWaitTimeMinutes >= 0);
            Assert.AreEqual(0, result.FieldErrors.Count);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public async Task JoinQueueAsync_WithInvalidQueueId_ReturnsFieldError()
        {
            // Arrange
            var request = new JoinQueueRequest
            {
                QueueId = "invalid-guid",
                CustomerName = "John Doe",
                PhoneNumber = "+5511999999999"
            };

            // Act
            var result = await _service.JoinQueueAsync(request, "user123");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("QueueId"));
            Assert.AreEqual("Invalid queue ID format.", result.FieldErrors["QueueId"]);
        }

        [TestMethod]
        public async Task JoinQueueAsync_WithEmptyName_ReturnsFieldError()
        {
            // Arrange
            var request = new JoinQueueRequest
            {
                QueueId = Guid.NewGuid().ToString(),
                CustomerName = "",
                PhoneNumber = "+5511999999999"
            };

            // Act
            var result = await _service.JoinQueueAsync(request, "user123");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("CustomerName"));
            Assert.AreEqual("Customer name is required.", result.FieldErrors["CustomerName"]);
        }

        [TestMethod]
        public async Task JoinQueueAsync_WithNonExistentQueue_ReturnsError()
        {
            // Arrange
            var queueId = Guid.NewGuid();
            var request = new JoinQueueRequest
            {
                QueueId = queueId.ToString(),
                CustomerName = "John Doe",
                PhoneNumber = "+5511999999999"
            };

            _queueRepoMock.Setup(r => r.GetByIdAsync(queueId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync((Queue?)null);

            // Act
            var result = await _service.JoinQueueAsync(request, "user123");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("Queue not found."));
        }

        [TestMethod]
        public async Task JoinQueueAsync_WithInactiveQueue_ReturnsError()
        {
            // Arrange
            var queueId = Guid.NewGuid();
            var locationId = Guid.NewGuid();
            var request = new JoinQueueRequest
            {
                QueueId = queueId.ToString(),
                CustomerName = "John Doe",
                PhoneNumber = "+5511999999999"
            };

            var inactiveQueue = new Queue(locationId, 50, 15, "system");
            inactiveQueue.Deactivate("admin");

            _queueRepoMock.Setup(r => r.GetByIdAsync(queueId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(inactiveQueue);

            // Act
            var result = await _service.JoinQueueAsync(request, "user123");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("Queue is currently inactive."));
        }

        [TestMethod]
        public async Task JoinQueueAsync_WithNewCustomer_CreatesCustomerAndJoinsQueue()
        {
            // Arrange
            var queueId = Guid.NewGuid();
            var locationId = Guid.NewGuid();
            
            var request = new JoinQueueRequest
            {
                QueueId = queueId.ToString(),
                CustomerName = "Jane Doe",
                PhoneNumber = "+5511888888888",
                Email = "jane@example.com",
                IsAnonymous = false
            };

            var existingQueue = new Queue(locationId, 50, 15, "system");

            _queueRepoMock.Setup(r => r.GetByIdAsync(queueId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(existingQueue);
              _customerRepoMock.Setup(r => r.GetByPhoneNumberAsync("+5511888888888", It.IsAny<CancellationToken>()))
                            .ReturnsAsync((Customer?)null);            _customerRepoMock.Setup(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync((Customer customer, CancellationToken _) => customer);

            _queueRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Queue>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((Queue q, CancellationToken _) => q);

            // Act
            var result = await _service.JoinQueueAsync(request, "user123");

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.QueueEntryId);
            Assert.AreEqual(1, result.Position);
            
            // Verify customer was created
            _customerRepoMock.Verify(r => r.AddAsync(
                It.Is<Customer>(c => c.Name == "Jane Doe" && c.PhoneNumber!.Value == "+5511888888888"), 
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task JoinQueueAsync_WithAnonymousCustomer_CreatesAnonymousCustomerAndJoinsQueue()
        {
            // Arrange
            var queueId = Guid.NewGuid();
            var locationId = Guid.NewGuid();
            
            var request = new JoinQueueRequest
            {
                QueueId = queueId.ToString(),
                CustomerName = "Anonymous Customer",
                IsAnonymous = true
            };

            var existingQueue = new Queue(locationId, 50, 15, "system");

            _queueRepoMock.Setup(r => r.GetByIdAsync(queueId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(existingQueue);            _customerRepoMock.Setup(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync((Customer customer, CancellationToken _) => customer);

            _queueRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Queue>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((Queue q, CancellationToken _) => q);

            // Act
            var result = await _service.JoinQueueAsync(request, "user123");

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.QueueEntryId);
            Assert.AreEqual(1, result.Position);
            
            // Verify anonymous customer was created
            _customerRepoMock.Verify(r => r.AddAsync(
                It.Is<Customer>(c => c.Name == "Anonymous Customer" && c.IsAnonymous == true), 
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task JoinQueueAsync_WithFullQueue_ReturnsError()
        {
            // Arrange
            var queueId = Guid.NewGuid();
            var locationId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            
            var request = new JoinQueueRequest
            {
                QueueId = queueId.ToString(),
                CustomerName = "John Doe",
                PhoneNumber = "+5511999999999"
            };

            var fullQueue = new Queue(locationId, 1, 15, "system"); // Max size 1
            // Add one customer to fill it up
            fullQueue.AddCustomerToQueue(customerId, "Existing Customer");

            var existingCustomer = new Customer("John Doe", "+5511999999999", null, false);

            _queueRepoMock.Setup(r => r.GetByIdAsync(queueId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(fullQueue);
              _customerRepoMock.Setup(r => r.GetByPhoneNumberAsync("+5511999999999", It.IsAny<CancellationToken>()))
                            .ReturnsAsync(existingCustomer);

            // Act
            var result = await _service.JoinQueueAsync(request, "user123");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Any(e => e.Contains("Queue has reached its maximum size")));
        }
    }
}
