using CycleShopAPI.Models;

namespace CycleShopAPI.Services
{
    public interface IBrandService
    {
        Task<IEnumerable<Brand>> GetAllBrandsAsync();
        Task<Brand> GetBrandByIdAsync(Guid brandId);
        Task<Brand> CreateBrandAsync(Brand brand);
        Task<Brand> UpdateBrandAsync(Guid brandId, Brand brand);
        Task<bool> DeleteBrandAsync(Guid brandId);
    }
}