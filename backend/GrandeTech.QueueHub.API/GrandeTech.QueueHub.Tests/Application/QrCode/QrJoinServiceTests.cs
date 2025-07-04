using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Grande.Fila.API.Application.QrCode;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.Locations;
using Grande.Fila.API.Domain.Common.ValueObjects;

namespace Grande.Fila.API.Tests.Application.QrCode
{
    [TestClass]
    public class QrJoinServiceTests
    {
        private Mock<IQueueRepository> _mockQueueRepo = null!;
        private Mock<ILocationRepository> _mockLocationRepo = null!;
        private Mock<IQrCodeGenerator> _mockQrGenerator = null!;
        private Mock<ILogger<QrJoinService>> _mockLogger = null!;
        private QrJoinService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockQueueRepo = new Mock<IQueueRepository>();
            _mockLocationRepo = new Mock<ILocationRepository>();
            _mockQrGenerator = new Mock<IQrCodeGenerator>();
            _mockLogger = new Mock<ILogger<QrJoinService>>();
            _service = new QrJoinService(_mockQueueRepo.Object, _mockLocationRepo.Object, _mockQrGenerator.Object, _mockLogger.Object);
        }

        [TestMethod]
        public async Task ExecuteAsync_ValidLocationId_GeneratesQrCode()
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

            var request = new QrJoinRequest
            {
                LocationId = locationId.ToString()
            };

            _mockLocationRepo.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Location, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _mockQrGenerator.Setup(g => g.GenerateQrCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("base64-qr-code");

            // Act
            var result = await _service.ExecuteAsync(request, "system");

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.QrCodeBase64);
            Assert.IsNotNull(result.JoinUrl);
        }

        [TestMethod]
        public async Task ExecuteAsync_InvalidLocationId_ReturnsError()
        {
            // Arrange
            var request = new QrJoinRequest
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