using CycleShopAPI.Models;
using CycleShopAPI.Models.DTOs;

namespace CycleShopAPI.Services
{
    public interface IInventoryService
    {
        Task<IEnumerable<Inventory>> GetAllInventoryAsync();
        Task<Inventory> GetInventoryByIdAsync(Guid id);
        Task<Inventory> GetInventoryByCycleIdAsync(Guid cycleId);
        Task<Inventory> CreateInventoryAsync(CreateInventoryDTO inventoryDto);
        Task<bool> UpdateInventoryAsync(Guid id, UpdateInventoryDTO inventory);
        Task<bool> UpdateStockQuantityAsync(Guid cycleId, int quantityChange);
        Task<IEnumerable<Inventory>> GetLowStockInventoryAsync(int threshold = 0);
        Task<IEnumerable<InventoryHistory>> GetInventoryHistoryAsync(Guid cycleId);
    }
}