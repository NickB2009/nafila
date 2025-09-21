using System;
using System.Collections.Generic;
using System.Linq;
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
    public class GetPublicSalonDetailServiceTests
    {
        private Mock<ILocationRepository> _mockLocationRepository;
        private Mock<IQueueRepository> _mockQueueRepository;
        private Mock<IServicesOfferedRepository> _mockServiceRepository;
        private Mock<ILogger<GetPublicSalonDetailService>> _mockLogger;
        private GetPublicSalonDetailService _service;

        [TestInitialize]
        public void Setup()
        {
            _mockLocationRepository = new Mock<ILocationRepository>();
            _mockQueueRepository = new Mock<IQueueRepository>();
            _mockServiceRepository = new Mock<IServicesOfferedRepository>();
            _mockLogger = new Mock<ILogger<GetPublicSalonDetailService>>();
            
            _service = new GetPublicSalonDetailService(
                _mockLocationRepository.Object,
                _mockQueueRepository.Object,
                _mockServiceRepository.Object,
                _mockLogger.Object);
        }

        [TestMethod]
        public async Task ExecuteAsync_WithValidSalonId_ReturnsSalonDetail()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var organizationId = Guid.NewGuid();
            
            var address = Address.Create("123 Main St", "100", "", "Downtown", "São Paulo", "SP", "Brazil", "01310-100");
            var location = new Location(
                "Barbearia Premium",
                "barbearia-premium",
                "Premium barbershop experience",
                organizationId,
                address,
                "+5511999999999",
                "contato@barberiapremium.com",
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
            queue.AddCustomerToQueue(Guid.NewGuid(), "Customer 3");

            var service1 = new ServiceOffered("Corte Masculino", "Traditional haircut", locationId, 30, 25.00m, null, "system");
            var service2 = new ServiceOffered("Barba", "Beard trim", locationId, 15, 15.00m, null, "system");
            var service3 = new ServiceOffered("Sobrancelha", "Eyebrow trim", locationId, 10, 10.00m, null, "system");

            _mockLocationRepository
                .Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(location);

            _mockQueueRepository
                .Setup(r => r.GetActiveQueueByLocationIdAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(queue);

            _mockServiceRepository
                .Setup(r => r.GetActiveServiceTypesAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ServiceOffered> { service1, service2, service3 });

            // Act
            var result = await _service.ExecuteAsync(locationId.ToString());

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Salon);
            Assert.AreEqual(locationId.ToString(), result.Salon.Id);
            Assert.AreEqual("Barbearia Premium", result.Salon.Name);
            Assert.AreEqual("123 Main St, 100, Downtown, São Paulo, SP, Brazil - 01310-100", result.Salon.Address);
            Assert.AreEqual(0, result.Salon.Latitude); // Address.Create doesn't set lat/long
            Assert.AreEqual(0, result.Salon.Longitude);
            Assert.IsTrue(result.Salon.IsOpen);
            Assert.AreEqual(3, result.Salon.QueueLength);
            Assert.AreEqual(3, result.Salon.Services.Count);
            Assert.IsTrue(result.Salon.Services.Contains("Corte Masculino"));
            Assert.IsTrue(result.Salon.Services.Contains("Barba"));
            Assert.IsTrue(result.Salon.Services.Contains("Sobrancelha"));
            
            // Check business hours (24/7 for testing)
            Assert.IsTrue(result.Salon.BusinessHours.ContainsKey("monday"));
            Assert.IsTrue(result.Salon.BusinessHours.ContainsKey("tuesday"));
            Assert.IsTrue(result.Salon.BusinessHours.ContainsKey("sunday"));
            Assert.AreEqual("00:00-23:59", result.Salon.BusinessHours["monday"]);
            Assert.AreEqual("closed", result.Salon.BusinessHours["sunday"]);
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
        public async Task ExecuteAsync_CalculatesIsPopularCorrectly()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var organizationId = Guid.NewGuid();
            
            var address = Address.Create("123 Main St", "", "", "", "City", "State", "Country", "12345");
            var location = new Location(
                "Popular Salon",
                "popular-salon",
                "Very busy salon",
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
            
            // Add many customers to make it popular
            for (int i = 0; i < 10; i++)
            {
                queue.AddCustomerToQueue(Guid.NewGuid(), $"Customer {i + 1}");
            }

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
            Assert.IsNotNull(result.Salon);
            Assert.IsTrue(result.Salon.IsPopular); // Should be true because queue length > 5 (example threshold)
            Assert.AreEqual(10, result.Salon.QueueLength);
        }
    }
}