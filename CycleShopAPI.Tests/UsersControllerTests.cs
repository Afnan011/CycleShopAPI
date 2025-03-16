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
    public class UsersControllerTests
    {
        private Mock<IUserService> _mockUserService = null!;
        private UsersController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _mockUserService = new Mock<IUserService>();
            _controller = new UsersController(_mockUserService.Object);
        }

        [Test]
        public async Task GetUsers_ReturnsAllUsers()
        {
            // Arrange
            var expectedUsers = new List<User>
            {
                new User { UserId = Guid.NewGuid(), Username = "test1" },
                new User { UserId = Guid.NewGuid(), Username = "test2" }
            };
            _mockUserService.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(expectedUsers);

            // Act
            var result = await _controller.GetUsers();

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo(expectedUsers));
        }

        [Test]
        public async Task GetUser_WithValidId_ReturnsUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var expectedUser = new User { UserId = userId, Username = "test" };
            _mockUserService.Setup(s => s.GetUserByIdAsync(userId)).ReturnsAsync(expectedUser);

            // Act
            var result = await _controller.GetUser(userId);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo(expectedUser));
        }

        [Test]
        public async Task GetUser_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockUserService.Setup(s => s.GetUserByIdAsync(userId)).ReturnsAsync((User?)null);

            // Act
            var result = await _controller.GetUser(userId);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task CreateUser_WithValidData_ReturnsCreatedUser()
        {
            // Arrange
            var createRequest = new CreateUserRequestDTO 
            { 
                Username = "newuser",
                Email = "newuser@test.com",
                Password = "password123",
                Role = UserRole.employee
            };
            var createdUser = new User 
            { 
                UserId = Guid.NewGuid(),
                Username = createRequest.Username,
                Email = createRequest.Email,
                Role = createRequest.Role.Value
            };
            _mockUserService.Setup(s => s.CreateUserAsync(It.IsAny<User>(), createRequest.Password))
                          .ReturnsAsync(createdUser);

            // Act
            var result = await _controller.CreateUser(createRequest);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
            var createdResult = result.Result as CreatedAtActionResult;
            Assert.That(createdResult, Is.Not.Null);
            Assert.That(createdResult!.Value, Is.EqualTo(createdUser));
        }

        [Test]
        public async Task UpdateUser_WithValidData_ReturnsNoContent()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var updateRequest = new UpdateUserRequestDTO 
            { 
                Username = "updated",
                Email = "updated@test.com"
            };
            var existingUser = new User { UserId = userId, Username = "old" };
            _mockUserService.Setup(s => s.GetUserByIdAsync(userId)).ReturnsAsync(existingUser);
            _mockUserService.Setup(s => s.UpdateUserAsync(It.IsAny<User>())).ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateUser(userId, updateRequest);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task DeleteUser_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockUserService.Setup(s => s.DeleteUserAsync(userId)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task Authenticate_WithValidCredentials_ReturnsToken()
        {
            // Arrange
            var request = new AuthenticateRequestDTO 
            { 
                UsernameOrEmail = "test@test.com",
                Password = "password123"
            };
            var user = new User { Username = "test", Role = UserRole.employee };
            _mockUserService.Setup(s => s.ValidateCredentialsAsync(request.UsernameOrEmail, request.Password))
                          .ReturnsAsync((true, "token", user));

            // Act
            var result = await _controller.Authenticate(request);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            var authResponse = okResult!.Value as AuthResponseDTO;
            Assert.That(authResponse, Is.Not.Null);
            Assert.That(authResponse!.Token, Is.EqualTo("token"));
            Assert.That(authResponse.Username, Is.EqualTo(user.Username));
        }

        [Test]
        public async Task ChangePassword_WithValidData_ReturnsNoContent()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new ChangePasswordRequestDTO 
            { 
                CurrentPassword = "old",
                NewPassword = "new"
            };
            _mockUserService.Setup(s => s.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword))
                          .ReturnsAsync(true);

            // Act
            var result = await _controller.ChangePassword(userId, request);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }
    }
}