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
using Grande.Fila.API.Domain.Organizations;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.ServicesOffered;
using Grande.Fila.API.Domain.Common.ValueObjects;

namespace Grande.Fila.API.Tests.Application.Public
{
    [TestClass]
    public class GetPublicSalonsServiceTests
    {
        private Mock<ILocationRepository> _mockLocationRepository;
        private Mock<IOrganizationRepository> _mockOrganizationRepository;
        private Mock<IQueueRepository> _mockQueueRepository;
        private Mock<IServicesOfferedRepository> _mockServiceRepository;
        private Mock<ILogger<GetPublicSalonsService>> _mockLogger;
        private GetPublicSalonsService _service;

        [TestInitialize]
        public void Setup()
        {
            _mockLocationRepository = new Mock<ILocationRepository>();
            _mockOrganizationRepository = new Mock<IOrganizationRepository>();
            _mockQueueRepository = new Mock<IQueueRepository>();
            _mockServiceRepository = new Mock<IServicesOfferedRepository>();
            _mockLogger = new Mock<ILogger<GetPublicSalonsService>>();
            
            _service = new GetPublicSalonsService(
                _mockLocationRepository.Object,
                _mockOrganizationRepository.Object,
                _mockQueueRepository.Object,
                _mockServiceRepository.Object,
                _mockLogger.Object);
        }

        [TestMethod]
        public async Task ExecuteAsync_WithActiveLocations_ReturnsPublicSalons()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var organizationId = Guid.NewGuid();
            
            var address = Address.Create("123 Main St", "100", "", "Downtown", "São Paulo", "SP", "Brazil", "01310-100");
            
            // Create business hours that are always open (24/7)
            var alwaysOpenHours = WeeklyBusinessHours.Create(
                DayBusinessHours.Create(TimeSpan.Zero, TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59))), // Monday
                DayBusinessHours.Create(TimeSpan.Zero, TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59))), // Tuesday
                DayBusinessHours.Create(TimeSpan.Zero, TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59))), // Wednesday
                DayBusinessHours.Create(TimeSpan.Zero, TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59))), // Thursday
                DayBusinessHours.Create(TimeSpan.Zero, TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59))), // Friday
                DayBusinessHours.Create(TimeSpan.Zero, TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59))), // Saturday
                DayBusinessHours.Create(TimeSpan.Zero, TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59)))  // Sunday
            );
            
            var location = new Location(
                "Barbearia do João",
                "barbearia-joao",
                "Traditional barbershop",
                organizationId,
                address,
                "+5511999999999",
                "contato@barbearia.com",
                TimeSpan.Zero, // Open at midnight (00:00)
                TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59)), // Close at 23:59 (24/7)
                50,
                15,
                "system"
            );
            
            // Update the business hours to ensure it's always open
            location.UpdateWeeklyHours(alwaysOpenHours, "test");

            // Set the location ID using reflection
            var idProperty = typeof(Location).GetProperty("Id");
            idProperty?.SetValue(location, locationId);

            var queue = new Queue(locationId, 50, 15, "system");
            var queueEntry1 = queue.AddCustomerToQueue(Guid.NewGuid(), "Customer 1");
            var queueEntry2 = queue.AddCustomerToQueue(Guid.NewGuid(), "Customer 2");

            var service1 = new ServiceOffered("Corte Masculino", "Traditional haircut", locationId, 30, 25.00m, null, "system");
            var service2 = new ServiceOffered("Barba", "Beard trim", locationId, 15, 15.00m, null, "system");

            _mockLocationRepository
                .Setup(r => r.GetActiveLocationsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Location> { location });

            _mockQueueRepository
                .Setup(r => r.GetByLocationIdAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(queue);

            _mockServiceRepository
                .Setup(r => r.GetActiveServiceTypesAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ServiceOffered> { service1, service2 });

            // Act
            var result = await _service.ExecuteAsync();

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.Salons.Count);
            
            var salon = result.Salons.First();
            Assert.AreEqual(locationId.ToString(), salon.Id);
            Assert.AreEqual("Barbearia do João", salon.Name);
            Assert.AreEqual("123 Main St, 100, Downtown, São Paulo, SP, Brazil - 01310-100", salon.Address);
            Assert.AreEqual(0, salon.Latitude); // Address.Create doesn't set lat/long
            Assert.AreEqual(0, salon.Longitude);
            Assert.IsTrue(salon.IsOpen);
            Assert.AreEqual(2, salon.QueueLength);
            Assert.IsTrue(salon.CurrentWaitTimeMinutes > 0);
            Assert.AreEqual(2, salon.Services.Count);
            Assert.IsTrue(salon.Services.Contains("Corte Masculino"));
            Assert.IsTrue(salon.Services.Contains("Barba"));
        }

        [TestMethod]
        public async Task ExecuteAsync_WithNoActiveLocations_ReturnsEmptyList()
        {
            // Arrange
            _mockLocationRepository
                .Setup(r => r.GetActiveLocationsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Location>());

            // Act
            var result = await _service.ExecuteAsync();

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, result.Salons.Count);
        }

        [TestMethod]
        public async Task ExecuteAsync_RepositoryThrowsException_ReturnsError()
        {
            // Arrange
            _mockLocationRepository
                .Setup(r => r.GetActiveLocationsAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.ExecuteAsync();

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("An unexpected error occurred while retrieving salons"));
        }

        [TestMethod]
        public async Task ExecuteAsync_CalculatesIsFastCorrectly()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var organizationId = Guid.NewGuid();
            
            var address = Address.Create("123 Main St", "", "", "", "City", "State", "Country", "12345");
            var location = new Location(
                "Fast Salon",
                "fast-salon",
                "Quick service salon",
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

            // Update average service time to 5 minutes for fast service
            location.UpdateAverageServiceTime(5, "system");

            var queue = new Queue(locationId, 50, 15, "system");
            queue.AddCustomerToQueue(Guid.NewGuid(), "Customer 1"); // Only 1 customer, 5 min wait

            _mockLocationRepository
                .Setup(r => r.GetActiveLocationsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Location> { location });

            _mockQueueRepository
                .Setup(r => r.GetByLocationIdAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(queue);

            _mockServiceRepository
                .Setup(r => r.GetActiveServiceTypesAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ServiceOffered>());

            // Act
            var result = await _service.ExecuteAsync();

            // Assert
            Assert.IsTrue(result.Success);
            var salon = result.Salons.First();
            Assert.IsTrue(salon.IsFast); // Should be true because wait time < 10 minutes
            Assert.IsTrue(salon.CurrentWaitTimeMinutes < 10);
        }
    }
}