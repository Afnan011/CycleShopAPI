using CycleShopAPI.Models;

namespace CycleShopAPI.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(Guid id);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User> CreateUserAsync(User user, string password);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(Guid id);
        Task<(bool success, string? token, User? user)> ValidateCredentialsAsync(string usernameOrEmail, string password);
        Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
        string GenerateJwtToken(User user);
    }
}