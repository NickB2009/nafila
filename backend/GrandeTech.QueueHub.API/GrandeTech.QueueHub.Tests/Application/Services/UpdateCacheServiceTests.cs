using Grande.Fila.API.Application.Services;
using Grande.Fila.API.Application.Services.Cache;
using Grande.Fila.API.Domain.Locations;
using Microsoft.Extensions.Logging;
using Moq;

namespace Grande.Fila.API.Tests.Application.Services
{
    [TestClass]
    public class UpdateCacheServiceTests
    {
        private Mock<ILocationRepository> _mockLocationRepo = null!;
        private Mock<IAverageWaitTimeCache> _mockCache = null!;
        private Mock<ILogger<UpdateCacheService>> _mockLogger = null!;
        private UpdateCacheService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockLocationRepo = new Mock<ILocationRepository>();
            _mockCache = new Mock<IAverageWaitTimeCache>();
            _mockLogger = new Mock<ILogger<UpdateCacheService>>();
            _service = new UpdateCacheService(_mockLocationRepo.Object, _mockCache.Object, _mockLogger.Object);
        }

        [TestMethod]
        public async Task ExecuteAsync_ValidRequest_UpdatesCache()
        {
            // Arrange
            var locId = Guid.NewGuid();
            _mockLocationRepo.Setup(r => r.ExistsAsync(l => l.Id == locId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var request = new UpdateCacheRequest
            {
                LocationId = locId.ToString(),
                AverageServiceTimeMinutes = 12.5
            };

            // Act
            var result = await _service.ExecuteAsync(request, "system");

            // Assert
            Assert.IsTrue(result.Success);
            _mockCache.Verify(c => c.SetAverage(locId, 12.5), Times.Once);
        }

        [TestMethod]
        public async Task ExecuteAsync_InvalidLocationId_ReturnsError()
        {
            // Arrange
            var request = new UpdateCacheRequest
            {
                LocationId = "not-a-guid",
                AverageServiceTimeMinutes = 10
            };

            // Act
            var result = await _service.ExecuteAsync(request, "system");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("LocationId"));
        }
    }
}