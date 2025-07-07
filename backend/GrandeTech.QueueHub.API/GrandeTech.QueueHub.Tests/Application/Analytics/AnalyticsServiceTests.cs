using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Grande.Fila.API.Application.Analytics;
using Grande.Fila.API.Domain.Organizations;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.AuditLogs;
using Grande.Fila.API.Domain.Locations;
using Grande.Fila.API.Domain.Staff;

namespace Grande.Fila.API.Tests.Application.Analytics
{
    [TestClass]
    public class AnalyticsServiceTests
    {
        private Mock<IOrganizationRepository> _organizationRepoMock = null!;
        private Mock<IQueueRepository> _queueRepoMock = null!;
        private Mock<ILocationRepository> _locationRepoMock = null!;
        private Mock<IStaffMemberRepository> _staffMemberRepoMock = null!;
        private Mock<IAuditLogRepository> _auditLogRepoMock = null!;
        private AnalyticsService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _organizationRepoMock = new Mock<IOrganizationRepository>();
            _queueRepoMock = new Mock<IQueueRepository>();
            _locationRepoMock = new Mock<ILocationRepository>();
            _staffMemberRepoMock = new Mock<IStaffMemberRepository>();
            _auditLogRepoMock = new Mock<IAuditLogRepository>();

            _service = new AnalyticsService(
                _organizationRepoMock.Object,
                _queueRepoMock.Object,
                _locationRepoMock.Object,
                _staffMemberRepoMock.Object,
                _auditLogRepoMock.Object);
        }

        [TestMethod]
        public async Task Should_GetCrossBarbershopAnalytics_With_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var request = new CrossBarbershopAnalyticsRequest
            {
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow,
                IncludeActiveQueues = true,
                IncludeCompletedServices = true,
                IncludeAverageWaitTimes = true,
                IncludeStaffUtilization = true
            };

            var organizations = CreateTestOrganizations();
            var queues = CreateTestQueues();
            var locations = CreateTestLocations();
            var staffMembers = CreateTestStaffMembers();

            _organizationRepoMock.Setup(r => r.GetOrganizationsWithAnalyticsSharingAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(organizations);
            _queueRepoMock.Setup(r => r.GetQueuesByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queues);
            _locationRepoMock.Setup(r => r.GetLocationsByOrganizationIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(locations);
            _staffMemberRepoMock.Setup(r => r.GetStaffMembersByLocationIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(staffMembers);

            // Act
            var result = await _service.GetCrossBarbershopAnalyticsAsync(request, "platformAdmin", "Admin", CancellationToken.None);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.AnalyticsData);
            Assert.IsTrue(result.AnalyticsData.TotalOrganizations > 0);
            Assert.IsTrue(result.AnalyticsData.TotalActiveQueues >= 0);
            Assert.IsTrue(result.AnalyticsData.TotalCompletedServices >= 0);
            Assert.IsTrue(result.AnalyticsData.AverageWaitTimeMinutes >= 0);
            Assert.IsTrue(result.AnalyticsData.StaffUtilizationPercentage >= 0);
            Assert.IsTrue(result.Errors.Count == 0);
        }

        [TestMethod]
        public async Task Should_FailGetCrossBarbershopAnalytics_When_NonPlatformAdmin()
        {
            // Arrange
            var request = new CrossBarbershopAnalyticsRequest
            {
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow
            };

            // Act
            var result = await _service.GetCrossBarbershopAnalyticsAsync(request, "regularUser", "Barber", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("Forbidden: Only platform administrators can access cross-barbershop analytics."));
        }

        [TestMethod]
        public async Task Should_GetOrganizationAnalytics_With_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var organizationId = Guid.NewGuid();
            var request = new OrganizationAnalyticsRequest
            {
                OrganizationId = organizationId.ToString(),
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow,
                IncludeQueueMetrics = true,
                IncludeServiceMetrics = true,
                IncludeStaffMetrics = true
            };

            var organization = CreateTestOrganization(organizationId);
            var queues = CreateTestQueues();
            var locations = CreateTestLocations();
            var staffMembers = CreateTestStaffMembers();

            _organizationRepoMock.Setup(r => r.GetByIdAsync(organizationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(organization);
            _queueRepoMock.Setup(r => r.GetQueuesByOrganizationIdAsync(organizationId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queues);
            _locationRepoMock.Setup(r => r.GetLocationsByOrganizationIdAsync(organizationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(locations);
            _staffMemberRepoMock.Setup(r => r.GetStaffMembersByOrganizationIdAsync(organizationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(staffMembers);

            // Act
            var result = await _service.GetOrganizationAnalyticsAsync(request, "admin", "Owner", CancellationToken.None);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.AnalyticsData);
            Assert.AreEqual(organizationId, result.AnalyticsData.OrganizationId);
            Assert.IsTrue(result.AnalyticsData.TotalLocations > 0);
            Assert.IsTrue(result.AnalyticsData.TotalStaffMembers > 0);
            Assert.IsTrue(result.Errors.Count == 0);
        }

        [TestMethod]
        public async Task Should_ValidateAnalyticsRequest_With_InvalidDateRange_ReturnsFail()
        {
            // Arrange
            var request = new CrossBarbershopAnalyticsRequest
            {
                StartDate = DateTime.UtcNow.AddDays(1), // Future date
                EndDate = DateTime.UtcNow.AddDays(-1)   // Past date
            };

            // Act
            var result = await _service.GetCrossBarbershopAnalyticsAsync(request, "platformAdmin", "Admin", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("StartDate"));
            Assert.IsTrue(result.FieldErrors.ContainsKey("EndDate"));
        }

        [TestMethod]
        public async Task Should_GetTopPerformingOrganizations_With_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var request = new TopPerformingOrganizationsRequest
            {
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow,
                MetricType = "CompletedServices",
                MaxResults = 10
            };

            var organizations = CreateTestOrganizations();
            var queues = CreateTestQueues();

            _organizationRepoMock.Setup(r => r.GetOrganizationsWithAnalyticsSharingAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(organizations);
            _queueRepoMock.Setup(r => r.GetQueuesByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queues);

            // Act
            var result = await _service.GetTopPerformingOrganizationsAsync(request, "platformAdmin", "Admin", CancellationToken.None);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.TopOrganizations);
            Assert.IsTrue(result.TopOrganizations.Count <= request.MaxResults);
            Assert.IsTrue(result.Errors.Count == 0);
        }

        private List<Organization> CreateTestOrganizations()
        {
            var org1 = new Organization("Test Barbershop 1", "test-barbershop-1", "Test Description 1", "test1@example.com", "+1234567890", "https://test1.com", null, Guid.NewGuid(), "test");
            var org2 = new Organization("Test Barbershop 2", "test-barbershop-2", "Test Description 2", "test2@example.com", "+1234567891", "https://test2.com", null, Guid.NewGuid(), "test");
            
            // Enable analytics sharing using reflection
            var analyticsProperty = typeof(Organization).GetProperty("SharesDataForAnalytics");
            analyticsProperty?.SetValue(org1, true);
            analyticsProperty?.SetValue(org2, true);

            return new List<Organization> { org1, org2 };
        }

        private Organization CreateTestOrganization(Guid id)
        {
            var org = new Organization("Test Barbershop", "test-barbershop", "Test Description", "test@example.com", "+1234567890", "https://test.com", null, Guid.NewGuid(), "test");
            
            // Set ID using reflection
            var idProperty = typeof(Grande.Fila.API.Domain.Common.BaseEntity).GetProperty("Id");
            idProperty?.SetValue(org, id);

            return org;
        }

        private List<Queue> CreateTestQueues()
        {
            var queue1 = new Queue(Guid.NewGuid(), 50, 30, "test");
            var queue2 = new Queue(Guid.NewGuid(), 40, 25, "test");
            
            return new List<Queue> { queue1, queue2 };
        }

        private List<Location> CreateTestLocations()
        {
            var address1 = Grande.Fila.API.Domain.Common.ValueObjects.Address.Create("123 Main St", "1", "", "Downtown", "City", "State", "Country", "12345");
            var address2 = Grande.Fila.API.Domain.Common.ValueObjects.Address.Create("456 Oak Ave", "2", "", "Uptown", "City", "State", "Country", "12346");
            
            var location1 = new Location("Test Location 1", "test-location-1", "Description 1", Guid.NewGuid(), address1, 
                "+1234567890", "test1@example.com", TimeSpan.FromHours(9), TimeSpan.FromHours(17), 50, 30, "test");
            var location2 = new Location("Test Location 2", "test-location-2", "Description 2", Guid.NewGuid(), address2, 
                "+1234567891", "test2@example.com", TimeSpan.FromHours(9), TimeSpan.FromHours(17), 40, 25, "test");
            
            return new List<Location> { location1, location2 };
        }

        private List<StaffMember> CreateTestStaffMembers()
        {
            var staff1 = new StaffMember("John Doe", Guid.NewGuid(), "john.doe@example.com", "+1234567890", 
                null, "Barber", "john.doe", null, "test");
            var staff2 = new StaffMember("Jane Smith", Guid.NewGuid(), "jane.smith@example.com", "+1234567891", 
                null, "Barber", "jane.smith", null, "test");
            
            return new List<StaffMember> { staff1, staff2 };
        }
    }
} 