using CycleShopAPI.Models;
using CycleShopAPI.Models.DTOs;
using CycleShopAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CycleShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BrandsController : ControllerBase
    {
        private readonly IBrandService _brandService;

        public BrandsController(IBrandService brandService)
        {
            _brandService = brandService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Brand>>> GetBrands()
        {
            var brands = await _brandService.GetAllBrandsAsync();
            return Ok(brands);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Brand>> GetBrand(Guid id)
        {
            var brand = await _brandService.GetBrandByIdAsync(id);
            if (brand == null)
                return NotFound();

            return Ok(brand);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<Brand>> CreateBrand(CreateBrandDTO createBrandDto)
        {
            var brand = new Brand
            {
                Name = createBrandDto.Name,
                Description = createBrandDto.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdBrand = await _brandService.CreateBrandAsync(brand);
            return CreatedAtAction(nameof(GetBrand), new { id = createdBrand.BrandId }, createdBrand);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateBrand(Guid id, CreateBrandDTO updateBrandDto)
        {
            var existingBrand = await _brandService.GetBrandByIdAsync(id);
            if (existingBrand == null)
                return NotFound();

            existingBrand.Name = updateBrandDto.Name;
            existingBrand.Description = updateBrandDto.Description;
            existingBrand.UpdatedAt = DateTime.UtcNow;

            var updatedBrand = await _brandService.UpdateBrandAsync(id, existingBrand);
            if (updatedBrand == null)
                return NotFound();

            return Ok(updatedBrand);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteBrand(Guid id)
        {
            var result = await _brandService.DeleteBrandAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}