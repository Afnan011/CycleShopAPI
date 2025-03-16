using NUnit.Framework;
using Moq;
using CycleShopAPI.Controllers;
using CycleShopAPI.Services;
using CycleShopAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CycleShopAPI.Tests
{
    [TestFixture]
    public class AddressesControllerTests
    {
        private Mock<IAddressService> _mockAddressService = null!;
        private AddressesController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _mockAddressService = new Mock<IAddressService>();
            _controller = new AddressesController(_mockAddressService.Object);
        }

        [Test]
        public async Task GetAddress_WithValidId_ReturnsAddress()
        {
            // Arrange
            var addressId = Guid.NewGuid();
            var expectedAddress = new Address 
            { 
                AddressId = addressId,
                StreetLine1 = "123 Main St",
                City = "Test City",
                State = "TS",
                PostalCode = "12345",
                Country = "Test Country"
            };
            _mockAddressService.Setup(s => s.GetAddressByIdAsync(addressId)).ReturnsAsync(expectedAddress);

            // Act
            var result = await _controller.GetAddress(addressId);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo(expectedAddress));
        }

        [Test]
        public async Task GetAddress_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var addressId = Guid.NewGuid();
            _mockAddressService.Setup(s => s.GetAddressByIdAsync(addressId))!.ReturnsAsync((Address?)null);

            // Act
            var result = await _controller.GetAddress(addressId);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task CreateAddress_WithValidData_ReturnsCreatedAddress()
        {
            // Arrange
            var address = new Address
            {
                StreetLine1 = "123 Main St",
                StreetLine2 = "Apt 4B",
                City = "Test City",
                State = "TS",
                PostalCode = "12345",
                Country = "Test Country"
            };

            var createdAddress = new Address
            {
                AddressId = Guid.NewGuid(),
                StreetLine1 = address.StreetLine1,
                StreetLine2 = address.StreetLine2,
                City = address.City,
                State = address.State,
                PostalCode = address.PostalCode,
                Country = address.Country
            };

            _mockAddressService.Setup(s => s.CreateAddressAsync(It.IsAny<Address>()))
                              .ReturnsAsync(createdAddress);

            // Act
            var result = await _controller.CreateAddress(address);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
            var createdResult = result.Result as CreatedAtActionResult;
            Assert.That(createdResult, Is.Not.Null);
            Assert.That(createdResult!.Value, Is.EqualTo(createdAddress));
        }

        [Test]
        public async Task UpdateAddress_WithValidData_ReturnsUpdatedAddress()
        {
            // Arrange
            var addressId = Guid.NewGuid();
            var address = new Address
            {
                AddressId = addressId,
                StreetLine1 = "456 Updated St",
                City = "Updated City",
                State = "UC",
                PostalCode = "54321",
                Country = "Updated Country"
            };

            var updatedAddress = new Address
            {
                AddressId = addressId,
                StreetLine1 = address.StreetLine1,
                City = address.City,
                State = address.State,
                PostalCode = address.PostalCode,
                Country = address.Country
            };

            _mockAddressService.Setup(s => s.UpdateAddressAsync(addressId, It.IsAny<Address>()))
                              .ReturnsAsync(updatedAddress);

            // Act
            var result = await _controller.UpdateAddress(addressId, address);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo(updatedAddress));
        }

        [Test]
        public async Task UpdateAddress_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var addressId = Guid.NewGuid();
            var address = new Address
            {
                AddressId = addressId,
                StreetLine1 = "456 Updated St",
                City = "Updated City"
            };

            _mockAddressService.Setup(s => s.UpdateAddressAsync(addressId, It.IsAny<Address>()))!
                              .ReturnsAsync((Address?)null);

            // Act
            var result = await _controller.UpdateAddress(addressId, address);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task DeleteAddress_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var addressId = Guid.NewGuid();
            _mockAddressService.Setup(s => s.DeleteAddressAsync(addressId)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteAddress(addressId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task DeleteAddress_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var addressId = Guid.NewGuid();
            _mockAddressService.Setup(s => s.DeleteAddressAsync(addressId)).ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteAddress(addressId);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }
    }
}