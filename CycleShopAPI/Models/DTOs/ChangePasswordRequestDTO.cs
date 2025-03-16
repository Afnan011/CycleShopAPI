namespace CycleShopAPI.Models.DTOs
{
    public class ChangePasswordRequestDTO
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
