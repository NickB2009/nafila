using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Grande.Fila.API.Application.Locations;
using Grande.Fila.API.Application.Locations.Requests;
using Grande.Fila.API.Domain.Locations;
using Grande.Fila.API.Domain.Common.ValueObjects;

namespace Grande.Fila.API.Tests.Application.Locations
{
    [TestClass]
    public class ToggleQueueServiceTests
    {
        private Mock<ILocationRepository> _mockLocationRepository;
        private Mock<ILogger<ToggleQueueService>> _mockLogger;
        private ToggleQueueService _service;

        [TestInitialize]
        public void Setup()
        {
            _mockLocationRepository = new Mock<ILocationRepository>();
            _mockLogger = new Mock<ILogger<ToggleQueueService>>();
            _service = new ToggleQueueService(_mockLocationRepository.Object, _mockLogger.Object);
        }

        [TestMethod]
        public async Task ExecuteAsync_EnableQueue_SuccessfullyEnablesQueue()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var organizationId = Guid.NewGuid();
            var address = Address.Create("123 Main St", "", "", "", "City", "State", "Country", "12345");
            var location = new Location(
                "Test Location",
                "test-location",
                "Description",
                organizationId,
                address,
                null,
                null,
                TimeSpan.FromHours(8),
                TimeSpan.FromHours(18),
                100,
                15,
                "admin"
            );

            // Disable queue first
            location.DisableQueue("admin");

            _mockLocationRepository
                .Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(location);

            _mockLocationRepository
                .Setup(r => r.UpdateAsync(It.IsAny<Location>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(location);

            var request = new ToggleQueueRequest
            {
                LocationId = locationId.ToString(),
                EnableQueue = true
            };

            // Act
            var result = await _service.ExecuteAsync(request, "admin");

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.IsQueueEnabled);
            Assert.AreEqual(locationId.ToString(), result.LocationId);

            _mockLocationRepository.Verify(
                r => r.UpdateAsync(It.Is<Location>(l => l.IsQueueEnabled), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [TestMethod]
        public async Task ExecuteAsync_DisableQueue_SuccessfullyDisablesQueue()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var organizationId = Guid.NewGuid();
            var address = Address.Create("123 Main St", "", "", "", "City", "State", "Country", "12345");
            var location = new Location(
                "Test Location",
                "test-location",
                "Description",
                organizationId,
                address,
                null,
                null,
                TimeSpan.FromHours(8),
                TimeSpan.FromHours(18),
                100,
                15,
                "admin"
            );

            _mockLocationRepository
                .Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(location);

            _mockLocationRepository
                .Setup(r => r.UpdateAsync(It.IsAny<Location>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(location);

            var request = new ToggleQueueRequest
            {
                LocationId = locationId.ToString(),
                EnableQueue = false
            };

            // Act
            var result = await _service.ExecuteAsync(request, "admin");

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsFalse(result.IsQueueEnabled);
            Assert.AreEqual(locationId.ToString(), result.LocationId);

            _mockLocationRepository.Verify(
                r => r.UpdateAsync(It.Is<Location>(l => !l.IsQueueEnabled), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [TestMethod]
        public async Task ExecuteAsync_InvalidLocationId_ReturnsError()
        {
            // Arrange
            var request = new ToggleQueueRequest
            {
                LocationId = "invalid-guid",
                EnableQueue = true
            };

            // Act
            var result = await _service.ExecuteAsync(request, "admin");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("LocationId"));
            Assert.AreEqual("Invalid location ID format", result.FieldErrors["LocationId"]);
        }

        [TestMethod]
        public async Task ExecuteAsync_LocationNotFound_ReturnsError()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            _mockLocationRepository
                .Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Location?)null);

            var request = new ToggleQueueRequest
            {
                LocationId = locationId.ToString(),
                EnableQueue = true
            };

            // Act
            var result = await _service.ExecuteAsync(request, "admin");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("Location not found"));
        }

        [TestMethod]
        public async Task ExecuteAsync_RepositoryThrowsException_ReturnsError()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            _mockLocationRepository
                .Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            var request = new ToggleQueueRequest
            {
                LocationId = locationId.ToString(),
                EnableQueue = true
            };

            // Act
            var result = await _service.ExecuteAsync(request, "admin");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("An unexpected error occurred while updating queue status"));
        }
    }
}