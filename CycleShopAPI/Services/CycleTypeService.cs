using CycleShopAPI.Data;
using CycleShopAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CycleShopAPI.Services
{
    public class CycleTypeService : ICycleTypeService
    {
        private readonly CycleShopContext _context;

        public CycleTypeService(CycleShopContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CycleType>> GetAllCycleTypesAsync()
        {
            return await _context.CycleTypes.ToListAsync();
        }

        public async Task<CycleType> GetCycleTypeByIdAsync(Guid cycleTypeId)
        {
            return await _context.CycleTypes.FindAsync(cycleTypeId);
        }

        public async Task<CycleType> CreateCycleTypeAsync(CycleType cycleType)
        {
            _context.CycleTypes.Add(cycleType);
            await _context.SaveChangesAsync();
            return cycleType;
        }

        public async Task<CycleType> UpdateCycleTypeAsync(Guid cycleTypeId, CycleType cycleType)
        {
            var existingCycleType = await _context.CycleTypes.FindAsync(cycleTypeId);
            if (existingCycleType == null)
                return null;

            existingCycleType.Name = cycleType.Name;
            existingCycleType.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingCycleType;
        }

        public async Task<bool> DeleteCycleTypeAsync(Guid cycleTypeId)
        {
            var cycleType = await _context.CycleTypes.FindAsync(cycleTypeId);
            if (cycleType == null)
                return false;

            _context.CycleTypes.Remove(cycleType);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}