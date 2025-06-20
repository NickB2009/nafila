using System;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Application.Staff;
using Grande.Fila.API.Domain.Staff;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Grande.Fila.Tests.Application.Staff
{
    [TestClass]
    public class StartBreakServiceTests
    {
        private Mock<IStaffMemberRepository> _staffRepoMock = null!;
        private StartBreakService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _staffRepoMock = new Mock<IStaffMemberRepository>();
            _service = new StartBreakService(_staffRepoMock.Object);
        }

        [TestMethod]
        public async Task StartBreakAsync_WithValidRequest_ReturnsSuccess()
        {
            // Arrange
            var staffMemberId = Guid.NewGuid();
            var request = new StartBreakRequest
            {
                StaffMemberId = staffMemberId.ToString(),
                DurationMinutes = 15,
                Reason = "Lunch break"
            };

            var staffMember = new StaffMember(
                "John Doe",
                Guid.NewGuid(),
                "john@barbershop.com",
                "+5511999999999",
                null,
                "Barber",
                "johndoe",
                null,
                "system"
            );

            _staffRepoMock.Setup(r => r.GetByIdAsync(staffMemberId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(staffMember);
            _staffRepoMock.Setup(r => r.UpdateAsync(It.IsAny<StaffMember>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((StaffMember staff, CancellationToken _) => staff);

            // Act
            var result = await _service.StartBreakAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(request.StaffMemberId, result.StaffMemberId);
            Assert.IsNotNull(result.BreakId);
            Assert.AreEqual("on-break", result.NewStatus);
            Assert.AreEqual(0, result.FieldErrors.Count);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public async Task StartBreakAsync_WithInvalidStaffMemberId_ReturnsFieldError()
        {
            // Arrange
            var request = new StartBreakRequest
            {
                StaffMemberId = "invalid-guid",
                DurationMinutes = 10
            };

            // Act
            var result = await _service.StartBreakAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("StaffMemberId"));
            Assert.AreEqual("Invalid staff member ID format.", result.FieldErrors["StaffMemberId"]);
        }

        [TestMethod]
        public async Task StartBreakAsync_WithEmptyStaffMemberId_ReturnsFieldError()
        {
            // Arrange
            var request = new StartBreakRequest
            {
                StaffMemberId = "",
                DurationMinutes = 10
            };

            // Act
            var result = await _service.StartBreakAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("StaffMemberId"));
            Assert.AreEqual("Staff member ID is required.", result.FieldErrors["StaffMemberId"]);
        }

        [TestMethod]
        public async Task StartBreakAsync_WithZeroOrNegativeDuration_ReturnsFieldError()
        {
            // Arrange
            var staffMemberId = Guid.NewGuid();
            var request = new StartBreakRequest
            {
                StaffMemberId = staffMemberId.ToString(),
                DurationMinutes = 0
            };

            // Act
            var result = await _service.StartBreakAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("DurationMinutes"));
            Assert.AreEqual("Duration must be greater than 0.", result.FieldErrors["DurationMinutes"]);
        }

        [TestMethod]
        public async Task StartBreakAsync_WithNonExistentStaffMember_ReturnsError()
        {
            // Arrange
            var staffMemberId = Guid.NewGuid();
            var request = new StartBreakRequest
            {
                StaffMemberId = staffMemberId.ToString(),
                DurationMinutes = 10
            };

            _staffRepoMock.Setup(r => r.GetByIdAsync(staffMemberId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync((StaffMember?)null);

            // Act
            var result = await _service.StartBreakAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("Staff member not found."));
        }

        [TestMethod]
        public async Task StartBreakAsync_WithInactiveStaffMember_ReturnsError()
        {
            // Arrange
            var staffMemberId = Guid.NewGuid();
            var request = new StartBreakRequest
            {
                StaffMemberId = staffMemberId.ToString(),
                DurationMinutes = 10
            };

            var staffMember = new StaffMember(
                "John Doe",
                Guid.NewGuid(),
                "john@barbershop.com",
                "+5511999999999",
                null,
                "Barber",
                "johndoe",
                null,
                "system"
            );
            staffMember.Deactivate("admin");

            _staffRepoMock.Setup(r => r.GetByIdAsync(staffMemberId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(staffMember);

            // Act
            var result = await _service.StartBreakAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("Cannot start break for inactive staff member."));
        }

        [TestMethod]
        public async Task StartBreakAsync_WhenAlreadyOnBreak_ReturnsError()
        {
            // Arrange
            var staffMemberId = Guid.NewGuid();
            var request = new StartBreakRequest
            {
                StaffMemberId = staffMemberId.ToString(),
                DurationMinutes = 10
            };

            var staffMember = new StaffMember(
                "John Doe",
                Guid.NewGuid(),
                "john@barbershop.com",
                "+5511999999999",
                null,
                "Barber",
                "johndoe",
                null,
                "system"
            );
            staffMember.StartBreak(TimeSpan.FromMinutes(10), "Break", "admin");

            _staffRepoMock.Setup(r => r.GetByIdAsync(staffMemberId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(staffMember);

            // Act
            var result = await _service.StartBreakAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Exists(e => e.Contains("already on break")));
        }
    }
} 