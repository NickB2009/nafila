using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Application.Staff;
using Grande.Fila.API.Domain.Staff;
using Grande.Fila.API.Domain.Locations;
using Grande.Fila.API.Domain.AuditLogs;
using Grande.Fila.API.Domain.Users;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Grande.Fila.Tests.Application.Staff
{
    [TestClass]
    public class AddBarberServiceTests
    {        private Mock<IStaffMemberRepository> _staffRepoMock = null!;
        private Mock<ILocationRepository> _locationRepoMock = null!;
        private Mock<IAuditLogRepository> _auditLogRepoMock = null!;
        private AddBarberService _service = null!;        [TestInitialize]
        public void Setup()
        {
            _staffRepoMock = new Mock<IStaffMemberRepository>();
            _locationRepoMock = new Mock<ILocationRepository>();
            _auditLogRepoMock = new Mock<IAuditLogRepository>();
            _service = new AddBarberService(_staffRepoMock.Object, _locationRepoMock.Object, _auditLogRepoMock.Object);
        }        [TestMethod]
        public async Task Should_AddBarber_With_ValidData_ReturnsSuccess()
        {
            // Arrange
            var request = new AddBarberRequest
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                PhoneNumber = "+5511999999999",
                LocationId = Guid.NewGuid().ToString(),
                ServiceTypeIds = new List<string> { Guid.NewGuid().ToString() },
                Address = "Rua Exemplo, 123",
                Notes = "Experienced barber",
                Username = "johndoe" 
            };
            
            _staffRepoMock.Setup(r => r.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            _staffRepoMock.Setup(r => r.ExistsByUsernameAsync(request.Username, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            _locationRepoMock.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Location, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _staffRepoMock.Setup(r => r.AddAsync(It.IsAny<StaffMember>(), It.IsAny<CancellationToken>())).ReturnsAsync((StaffMember staff, CancellationToken _) => staff);

            // Act
            var result = await _service.AddBarberAsync(request, "adminUserId", UserRoles.Owner);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.BarberId);
            Assert.AreEqual("Active", result.Status);
        }
    }
} 