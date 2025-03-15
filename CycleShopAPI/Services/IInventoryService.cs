using CycleShopAPI.Models;

namespace CycleShopAPI.Services
{
    public interface IInventoryService
    {
        Task<IEnumerable<Inventory>> GetAllInventoryAsync();
        Task<Inventory> GetInventoryByIdAsync(Guid id);
        Task<Inventory> GetInventoryByCycleIdAsync(Guid cycleId);
        Task<Inventory> CreateInventoryAsync(Inventory inventory);
        Task<bool> UpdateInventoryAsync(Inventory inventory);
        Task<bool> UpdateStockQuantityAsync(Guid cycleId, int quantityChange);
        Task<IEnumerable<Inventory>> GetLowStockInventoryAsync(int threshold = 0);
        Task<IEnumerable<InventoryHistory>> GetInventoryHistoryAsync(Guid cycleId);
    }
}