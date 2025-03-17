using System.ComponentModel.DataAnnotations;

namespace CycleShopAPI.Models.DTOs
{
    public class UpdateBrandDTO
    {
        public string? Name { get; set; }
        
        public string? Description { get; set; }
    }
}