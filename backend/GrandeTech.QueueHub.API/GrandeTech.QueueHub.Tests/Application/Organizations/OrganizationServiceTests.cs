using System;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Application.Organizations;
using Grande.Fila.API.Domain.Organizations;
using Grande.Fila.API.Domain.Subscriptions;
using Grande.Fila.API.Domain.AuditLogs;
using Grande.Fila.API.Domain.Common;
using Grande.Fila.API.Domain.Common.ValueObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Grande.Fila.Tests.Application.Organizations
{
    [TestClass]
    public class OrganizationServiceTests
    {
        private Mock<IOrganizationRepository> _organizationRepoMock = null!;
        private Mock<ISubscriptionPlanRepository> _subscriptionRepoMock = null!;
        private Mock<IAuditLogRepository> _auditLogRepoMock = null!;
        private OrganizationService _service = null!;        [TestInitialize]
        public void Setup()
        {
            _organizationRepoMock = new Mock<IOrganizationRepository>();
            _subscriptionRepoMock = new Mock<ISubscriptionPlanRepository>();
            _auditLogRepoMock = new Mock<IAuditLogRepository>();
            _service = new OrganizationService(
                _organizationRepoMock.Object,
                _auditLogRepoMock.Object);
        }

        [TestMethod]
        public async Task Should_UpdateOrganization_With_ValidData_ReturnsSuccess()
        {
            // Arrange
            var organizationId = Guid.NewGuid();
            var organization = CreateTestOrganization(organizationId);
            
            var request = new UpdateOrganizationRequest
            {
                OrganizationId = organizationId.ToString(),
                Name = "Updated Organization Name",
                Description = "Updated description",
                ContactEmail = "updated@testorg.com",
                ContactPhone = "+5511888888888",
                WebsiteUrl = "https://updated.com"
            };

            _organizationRepoMock.Setup(r => r.GetByIdAsync(organizationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(organization);
            _organizationRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Organization>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Organization org, CancellationToken _) => org);

            // Act
            var result = await _service.UpdateOrganizationAsync(request, "adminUserId", "Admin", CancellationToken.None);            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.Errors.Count == 0);
            Assert.IsTrue(result.FieldErrors.Count == 0);
        }

        [TestMethod]
        public async Task Should_UpdateBranding_With_ValidData_ReturnsSuccess()
        {
            // Arrange
            var organizationId = Guid.NewGuid();
            var organization = CreateTestOrganization(organizationId);
              var request = new UpdateBrandingRequest
            {
                OrganizationId = organizationId.ToString(),
                PrimaryColor = "#FF0000",
                SecondaryColor = "#00FF00",
                LogoUrl = "https://example.com/logo.png",
                FaviconUrl = "https://example.com/favicon.ico",
                TagLine = "Updated tagline"
            };

            _organizationRepoMock.Setup(r => r.GetByIdAsync(organizationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(organization);
            _organizationRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Organization>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Organization org, CancellationToken _) => org);

            // Act
            var result = await _service.UpdateBrandingAsync(request, "adminUserId", "Admin", CancellationToken.None);            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.Errors.Count == 0);
            Assert.IsTrue(result.FieldErrors.Count == 0);
        }

        [TestMethod]
        public async Task Should_ChangeSubscriptionPlan_With_ValidData_ReturnsSuccess()
        {
            // Arrange
            var organizationId = Guid.NewGuid();
            var subscriptionPlanId = Guid.NewGuid();
            var organization = CreateTestOrganization(organizationId);
            var subscriptionPlan = new SubscriptionPlan("Premium Plan", "Premium subscription", 59.99m, 59.99m, 2, 10, true, false, false, false, false, 200, false, "system");

            _organizationRepoMock.Setup(r => r.GetByIdAsync(organizationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(organization);
            _subscriptionRepoMock.Setup(r => r.GetByIdAsync(subscriptionPlanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptionPlan);
            _organizationRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Organization>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Organization org, CancellationToken _) => org);

            // Act
            var result = await _service.ChangeSubscriptionPlanAsync(
                organizationId.ToString(), 
                subscriptionPlanId.ToString(), 
                "adminUserId", 
                "Admin", 
                CancellationToken.None);            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.Errors.Count == 0);
            Assert.IsTrue(result.FieldErrors.Count == 0);
        }

        [TestMethod]
        public async Task Should_SetAnalyticsSharing_With_ValidData_ReturnsSuccess()
        {
            // Arrange
            var organizationId = Guid.NewGuid();
            var organization = CreateTestOrganization(organizationId);

            _organizationRepoMock.Setup(r => r.GetByIdAsync(organizationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(organization);
            _organizationRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Organization>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Organization org, CancellationToken _) => org);

            // Act
            var result = await _service.SetAnalyticsSharingAsync(
                organizationId.ToString(), 
                true, 
                "adminUserId", 
                "Admin", 
                CancellationToken.None);            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.Errors.Count == 0);
            Assert.IsTrue(result.FieldErrors.Count == 0);
        }

        [TestMethod]
        public async Task Should_ActivateOrganization_With_ValidData_ReturnsSuccess()
        {            // Arrange
            var organizationId = Guid.NewGuid();
            var organization = CreateTestOrganization(organizationId);
            organization.Deactivate("testUser"); // Start with deactivated organization

            _organizationRepoMock.Setup(r => r.GetByIdAsync(organizationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(organization);
            _organizationRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Organization>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Organization org, CancellationToken _) => org);

            // Act
            var result = await _service.ActivateOrganizationAsync(
                organizationId.ToString(), 
                "adminUserId", 
                "Admin", 
                CancellationToken.None);            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.Errors.Count == 0);
            Assert.IsTrue(result.FieldErrors.Count == 0);
        }

        [TestMethod]
        public async Task Should_DeactivateOrganization_With_ValidData_ReturnsSuccess()
        {
            // Arrange
            var organizationId = Guid.NewGuid();
            var organization = CreateTestOrganization(organizationId);

            _organizationRepoMock.Setup(r => r.GetByIdAsync(organizationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(organization);
            _organizationRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Organization>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Organization org, CancellationToken _) => org);

            // Act
            var result = await _service.DeactivateOrganizationAsync(
                organizationId.ToString(), 
                "adminUserId", 
                "Admin", 
                CancellationToken.None);            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.Errors.Count == 0);
            Assert.IsTrue(result.FieldErrors.Count == 0);
        }

        [TestMethod]
        public async Task Should_FailOperation_When_OrganizationNotFound()
        {
            // Arrange
            var organizationId = Guid.NewGuid();
            var request = new UpdateOrganizationRequest
            {
                OrganizationId = organizationId.ToString(),
                Name = "Updated Name"
            };

            _organizationRepoMock.Setup(r => r.GetByIdAsync(organizationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Organization?)null);

            // Act
            var result = await _service.UpdateOrganizationAsync(request, "adminUserId", "Admin", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("Organization not found."));
        }

        [TestMethod]
        public async Task Should_FailOperation_When_InvalidGuidFormat()
        {
            // Arrange
            var request = new UpdateOrganizationRequest
            {
                OrganizationId = "invalid-guid",
                Name = "Updated Name"
            };

            // Act
            var result = await _service.UpdateOrganizationAsync(request, "adminUserId", "Admin", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("OrganizationId"));
            Assert.AreEqual("Invalid organization ID format.", result.FieldErrors["OrganizationId"]);
        }        private Organization CreateTestOrganization(Guid id)
        {
            var brandingConfig = BrandingConfig.Create(
                "#FF5722", "#FFC107", "", "", "Test Company", "Testing Excellence");
            
            var organization = new Organization(
                "Test Organization",
                "test-org",
                "Test description",
                "test@example.com",
                "+5511999999999",
                "https://test.com",
                brandingConfig,
                Guid.NewGuid(),
                "testUser");

            // Use reflection to set the Id since it's likely readonly
            var idProperty = typeof(Organization).GetProperty("Id");
            if (idProperty?.CanWrite == true)
            {
                idProperty.SetValue(organization, id);
            }

            return organization;
        }
    }
}
