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
    public class InventoryControllerTests
    {
        private Mock<IInventoryService> _mockInventoryService = null!;
        private InventoryController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _mockInventoryService = new Mock<IInventoryService>();
            _controller = new InventoryController(_mockInventoryService.Object);
        }

        [Test]
        public async Task GetAllInventory_ReturnsAllInventory()
        {
            // Arrange
            var expectedInventory = new List<Inventory>
            {
                new Inventory { InventoryId = Guid.NewGuid(), CycleId = Guid.NewGuid(), StockQuantity = 10 },
                new Inventory { InventoryId = Guid.NewGuid(), CycleId = Guid.NewGuid(), StockQuantity = 15 }
            };
            _mockInventoryService.Setup(s => s.GetAllInventoryAsync()).ReturnsAsync(expectedInventory);

            // Act
            var result = await _controller.GetAllInventory();

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo(expectedInventory));
        }

        [Test]
        public async Task GetInventory_WithValidId_ReturnsInventory()
        {
            // Arrange
            var inventoryId = Guid.NewGuid();
            var expectedInventory = new Inventory 
            { 
                InventoryId = inventoryId,
                CycleId = Guid.NewGuid(),
                StockQuantity = 10
            };
            _mockInventoryService.Setup(s => s.GetInventoryByIdAsync(inventoryId)).ReturnsAsync(expectedInventory);

            // Act
            var result = await _controller.GetInventory(inventoryId);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo(expectedInventory));
        }

        [Test]
        public async Task GetInventoryByCycleId_WithValidId_ReturnsInventory()
        {
            // Arrange
            var cycleId = Guid.NewGuid();
            var expectedInventory = new Inventory 
            { 
                InventoryId = Guid.NewGuid(),
                CycleId = cycleId,
                StockQuantity = 10
            };
            _mockInventoryService.Setup(s => s.GetInventoryByCycleIdAsync(cycleId)).ReturnsAsync(expectedInventory);

            // Act
            var result = await _controller.GetInventoryByCycleId(cycleId);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo(expectedInventory));
        }

        [Test]
        public async Task CreateInventory_WithValidData_ReturnsCreatedInventory()
        {
            // Arrange
            var inventory = new Inventory
            {
                CycleId = Guid.NewGuid(),
                StockQuantity = 20,
                ReorderThreshold = 5,
                WarehouseLocation = "A1"
            };

            var createdInventory = new Inventory
            {
                InventoryId = Guid.NewGuid(),
                CycleId = inventory.CycleId,
                StockQuantity = inventory.StockQuantity,
                ReorderThreshold = inventory.ReorderThreshold,
                WarehouseLocation = inventory.WarehouseLocation,
                LastStockUpdate = DateTime.UtcNow
            };

            _mockInventoryService.Setup(s => s.CreateInventoryAsync(It.IsAny<Inventory>()))
                                .ReturnsAsync(createdInventory);

            // Act
            var result = await _controller.CreateInventory(inventory);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
            var createdResult = result.Result as CreatedAtActionResult;
            Assert.That(createdResult, Is.Not.Null);
            Assert.That(createdResult!.Value, Is.EqualTo(createdInventory));
        }

        [Test]
        public async Task UpdateInventory_WithValidData_ReturnsNoContent()
        {
            // Arrange
            var inventoryId = Guid.NewGuid();
            var inventory = new Inventory
            {
                InventoryId = inventoryId,
                CycleId = Guid.NewGuid(),
                StockQuantity = 25
            };

            _mockInventoryService.Setup(s => s.UpdateInventoryAsync(It.IsAny<Inventory>()))
                                .ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateInventory(inventoryId, inventory);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task UpdateStock_WithValidData_ReturnsNoContent()
        {
            // Arrange
            var cycleId = Guid.NewGuid();
            var request = new StockUpdateRequestDTO { QuantityChange = 5 };
            _mockInventoryService.Setup(s => s.UpdateStockQuantityAsync(cycleId, request.QuantityChange))
                                .ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateStock(cycleId, request);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task GetLowStockInventory_ReturnsFilteredInventory()
        {
            // Arrange
            var threshold = 10;
            var expectedInventory = new List<Inventory>
            {
                new Inventory { InventoryId = Guid.NewGuid(), StockQuantity = 5 },
                new Inventory { InventoryId = Guid.NewGuid(), StockQuantity = 8 }
            };
            _mockInventoryService.Setup(s => s.GetLowStockInventoryAsync(threshold))
                                .ReturnsAsync(expectedInventory);

            // Act
            var result = await _controller.GetLowStockInventory(threshold);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo(expectedInventory));
        }

        [Test]
        public async Task GetInventoryHistory_ReturnsHistory()
        {
            // Arrange
            var cycleId = Guid.NewGuid();
            var expectedHistory = new List<InventoryHistory>
            {
                new InventoryHistory 
                { 
                    HistoryId = Guid.NewGuid(), 
                    CycleId = cycleId,
                    PreviousQuantity = 10,
                    NewQuantity = 15,
                    ChangeReason = "Stock Update"
                },
                new InventoryHistory 
                { 
                    HistoryId = Guid.NewGuid(), 
                    CycleId = cycleId,
                    PreviousQuantity = 15,
                    NewQuantity = 13,
                    ChangeReason = "Order Fulfillment"
                }
            };
            _mockInventoryService.Setup(s => s.GetInventoryHistoryAsync(cycleId))
                                .ReturnsAsync(expectedHistory);

            // Act
            var result = await _controller.GetInventoryHistory(cycleId);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo(expectedHistory));
        }
    }
}