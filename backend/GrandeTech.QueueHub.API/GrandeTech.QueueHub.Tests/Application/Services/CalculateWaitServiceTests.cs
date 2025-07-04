using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Grande.Fila.API.Application.Services;
using Grande.Fila.API.Domain.Locations;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.Staff;
using Grande.Fila.API.Application.Services.Cache;
using Grande.Fila.API.Domain.Common.ValueObjects;

namespace Grande.Fila.API.Tests.Application.Services
{
    [TestClass]
    public class CalculateWaitServiceTests
    {
        private Mock<ILocationRepository> _mockLocationRepo = null!;
        private Mock<IQueueRepository> _mockQueueRepo = null!;
        private Mock<IStaffMemberRepository> _mockStaffRepo = null!;
        private Mock<IAverageWaitTimeCache> _mockCache = null!;
        private Mock<ILogger<CalculateWaitService>> _mockLogger = null!;
        private CalculateWaitService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockLocationRepo = new Mock<ILocationRepository>();
            _mockQueueRepo = new Mock<IQueueRepository>();
            _mockStaffRepo = new Mock<IStaffMemberRepository>();
            _mockCache = new Mock<IAverageWaitTimeCache>();
            _mockLogger = new Mock<ILogger<CalculateWaitService>>();
            _service = new CalculateWaitService(_mockLocationRepo.Object, _mockQueueRepo.Object, _mockStaffRepo.Object, _mockCache.Object, _mockLogger.Object);
        }

        [TestMethod]
        public async Task ExecuteAsync_ValidData_CalculatesAccurateWaitTime()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var queueId = Guid.NewGuid();
            var entryId = Guid.NewGuid();
            var organizationId = Guid.NewGuid();

            // Create test location
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
            
            // Set the location ID using reflection
            var locationIdProperty = typeof(Location).GetProperty("Id");
            locationIdProperty?.SetValue(location, locationId);

            // Create test queue with entries
            var queue = new Queue(locationId, 50, 15, "admin");
            
            // Set the queue ID using reflection
            var queueIdProperty = typeof(Queue).GetProperty("Id");
            queueIdProperty?.SetValue(queue, queueId);
            
            queue.AddCustomerToQueue(Guid.NewGuid(), "Customer 1", null, null, "admin");
            var queueEntry = queue.AddCustomerToQueue(Guid.NewGuid(), "Customer 2", null, null, "admin");

            // Create test staff member
            var staffMember = new StaffMember(
                "Test Staff",
                locationId,
                "staff@test.com",
                "+1234567890",
                null,
                "Barber",
                "teststaff",
                null,
                "admin");

            // Setup mocks
            _mockLocationRepo.Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(location);

            _mockQueueRepo.Setup(r => r.GetByIdAsync(queueId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(queue);

            _mockStaffRepo.Setup(r => r.GetByLocationAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StaffMember> { staffMember });

            _mockCache.Setup(c => c.TryGetAverage(locationId, out It.Ref<double>.IsAny))
                .Returns((Guid id, out double avg) => { avg = 25.0; return true; });

            var request = new CalculateWaitRequest
            {
                LocationId = locationId.ToString(),
                QueueId = queueId.ToString(),
                EntryId = queueEntry.Id.ToString()
            };

            // Act
            var result = await _service.ExecuteAsync(request, "system");

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.EstimatedWaitMinutes >= 0);
        }

        [TestMethod]
        public async Task ExecuteAsync_InvalidLocationId_ReturnsError()
        {
            // Arrange
            var request = new CalculateWaitRequest
            {
                LocationId = "invalid-guid",
                QueueId = Guid.NewGuid().ToString(),
                EntryId = Guid.NewGuid().ToString()
            };

            // Act
            var result = await _service.ExecuteAsync(request, "system");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("LocationId"));
        }
    }
} 