using CycleShopAPI.Data;
using CycleShopAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CycleShopAPI.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly CycleShopContext _context;

        public InventoryService(CycleShopContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Inventory>> GetAllInventoryAsync()
        {
            return await _context.Inventories
                .Include(i => i.Cycle)
                .ThenInclude(c => c.Brand)
                .Include(i => i.Cycle)
                .ThenInclude(c => c.CycleType)
                .ToListAsync();
        }

        public async Task<Inventory> GetInventoryByIdAsync(Guid id)
        {
            return await _context.Inventories
                .Include(i => i.Cycle)
                .ThenInclude(c => c.Brand)
                .Include(i => i.Cycle)
                .ThenInclude(c => c.CycleType)
                .FirstOrDefaultAsync(i => i.InventoryId == id);
        }

        public async Task<Inventory> GetInventoryByCycleIdAsync(Guid cycleId)
        {
            return await _context.Inventories
                .Include(i => i.Cycle)
                .ThenInclude(c => c.Brand)
                .Include(i => i.Cycle)
                .ThenInclude(c => c.CycleType)
                .FirstOrDefaultAsync(i => i.CycleId == cycleId);
        }

        public async Task<Inventory> CreateInventoryAsync(Inventory inventory)
        {
            // Check if inventory for this cycle already exists
            var existingInventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.CycleId == inventory.CycleId);
            
            if (existingInventory != null)
            {
                throw new InvalidOperationException($"Inventory for cycle ID {inventory.CycleId} already exists");
            }

            // Verify cycle exists
            var cycle = await _context.Cycles.FindAsync(inventory.CycleId);
            if (cycle == null)
            {
                throw new InvalidOperationException($"Cycle with ID {inventory.CycleId} does not exist");
            }

            inventory.LastStockUpdate = DateTime.UtcNow;
            await _context.Inventories.AddAsync(inventory);
            
            // Create inventory history record
            var history = new InventoryHistory
            {
                CycleId = inventory.CycleId,
                PreviousQuantity = 0,
                NewQuantity = inventory.StockQuantity,
                ChangeReason = "Initial inventory",
                CreatedAt = DateTime.UtcNow
            };
            await _context.InventoryHistories.AddAsync(history);
            
            await _context.SaveChangesAsync();
            return inventory;
        }

        public async Task<bool> UpdateInventoryAsync(Inventory inventory)
        {
            var existingInventory = await _context.Inventories.FindAsync(inventory.InventoryId);
            if (existingInventory == null)
            {
                return false;
            }

            int oldQuantity = existingInventory.StockQuantity;

            // Update inventory properties
            existingInventory.StockQuantity = inventory.StockQuantity;
            existingInventory.ReorderThreshold = inventory.ReorderThreshold;
            existingInventory.WarehouseLocation = inventory.WarehouseLocation;
            existingInventory.LastStockUpdate = DateTime.UtcNow;

            _context.Inventories.Update(existingInventory);
            
            // Create inventory history record
            var history = new InventoryHistory
            {
                CycleId = existingInventory.CycleId,
                PreviousQuantity = oldQuantity,
                NewQuantity = inventory.StockQuantity,
                ChangeReason = "Manual inventory update",
                CreatedAt = DateTime.UtcNow
            };
            await _context.InventoryHistories.AddAsync(history);
            
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateStockQuantityAsync(Guid cycleId, int quantityChange)
        {
            var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.CycleId == cycleId);
            if (inventory == null)
            {
                return false;
            }

            int oldQuantity = inventory.StockQuantity;
            inventory.StockQuantity += quantityChange;
            
            // Prevent negative inventory
            if (inventory.StockQuantity < 0)
            {
                inventory.StockQuantity = 0;
                quantityChange = -oldQuantity;
            }
            
            inventory.LastStockUpdate = DateTime.UtcNow;
            
            // Create inventory history record
            var history = new InventoryHistory
            {
                CycleId = cycleId,
                PreviousQuantity = oldQuantity,
                NewQuantity = inventory.StockQuantity,
                ChangeReason = quantityChange > 0 ? "Stock added" : "Stock removed",
                CreatedAt = DateTime.UtcNow
            };
            
            await _context.InventoryHistories.AddAsync(history);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<Inventory>> GetLowStockInventoryAsync(int threshold = 0)
        {
            return await _context.Inventories
                .Include(i => i.Cycle)
                .ThenInclude(c => c.Brand)
                .Include(i => i.Cycle)
                .ThenInclude(c => c.CycleType)
                .Where(i => threshold > 0 
                    ? i.StockQuantity <= threshold 
                    : i.StockQuantity <= i.ReorderThreshold)
                .ToListAsync();
        }

        public async Task<IEnumerable<InventoryHistory>> GetInventoryHistoryAsync(Guid cycleId)
        {
            return await _context.InventoryHistories
                .Include(ih => ih.Cycle)
                .ThenInclude(c => c.Brand)
                .Where(ih => ih.CycleId == cycleId)
                .OrderByDescending(ih => ih.CreatedAt)
                .ToListAsync();
        }
    }
}