namespace CycleShopAPI.Models.DTOs
{
    public class AuthenticateRequestDTO
    {
        public string UsernameOrEmail { get; set; }
        public string Password { get; set; }
    }
}
