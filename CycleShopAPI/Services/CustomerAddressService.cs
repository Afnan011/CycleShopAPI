using CycleShopAPI.Data;
using CycleShopAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CycleShopAPI.Services
{
    public class CustomerAddressService : ICustomerAddressService
    {
        private readonly CycleShopContext _context;

        public CustomerAddressService(CycleShopContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CustomerAddress>> GetCustomerAddressesAsync(Guid customerId)
        {
            return await _context.CustomerAddresses
                .Include(ca => ca.Address)
                .Where(ca => ca.CustomerId == customerId)
                .ToListAsync();
        }

        public async Task<CustomerAddress> GetCustomerAddressByIdAsync(Guid customerAddressId)
        {
            return await _context.CustomerAddresses
                .Include(ca => ca.Address)
                .FirstOrDefaultAsync(ca => ca.CustomerAddressId == customerAddressId);
        }

        public async Task<CustomerAddress> AddCustomerAddressAsync(Guid customerId, Address address, bool isDefault = false, string addressType = "Shipping")
        {
            // Check if customer exists
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
            {
                return null;
            }

            // Create and save address
            address.CreatedAt = DateTime.UtcNow;
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            // If this is set as default and there are other addresses, unset default flag on others
            if (isDefault)
            {
                var existingDefaultAddresses = await _context.CustomerAddresses
                    .Where(ca => ca.CustomerId == customerId && ca.IsDefault && ca.AddressType == addressType)
                    .ToListAsync();

                foreach (var existingDefault in existingDefaultAddresses)
                {
                    existingDefault.IsDefault = false;
                    _context.CustomerAddresses.Update(existingDefault);
                }
            }

            // Create the customer address relationship
            var customerAddress = new CustomerAddress
            {
                CustomerId = customerId,
                AddressId = address.AddressId,
                IsDefault = isDefault,
                AddressType = addressType,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.CustomerAddresses.Add(customerAddress);
            await _context.SaveChangesAsync();

            // Load the address into the relationship
            await _context.Entry(customerAddress).Reference(ca => ca.Address).LoadAsync();

            return customerAddress;
        }

        public async Task<CustomerAddress> UpdateCustomerAddressAsync(Guid customerAddressId, Address address)
        {
            var customerAddress = await _context.CustomerAddresses
                .Include(ca => ca.Address)
                .FirstOrDefaultAsync(ca => ca.CustomerAddressId == customerAddressId);

            if (customerAddress == null)
            {
                return null;
            }

            // Update the address fields
            customerAddress.Address.StreetLine1 = address.StreetLine1;
            customerAddress.Address.StreetLine2 = address.StreetLine2;
            customerAddress.Address.City = address.City;
            customerAddress.Address.State = address.State;
            customerAddress.Address.PostalCode = address.PostalCode;
            customerAddress.Address.Country = address.Country;
            customerAddress.UpdatedAt = DateTime.UtcNow;

            _context.CustomerAddresses.Update(customerAddress);
            await _context.SaveChangesAsync();

            return customerAddress;
        }

        public async Task<bool> DeleteCustomerAddressAsync(Guid customerAddressId)
        {
            var customerAddress = await _context.CustomerAddresses.FindAsync(customerAddressId);
            if (customerAddress == null)
            {
                return false;
            }

            _context.CustomerAddresses.Remove(customerAddress);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> SetDefaultAddressAsync(Guid customerId, Guid customerAddressId)
        {
            // Find the address to set as default
            var customerAddress = await _context.CustomerAddresses
                .FirstOrDefaultAsync(ca => ca.CustomerAddressId == customerAddressId && ca.CustomerId == customerId);

            if (customerAddress == null)
            {
                return false;
            }

            // Find any existing default addresses and unset them
            var existingDefaultAddresses = await _context.CustomerAddresses
                .Where(ca => ca.CustomerId == customerId && 
                       ca.IsDefault && 
                       ca.AddressType == customerAddress.AddressType && 
                       ca.CustomerAddressId != customerAddressId)
                .ToListAsync();

            foreach (var existingDefault in existingDefaultAddresses)
            {
                existingDefault.IsDefault = false;
                _context.CustomerAddresses.Update(existingDefault);
            }

            // Set the new default
            customerAddress.IsDefault = true;
            customerAddress.UpdatedAt = DateTime.UtcNow;
            _context.CustomerAddresses.Update(customerAddress);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Address>> GetAddressesByCustomerIdAsync(Guid customerId)
        {
            var customerAddresses = await _context.CustomerAddresses
                .Include(ca => ca.Address)
                .Where(ca => ca.CustomerId == customerId)
                .ToListAsync();

            return customerAddresses.Select(ca => ca.Address).ToList();
        }
    }
}
