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
    public class SubscriptionPlanServiceTests
    {
        private Mock<ISubscriptionPlanRepository> _subscriptionPlanRepoMock = null!;
        private Mock<IAuditLogRepository> _auditLogRepoMock = null!;
        private SubscriptionPlanService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _subscriptionPlanRepoMock = new Mock<ISubscriptionPlanRepository>();
            _auditLogRepoMock = new Mock<IAuditLogRepository>();
            
            _service = new SubscriptionPlanService(
                _subscriptionPlanRepoMock.Object, 
                _auditLogRepoMock.Object);
        }

        [TestMethod]
        public async Task Should_UpdateSubscriptionPlan_With_ValidData_ReturnsSuccess()
        {
            // Arrange
            var subscriptionPlanId = Guid.NewGuid();
            var subscriptionPlan = CreateTestSubscriptionPlan(subscriptionPlanId);
            
            var request = new UpdateSubscriptionPlanRequest
            {
                SubscriptionPlanId = subscriptionPlanId.ToString(),
                Name = "Updated Premium Plan",
                Description = "Updated premium subscription plan",
                MonthlyPrice = 39.99m,
                YearlyPrice = 399.99m,
                MaxLocations = 10,
                MaxStaffPerLocation = 30,
                IncludesAnalytics = true,
                IncludesAdvancedReporting = true,
                IncludesCustomBranding = true,
                IncludesAdvertising = true,
                IncludesMultipleLocations = true,
                MaxQueueEntriesPerDay = 1000,
                IsFeatured = true,
                IsDefault = false
            };

            _subscriptionPlanRepoMock.Setup(r => r.GetByIdAsync(subscriptionPlanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptionPlan);
            _subscriptionPlanRepoMock.Setup(r => r.GetByNameAsync(request.Name, It.IsAny<CancellationToken>()))
                .ReturnsAsync((SubscriptionPlan?)null);
            _subscriptionPlanRepoMock.Setup(r => r.UpdateAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((SubscriptionPlan plan, CancellationToken _) => plan);

            // Act
            var result = await _service.UpdateSubscriptionPlanAsync(request, "adminUserId", UserRoles.PlatformAdmin, CancellationToken.None);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.Errors.Count == 0);
            Assert.IsTrue(result.FieldErrors.Count == 0);
        }

        [TestMethod]
        public async Task Should_ActivateSubscriptionPlan_With_ValidId_ReturnsSuccess()
        {
            // Arrange
            var subscriptionPlanId = Guid.NewGuid();
            var subscriptionPlan = CreateTestSubscriptionPlan(subscriptionPlanId);
            subscriptionPlan.Deactivate("test");

            _subscriptionPlanRepoMock.Setup(r => r.GetByIdAsync(subscriptionPlanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptionPlan);
            _subscriptionPlanRepoMock.Setup(r => r.UpdateAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((SubscriptionPlan plan, CancellationToken _) => plan);

            // Act
            var result = await _service.ActivateSubscriptionPlanAsync(subscriptionPlanId.ToString(), "adminUserId", UserRoles.PlatformAdmin, CancellationToken.None);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.Errors.Count == 0);
            Assert.IsTrue(result.FieldErrors.Count == 0);
        }

        [TestMethod]
        public async Task Should_DeactivateSubscriptionPlan_With_ValidId_ReturnsSuccess()
        {
            // Arrange
            var subscriptionPlanId = Guid.NewGuid();
            var subscriptionPlan = CreateTestSubscriptionPlan(subscriptionPlanId);

            _subscriptionPlanRepoMock.Setup(r => r.GetByIdAsync(subscriptionPlanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptionPlan);
            _subscriptionPlanRepoMock.Setup(r => r.UpdateAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((SubscriptionPlan plan, CancellationToken _) => plan);

            // Act
            var result = await _service.DeactivateSubscriptionPlanAsync(subscriptionPlanId.ToString(), "adminUserId", UserRoles.PlatformAdmin, CancellationToken.None);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.Errors.Count == 0);
            Assert.IsTrue(result.FieldErrors.Count == 0);
        }

        [TestMethod]
        public async Task Should_FailUpdate_When_SubscriptionPlanNotFound()
        {
            // Arrange
            var request = new UpdateSubscriptionPlanRequest
            {
                SubscriptionPlanId = Guid.NewGuid().ToString(),
                Name = "Updated Plan",
                MonthlyPrice = 29.99m,
                YearlyPrice = 299.99m,
                MaxLocations = 1,
                MaxStaffPerLocation = 5,
                MaxQueueEntriesPerDay = 100
            };

            _subscriptionPlanRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((SubscriptionPlan?)null);

            // Act
            var result = await _service.UpdateSubscriptionPlanAsync(request, "adminUserId", UserRoles.PlatformAdmin, CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("Subscription plan not found."));
        }

        [TestMethod]
        public async Task Should_FailOperations_When_NonAdminUser()
        {
            // Arrange
            var subscriptionPlanId = Guid.NewGuid();
            var request = new UpdateSubscriptionPlanRequest
            {
                SubscriptionPlanId = subscriptionPlanId.ToString(),
                Name = "Test Plan",
                MonthlyPrice = 29.99m,
                YearlyPrice = 299.99m,
                MaxLocations = 1,
                MaxStaffPerLocation = 5,
                MaxQueueEntriesPerDay = 100
            };

            // Act & Assert - Update
            var updateResult = await _service.UpdateSubscriptionPlanAsync(request, "regularUserId", UserRoles.Staff, CancellationToken.None);
            Assert.IsFalse(updateResult.Success);
            Assert.IsTrue(updateResult.Errors.Contains("Forbidden: Only platform Admin can manage subscription plans."));

            // Act & Assert - Activate
            var activateResult = await _service.ActivateSubscriptionPlanAsync(subscriptionPlanId.ToString(), "regularUserId", UserRoles.Staff, CancellationToken.None);
            Assert.IsFalse(activateResult.Success);
            Assert.IsTrue(activateResult.Errors.Contains("Forbidden: Only platform Admin can manage subscription plans."));

            // Act & Assert - Deactivate
            var deactivateResult = await _service.DeactivateSubscriptionPlanAsync(subscriptionPlanId.ToString(), "regularUserId", UserRoles.Staff, CancellationToken.None);
            Assert.IsFalse(deactivateResult.Success);
            Assert.IsTrue(deactivateResult.Errors.Contains("Forbidden: Only platform Admin can manage subscription plans."));
        }

        private SubscriptionPlan CreateTestSubscriptionPlan(Guid id)
        {
            var plan = new SubscriptionPlan(
                "Test Plan",
                "Test Description",
                29.99m,
                299.99m,
                5,
                20,
                true,
                false,
                false,
                false,
                false,
                500,
                false,
                "test"
            );

            // Set the ID using reflection since it's protected
            var idProperty = typeof(Grande.Fila.API.Domain.Common.BaseEntity).GetProperty("Id");
            idProperty?.SetValue(plan, id);

            return plan;
        }
    }
} 