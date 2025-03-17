using System.ComponentModel.DataAnnotations;

namespace CycleShopAPI.Models.DTOs
{
    public class CreateInventoryDTO
    {
        [Required]
        public Guid CycleId { get; set; }
        
        [Required]
        public int StockQuantity { get; set; }
        
        public int ReorderThreshold { get; set; } = 5;
        
        [Required]
        public string WarehouseLocation { get; set; }
    }
}