namespace CycleShopAPI.Models.DTOs
{
    public class UpdateUserRequestDTO
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public UserRole? Role { get; set; }
        public bool? IsActive { get; set; }

        public string? ImageUrl { get; set; }
    }
}
