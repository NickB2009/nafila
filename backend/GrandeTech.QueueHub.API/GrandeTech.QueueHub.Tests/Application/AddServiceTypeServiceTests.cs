using System;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Domain.Services;
using GrandeTech.QueueHub.API.Infrastructure.Repositories.Bogus;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrandeTech.QueueHub.Tests.Application
{
    [TestClass]
    public class AddServiceTypeServiceTests
    {
        [TestMethod]
        public async Task AddServiceType_WithValidData_Succeeds()
        {
            // Arrange
            var bogusRepo = new BogusServiceTypeRepository();
            var service = new AddServiceTypeService(bogusRepo);
            var request = new AddServiceTypeRequest
            {
                Name = "Haircut",
                Description = "Standard haircut",
                LocationId = Guid.NewGuid(),
                EstimatedDurationMinutes = 30,
                Price = 50m,
                ImageUrl = "https://example.com/haircut.jpg",
                CreatedBy = "test-user"
            };

            // Act
            var result = await service.ExecuteAsync(request);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreNotEqual(Guid.Empty, result.ServiceTypeId);
            var added = await bogusRepo.GetByIdAsync(result.ServiceTypeId);
            Assert.IsNotNull(added);
            Assert.AreEqual(request.Name, added!.Name);
            Assert.AreEqual(request.Description, added.Description);
            Assert.AreEqual(request.LocationId, added.LocationId);
            Assert.AreEqual(request.EstimatedDurationMinutes, added.EstimatedDurationMinutes);
            Assert.AreEqual(request.Price, added.Price?.Amount);
            Assert.AreEqual(request.ImageUrl, added.ImageUrl);
            Assert.AreEqual(request.CreatedBy, added.CreatedBy);
        }
    }
} 