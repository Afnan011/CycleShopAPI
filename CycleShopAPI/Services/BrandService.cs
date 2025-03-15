using CycleShopAPI.Data;
using CycleShopAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CycleShopAPI.Services
{
    public class BrandService : IBrandService
    {
        private readonly CycleShopContext _context;

        public BrandService(CycleShopContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Brand>> GetAllBrandsAsync()
        {
            return await _context.Brands.ToListAsync();
        }

        public async Task<Brand> GetBrandByIdAsync(Guid brandId)
        {
            return await _context.Brands.FindAsync(brandId);
        }

        public async Task<Brand> CreateBrandAsync(Brand brand)
        {
            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();
            return brand;
        }

        public async Task<Brand> UpdateBrandAsync(Guid brandId, Brand brand)
        {
            var existingBrand = await _context.Brands.FindAsync(brandId);
            if (existingBrand == null)
                return null;

            existingBrand.Name = brand.Name;
            existingBrand.Description = brand.Description;
            existingBrand.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingBrand;
        }

        public async Task<bool> DeleteBrandAsync(Guid brandId)
        {
            var brand = await _context.Brands.FindAsync(brandId);
            if (brand == null)
                return false;

            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}