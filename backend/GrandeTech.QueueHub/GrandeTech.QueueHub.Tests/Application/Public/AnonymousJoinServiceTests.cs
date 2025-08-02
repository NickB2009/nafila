using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Application.Public;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.Customers;
using Grande.Fila.API.Domain.Locations;
using Grande.Fila.API.Domain.Staff;
using Grande.Fila.API.Application.Services.Cache;
using Moq;
using System.Linq;

namespace Grande.Fila.API.Tests.Application.Public
{
    [TestClass]
    public class AnonymousJoinServiceTests
    {
        private Mock<IQueueRepository> _mockQueueRepository;
        private Mock<ICustomerRepository> _mockCustomerRepository;
        private Mock<ILocationRepository> _mockLocationRepository;
        private Mock<IStaffMemberRepository> _mockStaffRepository;
        private Mock<IAverageWaitTimeCache> _mockCache;
        private AnonymousJoinService _service;

        [TestInitialize]
        public void Setup()
        {
            _mockQueueRepository = new Mock<IQueueRepository>();
            _mockCustomerRepository = new Mock<ICustomerRepository>();
            _mockLocationRepository = new Mock<ILocationRepository>();
            _mockStaffRepository = new Mock<IStaffMemberRepository>();
            _mockCache = new Mock<IAverageWaitTimeCache>();
            
            _service = new AnonymousJoinService(
                _mockQueueRepository.Object,
                _mockCustomerRepository.Object,
                _mockLocationRepository.Object,
                _mockStaffRepository.Object,
                _mockCache.Object
            );
        }

        [TestMethod]
        public async Task ExecuteAsync_WithValidRequest_ShouldReturnSuccess()
        {
            // Arrange
            var salonId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var request = new AnonymousJoinRequest
            {
                SalonId = salonId.ToString(),
                Name = "Test User",
                Email = "test@example.com",
                AnonymousUserId = Guid.NewGuid().ToString(),
                ServiceRequested = "Haircut",
                EmailNotifications = true,
                BrowserNotifications = true
            };

            var location = new Location(
                organizationId: Guid.NewGuid(),
                name: "Test Salon",
                address: "123 Test St",
                phoneNumber: "123-456-7890",
                email: "test@salon.com"
            );

            var queue = new Queue(
                locationId: salonId,
                maxSize: 100,
                lateClientCapTimeInMinutes: 15,
                createdBy: "system"
            );

            var customer = new Customer(
                name: request.Name,
                phoneNumber: null,
                email: request.Email,
                isAnonymous: true
            );

            _mockLocationRepository.Setup(x => x.GetByIdAsync(salonId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(location);

            _mockQueueRepository.Setup(x => x.GetActiveQueueByLocationIdAsync(salonId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(queue);

            _mockCustomerRepository.Setup(x => x.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(customer);

            _mockStaffRepository.Setup(x => x.GetByLocationAsync(salonId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { CreateMockStaffMember(isActive: true) });

            _mockCache.Setup(x => x.TryGetAverage(salonId, out It.Ref<double>.IsAny))
                .Returns(false);

            // Act
            var result = await _service.ExecuteAsync(request, CancellationToken.None);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Id);
            Assert.AreEqual(1, result.Position);
            Assert.IsTrue(result.EstimatedWaitMinutes > 0);
            Assert.IsNotNull(result.JoinedAt);
            Assert.AreEqual("waiting", result.Status);
        }

        [TestMethod]
        public async Task ExecuteAsync_WithInvalidSalonId_ShouldReturnFieldError()
        {
            // Arrange
            var request = new AnonymousJoinRequest
            {
                SalonId = "invalid-guid",
                Name = "Test User",
                Email = "test@example.com",
                AnonymousUserId = Guid.NewGuid().ToString(),
                ServiceRequested = "Haircut"
            };

            // Act
            var result = await _service.ExecuteAsync(request, CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("SalonId"));
        }

        [TestMethod]
        public async Task ExecuteAsync_WithEmptyName_ShouldReturnFieldError()
        {
            // Arrange
            var request = new AnonymousJoinRequest
            {
                SalonId = Guid.NewGuid().ToString(),
                Name = "",
                Email = "test@example.com",
                AnonymousUserId = Guid.NewGuid().ToString(),
                ServiceRequested = "Haircut"
            };

            // Act
            var result = await _service.ExecuteAsync(request, CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("Name"));
        }

        [TestMethod]
        public async Task ExecuteAsync_WithInvalidEmail_ShouldReturnFieldError()
        {
            // Arrange
            var request = new AnonymousJoinRequest
            {
                SalonId = Guid.NewGuid().ToString(),
                Name = "Test User",
                Email = "invalid-email",
                AnonymousUserId = Guid.NewGuid().ToString(),
                ServiceRequested = "Haircut"
            };

            // Act
            var result = await _service.ExecuteAsync(request, CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("Email"));
        }

        [TestMethod]
        public async Task ExecuteAsync_WithNonExistentSalon_ShouldReturnNotFoundError()
        {
            // Arrange
            var salonId = Guid.NewGuid();
            var request = new AnonymousJoinRequest
            {
                SalonId = salonId.ToString(),
                Name = "Test User",
                Email = "test@example.com",
                AnonymousUserId = Guid.NewGuid().ToString(),
                ServiceRequested = "Haircut"
            };

            _mockLocationRepository.Setup(x => x.GetByIdAsync(salonId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Location?)null);

            // Act
            var result = await _service.ExecuteAsync(request, CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("Salon not found"));
        }

        [TestMethod]
        public async Task ExecuteAsync_WithInactiveQueue_ShouldReturnNotAcceptingError()
        {
            // Arrange
            var salonId = Guid.NewGuid();
            var request = new AnonymousJoinRequest
            {
                SalonId = salonId.ToString(),
                Name = "Test User",
                Email = "test@example.com",
                AnonymousUserId = Guid.NewGuid().ToString(),
                ServiceRequested = "Haircut"
            };

            var location = new Location(
                organizationId: Guid.NewGuid(),
                name: "Test Salon",
                address: "123 Test St",
                phoneNumber: "123-456-7890",
                email: "test@salon.com"
            );

            _mockLocationRepository.Setup(x => x.GetByIdAsync(salonId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(location);

            _mockQueueRepository.Setup(x => x.GetActiveQueueByLocationIdAsync(salonId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Queue?)null);

            // Act
            var result = await _service.ExecuteAsync(request, CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("Salon not found or not accepting customers"));
        }

        [TestMethod]
        public async Task ExecuteAsync_WithCustomerAlreadyInQueue_ShouldReturnConflictError()
        {
            // Arrange
            var salonId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var request = new AnonymousJoinRequest
            {
                SalonId = salonId.ToString(),
                Name = "Test User",
                Email = "test@example.com",
                AnonymousUserId = Guid.NewGuid().ToString(),
                ServiceRequested = "Haircut"
            };

            var location = new Location(
                organizationId: Guid.NewGuid(),
                name: "Test Salon",
                address: "123 Test St",
                phoneNumber: "123-456-7890",
                email: "test@salon.com"
            );

            var queue = new Queue(
                locationId: salonId,
                maxSize: 100,
                lateClientCapTimeInMinutes: 15,
                createdBy: "system"
            );

            // Add existing customer to queue to simulate conflict
            queue.AddCustomerToQueue(customerId, "Existing Customer", null, null, null);

            var customer = new Customer(
                name: request.Name,
                phoneNumber: null,
                email: request.Email,
                isAnonymous: true
            );
            // Set the customer ID to simulate finding existing customer
            typeof(Customer).GetProperty("Id")?.SetValue(customer, customerId);

            _mockLocationRepository.Setup(x => x.GetByIdAsync(salonId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(location);

            _mockQueueRepository.Setup(x => x.GetActiveQueueByLocationIdAsync(salonId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(queue);

            _mockCustomerRepository.Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(customer);

            // Act
            var result = await _service.ExecuteAsync(request, CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("User already in queue for this salon"));
        }

        private StaffMember CreateMockStaffMember(bool isActive = true)
        {
            var staff = new StaffMember(
                locationId: Guid.NewGuid(),
                name: "Test Staff",
                role: "Barber",
                phoneNumber: "123-456-7890",
                email: "staff@test.com"
            );
            
            if (isActive)
            {
                staff.Activate();
            }
            
            return staff;
        }
    }
}