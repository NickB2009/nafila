using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Grande.Fila.API.Application.Public;
using Grande.Fila.API.Domain.Locations;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.ServicesOffered;
using Grande.Fila.API.Domain.Common.ValueObjects;

namespace Grande.Fila.API.Tests.Application.Public
{
    [TestClass]
    public class GetPublicQueueStatusServiceTests
    {
        private Mock<ILocationRepository> _mockLocationRepository;
        private Mock<IQueueRepository> _mockQueueRepository;
        private Mock<IServicesOfferedRepository> _mockServiceRepository;
        private Mock<ILogger<GetPublicQueueStatusService>> _mockLogger;
        private GetPublicQueueStatusService _service;

        [TestInitialize]
        public void Setup()
        {
            _mockLocationRepository = new Mock<ILocationRepository>();
            _mockQueueRepository = new Mock<IQueueRepository>();
            _mockServiceRepository = new Mock<IServicesOfferedRepository>();
            _mockLogger = new Mock<ILogger<GetPublicQueueStatusService>>();
            
            _service = new GetPublicQueueStatusService(
                _mockLocationRepository.Object,
                _mockQueueRepository.Object,
                _mockServiceRepository.Object,
                _mockLogger.Object);
        }

        [TestMethod]
        public async Task ExecuteAsync_WithValidSalonId_ReturnsQueueStatus()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var organizationId = Guid.NewGuid();
            
            var address = Address.Create("123 Main St", "", "", "", "City", "State", "Country", "12345");
            var location = new Location(
                "Test Salon",
                "test-salon",
                "Test Description",
                organizationId,
                address,
                null,
                null,
                TimeSpan.Zero, // Open at midnight (00:00)
                TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59)), // Close at 23:59 (24/7)
                50,
                15,
                "system"
            );

            // Set the location ID using reflection
            var idProperty = typeof(Location).GetProperty("Id");
            idProperty?.SetValue(location, locationId);

            var queue = new Queue(locationId, 50, 15, "system");
            queue.AddCustomerToQueue(Guid.NewGuid(), "Customer 1");
            queue.AddCustomerToQueue(Guid.NewGuid(), "Customer 2");

            var service1 = new ServiceOffered("Corte Masculino", "Traditional haircut", locationId, 30, 25.00m, null, "system");
            var service2 = new ServiceOffered("Barba", "Beard trim", locationId, 15, 15.00m, null, "system");

            _mockLocationRepository
                .Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(location);

            _mockQueueRepository
                .Setup(r => r.GetActiveQueueByLocationIdAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(queue);

            _mockServiceRepository
                .Setup(r => r.GetActiveServiceTypesAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ServiceOffered> { service1, service2 });

            // Act
            var result = await _service.ExecuteAsync(locationId.ToString());

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.QueueStatus);
            Assert.AreEqual(locationId.ToString(), result.QueueStatus.SalonId);
            Assert.AreEqual("Test Salon", result.QueueStatus.SalonName);
            Assert.AreEqual(2, result.QueueStatus.QueueLength);
            Assert.IsTrue(result.QueueStatus.IsAcceptingCustomers);
            Assert.AreEqual(2, result.QueueStatus.AvailableServices.Count);
            Assert.IsTrue(result.QueueStatus.AvailableServices.Contains("Corte Masculino"));
            Assert.IsTrue(result.QueueStatus.AvailableServices.Contains("Barba"));
        }

        [TestMethod]
        public async Task ExecuteAsync_WithInvalidSalonId_ReturnsError()
        {
            // Arrange
            var invalidId = "invalid-guid";

            // Act
            var result = await _service.ExecuteAsync(invalidId);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("SalonId"));
            Assert.AreEqual("Invalid salon ID format", result.FieldErrors["SalonId"]);
        }

        [TestMethod]
        public async Task ExecuteAsync_WithNonExistentSalon_ReturnsError()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            
            _mockLocationRepository
                .Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Location?)null);

            // Act
            var result = await _service.ExecuteAsync(locationId.ToString());

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("Salon not found"));
        }

        [TestMethod]
        public async Task ExecuteAsync_WithInactiveSalon_ReturnsNotAcceptingCustomers()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var organizationId = Guid.NewGuid();
            
            var address = Address.Create("123 Main St", "", "", "", "City", "State", "Country", "12345");
            var location = new Location(
                "Inactive Salon",
                "inactive-salon",
                "Test Description",
                organizationId,
                address,
                null,
                null,
                TimeSpan.FromHours(8),
                TimeSpan.FromHours(18),
                50,
                15,
                "system"
            );

            // Set the location ID using reflection
            var idProperty = typeof(Location).GetProperty("Id");
            idProperty?.SetValue(location, locationId);

            // Deactivate the location
            location.Deactivate("system");

            var queue = new Queue(locationId, 50, 15, "system");

            _mockLocationRepository
                .Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(location);

            _mockQueueRepository
                .Setup(r => r.GetActiveQueueByLocationIdAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(queue);

            _mockServiceRepository
                .Setup(r => r.GetActiveServiceTypesAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ServiceOffered>());

            // Act
            var result = await _service.ExecuteAsync(locationId.ToString());

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.QueueStatus);
            Assert.IsFalse(result.QueueStatus.IsAcceptingCustomers);
        }

        [TestMethod]
        public async Task ExecuteAsync_RepositoryThrowsException_ReturnsError()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            
            _mockLocationRepository
                .Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.ExecuteAsync(locationId.ToString());

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("An unexpected error occurred while retrieving queue status"));
        }
    }
}