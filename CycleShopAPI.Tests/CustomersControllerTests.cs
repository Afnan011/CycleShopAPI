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
    public class CustomersControllerTests
    {
        private Mock<ICustomerService> _mockCustomerService = null!;
        private Mock<IAddressService> _mockAddressService = null!;
        private CustomersController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _mockCustomerService = new Mock<ICustomerService>();
            _mockAddressService = new Mock<IAddressService>();
            _controller = new CustomersController(_mockCustomerService.Object, _mockAddressService.Object);
        }

        [Test]
        public async Task GetCustomers_ReturnsAllCustomers()
        {
            // Arrange
            var expectedCustomers = new List<Customer>
            {
                new Customer { CustomerId = Guid.NewGuid(), FirstName = "John", LastName = "Doe" },
                new Customer { CustomerId = Guid.NewGuid(), FirstName = "Jane", LastName = "Smith" }
            };
            _mockCustomerService.Setup(s => s.GetAllCustomersAsync()).ReturnsAsync(expectedCustomers);

            // Act
            var result = await _controller.GetCustomers();

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo(expectedCustomers));
        }

        [Test]
        public async Task GetCustomer_WithValidId_ReturnsCustomer()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var expectedCustomer = new Customer 
            { 
                CustomerId = customerId,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com"
            };
            _mockCustomerService.Setup(s => s.GetCustomerByIdAsync(customerId)).ReturnsAsync(expectedCustomer);

            // Act
            var result = await _controller.GetCustomer(customerId);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo(expectedCustomer));
        }

        [Test]
        public async Task CreateCustomer_WithValidData_ReturnsCreatedCustomer()
        {
            // Arrange
            var createRequest = new CreateCustomerDTO
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Phone = "1234567890",
                BillingAddress = new AddressDTO
                {
                    StreetLine1 = "123 Main St",
                    City = "Sample City",
                    State = "ST",
                    PostalCode = "12345",
                    Country = "Country"
                }
            };

            var createdCustomer = new Customer
            {
                CustomerId = Guid.NewGuid(),
                FirstName = createRequest.FirstName,
                LastName = createRequest.LastName,
                Email = createRequest.Email,
                Phone = createRequest.Phone,
                BillingAddress = new Address
                {
                    AddressId = Guid.NewGuid(),
                    StreetLine1 = createRequest.BillingAddress.StreetLine1,
                    City = createRequest.BillingAddress.City,
                    State = createRequest.BillingAddress.State,
                    PostalCode = createRequest.BillingAddress.PostalCode,
                    Country = createRequest.BillingAddress.Country
                }
            };

            _mockCustomerService.Setup(s => s.CreateCustomerAsync(It.IsAny<Customer>()))
                               .ReturnsAsync(createdCustomer);

            // Act
            var result = await _controller.CreateCustomer(createRequest);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
            var createdResult = result.Result as CreatedAtActionResult;
            Assert.That(createdResult, Is.Not.Null);
            Assert.That(createdResult!.Value, Is.EqualTo(createdCustomer));
        }

        [Test]
        public async Task UpdateCustomer_WithValidData_ReturnsUpdatedCustomer()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var updateRequest = new UpdateCustomerDTO
            {
                FirstName = "John Updated",
                LastName = "Doe Updated",
                Email = "john.updated@example.com",
                Phone = "0987654321"
            };

            var existingCustomer = new Customer
            {
                CustomerId = customerId,
                FirstName = "John",
                LastName = "Doe"
            };

            var updatedCustomer = new Customer
            {
                CustomerId = customerId,
                FirstName = updateRequest.FirstName,
                LastName = updateRequest.LastName,
                Email = updateRequest.Email,
                Phone = updateRequest.Phone
            };

            _mockCustomerService.Setup(s => s.GetCustomerByIdAsync(customerId)).ReturnsAsync(existingCustomer);
            _mockCustomerService.Setup(s => s.UpdateCustomerAsync(customerId, It.IsAny<Customer>()))
                               .ReturnsAsync(updatedCustomer);

            // Act
            var result = await _controller.UpdateCustomer(customerId, updateRequest);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo(updatedCustomer));
        }

        [Test]
        public async Task DeleteCustomer_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            _mockCustomerService.Setup(s => s.DeleteCustomerAsync(customerId)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteCustomer(customerId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task UpdateLoyaltyPoints_WithValidData_ReturnsUpdatedCustomer()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var points = 100;
            var updatedCustomer = new Customer
            {
                CustomerId = customerId,
                FirstName = "John",
                LastName = "Doe",
                LoyaltyPoints = points
            };

            _mockCustomerService.Setup(s => s.UpdateCustomerLoyaltyPointsAsync(customerId, points))
                               .ReturnsAsync(updatedCustomer);

            // Act
            var result = await _controller.UpdateLoyaltyPoints(customerId, points);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo(updatedCustomer));
        }
    }
}