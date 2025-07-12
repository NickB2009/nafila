using System;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Application.SubscriptionPlans;
using Grande.Fila.API.Domain.Subscriptions;
using Grande.Fila.API.Domain.AuditLogs;
using Grande.Fila.API.Domain.Users;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Grande.Fila.Tests.Application.SubscriptionPlans
{
    [TestClass]
    public class CreateSubscriptionPlanServiceTests
    {
        private Mock<ISubscriptionPlanRepository> _subscriptionPlanRepoMock = null!;
        private Mock<IAuditLogRepository> _auditLogRepoMock = null!;
        private CreateSubscriptionPlanService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _subscriptionPlanRepoMock = new Mock<ISubscriptionPlanRepository>();
            _auditLogRepoMock = new Mock<IAuditLogRepository>();
            
            _service = new CreateSubscriptionPlanService(
                _subscriptionPlanRepoMock.Object, 
                _auditLogRepoMock.Object);
        }

        [TestMethod]
        public async Task Should_CreateSubscriptionPlan_With_ValidData_ReturnsSuccess()
        {
            // Arrange
            var request = new CreateSubscriptionPlanRequest
            {
                Name = "Premium Plan",
                Description = "A premium subscription plan",
                MonthlyPrice = 29.99m,
                YearlyPrice = 299.99m,
                MaxLocations = 5,
                MaxStaffPerLocation = 20,
                IncludesAnalytics = true,
                IncludesAdvancedReporting = true,
                IncludesCustomBranding = true,
                IncludesAdvertising = false,
                IncludesMultipleLocations = true,
                MaxQueueEntriesPerDay = 500,
                IsFeatured = true,
                IsDefault = false
            };

            _subscriptionPlanRepoMock.Setup(r => r.GetByNameAsync(request.Name, It.IsAny<CancellationToken>()))
                .ReturnsAsync((SubscriptionPlan?)null);
            _subscriptionPlanRepoMock.Setup(r => r.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((SubscriptionPlan plan, CancellationToken _) => plan);

            // Act
            var result = await _service.CreateSubscriptionPlanAsync(request, "adminUserId", UserRoles.PlatformAdmin, CancellationToken.None);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.SubscriptionPlanId);
            Assert.AreEqual(request.Name, result.Name);
            Assert.IsTrue(result.Errors.Count == 0);
            Assert.IsTrue(result.FieldErrors.Count == 0);
        }

        [TestMethod]
        public async Task Should_FailCreation_When_NameAlreadyExists()
        {
            // Arrange
            var request = new CreateSubscriptionPlanRequest
            {
                Name = "Existing Plan",
                Description = "A test plan",
                MonthlyPrice = 29.99m,
                YearlyPrice = 299.99m,
                MaxLocations = 1,
                MaxStaffPerLocation = 5,
                MaxQueueEntriesPerDay = 100
            };

            var existingPlan = new SubscriptionPlan("Existing Plan", "Description", 29.99m, 299.99m, 1, 5, false, false, false, false, false, 100, false, "system");
            
            _subscriptionPlanRepoMock.Setup(r => r.GetByNameAsync(request.Name, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingPlan);

            // Act
            var result = await _service.CreateSubscriptionPlanAsync(request, "adminUserId", UserRoles.PlatformAdmin, CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("Name"));
            Assert.AreEqual("Subscription plan name already exists.", result.FieldErrors["Name"]);
        }

        [TestMethod]
        public async Task Should_FailCreation_When_RequiredFieldsEmpty()
        {
            // Arrange
            var request = new CreateSubscriptionPlanRequest
            {
                Name = "",
                Description = "",
                MonthlyPrice = 0,
                YearlyPrice = 0,
                MaxLocations = 0,
                MaxStaffPerLocation = 0,
                MaxQueueEntriesPerDay = 0
            };

            // Act
            var result = await _service.CreateSubscriptionPlanAsync(request, "adminUserId", UserRoles.PlatformAdmin, CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("Name"));
            Assert.IsTrue(result.FieldErrors.ContainsKey("MonthlyPrice"));
            Assert.IsTrue(result.FieldErrors.ContainsKey("YearlyPrice"));
            Assert.IsTrue(result.FieldErrors.ContainsKey("MaxLocations"));
            Assert.IsTrue(result.FieldErrors.ContainsKey("MaxStaffPerLocation"));
            Assert.IsTrue(result.FieldErrors.ContainsKey("MaxQueueEntriesPerDay"));
        }

        [TestMethod]
        public async Task Should_FailCreation_When_InvalidPrices()
        {
            // Arrange
            var request = new CreateSubscriptionPlanRequest
            {
                Name = "Test Plan",
                Description = "A test plan",
                MonthlyPrice = -10.00m,
                YearlyPrice = -100.00m,
                MaxLocations = 1,
                MaxStaffPerLocation = 5,
                MaxQueueEntriesPerDay = 100
            };

            // Act
            var result = await _service.CreateSubscriptionPlanAsync(request, "adminUserId", UserRoles.PlatformAdmin, CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("MonthlyPrice"));
            Assert.IsTrue(result.FieldErrors.ContainsKey("YearlyPrice"));
            Assert.AreEqual("Monthly price must be greater than zero.", result.FieldErrors["MonthlyPrice"]);
            Assert.AreEqual("Yearly price must be greater than zero.", result.FieldErrors["YearlyPrice"]);
        }

        [TestMethod]
        public async Task Should_FailCreation_When_NonAdminUser()
        {
            // Arrange
            var request = new CreateSubscriptionPlanRequest
            {
                Name = "Test Plan",
                Description = "A test plan",
                MonthlyPrice = 29.99m,
                YearlyPrice = 299.99m,
                MaxLocations = 1,
                MaxStaffPerLocation = 5,
                MaxQueueEntriesPerDay = 100
            };

            // Act
            var result = await _service.CreateSubscriptionPlanAsync(request, "regularUserId", UserRoles.Staff, CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("Forbidden: Only platform Admin can create subscription plans."));
        }
    }
} 