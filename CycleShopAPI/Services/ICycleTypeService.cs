using CycleShopAPI.Models;

namespace CycleShopAPI.Services
{
    public interface ICycleTypeService
    {
        Task<IEnumerable<CycleType>> GetAllCycleTypesAsync();
        Task<CycleType> GetCycleTypeByIdAsync(Guid cycleTypeId);
        Task<CycleType> CreateCycleTypeAsync(CycleType cycleType);
        Task<CycleType> UpdateCycleTypeAsync(Guid cycleTypeId, CycleType cycleType);
        Task<bool> DeleteCycleTypeAsync(Guid cycleTypeId);
    }
}