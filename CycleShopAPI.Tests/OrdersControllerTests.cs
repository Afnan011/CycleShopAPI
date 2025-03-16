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
    public class OrdersControllerTests
    {
        private Mock<IOrderService> _mockOrderService = null!;
        private OrdersController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _mockOrderService = new Mock<IOrderService>();
            _controller = new OrdersController(_mockOrderService.Object);
        }

        [Test]
        public async Task GetAllOrders_ReturnsAllOrders()
        {
            // Arrange
            var expectedOrders = new List<Order>
            {
                new Order { OrderId = Guid.NewGuid(), CustomerId = Guid.NewGuid() },
                new Order { OrderId = Guid.NewGuid(), CustomerId = Guid.NewGuid() }
            };
            _mockOrderService.Setup(s => s.GetAllOrdersAsync()).ReturnsAsync(expectedOrders);

            // Act
            var result = await _controller.GetAllOrders();

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo(expectedOrders));
        }

        [Test]
        public async Task GetOrder_WithValidId_ReturnsOrder()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var expectedOrder = new Order { OrderId = orderId, CustomerId = Guid.NewGuid() };
            _mockOrderService.Setup(s => s.GetOrderByIdAsync(orderId)).ReturnsAsync(expectedOrder);

            // Act
            var result = await _controller.GetOrder(orderId);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo(expectedOrder));
        }

        [Test]
        public async Task GetOrdersByCustomer_ReturnsCustomerOrders()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var expectedOrders = new List<Order>
            {
                new Order { OrderId = Guid.NewGuid(), CustomerId = customerId },
                new Order { OrderId = Guid.NewGuid(), CustomerId = customerId }
            };
            _mockOrderService.Setup(s => s.GetOrdersByCustomerIdAsync(customerId)).ReturnsAsync(expectedOrders);

            // Act
            var result = await _controller.GetOrdersByCustomer(customerId);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo(expectedOrders));
        }

        [Test]
        public async Task CreateOrder_WithValidData_ReturnsCreatedOrder()
        {
            // Arrange
            var createRequest = new CreateOrderRequestDTO
            {
                CustomerId = Guid.NewGuid(),
                EmployeeId = Guid.NewGuid(),
                ShippingAddressId = Guid.NewGuid(),
                Items = new List<OrderItem>
                {
                    new OrderItem { CycleId = Guid.NewGuid(), Quantity = 1 }
                }
            };

            var createdOrder = new Order
            {
                OrderId = Guid.NewGuid(),
                CustomerId = createRequest.CustomerId,
                EmployeeId = createRequest.EmployeeId,
                ShippingAddressId = createRequest.ShippingAddressId
            };

            _mockOrderService.Setup(s => s.CreateOrderAsync(It.IsAny<Order>(), It.IsAny<List<OrderItem>>()))
                           .ReturnsAsync(createdOrder);

            // Act
            var result = await _controller.CreateOrder(createRequest);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
            var createdResult = result.Result as CreatedAtActionResult;
            Assert.That(createdResult, Is.Not.Null);
            Assert.That(createdResult!.Value, Is.EqualTo(createdOrder));
        }

        [Test]
        public async Task UpdateOrder_WithValidData_ReturnsNoContent()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var updateRequest = new UpdateOrderRequestDTO
            {
                Status = OrderStatus.processing,
                ShippingAddressId = Guid.NewGuid()
            };

            var existingOrder = new Order { OrderId = orderId };
            _mockOrderService.Setup(s => s.GetOrderByIdAsync(orderId)).ReturnsAsync(existingOrder);
            _mockOrderService.Setup(s => s.UpdateOrderAsync(It.IsAny<Order>())).ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateOrder(orderId, updateRequest);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task UpdateOrderStatus_WithValidData_ReturnsNoContent()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var request = new UpdateOrderStatusRequest { Status = OrderStatus.completed };
            _mockOrderService.Setup(s => s.UpdateOrderStatusAsync(orderId, request.Status)).ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateOrderStatus(orderId, request);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task CancelOrder_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            _mockOrderService.Setup(s => s.CancelOrderAsync(orderId)).ReturnsAsync(true);

            // Act
            var result = await _controller.CancelOrder(orderId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task DeleteOrder_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            _mockOrderService.Setup(s => s.DeleteOrderAsync(orderId)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteOrder(orderId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task GetOrderItems_ReturnsOrderItems()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var expectedItems = new List<OrderItem>
            {
                new OrderItem { OrderId = orderId, CycleId = Guid.NewGuid() },
                new OrderItem { OrderId = orderId, CycleId = Guid.NewGuid() }
            };
            _mockOrderService.Setup(s => s.GetOrderItemsByOrderIdAsync(orderId)).ReturnsAsync(expectedItems);

            // Act
            var result = await _controller.GetOrderItems(orderId);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo(expectedItems));
        }

        [Test]
        public async Task CalculateOrderTotal_ReturnsTotal()
        {
            // Arrange
            var items = new List<OrderItem>
            {
                new OrderItem { CycleId = Guid.NewGuid(), Quantity = 2, UnitPrice = 100 }
            };
            var expectedTotal = 200m;
            _mockOrderService.Setup(s => s.CalculateOrderTotalAsync(items)).ReturnsAsync(expectedTotal);

            // Act
            var result = await _controller.CalculateOrderTotal(items);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo(expectedTotal));
        }
    }
}