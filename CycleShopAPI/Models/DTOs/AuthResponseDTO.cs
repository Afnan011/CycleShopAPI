namespace CycleShopAPI.Models.DTOs
{
    public class AuthResponseDTO
    {
        public string Token { get; set; }
        public string Username { get; set; }
        public UserRole Role { get; set; }
    }
}
