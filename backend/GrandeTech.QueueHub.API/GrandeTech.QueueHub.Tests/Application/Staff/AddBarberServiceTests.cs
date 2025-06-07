using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Application.Staff;
using GrandeTech.QueueHub.API.Domain.Staff;
using GrandeTech.QueueHub.API.Domain.ServicesProviders;
using GrandeTech.QueueHub.API.Domain.AuditLogs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GrandeTech.QueueHub.Tests.Application.Staff
{
    [TestClass]
    public class AddBarberServiceTests
    {
        private Mock<IStaffMemberRepository> _staffRepoMock = null!;
        private Mock<IServicesProviderRepository> _spRepoMock = null!;
        private Mock<IAuditLogRepository> _auditLogRepoMock = null!;
        private AddBarberService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _staffRepoMock = new Mock<IStaffMemberRepository>();
            _spRepoMock = new Mock<IServicesProviderRepository>();
            _auditLogRepoMock = new Mock<IAuditLogRepository>();
            _service = new AddBarberService(_staffRepoMock.Object, _spRepoMock.Object, _auditLogRepoMock.Object);
        }

        [TestMethod]
        public async Task Should_AddBarber_With_ValidData_ReturnsSuccess()
        {
            // Arrange
            var request = new AddBarberRequest
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                PhoneNumber = "+5511999999999",
                ServicesProviderId = Guid.NewGuid().ToString(),
                ServiceTypeIds = new List<string> { Guid.NewGuid().ToString() },
                Address = "Rua Exemplo, 123",
                Notes = "Experienced barber",
                Username = "johndoe" 
            };
            _staffRepoMock.Setup(r => r.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            _staffRepoMock.Setup(r => r.ExistsByUsernameAsync(request.Username, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            _spRepoMock.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ServicesProvider, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _staffRepoMock.Setup(r => r.AddAsync(It.IsAny<StaffMember>(), It.IsAny<CancellationToken>())).ReturnsAsync((StaffMember staff, CancellationToken _) => staff);

            // Act
            var result = await _service.AddBarberAsync(request, "adminUserId");

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.BarberId);
            Assert.AreEqual("Active", result.Status);
        }
    }
} 