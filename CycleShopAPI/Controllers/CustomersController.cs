using CycleShopAPI.Models;
using CycleShopAPI.Models.DTOs;
using CycleShopAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CycleShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly IAddressService _addressService;

        public CustomersController(ICustomerService customerService, IAddressService addressService)
        {
            _customerService = customerService;
            _addressService = addressService;
        }

        [HttpGet]
        [Authorize(Roles = "admin,employee")]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return Ok(customers);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "admin,employee")]
        public async Task<ActionResult<Customer>> GetCustomer(Guid id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
                return NotFound();

            return Ok(customer);
        }

        [HttpPost]
        [Authorize(Roles = "admin,employee")]
        public async Task<ActionResult<Customer>> CreateCustomer(CreateCustomerDTO createCustomerDto)
        {
            var customer = new Customer
            {
                FirstName = createCustomerDto.FirstName,
                LastName = createCustomerDto.LastName,
                Email = createCustomerDto.Email,
                Phone = createCustomerDto.Phone
            };

            // Create billing address if provided
            if (createCustomerDto.BillingAddress != null)
            {
                var billingAddress = new Address
                {
                    StreetLine1 = createCustomerDto.BillingAddress.StreetLine1,
                    StreetLine2 = createCustomerDto.BillingAddress.StreetLine2,
                    City = createCustomerDto.BillingAddress.City,
                    State = createCustomerDto.BillingAddress.State,
                    PostalCode = createCustomerDto.BillingAddress.PostalCode,
                    Country = createCustomerDto.BillingAddress.Country
                };
                customer.BillingAddress = billingAddress;
            }

            // Create shipping address if provided
            if (createCustomerDto.ShippingAddress != null)
            {
                var shippingAddress = new Address
                {
                    StreetLine1 = createCustomerDto.ShippingAddress.StreetLine1,
                    StreetLine2 = createCustomerDto.ShippingAddress.StreetLine2,
                    City = createCustomerDto.ShippingAddress.City,
                    State = createCustomerDto.ShippingAddress.State,
                    PostalCode = createCustomerDto.ShippingAddress.PostalCode,
                    Country = createCustomerDto.ShippingAddress.Country
                };
                customer.ShippingAddress = shippingAddress;
            }

            var createdCustomer = await _customerService.CreateCustomerAsync(customer);
            return CreatedAtAction(nameof(GetCustomer), new { id = createdCustomer.CustomerId }, createdCustomer);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin,employee")]
        public async Task<IActionResult> UpdateCustomer(Guid id, CreateCustomerDTO updateCustomerDto)
        {
            var existingCustomer = await _customerService.GetCustomerByIdAsync(id);
            if (existingCustomer == null)
                return NotFound();

            existingCustomer.FirstName = updateCustomerDto.FirstName;
            existingCustomer.LastName = updateCustomerDto.LastName;
            existingCustomer.Email = updateCustomerDto.Email;
            existingCustomer.Phone = updateCustomerDto.Phone;

            // Update billing address
            if (updateCustomerDto.BillingAddress != null)
            {
                if (existingCustomer.BillingAddress == null)
                {
                    existingCustomer.BillingAddress = new Address();
                }
                existingCustomer.BillingAddress.StreetLine1 = updateCustomerDto.BillingAddress.StreetLine1;
                existingCustomer.BillingAddress.StreetLine2 = updateCustomerDto.BillingAddress.StreetLine2;
                existingCustomer.BillingAddress.City = updateCustomerDto.BillingAddress.City;
                existingCustomer.BillingAddress.State = updateCustomerDto.BillingAddress.State;
                existingCustomer.BillingAddress.PostalCode = updateCustomerDto.BillingAddress.PostalCode;
                existingCustomer.BillingAddress.Country = updateCustomerDto.BillingAddress.Country;
            }

            // Update shipping address
            if (updateCustomerDto.ShippingAddress != null)
            {
                if (existingCustomer.ShippingAddress == null)
                {
                    existingCustomer.ShippingAddress = new Address();
                }
                existingCustomer.ShippingAddress.StreetLine1 = updateCustomerDto.ShippingAddress.StreetLine1;
                existingCustomer.ShippingAddress.StreetLine2 = updateCustomerDto.ShippingAddress.StreetLine2;
                existingCustomer.ShippingAddress.City = updateCustomerDto.ShippingAddress.City;
                existingCustomer.ShippingAddress.State = updateCustomerDto.ShippingAddress.State;
                existingCustomer.ShippingAddress.PostalCode = updateCustomerDto.ShippingAddress.PostalCode;
                existingCustomer.ShippingAddress.Country = updateCustomerDto.ShippingAddress.Country;
            }

            var updatedCustomer = await _customerService.UpdateCustomerAsync(id, existingCustomer);
            if (updatedCustomer == null)
                return NotFound();

            return Ok(updatedCustomer);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteCustomer(Guid id)
        {
            var result = await _customerService.DeleteCustomerAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpPatch("{id}/loyalty-points")]
        [Authorize(Roles = "admin,employee")]
        public async Task<IActionResult> UpdateLoyaltyPoints(Guid id, [FromBody] int points)
        {
            var customer = await _customerService.UpdateCustomerLoyaltyPointsAsync(id, points);
            if (customer == null)
                return NotFound();

            return Ok(customer);
        }
    }
}