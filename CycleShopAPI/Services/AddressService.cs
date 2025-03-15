using CycleShopAPI.Data;
using CycleShopAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CycleShopAPI.Services
{
    public class AddressService : IAddressService
    {
        private readonly CycleShopContext _context;

        public AddressService(CycleShopContext context)
        {
            _context = context;
        }

        public async Task<Address> GetAddressByIdAsync(Guid addressId)
        {
            return await _context.Addresses.FindAsync(addressId);
        }

        public async Task<Address> CreateAddressAsync(Address address)
        {
            address.CreatedAt = DateTime.UtcNow;
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();
            return address;
        }

        public async Task<Address> UpdateAddressAsync(Guid addressId, Address address)
        {
            var existingAddress = await _context.Addresses.FindAsync(addressId);
            if (existingAddress == null)
                return null;

            existingAddress.StreetLine1 = address.StreetLine1;
            existingAddress.StreetLine2 = address.StreetLine2;
            existingAddress.City = address.City;
            existingAddress.State = address.State;
            existingAddress.PostalCode = address.PostalCode;
            existingAddress.Country = address.Country;

            await _context.SaveChangesAsync();
            return existingAddress;
        }

        public async Task<bool> DeleteAddressAsync(Guid addressId)
        {
            var address = await _context.Addresses.FindAsync(addressId);
            if (address == null)
                return false;

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}