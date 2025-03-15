using CycleShopAPI.Models;
using CycleShopAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CycleShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CycleTypesController : ControllerBase
    {
        private readonly ICycleTypeService _cycleTypeService;

        public CycleTypesController(ICycleTypeService cycleTypeService)
        {
            _cycleTypeService = cycleTypeService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CycleType>>> GetCycleTypes()
        {
            var cycleTypes = await _cycleTypeService.GetAllCycleTypesAsync();
            return Ok(cycleTypes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CycleType>> GetCycleType(Guid id)
        {
            var cycleType = await _cycleTypeService.GetCycleTypeByIdAsync(id);
            if (cycleType == null)
                return NotFound();

            return Ok(cycleType);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<CycleType>> CreateCycleType([FromBody] string name)
        {
            var cycleType = new CycleType
            {
                Name = name,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdCycleType = await _cycleTypeService.CreateCycleTypeAsync(cycleType);
            return CreatedAtAction(nameof(GetCycleType), new { id = createdCycleType.CycleTypeId }, createdCycleType);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateCycleType(Guid id, [FromBody] string name)
        {
            var existingCycleType = await _cycleTypeService.GetCycleTypeByIdAsync(id);
            if (existingCycleType == null)
                return NotFound();

            existingCycleType.Name = name;
            existingCycleType.UpdatedAt = DateTime.UtcNow;

            var updatedCycleType = await _cycleTypeService.UpdateCycleTypeAsync(id, existingCycleType);
            if (updatedCycleType == null)
                return NotFound();

            return Ok(updatedCycleType);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteCycleType(Guid id)
        {
            var result = await _cycleTypeService.DeleteCycleTypeAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}