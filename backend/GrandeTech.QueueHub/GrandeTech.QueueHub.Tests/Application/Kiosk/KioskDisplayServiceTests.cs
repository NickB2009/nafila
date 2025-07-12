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
using Grande.Fila.API.Domain.Staff;
using Grande.Fila.API.Application.Services.Cache;

namespace Grande.Fila.API.Tests.Application.Kiosk
{
    [TestClass]
    public class KioskDisplayServiceTests
    {
        private Mock<IQueueRepository> _mockQueueRepo = null!;
        private Mock<ILocationRepository> _mockLocationRepo = null!;
        private Mock<IStaffMemberRepository> _mockStaffRepo = null!;
        private Mock<IAverageWaitTimeCache> _mockCache = null!;
        private Mock<ILogger<KioskDisplayService>> _mockLogger = null!;
        private KioskDisplayService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockQueueRepo = new Mock<IQueueRepository>();
            _mockLocationRepo = new Mock<ILocationRepository>();
            _mockStaffRepo = new Mock<IStaffMemberRepository>();
            _mockCache = new Mock<IAverageWaitTimeCache>();
            _mockLogger = new Mock<ILogger<KioskDisplayService>>();
            _service = new KioskDisplayService(
                _mockQueueRepo.Object, 
                _mockLocationRepo.Object, 
                _mockStaffRepo.Object,
                _mockCache.Object,
                _mockLogger.Object);
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

            var location = CreateTestLocation(locationId);
            _mockLocationRepo.Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>())).ReturnsAsync(location);
            _mockQueueRepo.Setup(r => r.GetByLocationAsync(locationId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<Queue>());
            _mockStaffRepo.Setup(r => r.GetByLocationAsync(locationId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<StaffMember>());

            // Act
            var result = await _service.ExecuteAsync(request, "system");

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.QueueEntries);
        }

        [TestMethod]
        public async Task ExecuteAsync_WithQueueEntries_CalculatesWaitTimeConsistently()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var queueId = Guid.NewGuid();
            var request = new KioskDisplayRequest
            {
                LocationId = locationId.ToString()
            };

            var location = CreateTestLocation(locationId, averageServiceTime: 30.0);
            var queue = CreateTestQueue(queueId, locationId);
            var staffMember = CreateTestStaffMember(locationId);

            // Add queue entries using the proper domain method
            var entry1 = queue.AddCustomerToQueue(Guid.NewGuid(), "Customer 1");
            var entry2 = queue.AddCustomerToQueue(Guid.NewGuid(), "Customer 2");
            var entry3 = queue.AddCustomerToQueue(Guid.NewGuid(), "Customer 3");

            _mockLocationRepo.Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>())).ReturnsAsync(location);
            _mockQueueRepo.Setup(r => r.GetByLocationAsync(locationId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<Queue> { queue });
            _mockStaffRepo.Setup(r => r.GetByLocationAsync(locationId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<StaffMember> { staffMember });

            // Setup cache to return specific average time
            _mockCache.Setup(c => c.TryGetAverage(locationId, out It.Ref<double>.IsAny))
                .Returns((Guid id, out double avg) => { avg = 25.0; return true; });

            // Act
            var result = await _service.ExecuteAsync(request, "system");

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(3, result.QueueEntries.Count);
            
            // First customer (position 1) should have 0 wait time (no one ahead)
            var firstEntry = result.QueueEntries.Find(e => e.Position == 1);
            Assert.IsNotNull(firstEntry);
            Assert.AreEqual(0, firstEntry.EstimatedWaitMinutes);
            Assert.AreEqual("0 min", firstEntry.EstimatedWaitTime);

            // Second customer (position 2) should have 25 minutes wait time (1 person ahead, 1 staff, 25 min avg)
            var secondEntry = result.QueueEntries.Find(e => e.Position == 2);
            Assert.IsNotNull(secondEntry);
            Assert.AreEqual(25, secondEntry.EstimatedWaitMinutes);
            Assert.AreEqual("25 min", secondEntry.EstimatedWaitTime);

            // Third customer (position 3) should have 50 minutes wait time (2 people ahead, 1 staff, 25 min avg)
            var thirdEntry = result.QueueEntries.Find(e => e.Position == 3);
            Assert.IsNotNull(thirdEntry);
            Assert.AreEqual(50, thirdEntry.EstimatedWaitMinutes);
            Assert.AreEqual("50 min", thirdEntry.EstimatedWaitTime);
        }

        [TestMethod]
        public async Task ExecuteAsync_NoActiveStaff_ReturnsMinusOneForWaitTime()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var queueId = Guid.NewGuid();
            var request = new KioskDisplayRequest
            {
                LocationId = locationId.ToString()
            };

            var location = CreateTestLocation(locationId);
            var queue = CreateTestQueue(queueId, locationId);
            var entry = queue.AddCustomerToQueue(Guid.NewGuid(), "Customer");

            _mockLocationRepo.Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>())).ReturnsAsync(location);
            _mockQueueRepo.Setup(r => r.GetByLocationAsync(locationId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<Queue> { queue });
            _mockStaffRepo.Setup(r => r.GetByLocationAsync(locationId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<StaffMember>()); // No staff

            // Act
            var result = await _service.ExecuteAsync(request, "system");

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.QueueEntries.Count);
            
            var queueEntry = result.QueueEntries[0];
            Assert.AreEqual(-1, queueEntry.EstimatedWaitMinutes);
            Assert.AreEqual("N/A", queueEntry.EstimatedWaitTime);
        }

        [TestMethod]
        public async Task ExecuteAsync_LongWaitTime_FormatsAsHours()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var queueId = Guid.NewGuid();
            var request = new KioskDisplayRequest
            {
                LocationId = locationId.ToString()
            };

            var location = CreateTestLocation(locationId, averageServiceTime: 60.0);
            var queue = CreateTestQueue(queueId, locationId);
            var staffMember = CreateTestStaffMember(locationId);

            // Create entries to simulate long wait
            for (int i = 1; i <= 5; i++)
            {
                queue.AddCustomerToQueue(Guid.NewGuid(), $"Customer {i}");
            }

            _mockLocationRepo.Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>())).ReturnsAsync(location);
            _mockQueueRepo.Setup(r => r.GetByLocationAsync(locationId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<Queue> { queue });
            _mockStaffRepo.Setup(r => r.GetByLocationAsync(locationId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<StaffMember> { staffMember });

            // Act
            var result = await _service.ExecuteAsync(request, "system");

            // Assert
            Assert.IsTrue(result.Success);
            
            // Last customer should have 4 * 60 = 240 minutes = 4 hours wait
            var lastEntry = result.QueueEntries.Find(e => e.Position == 5);
            Assert.IsNotNull(lastEntry);
            Assert.AreEqual(240, lastEntry.EstimatedWaitMinutes);
            Assert.AreEqual("4h", lastEntry.EstimatedWaitTime);
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

        [TestMethod]
        public async Task ExecuteAsync_LocationNotFound_ReturnsError()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var request = new KioskDisplayRequest
            {
                LocationId = locationId.ToString()
            };

            _mockLocationRepo.Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>())).ReturnsAsync((Location?)null);

            // Act
            var result = await _service.ExecuteAsync(request, "system");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("Location not found"));
        }

        private Location CreateTestLocation(Guid locationId, double averageServiceTime = 30.0)
        {
            var address = Grande.Fila.API.Domain.Common.ValueObjects.Address.Create(
                "123 Test St",
                "1",
                "",
                "Test Neighborhood",
                "Test City",
                "Test State",
                "Test Country",
                "12345"
            );

            var location = new Location(
                "Test Location",
                "test-location",
                "Test Description",
                Guid.NewGuid(), // organizationId
                address,
                "+1234567890", // contactPhone
                "test@example.com", // contactEmail
                TimeSpan.FromHours(9), // openingTime
                TimeSpan.FromHours(17), // closingTime
                50, // maxQueueSize
                15, // lateClientCapTimeInMinutes
                "test-system" // createdBy
            );
            
            // Set the Id using reflection since it's private
            location.GetType().GetProperty("Id")?.SetValue(location, locationId);
            
            // Update the average service time
            location.UpdateAverageServiceTime(averageServiceTime, "test-system");
            
            return location;
        }

        private Queue CreateTestQueue(Guid queueId, Guid locationId)
        {
            var queue = new Queue(locationId, 50, 15, "test-system");
            
            // Set the Id using reflection since it's private
            queue.GetType().GetProperty("Id")?.SetValue(queue, queueId);
            
            return queue;
        }

        private StaffMember CreateTestStaffMember(Guid locationId)
        {
            var staffMember = new StaffMember(
                "Test Staff",
                locationId,
                "test@example.com",
                "+1234567890",
                null, // profilePictureUrl
                "Barber",
                "teststaff",
                null, // userId
                "test-system" // createdBy
            );
            
            // Set as active (it should be active by default, but let's make sure)
            // IsActive should be true by default from the constructor
            
            return staffMember;
        }
    }
} 