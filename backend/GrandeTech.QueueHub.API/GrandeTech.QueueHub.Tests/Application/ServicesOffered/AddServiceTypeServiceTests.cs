using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Domain.Locations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using GrandeTech.QueueHub.API.Application.ServicesOffered;
using GrandeTech.QueueHub.API.Domain.ServicesOffered;

namespace GrandeTech.QueueHub.Tests.Application
{
    [TestClass]
    public class AddServiceTypeServiceTests
    {
        private Mock<IServicesOfferedRepository> _serviceTypeRepoMock;
        private Mock<ILocationRepository> _locationRepoMock;
        private AddServiceOfferedService _service;

        [TestInitialize]
        public void Setup()
        {
            _serviceTypeRepoMock = new Mock<IServicesOfferedRepository>();
            _locationRepoMock = new Mock<ILocationRepository>();
            _service = new AddServiceOfferedService(_serviceTypeRepoMock.Object, _locationRepoMock.Object);
        }

        [TestMethod]
        public async Task AddServiceTypeAsync_WithValidRequest_ReturnsSuccess()
        {
            // Arrange
            var request = new AddServiceOfferedRequest
            {
                LocationId = Guid.NewGuid().ToString(),
                Name = "Corte Masculino",
                Description = "Corte de cabelo masculino tradicional",
                EstimatedDurationMinutes = 30,
                Price = 25.00m
            };

            _locationRepoMock.Setup(x => x.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Location, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _serviceTypeRepoMock.Setup(x => x.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ServiceOffered, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _service.AddServiceTypeAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.ServiceTypeId);
            Assert.AreEqual(0, result.FieldErrors.Count);
            _serviceTypeRepoMock.Verify(x => x.AddAsync(It.IsAny<ServiceOffered>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task AddServiceTypeAsync_WithInvalidLocationId_ReturnsFieldError()
        {
            // Arrange
            var request = new AddServiceOfferedRequest
            {
                LocationId = "invalid-guid",
                Name = "Corte Masculino",
                EstimatedDurationMinutes = 30,
                Price = 25.00m
            };

            // Act
            var result = await _service.AddServiceTypeAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("LocationId"));
            Assert.AreEqual("Invalid location ID format.", result.FieldErrors["LocationId"]);
        }

        [TestMethod]
        public async Task AddServiceTypeAsync_WithNonExistentLocation_ReturnsFieldError()
        {
            // Arrange
            var request = new AddServiceOfferedRequest
            {
                LocationId = Guid.NewGuid().ToString(),
                Name = "Corte Masculino",
                EstimatedDurationMinutes = 30,
                Price = 25.00m
            };

            _locationRepoMock.Setup(x => x.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Location, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _service.AddServiceTypeAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("LocationId"));
            Assert.AreEqual("Location not found.", result.FieldErrors["LocationId"]);
        }

        [TestMethod]
        public async Task AddServiceTypeAsync_WithDuplicateServiceName_ReturnsFieldError()
        {
            // Arrange
            var request = new AddServiceOfferedRequest
            {
                LocationId = Guid.NewGuid().ToString(),
                Name = "Corte Masculino",
                EstimatedDurationMinutes = 30,
                Price = 25.00m
            };

            _locationRepoMock.Setup(x => x.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Location, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _serviceTypeRepoMock.Setup(x => x.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ServiceOffered, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.AddServiceTypeAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("Name"));
            Assert.AreEqual("A service with this name already exists at this location.", result.FieldErrors["Name"]);
        }

        [TestMethod]
        public async Task AddServiceTypeAsync_WithEmptyName_ReturnsFieldError()
        {
            // Arrange
            var request = new AddServiceOfferedRequest
            {
                LocationId = Guid.NewGuid().ToString(),
                Name = "",
                EstimatedDurationMinutes = 30,
                Price = 25.00m
            };

            // Act
            var result = await _service.AddServiceTypeAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("Name"));
            Assert.AreEqual("Service name is required.", result.FieldErrors["Name"]);
        }

        [TestMethod]
        public async Task AddServiceTypeAsync_WithInvalidDuration_ReturnsFieldError()
        {
            // Arrange
            var request = new AddServiceOfferedRequest
            {
                LocationId = Guid.NewGuid().ToString(),
                Name = "Corte Masculino",
                EstimatedDurationMinutes = 0,
                Price = 25.00m
            };

            // Act
            var result = await _service.AddServiceTypeAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("EstimatedDurationMinutes"));
            Assert.AreEqual("Duration must be greater than 0 minutes.", result.FieldErrors["EstimatedDurationMinutes"]);
        }

        [TestMethod]
        public async Task AddServiceTypeAsync_WithNegativePrice_ReturnsFieldError()
        {
            // Arrange
            var request = new AddServiceOfferedRequest
            {
                LocationId = Guid.NewGuid().ToString(),
                Name = "Corte Masculino",
                EstimatedDurationMinutes = 30,
                Price = -5.00m
            };

            // Act
            var result = await _service.AddServiceTypeAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("Price"));
            Assert.AreEqual("Price cannot be negative.", result.FieldErrors["Price"]);
        }
    }
}