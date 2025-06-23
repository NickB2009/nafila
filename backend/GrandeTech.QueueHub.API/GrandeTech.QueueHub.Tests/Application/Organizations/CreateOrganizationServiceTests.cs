using System;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Application.Organizations;
using Grande.Fila.API.Domain.Organizations;
using Grande.Fila.API.Domain.Subscriptions;
using Grande.Fila.API.Domain.AuditLogs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Grande.Fila.Tests.Application.Organizations
{
    [TestClass]
    public class CreateOrganizationServiceTests
    {
        private Mock<IOrganizationRepository> _organizationRepoMock = null!;
        private Mock<ISubscriptionPlanRepository> _subscriptionRepoMock = null!;
        private Mock<IAuditLogRepository> _auditLogRepoMock = null!;
        private CreateOrganizationService _service = null!;        [TestInitialize]
        public void Setup()
        {
            _organizationRepoMock = new Mock<IOrganizationRepository>();
            _subscriptionRepoMock = new Mock<ISubscriptionPlanRepository>();
            _auditLogRepoMock = new Mock<IAuditLogRepository>();            _service = new CreateOrganizationService(
                _organizationRepoMock.Object, 
                _auditLogRepoMock.Object,
                _subscriptionRepoMock.Object);
        }        [TestMethod]
        public async Task Should_CreateOrganization_With_ValidData_ReturnsSuccess()
        {
            // Arrange
            var request = new CreateOrganizationRequest
            {
                Name = "Test Organization",
                Slug = "test-org",
                Description = "A test organization",
                ContactEmail = "contact@testorg.com",
                ContactPhone = "+5511999999999",
                WebsiteUrl = "https://testorg.com",
                SubscriptionPlanId = Guid.NewGuid().ToString(),
                PrimaryColor = "#FF5722",
                SecondaryColor = "#FFC107",
                TagLine = "Testing Excellence"
            };

            var subscriptionPlan = new SubscriptionPlan("Basic Plan", "Basic subscription", 29.99m, 29.99m, 1, 5, true, false, false, false, false, 100, false, "system");
            
            _organizationRepoMock.Setup(r => r.IsSlugUniqueAsync(request.Slug, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _subscriptionRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptionPlan);
            _organizationRepoMock.Setup(r => r.AddAsync(It.IsAny<Organization>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Organization org, CancellationToken _) => org);

            // Act
            var result = await _service.CreateOrganizationAsync(request, "adminUserId", "Admin", CancellationToken.None);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.OrganizationId);
            Assert.AreEqual(request.Slug, result.Slug);
            Assert.IsTrue(result.Errors.Count == 0);
            Assert.IsTrue(result.FieldErrors.Count == 0);
        }        [TestMethod]
        public async Task Should_FailCreation_When_SlugNotUnique()
        {
            // Arrange
            var request = new CreateOrganizationRequest
            {
                Name = "Test Organization",
                Slug = "existing-slug",
                Description = "A test organization",
                ContactEmail = "contact@testorg.com",
                SubscriptionPlanId = Guid.NewGuid().ToString()
            };

            var subscriptionPlan = new SubscriptionPlan("Basic Plan", "Basic subscription", 29.99m, 29.99m, 1, 5, true, false, false, false, false, 100, false, "system");
            
            _organizationRepoMock.Setup(r => r.IsSlugUniqueAsync(request.Slug, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _subscriptionRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptionPlan);

            // Act
            var result = await _service.CreateOrganizationAsync(request, "adminUserId", "Admin", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("Slug"));
            Assert.AreEqual("Slug 'existing-slug' is already taken.", result.FieldErrors["Slug"]);
        }

        [TestMethod]
        public async Task Should_FailCreation_When_SubscriptionPlanNotFound()
        {
            // Arrange
            var request = new CreateOrganizationRequest
            {
                Name = "Test Organization",
                Slug = "test-org",
                Description = "A test organization",
                ContactEmail = "contact@testorg.com",
                SubscriptionPlanId = Guid.NewGuid().ToString()
            };

            _organizationRepoMock.Setup(r => r.IsSlugUniqueAsync(request.Slug, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _subscriptionRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((SubscriptionPlan?)null);

            // Act
            var result = await _service.CreateOrganizationAsync(request, "adminUserId", "Admin", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("SubscriptionPlanId"));
            Assert.AreEqual("Subscription plan not found.", result.FieldErrors["SubscriptionPlanId"]);
        }

        [TestMethod]
        public async Task Should_FailCreation_When_RequiredFieldsEmpty()
        {
            // Arrange
            var request = new CreateOrganizationRequest
            {
                Name = "",
                Slug = "",
                Description = "",
                ContactEmail = "",
                SubscriptionPlanId = ""
            };

            // Act
            var result = await _service.CreateOrganizationAsync(request, "adminUserId", "Admin", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("Name"));
            Assert.IsTrue(result.FieldErrors.ContainsKey("Slug"));
            Assert.IsTrue(result.FieldErrors.ContainsKey("ContactEmail"));
            Assert.IsTrue(result.FieldErrors.ContainsKey("SubscriptionPlanId"));
        }        [TestMethod]
        public async Task Should_FailCreation_When_InvalidEmailFormat()
        {
            // Arrange
            var request = new CreateOrganizationRequest
            {
                Name = "Test Organization",
                Slug = "test-org",
                Description = "A test organization",
                ContactEmail = "invalid-email",
                SubscriptionPlanId = Guid.NewGuid().ToString()
            };

            var subscriptionPlan = new SubscriptionPlan("Basic Plan", "Basic subscription", 29.99m, 29.99m, 1, 5, true, false, false, false, false, 100, false, "system");
            
            _organizationRepoMock.Setup(r => r.IsSlugUniqueAsync(request.Slug, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _subscriptionRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptionPlan);

            // Act
            var result = await _service.CreateOrganizationAsync(request, "adminUserId", "Admin", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("ContactEmail"));
            Assert.AreEqual("Enter a valid email address.", result.FieldErrors["ContactEmail"]);
        }        [TestMethod]
        public async Task Should_FailCreation_When_InvalidPhoneFormat()
        {
            // Arrange
            var request = new CreateOrganizationRequest
            {
                Name = "Test Organization",
                Slug = "test-org",
                Description = "A test organization",
                ContactEmail = "contact@testorg.com",
                ContactPhone = "invalid-phone",
                SubscriptionPlanId = Guid.NewGuid().ToString()
            };

            var subscriptionPlan = new SubscriptionPlan("Basic Plan", "Basic subscription", 29.99m, 29.99m, 1, 5, true, false, false, false, false, 100, false, "system");
            
            _organizationRepoMock.Setup(r => r.IsSlugUniqueAsync(request.Slug, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _subscriptionRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptionPlan);

            // Act
            var result = await _service.CreateOrganizationAsync(request, "adminUserId", "Admin", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("ContactPhone"));
            Assert.AreEqual("Invalid phone number format. Use format: +5511999999999", result.FieldErrors["ContactPhone"]);
        }
    }
}
