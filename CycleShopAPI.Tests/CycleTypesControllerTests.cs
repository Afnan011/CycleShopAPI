using NUnit.Framework;
using Moq;
using CycleShopAPI.Controllers;
using CycleShopAPI.Services;
using CycleShopAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CycleShopAPI.Tests
{
    [TestFixture]
    public class CycleTypesControllerTests
    {
        private Mock<ICycleTypeService> _mockCycleTypeService = null!;
        private CycleTypesController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _mockCycleTypeService = new Mock<ICycleTypeService>();
            _controller = new CycleTypesController(_mockCycleTypeService.Object);
        }

        [Test]
        public async Task GetCycleTypes_ReturnsAllTypes()
        {
            // Arrange
            var expectedTypes = new List<CycleType>
            {
                new CycleType { CycleTypeId = Guid.NewGuid(), Name = "Mountain" },
                new CycleType { CycleTypeId = Guid.NewGuid(), Name = "Road" }
            };
            _mockCycleTypeService.Setup(s => s.GetAllCycleTypesAsync()).ReturnsAsync(expectedTypes);

            // Act
            var result = await _controller.GetCycleTypes();

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo(expectedTypes));
        }

        [Test]
        public async Task GetCycleType_WithValidId_ReturnsCycleType()
        {
            // Arrange
            var typeId = Guid.NewGuid();
            var expectedType = new CycleType 
            { 
                CycleTypeId = typeId,
                Name = "Mountain",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _mockCycleTypeService.Setup(s => s.GetCycleTypeByIdAsync(typeId)).ReturnsAsync(expectedType);

            // Act
            var result = await _controller.GetCycleType(typeId);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo(expectedType));
        }

        [Test]
        public async Task GetCycleType_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var typeId = Guid.NewGuid();
            _mockCycleTypeService.Setup(s => s.GetCycleTypeByIdAsync(typeId))!.ReturnsAsync((CycleType?)null);

            // Act
            var result = await _controller.GetCycleType(typeId);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task CreateCycleType_WithValidName_ReturnsCreatedType()
        {
            // Arrange
            var typeName = "Electric";
            var createdType = new CycleType
            {
                CycleTypeId = Guid.NewGuid(),
                Name = typeName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockCycleTypeService.Setup(s => s.CreateCycleTypeAsync(It.IsAny<CycleType>()))
                                .ReturnsAsync(createdType);

            // Act
            var result = await _controller.CreateCycleType(typeName);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
            var createdResult = result.Result as CreatedAtActionResult;
            Assert.That(createdResult, Is.Not.Null);
            Assert.That(createdResult!.Value, Is.EqualTo(createdType));
        }

        [Test]
        public async Task UpdateCycleType_WithValidData_ReturnsUpdatedType()
        {
            // Arrange
            var typeId = Guid.NewGuid();
            var newName = "Updated Type";

            var existingType = new CycleType 
            { 
                CycleTypeId = typeId,
                Name = "Old Type"
            };

            var updatedType = new CycleType
            {
                CycleTypeId = typeId,
                Name = newName,
                UpdatedAt = DateTime.UtcNow
            };

            _mockCycleTypeService.Setup(s => s.GetCycleTypeByIdAsync(typeId)).ReturnsAsync(existingType);
            _mockCycleTypeService.Setup(s => s.UpdateCycleTypeAsync(typeId, It.IsAny<CycleType>()))
                                .ReturnsAsync(updatedType);

            // Act
            var result = await _controller.UpdateCycleType(typeId, newName);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo(updatedType));
        }

        [Test]
        public async Task UpdateCycleType_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var typeId = Guid.NewGuid();
            var newName = "Updated Type";

            _mockCycleTypeService.Setup(s => s.GetCycleTypeByIdAsync(typeId))!.ReturnsAsync((CycleType?)null);

            // Act
            var result = await _controller.UpdateCycleType(typeId, newName);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task DeleteCycleType_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var typeId = Guid.NewGuid();
            _mockCycleTypeService.Setup(s => s.DeleteCycleTypeAsync(typeId)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteCycleType(typeId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task DeleteCycleType_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var typeId = Guid.NewGuid();
            _mockCycleTypeService.Setup(s => s.DeleteCycleTypeAsync(typeId)).ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteCycleType(typeId);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }
    }
}