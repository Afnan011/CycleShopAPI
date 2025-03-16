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
    public class BrandsControllerTests
    {
        private Mock<IBrandService> _mockBrandService = null!;
        private BrandsController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _mockBrandService = new Mock<IBrandService>();
            _controller = new BrandsController(_mockBrandService.Object);
        }

        [Test]
        public async Task GetBrands_ReturnsAllBrands()
        {
            // Arrange
            var expectedBrands = new List<Brand>
            {
                new Brand { BrandId = Guid.NewGuid(), Name = "Brand1" },
                new Brand { BrandId = Guid.NewGuid(), Name = "Brand2" }
            };
            _mockBrandService.Setup(s => s.GetAllBrandsAsync()).ReturnsAsync(expectedBrands);

            // Act
            var result = await _controller.GetBrands();

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo(expectedBrands));
        }

        [Test]
        public async Task GetBrand_WithValidId_ReturnsBrand()
        {
            // Arrange
            var brandId = Guid.NewGuid();
            var expectedBrand = new Brand 
            { 
                BrandId = brandId,
                Name = "Test Brand",
                Description = "Test Description"
            };
            _mockBrandService.Setup(s => s.GetBrandByIdAsync(brandId)).ReturnsAsync(expectedBrand);

            // Act
            var result = await _controller.GetBrand(brandId);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo(expectedBrand));
        }

        [Test]
        public async Task GetBrand_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var brandId = Guid.NewGuid();
            _mockBrandService.Setup(s => s.GetBrandByIdAsync(brandId))!.ReturnsAsync((Brand?)null);

            // Act
            var result = await _controller.GetBrand(brandId);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task CreateBrand_WithValidData_ReturnsCreatedBrand()
        {
            // Arrange
            var createRequest = new CreateBrandDTO
            {
                Name = "New Brand",
                Description = "New Brand Description"
            };

            var createdBrand = new Brand
            {
                BrandId = Guid.NewGuid(),
                Name = createRequest.Name,
                Description = createRequest.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockBrandService.Setup(s => s.CreateBrandAsync(It.IsAny<Brand>()))
                            .ReturnsAsync(createdBrand);

            // Act
            var result = await _controller.CreateBrand(createRequest);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
            var createdResult = result.Result as CreatedAtActionResult;
            Assert.That(createdResult, Is.Not.Null);
            Assert.That(createdResult!.Value, Is.EqualTo(createdBrand));
        }

        [Test]
        public async Task UpdateBrand_WithValidData_ReturnsUpdatedBrand()
        {
            // Arrange
            var brandId = Guid.NewGuid();
            var updateRequest = new CreateBrandDTO
            {
                Name = "Updated Brand",
                Description = "Updated Description"
            };

            var existingBrand = new Brand 
            { 
                BrandId = brandId,
                Name = "Old Brand",
                Description = "Old Description"
            };

            var updatedBrand = new Brand
            {
                BrandId = brandId,
                Name = updateRequest.Name,
                Description = updateRequest.Description,
                UpdatedAt = DateTime.UtcNow
            };

            _mockBrandService.Setup(s => s.GetBrandByIdAsync(brandId)).ReturnsAsync(existingBrand);
            _mockBrandService.Setup(s => s.UpdateBrandAsync(brandId, It.IsAny<Brand>()))
                            .ReturnsAsync(updatedBrand);

            // Act
            var result = await _controller.UpdateBrand(brandId, updateRequest);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo(updatedBrand));
        }

        [Test]
        public async Task UpdateBrand_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var brandId = Guid.NewGuid();
            var updateRequest = new CreateBrandDTO
            {
                Name = "Updated Brand",
                Description = "Updated Description"
            };

            _mockBrandService.Setup(s => s.GetBrandByIdAsync(brandId))!.ReturnsAsync((Brand?)null);

            // Act
            var result = await _controller.UpdateBrand(brandId, updateRequest);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task DeleteBrand_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var brandId = Guid.NewGuid();
            _mockBrandService.Setup(s => s.DeleteBrandAsync(brandId)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteBrand(brandId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task DeleteBrand_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var brandId = Guid.NewGuid();
            _mockBrandService.Setup(s => s.DeleteBrandAsync(brandId)).ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteBrand(brandId);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }
    }
}