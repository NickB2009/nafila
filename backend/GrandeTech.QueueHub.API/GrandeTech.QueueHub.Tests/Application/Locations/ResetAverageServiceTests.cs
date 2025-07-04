using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Grande.Fila.API.Application.Locations;
using Grande.Fila.API.Domain.Locations;
using Grande.Fila.API.Domain.Common.ValueObjects;

namespace Grande.Fila.API.Tests.Application.Locations
{
    [TestClass]
    public class ResetAverageServiceTests
    {
        private Mock<ILocationRepository> _mockRepo = null!;
        private Mock<ILogger<ResetAverageService>> _mockLogger = null!;
        private ResetAverageService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockRepo = new Mock<ILocationRepository>();
            _mockLogger = new Mock<ILogger<ResetAverageService>>();
            _service = new ResetAverageService(_mockRepo.Object, _mockLogger.Object);
        }

        [TestMethod]
        public async Task ExecuteAsync_OldAveragesAreReset()
        {
            // Arrange
            var oldDate = DateTime.UtcNow.AddMonths(-4);
            var loc = CreateLocationWithLastReset(oldDate);
            var locations = new List<Location> { loc } as IReadOnlyList<Location>;

            _mockRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(locations);
            _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Location>(), It.IsAny<CancellationToken>())).ReturnsAsync(loc);

            var request = new ResetAverageRequest();

            // Act
            var result = await _service.ExecuteAsync(request, "system");

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.ResetCount);
            Assert.IsTrue(loc.LastAverageTimeReset > oldDate);
            _mockRepo.Verify(r => r.UpdateAsync(It.Is<Location>(l => l.Id == loc.Id), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task ExecuteAsync_RecentAverage_NoReset()
        {
            // Arrange
            var recentDate = DateTime.UtcNow.AddDays(-10);
            var loc = CreateLocationWithLastReset(recentDate);
            var locations = new List<Location> { loc } as IReadOnlyList<Location>;

            _mockRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(locations);

            var request = new ResetAverageRequest();

            // Act
            var result = await _service.ExecuteAsync(request, "system");

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, result.ResetCount);
            _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<Location>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        private static Location CreateLocationWithLastReset(DateTime lastReset)
        {
            var organizationId = Guid.NewGuid();
            var address = Address.Create("R. Teste", "", "", "", "City", "State", "Country", "12345");
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
                "seed"
            );
            // Directly set last reset via reflection since it's private set
            typeof(Location).GetProperty("LastAverageTimeReset")!.SetValue(location, lastReset);
            return location;
        }
    }
} 