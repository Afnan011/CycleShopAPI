using CycleShopAPI.Data;
using CycleShopAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CycleShopAPI.Services
{
    public class CycleService : ICycleService
    {
        private readonly CycleShopContext _context;

        public CycleService(CycleShopContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Cycle>> GetAllCyclesAsync()
        {
            return await _context.Cycles
                .Include(c => c.Brand)
                .Include(c => c.CycleType)
                .Where(c => !c.DeletedAt.HasValue)
                .ToListAsync();
        }

        public async Task<Cycle> GetCycleByIdAsync(Guid cycleId)
        {
            return await _context.Cycles
                .Include(c => c.Brand)
                .Include(c => c.CycleType)
                .FirstOrDefaultAsync(c => c.CycleId == cycleId && !c.DeletedAt.HasValue);
        }

        public async Task<IEnumerable<Cycle>> GetCyclesByBrandAsync(Guid brandId)
        {
            return await _context.Cycles
                .Include(c => c.Brand)
                .Include(c => c.CycleType)
                .Where(c => c.BrandId == brandId && !c.DeletedAt.HasValue)
                .ToListAsync();
        }

        public async Task<IEnumerable<Cycle>> GetCyclesByTypeAsync(Guid typeId)
        {
            return await _context.Cycles
                .Include(c => c.Brand)
                .Include(c => c.CycleType)
                .Where(c => c.TypeId == typeId && !c.DeletedAt.HasValue)
                .ToListAsync();
        }

        public async Task<Cycle> CreateCycleAsync(Cycle cycle)
        {
            cycle.CreatedAt = DateTime.UtcNow;
            cycle.UpdatedAt = DateTime.UtcNow;
            _context.Cycles.Add(cycle);
            await _context.SaveChangesAsync();
            return cycle;
        }

        public async Task<Cycle> UpdateCycleAsync(Guid cycleId, Cycle cycle)
        {
            var existingCycle = await _context.Cycles.FindAsync(cycleId);
            if (existingCycle == null || existingCycle.DeletedAt.HasValue)
                return null;

            existingCycle.ModelName = cycle.ModelName;
            existingCycle.SKU = cycle.SKU;
            existingCycle.BrandId = cycle.BrandId;
            existingCycle.TypeId = cycle.TypeId;
            existingCycle.Description = cycle.Description;
            existingCycle.Price = cycle.Price;
            existingCycle.CostPrice = cycle.CostPrice;
            existingCycle.IsActive = cycle.IsActive;
            existingCycle.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingCycle;
        }

        public async Task<bool> DeleteCycleAsync(Guid cycleId)
        {
            var cycle = await _context.Cycles.FindAsync(cycleId);
            if (cycle == null || cycle.DeletedAt.HasValue)
                return false;

            cycle.DeletedAt = DateTime.UtcNow;
            cycle.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}