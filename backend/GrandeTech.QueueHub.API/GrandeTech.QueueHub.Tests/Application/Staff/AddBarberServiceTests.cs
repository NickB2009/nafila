using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Application.Staff;
using GrandeTech.QueueHub.API.Domain.Staff;
using GrandeTech.QueueHub.API.Domain.ServiceProviders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GrandeTech.QueueHub.Tests.Application.Staff
{
    [TestClass]
    public class AddBarberServiceTests
    {
        private Mock<IStaffMemberRepository> _staffRepoMock = null!;
        private Mock<IServiceProviderRepository> _spRepoMock = null!;
        private AddBarberService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _staffRepoMock = new Mock<IStaffMemberRepository>();
            _spRepoMock = new Mock<IServiceProviderRepository>();
            _service = new AddBarberService(_staffRepoMock.Object, _spRepoMock.Object);
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
                ServiceProviderId = Guid.NewGuid(),
                ServiceTypeIds = new List<Guid> { Guid.NewGuid() },
                Address = "Rua Exemplo, 123",
                Notes = "Experienced barber"
            };
            _staffRepoMock.Setup(r => r.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            _spRepoMock.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ServiceProvider, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
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