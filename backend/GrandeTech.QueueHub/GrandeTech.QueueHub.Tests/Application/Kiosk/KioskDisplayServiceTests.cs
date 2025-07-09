using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Grande.Fila.API.Application.Kiosk;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.Locations;

namespace Grande.Fila.API.Tests.Application.Kiosk
{
    [TestClass]
    public class KioskDisplayServiceTests
    {
        private Mock<IQueueRepository> _mockQueueRepo = null!;
        private Mock<ILocationRepository> _mockLocationRepo = null!;
        private Mock<ILogger<KioskDisplayService>> _mockLogger = null!;
        private KioskDisplayService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockQueueRepo = new Mock<IQueueRepository>();
            _mockLocationRepo = new Mock<ILocationRepository>();
            _mockLogger = new Mock<ILogger<KioskDisplayService>>();
            _service = new KioskDisplayService(_mockQueueRepo.Object, _mockLocationRepo.Object, _mockLogger.Object);
        }

        [TestMethod]
        public async Task ExecuteAsync_ValidLocationId_ReturnsQueueDisplay()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var request = new KioskDisplayRequest
            {
                LocationId = locationId.ToString()
            };

            _mockLocationRepo.Setup(r => r.ExistsAsync(l => l.Id == locationId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockQueueRepo.Setup(r => r.GetByLocationAsync(locationId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<Queue>());

            // Act
            var result = await _service.ExecuteAsync(request, "system");

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.QueueEntries);
        }

        [TestMethod]
        public async Task ExecuteAsync_InvalidLocationId_ReturnsError()
        {
            // Arrange
            var request = new KioskDisplayRequest
            {
                LocationId = "invalid-guid"
            };

            // Act
            var result = await _service.ExecuteAsync(request, "system");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("LocationId"));
        }
    }
} 