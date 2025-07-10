using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Grande.Fila.API.Application.Queues;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.Customers;
using Grande.Fila.API.Domain.Staff;
using Grande.Fila.API.Infrastructure.Repositories.Bogus;

namespace Grande.Fila.API.Tests.Application.Queues
{
    [TestClass]
    public class BarberAddServiceTests
    {
        private Mock<IQueueRepository> _mockQueueRepository;
        private Mock<ICustomerRepository> _mockCustomerRepository;
        private Mock<IStaffMemberRepository> _mockStaffRepository;
        private BarberAddService _barberAddService;
        private Queue _testQueue;
        private StaffMember _testStaffMember;
        private Customer _testCustomer;

        [TestInitialize]
        public void Setup()
        {
            _mockQueueRepository = new Mock<IQueueRepository>();
            _mockCustomerRepository = new Mock<ICustomerRepository>();
            _mockStaffRepository = new Mock<IStaffMemberRepository>();
            _barberAddService = new BarberAddService(_mockQueueRepository.Object, _mockCustomerRepository.Object, _mockStaffRepository.Object);

            // Create test queue
            _testQueue = new Queue(
                locationId: Guid.NewGuid(),
                maxSize: 50,
                lateClientCapTimeInMinutes: 15,
                createdBy: "test-system"
            );

            // Create test staff member
            _testStaffMember = new StaffMember(
                name: "John Barber",
                locationId: _testQueue.LocationId,
                email: "john@barbershop.com",
                phoneNumber: "+1234567890",
                profilePictureUrl: null,
                role: "Barber",
                username: "johnbarber",
                userId: null,
                createdBy: "test-system"
            );

            // Create test customer
            _testCustomer = new Customer(
                name: "Jane Doe",
                phoneNumber: "+0987654321",
                email: "jane@example.com",
                isAnonymous: false
            );
        }

        [TestMethod]
        public async Task BarberAdd_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var request = new BarberAddRequest
            {
                QueueId = _testQueue.Id.ToString(),
                StaffMemberId = _testStaffMember.Id.ToString(),
                CustomerName = "New Customer",
                PhoneNumber = "+1234567890"
            };

            var customerWithCorrectName = new Customer(
                name: request.CustomerName,
                phoneNumber: request.PhoneNumber,
                email: null,
                isAnonymous: false
            );

            _mockStaffRepository
                .Setup(r => r.GetByIdAsync(_testStaffMember.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testStaffMember);

            _mockQueueRepository
                .Setup(r => r.GetByIdAsync(_testQueue.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testQueue);

            _mockCustomerRepository
                .Setup(r => r.GetByPhoneNumberAsync(request.PhoneNumber, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Customer)null);

            _mockCustomerRepository
                .Setup(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(customerWithCorrectName);

            // Act
            var result = await _barberAddService.BarberAddAsync(request, "test-user");

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.QueueEntryId);
            Assert.AreEqual("New Customer", result.CustomerName);
            Assert.IsTrue(result.Position > 0);
            Assert.IsNotNull(result.AddedAt);
            Assert.AreEqual(_testQueue.Id.ToString(), result.QueueId);
            Assert.AreEqual(_testStaffMember.Id.ToString(), result.StaffMemberId);
            Assert.AreEqual(0, result.FieldErrors.Count);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public async Task BarberAdd_EmptyCustomerName_ReturnsValidationError()
        {
            // Arrange
            var request = new BarberAddRequest
            {
                QueueId = _testQueue.Id.ToString(),
                StaffMemberId = _testStaffMember.Id.ToString(),
                CustomerName = ""
            };

            // Act
            var result = await _barberAddService.BarberAddAsync(request, "test-user");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("CustomerName"));
            Assert.AreEqual("Customer name is required.", result.FieldErrors["CustomerName"]);
        }

        [TestMethod]
        public async Task BarberAdd_InvalidQueueId_ReturnsValidationError()
        {
            // Arrange
            var request = new BarberAddRequest
            {
                QueueId = "invalid-guid",
                StaffMemberId = _testStaffMember.Id.ToString(),
                CustomerName = "Test Customer"
            };

            // Act
            var result = await _barberAddService.BarberAddAsync(request, "test-user");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("QueueId"));
            Assert.AreEqual("Invalid queue ID format.", result.FieldErrors["QueueId"]);
        }

        [TestMethod]
        public async Task BarberAdd_InvalidStaffMemberId_ReturnsValidationError()
        {
            // Arrange
            var request = new BarberAddRequest
            {
                QueueId = _testQueue.Id.ToString(),
                StaffMemberId = "invalid-guid",
                CustomerName = "Test Customer"
            };

            // Act
            var result = await _barberAddService.BarberAddAsync(request, "test-user");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("StaffMemberId"));
            Assert.AreEqual("Invalid staff member ID format.", result.FieldErrors["StaffMemberId"]);
        }

        [TestMethod]
        public async Task BarberAdd_StaffMemberNotFound_ReturnsError()
        {
            // Arrange
            var request = new BarberAddRequest
            {
                QueueId = _testQueue.Id.ToString(),
                StaffMemberId = Guid.NewGuid().ToString(),
                CustomerName = "Test Customer"
            };

            _mockStaffRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((StaffMember)null);

            // Act
            var result = await _barberAddService.BarberAddAsync(request, "test-user");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("Staff member not found."));
        }

        [TestMethod]
        public async Task BarberAdd_InactiveStaffMember_ReturnsError()
        {
            // Arrange
            var inactiveStaffMember = new StaffMember(
                name: "Inactive Barber",
                locationId: _testQueue.LocationId,
                email: "inactive@barbershop.com",
                phoneNumber: "+1234567890",
                profilePictureUrl: null,
                role: "Barber",
                username: "inactivebarber",
                userId: null,
                createdBy: "test-system"
            );
            inactiveStaffMember.Deactivate("test-system");

            var request = new BarberAddRequest
            {
                QueueId = _testQueue.Id.ToString(),
                StaffMemberId = inactiveStaffMember.Id.ToString(),
                CustomerName = "Test Customer"
            };

            _mockStaffRepository
                .Setup(r => r.GetByIdAsync(inactiveStaffMember.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(inactiveStaffMember);

            // Act
            var result = await _barberAddService.BarberAddAsync(request, "test-user");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("Cannot add customers for inactive staff member."));
        }

        [TestMethod]
        public async Task BarberAdd_StaffMemberOnBreak_ReturnsError()
        {
            // Arrange
            var staffOnBreak = new StaffMember(
                name: "Break Barber",
                locationId: _testQueue.LocationId,
                email: "break@barbershop.com",
                phoneNumber: "+1234567890",
                profilePictureUrl: null,
                role: "Barber",
                username: "breakbarber",
                userId: null,
                createdBy: "test-system"
            );
            staffOnBreak.StartBreak(TimeSpan.FromMinutes(30), "Lunch break", "test-system");

            var request = new BarberAddRequest
            {
                QueueId = _testQueue.Id.ToString(),
                StaffMemberId = staffOnBreak.Id.ToString(),
                CustomerName = "Test Customer"
            };

            _mockStaffRepository
                .Setup(r => r.GetByIdAsync(staffOnBreak.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(staffOnBreak);

            // Act
            var result = await _barberAddService.BarberAddAsync(request, "test-user");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("Cannot add customers while on break."));
        }

        [TestMethod]
        public async Task BarberAdd_QueueNotFound_ReturnsError()
        {
            // Arrange
            var request = new BarberAddRequest
            {
                QueueId = Guid.NewGuid().ToString(),
                StaffMemberId = _testStaffMember.Id.ToString(),
                CustomerName = "Test Customer"
            };

            _mockStaffRepository
                .Setup(r => r.GetByIdAsync(_testStaffMember.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testStaffMember);

            _mockQueueRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Queue)null);

            // Act
            var result = await _barberAddService.BarberAddAsync(request, "test-user");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("Queue not found."));
        }

        [TestMethod]
        public async Task BarberAdd_InactiveQueue_ReturnsError()
        {
            // Arrange
            var inactiveQueue = new Queue(
                locationId: Guid.NewGuid(),
                maxSize: 50,
                lateClientCapTimeInMinutes: 15,
                createdBy: "test-system"
            );
            inactiveQueue.Deactivate("test-system");

            var request = new BarberAddRequest
            {
                QueueId = inactiveQueue.Id.ToString(),
                StaffMemberId = _testStaffMember.Id.ToString(),
                CustomerName = "Test Customer"
            };

            _mockStaffRepository
                .Setup(r => r.GetByIdAsync(_testStaffMember.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testStaffMember);

            _mockQueueRepository
                .Setup(r => r.GetByIdAsync(inactiveQueue.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(inactiveQueue);

            // Act
            var result = await _barberAddService.BarberAddAsync(request, "test-user");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("Queue is currently inactive."));
        }

        [TestMethod]
        public async Task BarberAdd_FullQueue_ReturnsError()
        {
            // Arrange
            var fullQueue = new Queue(
                locationId: Guid.NewGuid(),
                maxSize: 1, // Small queue
                lateClientCapTimeInMinutes: 15,
                createdBy: "test-system"
            );

            // Add one customer to make queue full
            fullQueue.AddCustomerToQueue(
                customerId: Guid.NewGuid(),
                customerName: "Existing Customer",
                staffMemberId: _testStaffMember.Id
            );

            var request = new BarberAddRequest
            {
                QueueId = fullQueue.Id.ToString(),
                StaffMemberId = _testStaffMember.Id.ToString(),
                CustomerName = "New Customer"
            };

            _mockStaffRepository
                .Setup(r => r.GetByIdAsync(_testStaffMember.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testStaffMember);

            _mockQueueRepository
                .Setup(r => r.GetByIdAsync(fullQueue.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(fullQueue);

            _mockCustomerRepository
                .Setup(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testCustomer);

            // Act
            var result = await _barberAddService.BarberAddAsync(request, "test-user");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("Queue has reached its maximum size."));
        }

        [TestMethod]
        public async Task BarberAdd_ExistingCustomer_UpdatesCustomerAndReturnsSuccess()
        {
            // Arrange
            var existingCustomer = new Customer(
                name: "Old Name",
                phoneNumber: "+1234567890",
                email: "old@example.com",
                isAnonymous: false
            );

            var request = new BarberAddRequest
            {
                QueueId = _testQueue.Id.ToString(),
                StaffMemberId = _testStaffMember.Id.ToString(),
                CustomerName = "New Name",
                PhoneNumber = "+1234567890"
            };

            _mockStaffRepository
                .Setup(r => r.GetByIdAsync(_testStaffMember.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testStaffMember);

            _mockQueueRepository
                .Setup(r => r.GetByIdAsync(_testQueue.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testQueue);

            _mockCustomerRepository
                .Setup(r => r.GetByPhoneNumberAsync(request.PhoneNumber, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingCustomer);

            _mockCustomerRepository
                .Setup(r => r.UpdateAsync(existingCustomer, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingCustomer);

            // Act
            var result = await _barberAddService.BarberAddAsync(request, "test-user");

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual("New Name", result.CustomerName);
            _mockCustomerRepository.Verify(r => r.UpdateAsync(existingCustomer, It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task BarberAdd_AnonymousCustomer_ReturnsSuccess()
        {
            // Arrange
            var request = new BarberAddRequest
            {
                QueueId = _testQueue.Id.ToString(),
                StaffMemberId = _testStaffMember.Id.ToString(),
                CustomerName = "Anonymous Customer"
                // No phone or email - will be anonymous
            };

            var anonymousCustomer = new Customer(
                name: request.CustomerName,
                phoneNumber: null,
                email: null,
                isAnonymous: true
            );

            _mockStaffRepository
                .Setup(r => r.GetByIdAsync(_testStaffMember.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testStaffMember);

            _mockQueueRepository
                .Setup(r => r.GetByIdAsync(_testQueue.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testQueue);

            _mockCustomerRepository
                .Setup(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(anonymousCustomer);

            // Act
            var result = await _barberAddService.BarberAddAsync(request, "test-user");

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual("Anonymous Customer", result.CustomerName);
            _mockCustomerRepository.Verify(r => r.AddAsync(It.Is<Customer>(c => c.IsAnonymous), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task BarberAdd_RepositoryException_ReturnsError()
        {
            // Arrange
            var request = new BarberAddRequest
            {
                QueueId = _testQueue.Id.ToString(),
                StaffMemberId = _testStaffMember.Id.ToString(),
                CustomerName = "Test Customer"
            };

            _mockStaffRepository
                .Setup(r => r.GetByIdAsync(_testStaffMember.Id, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Database connection failed"));

            // Act
            var result = await _barberAddService.BarberAddAsync(request, "test-user");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("Database connection failed"));
        }
    }
} 