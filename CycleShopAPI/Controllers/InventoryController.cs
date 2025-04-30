using CycleShopAPI.Models;
using CycleShopAPI.Models.DTOs;
using CycleShopAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CycleShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Inventory>>> GetAllInventory()
        {
            var inventory = await _inventoryService.GetAllInventoryAsync();
            return Ok(inventory);
        }

        [HttpGet("{id}")]
        [AllowAnonymous] 
        public async Task<ActionResult<Inventory>> GetInventory(Guid id)
        {
            var inventory = await _inventoryService.GetInventoryByIdAsync(id);
            
            if (inventory == null)
                return NotFound();
                
            return Ok(inventory);
        }

        [HttpGet("cycle/{cycleId}")]
        [AllowAnonymous]
        public async Task<ActionResult<Inventory>> GetInventoryByCycleId(Guid cycleId)
        {
            var inventory = await _inventoryService.GetInventoryByCycleIdAsync(cycleId);
            
            if (inventory == null)
                return NotFound();
                
            return Ok(inventory);
        }

        [HttpPost]
        [Authorize(Roles = "admin")] 
        public async Task<ActionResult<Inventory>> CreateInventory(CreateInventoryDTO createInventoryDto)
        {
            try
            {
                var createdInventory = await _inventoryService.CreateInventoryAsync(createInventoryDto);
                return CreatedAtAction(nameof(GetInventory), new { id = createdInventory.InventoryId }, createdInventory);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin,employee")]
        public async Task<IActionResult> UpdateInventory(Guid id, UpdateInventoryDTO updateInventoryDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _inventoryService.UpdateInventoryAsync(id, updateInventoryDto);
                if (result)
                    return NoContent();

                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("stock/{cycleId}")]
        [Authorize(Roles = "admin,employee")] 
        public async Task<IActionResult> UpdateStock(Guid cycleId, StockUpdateRequestDTO request)
        {
            try
            {
                var result = await _inventoryService.UpdateStockQuantityAsync(cycleId, request.QuantityChange);
                if (result)
                    return NoContent();

                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("low-stock")]
        [Authorize(Roles = "admin,employee")] 
        public async Task<ActionResult<IEnumerable<Inventory>>> GetLowStockInventory([FromQuery] int? threshold)
        {
            var inventory = await _inventoryService.GetLowStockInventoryAsync(threshold ?? 0);
            return Ok(inventory);
        }

        [HttpGet("history/{cycleId}")]
        [Authorize(Roles = "admin,employee")] 
        public async Task<ActionResult<IEnumerable<InventoryHistory>>> GetInventoryHistory(Guid cycleId)
        {
            var history = await _inventoryService.GetInventoryHistoryAsync(cycleId);
            return Ok(history);
        }
    }
}