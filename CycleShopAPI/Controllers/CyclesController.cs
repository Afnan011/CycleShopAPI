using CycleShopAPI.Models;
using CycleShopAPI.Models.DTOs;
using CycleShopAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace CycleShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CyclesController : ControllerBase
    {
        private readonly ICycleService _cycleService;
        private readonly IBrandService _brandService;
        private readonly ICycleTypeService _cycleTypeService;

        public CyclesController(
            ICycleService cycleService, 
            IBrandService brandService, 
            ICycleTypeService cycleTypeService)
        {
            _cycleService = cycleService;
            _brandService = brandService;
            _cycleTypeService = cycleTypeService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cycle>>> GetCycles()
        {
            var cycles = await _cycleService.GetAllCyclesAsync();
            return Ok(cycles);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Cycle>> GetCycle(Guid id)
        {
            var cycle = await _cycleService.GetCycleByIdAsync(id);
            if (cycle == null)
                return NotFound();

            return Ok(cycle);
        }

        [HttpGet("brand/{brandId}")]
        public async Task<ActionResult<IEnumerable<Cycle>>> GetCyclesByBrand(Guid brandId)
        {
            var brand = await _brandService.GetBrandByIdAsync(brandId);
            if (brand == null)
                return NotFound("Brand not found");

            var cycles = await _cycleService.GetCyclesByBrandAsync(brandId);
            return Ok(cycles);
        }

        [HttpGet("type/{typeId}")]
        public async Task<ActionResult<IEnumerable<Cycle>>> GetCyclesByType(Guid typeId)
        {
            var cycleType = await _cycleTypeService.GetCycleTypeByIdAsync(typeId);
            if (cycleType == null)
                return NotFound("Cycle type not found");

            var cycles = await _cycleService.GetCyclesByTypeAsync(typeId);
            return Ok(cycles);
        }

        [HttpPost]
        [Authorize(Roles = "admin,employee")]
        public async Task<ActionResult<Cycle>> CreateCycle(CreateCycleDTO createCycleDto)
        {
            var brand = await _brandService.GetBrandByIdAsync(createCycleDto.BrandId);
            if (brand == null)
                return BadRequest("Invalid brand ID");

            var cycleType = await _cycleTypeService.GetCycleTypeByIdAsync(createCycleDto.TypeId);
            if (cycleType == null)
                return BadRequest("Invalid cycle type ID");

            var cycle = new Cycle
            {
                SKU = createCycleDto.SKU,  
                ModelName = createCycleDto.ModelName,
                BrandId = createCycleDto.BrandId,
                TypeId = createCycleDto.TypeId,
                Description = createCycleDto.Description,
                Price = createCycleDto.Price,
                CostPrice = createCycleDto.CostPrice,
                IsActive = createCycleDto.IsActive,
                ImageUrl = createCycleDto.ImageUrl
            };

            var createdCycle = await _cycleService.CreateCycleAsync(cycle);
            return CreatedAtAction(nameof(GetCycle), new { id = createdCycle.CycleId }, createdCycle);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin,employee")]
        public async Task<IActionResult> UpdateCycle(Guid id, UpdateCycleDTO updateCycleDto)
        {

            if (updateCycleDto == null)
                return BadRequest("Request cannot be null");

            var existingCycle = await _cycleService.GetCycleByIdAsync(id);
            if (existingCycle == null)
                return NotFound();

            existingCycle.SKU = updateCycleDto.SKU ?? existingCycle.SKU;
            existingCycle.ModelName = updateCycleDto.ModelName ?? existingCycle.ModelName;
            existingCycle.BrandId = updateCycleDto.BrandId ?? existingCycle.BrandId;
            existingCycle.TypeId = updateCycleDto.TypeId ?? existingCycle.TypeId;
            existingCycle.Description = updateCycleDto.Description ?? existingCycle.Description;
            existingCycle.Price = updateCycleDto.Price ?? existingCycle.Price;
            existingCycle.CostPrice = updateCycleDto.CostPrice ?? existingCycle.CostPrice;
            existingCycle.IsActive = updateCycleDto.IsActive ?? existingCycle.IsActive;
            existingCycle.ImageUrl = updateCycleDto.ImageUrl ?? existingCycle.ImageUrl;

            var updatedCycle = await _cycleService.UpdateCycleAsync(id, existingCycle);
            if (updatedCycle == null)
                return NotFound();

            return Ok(updatedCycle);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteCycle(Guid id)
        {
            var result = await _cycleService.DeleteCycleAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}