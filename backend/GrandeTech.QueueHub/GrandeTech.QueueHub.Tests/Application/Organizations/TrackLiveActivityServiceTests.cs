using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Application.Organizations;
using Grande.Fila.API.Domain.Organizations;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.Staff;
using Grande.Fila.API.Domain.Locations;
using Grande.Fila.API.Domain.Common.ValueObjects;
using Grande.Fila.API.Domain.Users;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Grande.Fila.Tests.Application.Organizations
{
    [TestClass]
    public class TrackLiveActivityServiceTests
    {
        private Mock<IOrganizationRepository> _mockOrganizationRepository = null!;
        private Mock<IQueueRepository> _mockQueueRepository = null!;
        private Mock<IStaffMemberRepository> _mockStaffRepository = null!;
        private Mock<ILocationRepository> _mockLocationRepository = null!;
        private TrackLiveActivityService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockOrganizationRepository = new Mock<IOrganizationRepository>();
            _mockQueueRepository = new Mock<IQueueRepository>();
            _mockStaffRepository = new Mock<IStaffMemberRepository>();
            _mockLocationRepository = new Mock<ILocationRepository>();
            
            _service = new TrackLiveActivityService(
                _mockOrganizationRepository.Object,
                _mockQueueRepository.Object,
                _mockStaffRepository.Object,
                _mockLocationRepository.Object);
        }

        [TestMethod]
        public async Task GetLiveActivityAsync_ValidOrganizationId_ReturnsSuccess()
        {
            // Arrange
            var organizationId = Guid.NewGuid();
            var locationId = Guid.NewGuid();
            var queueId = Guid.NewGuid();
            var staffId = Guid.NewGuid();

            var organization = CreateTestOrganization(organizationId);
            var location = CreateTestLocation(locationId, organizationId);
            var queue = CreateTestQueue(queueId, locationId);
            var staffMember = CreateTestStaffMember(staffId, locationId);

            // Set the location ID using reflection  
            var locationIdProperty = typeof(Location).GetProperty("Id");
            locationIdProperty?.SetValue(location, locationId);

            var request = new TrackLiveActivityRequest
            {
                OrganizationId = organizationId.ToString()
            };

            _mockOrganizationRepository.Setup(r => r.GetByIdAsync(organizationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(organization);
            _mockLocationRepository.Setup(r => r.GetByOrganizationAsync(organizationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Location> { location });
            _mockQueueRepository.Setup(r => r.GetAllByLocationIdAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Queue> { queue });
            _mockStaffRepository.Setup(r => r.GetActiveStaffMembersAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StaffMember> { staffMember });

            // Act
            var result = await _service.GetLiveActivityAsync(request, "admin-user", UserRoles.Owner, CancellationToken.None);

            // Assert
            if (!result.Success)
            {
                var errors = string.Join(", ", result.Errors);
                var fieldErrors = string.Join(", ", result.FieldErrors.Select(kv => $"{kv.Key}: {kv.Value}"));
                Assert.Fail($"Test failed. Errors: [{errors}], Field Errors: [{fieldErrors}]");
            }
            
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.LiveActivity);
            Assert.AreEqual(1, result.LiveActivity.Locations.Count);
            Assert.AreEqual(locationId.ToString(), result.LiveActivity.Locations[0].LocationId);
        }

        [TestMethod]
        public async Task GetLiveActivityAsync_InvalidOrganizationId_ReturnsValidationError()
        {
            // Arrange
            var request = new TrackLiveActivityRequest
            {
                OrganizationId = "invalid-guid"
            };

            // Act
            var result = await _service.GetLiveActivityAsync(request, "admin-user", UserRoles.Owner, CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("OrganizationId"));
            Assert.AreEqual("Invalid organization ID format.", result.FieldErrors["OrganizationId"]);
        }

        [TestMethod]
        public async Task GetLiveActivityAsync_NonAdminUser_ReturnsForbidden()
        {
            // Arrange
            var request = new TrackLiveActivityRequest
            {
                OrganizationId = Guid.NewGuid().ToString()
            };

            // Act
            var result = await _service.GetLiveActivityAsync(request, "user", UserRoles.Customer, CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Any(e => e.Contains("Forbidden")));
        }

        [TestMethod]
        public async Task GetLiveActivityAsync_OrganizationNotFound_ReturnsError()
        {
            // Arrange
            var organizationId = Guid.NewGuid();
            var request = new TrackLiveActivityRequest
            {
                OrganizationId = organizationId.ToString()
            };

            _mockOrganizationRepository.Setup(r => r.GetByIdAsync(organizationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Organization?)null);

            // Act
            var result = await _service.GetLiveActivityAsync(request, "admin-user", UserRoles.Owner, CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Any(e => e.Contains("Organization not found")));
        }

        [TestMethod]
        public async Task GetLiveActivityAsync_CalculatesCorrectMetrics()
        {
            // Arrange
            var organizationId = Guid.NewGuid();
            var locationId = Guid.NewGuid();
            var queueId = Guid.NewGuid();

            var organization = CreateTestOrganization(organizationId);
            var location = CreateTestLocation(locationId, organizationId);
            var queue = CreateTestQueueWithEntries(queueId, locationId);
            var activeStaff = CreateTestStaffMember(Guid.NewGuid(), locationId);
            var busyStaff = CreateTestStaffMember(Guid.NewGuid(), locationId);
            busyStaff.UpdateStatus("busy", "test");
            
            // Set the location ID using reflection  
            var locationIdProperty = typeof(Location).GetProperty("Id");
            locationIdProperty?.SetValue(location, locationId);

            var request = new TrackLiveActivityRequest
            {
                OrganizationId = organizationId.ToString()
            };

            _mockOrganizationRepository.Setup(r => r.GetByIdAsync(organizationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(organization);
            _mockLocationRepository.Setup(r => r.GetByOrganizationAsync(organizationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Location> { location });
            _mockQueueRepository.Setup(r => r.GetAllByLocationIdAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Queue> { queue });
            _mockStaffRepository.Setup(r => r.GetActiveStaffMembersAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StaffMember> { activeStaff, busyStaff });

            // Act
            var result = await _service.GetLiveActivityAsync(request, "admin-user", UserRoles.Owner, CancellationToken.None);

            // Assert
            if (!result.Success)
            {
                var errors = string.Join(", ", result.Errors);
                var fieldErrors = string.Join(", ", result.FieldErrors.Select(kv => $"{kv.Key}: {kv.Value}"));
                Assert.Fail($"Test failed. Errors: [{errors}], Field Errors: [{fieldErrors}]");
            }
            
            Assert.IsTrue(result.Success);
            var locationActivity = result.LiveActivity!.Locations[0];
            Assert.AreEqual(2, locationActivity.TotalCustomersWaiting);
            Assert.AreEqual(2, locationActivity.TotalStaffMembers);
            Assert.AreEqual(1, locationActivity.AvailableStaffMembers);
            Assert.AreEqual(1, locationActivity.BusyStaffMembers);
        }

        private Organization CreateTestOrganization(Guid id)
        {
            var organization = new Organization(
                "Test Organization",
                "test-org",
                "Test description",
                "test@example.com",
                "+5511999999999",
                "https://test.com",
                null,
                Guid.NewGuid(), // subscription plan ID
                "testUser");

            // Set the organization ID using reflection
            var idProperty = typeof(Organization).GetProperty("Id");
            idProperty?.SetValue(organization, id);

            return organization;
        }

        private Location CreateTestLocation(Guid id, Guid organizationId)
        {
            var address = Address.Create("123 Main St", "100", "", "Downtown", "City", "State", "Country", "12345");
            
            return new Location(
                "Test Location",
                "test-location",
                "Test Description",
                organizationId,
                address,
                "+1234567890",
                "location@test.com",
                new TimeSpan(9, 0, 0),
                new TimeSpan(17, 0, 0),
                100,
                15,
                "testUser");
        }

        private Queue CreateTestQueue(Guid id, Guid locationId)
        {
            return new Queue(locationId, 50, 15, "testUser");
        }

        private Queue CreateTestQueueWithEntries(Guid id, Guid locationId)
        {
            var queue = CreateTestQueue(id, locationId);
            
            // Add some queue entries
            queue.AddCustomerToQueue(Guid.NewGuid(), "Customer 1", null, null, "testUser");
            queue.AddCustomerToQueue(Guid.NewGuid(), "Customer 2", null, null, "testUser");

            return queue;
        }

        private StaffMember CreateTestStaffMember(Guid id, Guid locationId)
        {
            return new StaffMember(
                "Test Staff",
                locationId,
                "staff@test.com",
                "+1234567890",
                null,
                "Barber",
                "teststaff",
                null,
                "testUser");
        }
    }
}