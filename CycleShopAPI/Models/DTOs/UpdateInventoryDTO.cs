namespace CycleShopAPI.Models.DTOs
{
    public class UpdateInventoryDTO
    {
        public int? StockQuantity { get; set; }
        public int? ReorderThreshold { get; set; }
        public string? WarehouseLocation { get; set; }
    }
}