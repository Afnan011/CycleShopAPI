using CycleShopAPI.Models;
using CycleShopAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CycleShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AddressesController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public AddressesController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Address>> GetAddress(Guid id)
        {
            var address = await _addressService.GetAddressByIdAsync(id);
            if (address == null)
                return NotFound();

            return Ok(address);
        }

        [HttpPost]
        public async Task<ActionResult<Address>> CreateAddress(Address address)
        {
            var createdAddress = await _addressService.CreateAddressAsync(address);
            return CreatedAtAction(nameof(GetAddress), new { id = createdAddress.AddressId }, createdAddress);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAddress(Guid id, Address address)
        {
            var updatedAddress = await _addressService.UpdateAddressAsync(id, address);
            if (updatedAddress == null)
                return NotFound();

            return Ok(updatedAddress);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAddress(Guid id)
        {
            var result = await _addressService.DeleteAddressAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}