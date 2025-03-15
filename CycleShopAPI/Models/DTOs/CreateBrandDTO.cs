using System.ComponentModel.DataAnnotations;

namespace CycleShopAPI.Models.DTOs
{
    public class CreateBrandDTO
    {
        [Required]
        public string Name { get; set; }
        
        [Required]
        public string Description { get; set; }
    }
}