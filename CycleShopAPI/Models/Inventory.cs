namespace CycleShopAPI.Models
{
    public class Inventory
    {
        public Guid InventoryId { get; set; } = Guid.NewGuid();
        public Guid CycleId { get; set; }
        public Cycle Cycle { get; set; }
        public int StockQuantity { get; set; }
        public int ReorderThreshold { get; set; } = 5;
        public string WarehouseLocation { get; set; }
        public DateTime LastStockUpdate { get; set; } = DateTime.UtcNow;
    }
}
