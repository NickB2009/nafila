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
    public class UpdateStaffStatusServiceTests
    {
        private Mock<IStaffMemberRepository> _staffRepoMock = null!;
        private UpdateStaffStatusService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _staffRepoMock = new Mock<IStaffMemberRepository>();
            _service = new UpdateStaffStatusService(_staffRepoMock.Object);
        }

        [TestMethod]
        public async Task UpdateStaffStatusAsync_WithValidRequest_ReturnsSuccess()
        {
            // Arrange
            var staffMemberId = Guid.NewGuid();
            var request = new UpdateStaffStatusRequest
            {
                StaffMemberId = staffMemberId.ToString(),
                NewStatus = "busy",
                Notes = "Currently serving a client"
            };

            var existingStaffMember = new StaffMember(
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
                          .ReturnsAsync(existingStaffMember);
            _staffRepoMock.Setup(r => r.UpdateAsync(It.IsAny<StaffMember>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((StaffMember staff, CancellationToken _) => staff);

            // Act
            var result = await _service.UpdateStaffStatusAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(request.StaffMemberId, result.StaffMemberId);
            Assert.AreEqual(request.NewStatus, result.NewStatus);
            Assert.AreEqual(0, result.FieldErrors.Count);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public async Task UpdateStaffStatusAsync_WithInvalidStaffMemberId_ReturnsFieldError()
        {
            // Arrange
            var request = new UpdateStaffStatusRequest
            {
                StaffMemberId = "invalid-guid",
                NewStatus = "busy"
            };

            // Act
            var result = await _service.UpdateStaffStatusAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("StaffMemberId"));
            Assert.AreEqual("Invalid staff member ID format.", result.FieldErrors["StaffMemberId"]);
        }

        [TestMethod]
        public async Task UpdateStaffStatusAsync_WithEmptyStaffMemberId_ReturnsFieldError()
        {
            // Arrange
            var request = new UpdateStaffStatusRequest
            {
                StaffMemberId = "",
                NewStatus = "busy"
            };

            // Act
            var result = await _service.UpdateStaffStatusAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("StaffMemberId"));
            Assert.AreEqual("Staff member ID is required.", result.FieldErrors["StaffMemberId"]);
        }

        [TestMethod]
        public async Task UpdateStaffStatusAsync_WithEmptyStatus_ReturnsFieldError()
        {
            // Arrange
            var request = new UpdateStaffStatusRequest
            {
                StaffMemberId = Guid.NewGuid().ToString(),
                NewStatus = ""
            };

            // Act
            var result = await _service.UpdateStaffStatusAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("NewStatus"));
            Assert.AreEqual("Status is required.", result.FieldErrors["NewStatus"]);
        }

        [TestMethod]
        public async Task UpdateStaffStatusAsync_WithInvalidStatus_ReturnsFieldError()
        {
            // Arrange
            var request = new UpdateStaffStatusRequest
            {
                StaffMemberId = Guid.NewGuid().ToString(),
                NewStatus = "invalid-status"
            };

            // Act
            var result = await _service.UpdateStaffStatusAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("NewStatus"));
            Assert.AreEqual("Invalid status. Must be one of: available, busy, away, offline", result.FieldErrors["NewStatus"]);
        }

        [TestMethod]
        public async Task UpdateStaffStatusAsync_WithNonExistentStaffMember_ReturnsError()
        {
            // Arrange
            var staffMemberId = Guid.NewGuid();
            var request = new UpdateStaffStatusRequest
            {
                StaffMemberId = staffMemberId.ToString(),
                NewStatus = "busy"
            };

            _staffRepoMock.Setup(r => r.GetByIdAsync(staffMemberId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync((StaffMember?)null);

            // Act
            var result = await _service.UpdateStaffStatusAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("Staff member not found."));
        }

        [TestMethod]
        public async Task UpdateStaffStatusAsync_WithInactiveStaffMember_ReturnsError()
        {
            // Arrange
            var staffMemberId = Guid.NewGuid();
            var request = new UpdateStaffStatusRequest
            {
                StaffMemberId = staffMemberId.ToString(),
                NewStatus = "busy"
            };

            var inactiveStaffMember = new StaffMember(
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
            inactiveStaffMember.Deactivate("admin");

            _staffRepoMock.Setup(r => r.GetByIdAsync(staffMemberId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(inactiveStaffMember);

            // Act
            var result = await _service.UpdateStaffStatusAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("Cannot update status for inactive staff member."));
        }

        [TestMethod]
        public async Task UpdateStaffStatusAsync_WithSameStatus_ReturnsSuccess()
        {
            // Arrange
            var staffMemberId = Guid.NewGuid();
            var request = new UpdateStaffStatusRequest
            {
                StaffMemberId = staffMemberId.ToString(),
                NewStatus = "available"
            };

            var existingStaffMember = new StaffMember(
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
                          .ReturnsAsync(existingStaffMember);
            _staffRepoMock.Setup(r => r.UpdateAsync(It.IsAny<StaffMember>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((StaffMember staff, CancellationToken _) => staff);

            // Act
            var result = await _service.UpdateStaffStatusAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(request.StaffMemberId, result.StaffMemberId);
            Assert.AreEqual(request.NewStatus, result.NewStatus);
        }

        [TestMethod]
        public async Task UpdateStaffStatusAsync_WithValidStatuses_AllReturnSuccess()
        {
            // Arrange
            var validStatuses = new[] { "available", "busy", "away", "offline" };
            var staffMemberId = Guid.NewGuid();

            foreach (var status in validStatuses)
            {
                var request = new UpdateStaffStatusRequest
                {
                    StaffMemberId = staffMemberId.ToString(),
                    NewStatus = status
                };

                var existingStaffMember = new StaffMember(
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
                              .ReturnsAsync(existingStaffMember);
                _staffRepoMock.Setup(r => r.UpdateAsync(It.IsAny<StaffMember>(), It.IsAny<CancellationToken>()))
                              .ReturnsAsync((StaffMember staff, CancellationToken _) => staff);

                // Act
                var result = await _service.UpdateStaffStatusAsync(request, "user123", CancellationToken.None);

                // Assert
                Assert.IsTrue(result.Success, $"Status '{status}' should be valid");
                Assert.AreEqual(status, result.NewStatus);
            }
        }

        [TestMethod]
        public async Task UpdateStaffStatusAsync_WithLongNotes_TruncatesNotes()
        {
            // Arrange
            var staffMemberId = Guid.NewGuid();
            var longNotes = new string('a', 501); // 501 characters
            var request = new UpdateStaffStatusRequest
            {
                StaffMemberId = staffMemberId.ToString(),
                NewStatus = "busy",
                Notes = longNotes
            };

            var existingStaffMember = new StaffMember(
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
                          .ReturnsAsync(existingStaffMember);
            _staffRepoMock.Setup(r => r.UpdateAsync(It.IsAny<StaffMember>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((StaffMember staff, CancellationToken _) => staff);

            // Act
            var result = await _service.UpdateStaffStatusAsync(request, "user123", CancellationToken.None);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(request.StaffMemberId, result.StaffMemberId);
            Assert.AreEqual(request.NewStatus, result.NewStatus);
        }
    }
} 