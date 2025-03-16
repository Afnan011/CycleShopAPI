using NUnit.Framework;
using Moq;
using CycleShopAPI.Controllers;
using CycleShopAPI.Services;
using CycleShopAPI.Models;
using CycleShopAPI.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CycleShopAPI.Tests
{
    [TestFixture]
    public class CyclesControllerTests
    {
        private Mock<ICycleService> _mockCycleService = null!;
        private Mock<IBrandService> _mockBrandService = null!;
        private Mock<ICycleTypeService> _mockCycleTypeService = null!;
        private CyclesController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _mockCycleService = new Mock<ICycleService>();
            _mockBrandService = new Mock<IBrandService>();
            _mockCycleTypeService = new Mock<ICycleTypeService>();
            _controller = new CyclesController(
                _mockCycleService.Object,
                _mockBrandService.Object,
                _mockCycleTypeService.Object
            );
        }

        [Test]
        public async Task GetCycles_ReturnsAllCycles()
        {
            // Arrange
            var expectedCycles = new List<Cycle>
            {
                new Cycle { CycleId = Guid.NewGuid(), ModelName = "Model1" },
                new Cycle { CycleId = Guid.NewGuid(), ModelName = "Model2" }
            };
            _mockCycleService.Setup(s => s.GetAllCyclesAsync()).ReturnsAsync(expectedCycles);

            // Act
            var result = await _controller.GetCycles();

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo(expectedCycles));
        }

        [Test]
        public async Task GetCycle_WithValidId_ReturnsCycle()
        {
            // Arrange
            var cycleId = Guid.NewGuid();
            var expectedCycle = new Cycle { CycleId = cycleId, ModelName = "TestModel" };
            _mockCycleService.Setup(s => s.GetCycleByIdAsync(cycleId)).ReturnsAsync(expectedCycle);

            // Act
            var result = await _controller.GetCycle(cycleId);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo(expectedCycle));
        }

        [Test]
        public async Task GetCyclesByBrand_WithValidBrandId_ReturnsCycles()
        {
            // Arrange
            var brandId = Guid.NewGuid();
            var brand = new Brand { BrandId = brandId, Name = "TestBrand" };
            var expectedCycles = new List<Cycle>
            {
                new Cycle { CycleId = Guid.NewGuid(), BrandId = brandId }
            };

            _mockBrandService.Setup(s => s.GetBrandByIdAsync(brandId)).ReturnsAsync(brand);
            _mockCycleService.Setup(s => s.GetCyclesByBrandAsync(brandId)).ReturnsAsync(expectedCycles);

            // Act
            var result = await _controller.GetCyclesByBrand(brandId);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo(expectedCycles));
        }

        [Test]
        public async Task CreateCycle_WithValidData_ReturnsCreatedCycle()
        {
            // Arrange
            var createRequest = new CreateCycleDTO
            {
                ModelName = "NewModel",
                BrandId = Guid.NewGuid(),
                TypeId = Guid.NewGuid(),
                Price = 999.99m,
                CostPrice = 799.99m
            };

            var brand = new Brand { BrandId = createRequest.BrandId };
            var cycleType = new CycleType { CycleTypeId = createRequest.TypeId };

            _mockBrandService.Setup(s => s.GetBrandByIdAsync(createRequest.BrandId)).ReturnsAsync(brand);
            _mockCycleTypeService.Setup(s => s.GetCycleTypeByIdAsync(createRequest.TypeId)).ReturnsAsync(cycleType);

            var createdCycle = new Cycle
            {
                CycleId = Guid.NewGuid(),
                ModelName = createRequest.ModelName,
                BrandId = createRequest.BrandId,
                TypeId = createRequest.TypeId,
                Price = createRequest.Price,
                CostPrice = createRequest.CostPrice
            };

            _mockCycleService.Setup(s => s.CreateCycleAsync(It.IsAny<Cycle>())).ReturnsAsync(createdCycle);

            // Act
            var result = await _controller.CreateCycle(createRequest);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
            var createdResult = result.Result as CreatedAtActionResult;
            Assert.That(createdResult, Is.Not.Null);
            Assert.That(createdResult!.Value, Is.EqualTo(createdCycle));
        }

        [Test]
        public async Task UpdateCycle_WithValidData_ReturnsUpdatedCycle()
        {
            // Arrange
            var cycleId = Guid.NewGuid();
            var updateRequest = new UpdateCycleDTO
            {
                ModelName = "UpdatedModel",
                Price = 1099.99m
            };

            var existingCycle = new Cycle { CycleId = cycleId, ModelName = "OldModel" };
            _mockCycleService.Setup(s => s.GetCycleByIdAsync(cycleId)).ReturnsAsync(existingCycle);
            
            var updatedCycle = new Cycle
            {
                CycleId = cycleId,
                ModelName = updateRequest.ModelName,
                Price = updateRequest.Price.Value
            };
            _mockCycleService.Setup(s => s.UpdateCycleAsync(cycleId, It.IsAny<Cycle>())).ReturnsAsync(updatedCycle);

            // Act
            var result = await _controller.UpdateCycle(cycleId, updateRequest);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo(updatedCycle));
        }

        [Test]
        public async Task DeleteCycle_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var cycleId = Guid.NewGuid();
            _mockCycleService.Setup(s => s.DeleteCycleAsync(cycleId)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteCycle(cycleId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task GetCyclesByType_WithValidTypeId_ReturnsCycles()
        {
            // Arrange
            var typeId = Guid.NewGuid();
            var cycleType = new CycleType { CycleTypeId = typeId, Name = "TestType" };
            var expectedCycles = new List<Cycle>
            {
                new Cycle { CycleId = Guid.NewGuid(), TypeId = typeId }
            };

            _mockCycleTypeService.Setup(s => s.GetCycleTypeByIdAsync(typeId)).ReturnsAsync(cycleType);
            _mockCycleService.Setup(s => s.GetCyclesByTypeAsync(typeId)).ReturnsAsync(expectedCycles);

            // Act
            var result = await _controller.GetCyclesByType(typeId);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo(expectedCycles));
        }
    }
}