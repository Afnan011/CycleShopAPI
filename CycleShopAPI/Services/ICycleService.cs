using CycleShopAPI.Models;

namespace CycleShopAPI.Services
{
    public interface ICycleService
    {
        Task<IEnumerable<Cycle>> GetAllCyclesAsync();
        Task<Cycle> GetCycleByIdAsync(Guid cycleId);
        Task<IEnumerable<Cycle>> GetCyclesByBrandAsync(Guid brandId);
        Task<IEnumerable<Cycle>> GetCyclesByTypeAsync(Guid typeId);
        Task<Cycle> CreateCycleAsync(Cycle cycle);
        Task<Cycle> UpdateCycleAsync(Guid cycleId, Cycle cycle);
        Task<bool> DeleteCycleAsync(Guid cycleId);
    }
}