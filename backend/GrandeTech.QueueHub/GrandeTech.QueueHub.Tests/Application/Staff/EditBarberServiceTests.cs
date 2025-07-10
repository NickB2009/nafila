using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Grande.Fila.API.Application.Staff;
using Grande.Fila.API.Domain.Staff;
using Grande.Fila.API.Domain.Common.ValueObjects;

namespace Grande.Fila.API.Tests.Application.Staff
{
    [TestClass]
    public class EditBarberServiceTests
    {
        private Mock<IStaffMemberRepository> _mockStaffRepository;
        private Mock<ILogger<EditBarberService>> _mockLogger;
        private EditBarberService _service;

        [TestInitialize]
        public void Setup()
        {
            _mockStaffRepository = new Mock<IStaffMemberRepository>();
            _mockLogger = new Mock<ILogger<EditBarberService>>();
            _service = new EditBarberService(_mockStaffRepository.Object, _mockLogger.Object);
        }

        [TestMethod]
        public async Task EditBarberAsync_ValidRequest_UpdatesBarberSuccessfully()
        {
            // Arrange
            var staffId = Guid.NewGuid();
            var locationId = Guid.NewGuid();
            var existingStaff = new StaffMember(
                "John Doe",
                locationId,
                "john@example.com",
                "+1234567890",
                null,
                "Barber",
                "johndoe",
                null,
                "admin"
            );

            _mockStaffRepository
                .Setup(r => r.GetByIdAsync(staffId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingStaff);

            _mockStaffRepository
                .Setup(r => r.UpdateAsync(It.IsAny<StaffMember>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingStaff);

            var request = new EditBarberRequest
            {
                StaffMemberId = staffId.ToString(),
                Name = "John Smith",
                Email = "johnsmith@example.com",
                PhoneNumber = "+0987654321",
                ProfilePictureUrl = "https://example.com/profile.jpg",
                Role = "Senior Barber"
            };

            // Act
            var result = await _service.EditBarberAsync(request, "admin", "Admin");

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual("John Smith", result.Name);
            Assert.AreEqual("johnsmith@example.com", result.Email);
            Assert.AreEqual("+0987654321", result.PhoneNumber);
            Assert.AreEqual("https://example.com/profile.jpg", result.ProfilePictureUrl);
            Assert.AreEqual("Senior Barber", result.Role);

            _mockStaffRepository.Verify(
                r => r.UpdateAsync(It.Is<StaffMember>(s => 
                    s.Name == "John Smith" &&
                    s.Email!.Value == "johnsmith@example.com" &&
                    s.PhoneNumber!.Value == "+0987654321" &&
                    s.ProfilePictureUrl == "https://example.com/profile.jpg" &&
                    s.Role == "Senior Barber"
                ), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [TestMethod]
        public async Task EditBarberAsync_InvalidStaffId_ReturnsError()
        {
            // Arrange
            var request = new EditBarberRequest
            {
                StaffMemberId = "invalid-guid",
                Name = "John Smith"
            };

            // Act
            var result = await _service.EditBarberAsync(request, "admin", "Admin");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("StaffMemberId"));
            Assert.AreEqual("Invalid staff member ID format", result.FieldErrors["StaffMemberId"]);
        }

        [TestMethod]
        public async Task EditBarberAsync_EmptyName_ReturnsError()
        {
            // Arrange
            var request = new EditBarberRequest
            {
                StaffMemberId = Guid.NewGuid().ToString(),
                Name = ""
            };

            // Act
            var result = await _service.EditBarberAsync(request, "admin", "Admin");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FieldErrors.ContainsKey("Name"));
            Assert.AreEqual("Staff member name is required", result.FieldErrors["Name"]);
        }

        [TestMethod]
        public async Task EditBarberAsync_StaffNotFound_ReturnsError()
        {
            // Arrange
            var staffId = Guid.NewGuid();
            _mockStaffRepository
                .Setup(r => r.GetByIdAsync(staffId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((StaffMember?)null);

            var request = new EditBarberRequest
            {
                StaffMemberId = staffId.ToString(),
                Name = "John Smith"
            };

            // Act
            var result = await _service.EditBarberAsync(request, "admin", "Admin");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("Staff member not found"));
        }

        [TestMethod]
        public async Task EditBarberAsync_RepositoryThrowsException_ReturnsError()
        {
            // Arrange
            var staffId = Guid.NewGuid();
            _mockStaffRepository
                .Setup(r => r.GetByIdAsync(staffId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            var request = new EditBarberRequest
            {
                StaffMemberId = staffId.ToString(),
                Name = "John Smith"
            };

            // Act
            var result = await _service.EditBarberAsync(request, "admin", "Admin");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("An unexpected error occurred while updating the staff member"));
        }
    }
}