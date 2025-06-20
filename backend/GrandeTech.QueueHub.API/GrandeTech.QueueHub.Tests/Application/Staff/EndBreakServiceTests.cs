using System;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Application.Staff;
using Grande.Fila.API.Domain.Staff;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GrandeTech.QueueHub.Tests.Application.Staff
{
    [TestClass]
    public class EndBreakServiceTests
    {
        private Mock<IStaffMemberRepository> _staffRepoMock = null!;
        private EndBreakService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _staffRepoMock = new Mock<IStaffMemberRepository>();
            _service = new EndBreakService(_staffRepoMock.Object);
        }

        [TestMethod]
        public async Task EndBreakAsync_WithValidRequest_ReturnsSuccess()
        {
            // Arrange
            var staffMemberId = Guid.NewGuid();
            var request = new EndBreakRequest
            {
                StaffMemberId = staffMemberId.ToString(),
                BreakId = "" // will set below
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
            
            // Start a break first
            var staffBreak = staffMember.StartBreak(TimeSpan.FromMinutes(15), "Lunch break", "admin");
            request.BreakId = staffBreak.Id.ToString();

            _staffRepoMock.Setup(r => r.GetByIdAsync(staffMemberId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(staffMember);
            _staffRepoMock.Setup(r => r.UpdateAsync(It.IsAny<StaffMember>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((StaffMember staff, CancellationToken _) => staff);

            // Act
            var result = await _service.EndBreakAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(request.StaffMemberId, result.StaffMemberId);
            Assert.AreEqual(request.BreakId, result.BreakId);
            Assert.AreEqual("available", result.NewStatus);
            Assert.AreEqual(0, result.FieldErrors.Count);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public async Task EndBreakAsync_WithInvalidStaffMemberId_ReturnsFieldError()
        {
            // Arrange
            var request = new EndBreakRequest
            {
                StaffMemberId = "invalid-guid",
                BreakId = Guid.NewGuid().ToString()
            };

            // Act
            var result = await _service.EndBreakAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("StaffMemberId"));
            Assert.AreEqual("Invalid staff member ID format.", result.FieldErrors["StaffMemberId"]);
        }

        [TestMethod]
        public async Task EndBreakAsync_WithEmptyStaffMemberId_ReturnsFieldError()
        {
            // Arrange
            var request = new EndBreakRequest
            {
                StaffMemberId = "",
                BreakId = Guid.NewGuid().ToString()
            };

            // Act
            var result = await _service.EndBreakAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("StaffMemberId"));
            Assert.AreEqual("Staff member ID is required.", result.FieldErrors["StaffMemberId"]);
        }

        [TestMethod]
        public async Task EndBreakAsync_WithInvalidBreakId_ReturnsFieldError()
        {
            // Arrange
            var request = new EndBreakRequest
            {
                StaffMemberId = Guid.NewGuid().ToString(),
                BreakId = "invalid-guid"
            };

            // Act
            var result = await _service.EndBreakAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("BreakId"));
            Assert.AreEqual("Invalid break ID format.", result.FieldErrors["BreakId"]);
        }

        [TestMethod]
        public async Task EndBreakAsync_WithEmptyBreakId_ReturnsFieldError()
        {
            // Arrange
            var request = new EndBreakRequest
            {
                StaffMemberId = Guid.NewGuid().ToString(),
                BreakId = ""
            };

            // Act
            var result = await _service.EndBreakAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("BreakId"));
            Assert.AreEqual("Break ID is required.", result.FieldErrors["BreakId"]);
        }

        [TestMethod]
        public async Task EndBreakAsync_WithNonExistentStaffMember_ReturnsError()
        {
            // Arrange
            var staffMemberId = Guid.NewGuid();
            var request = new EndBreakRequest
            {
                StaffMemberId = staffMemberId.ToString(),
                BreakId = Guid.NewGuid().ToString()
            };

            _staffRepoMock.Setup(r => r.GetByIdAsync(staffMemberId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync((StaffMember?)null);

            // Act
            var result = await _service.EndBreakAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("Staff member not found."));
        }

        [TestMethod]
        public async Task EndBreakAsync_WithInactiveStaffMember_ReturnsError()
        {
            // Arrange
            var staffMemberId = Guid.NewGuid();
            var request = new EndBreakRequest
            {
                StaffMemberId = staffMemberId.ToString(),
                BreakId = Guid.NewGuid().ToString()
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
            var result = await _service.EndBreakAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("Cannot end break for inactive staff member."));
        }

        [TestMethod]
        public async Task EndBreakAsync_WhenNotOnBreak_ReturnsError()
        {
            // Arrange
            var staffMemberId = Guid.NewGuid();
            var request = new EndBreakRequest
            {
                StaffMemberId = staffMemberId.ToString(),
                BreakId = Guid.NewGuid().ToString()
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
            // Staff member is not on break (default status is "available")

            _staffRepoMock.Setup(r => r.GetByIdAsync(staffMemberId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(staffMember);

            // Act
            var result = await _service.EndBreakAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Exists(e => e.Contains("No active break found")));
        }

        [TestMethod]
        public async Task EndBreakAsync_WithNonExistentBreakId_ReturnsError()
        {
            // Arrange
            var staffMemberId = Guid.NewGuid();
            var nonExistentBreakId = Guid.NewGuid();
            var request = new EndBreakRequest
            {
                StaffMemberId = staffMemberId.ToString(),
                BreakId = nonExistentBreakId.ToString()
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
            
            // Start a break with a different ID
            var actualBreak = staffMember.StartBreak(TimeSpan.FromMinutes(15), "Lunch break", "admin");

            _staffRepoMock.Setup(r => r.GetByIdAsync(staffMemberId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(staffMember);

            // Act
            var result = await _service.EndBreakAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Exists(e => e.Contains("No active break found")));
        }
    }
} 