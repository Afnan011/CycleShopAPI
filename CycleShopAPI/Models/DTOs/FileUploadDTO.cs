using Microsoft.AspNetCore.Http;

namespace CycleShopAPI.Models.DTOs
{
    public class FileUploadDTO
    {
        public IFormFile File { get; set; }
        public string? Description { get; set; }
    }
}