using CycleShopAPI.Models;
using CycleShopAPI.Models.DTOs;
using CycleShopAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CycleShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<User>> GetUser(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            
            if (user == null)
                return NotFound();
                
            return Ok(user);
        }

        [HttpGet("by-username/{username}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<User>> GetUserByUsername(string username)
        {
            var user = await _userService.GetUserByUsernameAsync(username);
            
            if (user == null)
                return NotFound();
                
            return Ok(user);
        }

        [HttpGet("by-email/{email}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<User>> GetUserByEmail(string email)
        {
            var user = await _userService.GetUserByEmailAsync(email);
            
            if (user == null)
                return NotFound();
                
            return Ok(user);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<User>> CreateUser(CreateUserRequestDTO request)
        {
            try
            {
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                Role = request.Role ?? UserRole.employee,
                IsActive = true
            };
            
            var createdUser = await _userService.CreateUserAsync(user, request.Password);
            return CreatedAtAction(nameof(GetUser), new { id = createdUser.UserId }, createdUser);
            }
            catch (InvalidOperationException ex)
            {
            return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateUser(Guid id, UpdateUserRequestDTO request)
        {
            var existingUser = await _userService.GetUserByIdAsync(id);
            if (existingUser == null)
                return NotFound();

            if (request == null)
                return BadRequest("Request cannot be null");

            existingUser.Username = request.Username ?? existingUser.Username;
            existingUser.Email = request.Email ?? existingUser.Email;
            existingUser.Role = request.Role ?? existingUser.Role;
            existingUser.IsActive = request.IsActive ?? existingUser.IsActive;

            try{
                var result = await _userService.UpdateUserAsync(existingUser);
                if (result)
                    return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            
            return BadRequest("Failed to update user");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (result)
                return NoContent();
                
            return NotFound();
        }

        [HttpPost("authenticate")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDTO>> Authenticate(AuthenticateRequestDTO request)
        {
            var (success, token, user) = await _userService.ValidateCredentialsAsync(request.UsernameOrEmail, request.Password);
            
            if (!success)
                return Unauthorized(new { message = "Username/email or password is incorrect" });

            var response = new AuthResponseDTO
            {
                Token = token,
                Username = user.Username,
                Role = user.Role
            };
                
            return Ok(response);
        }

        [HttpPut("{id}/change-password")]
        public async Task<IActionResult> ChangePassword(Guid id, ChangePasswordRequestDTO request)
        {
            if (request == null)
            {
                return BadRequest("Request cannot be null");
            }

            var success = await _userService.ChangePasswordAsync(id, request.CurrentPassword, request.NewPassword);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}