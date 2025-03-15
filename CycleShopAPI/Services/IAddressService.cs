using CycleShopAPI.Models;

namespace CycleShopAPI.Services
{
    public interface IAddressService
    {
        Task<Address> GetAddressByIdAsync(Guid addressId);
        Task<Address> CreateAddressAsync(Address address);
        Task<Address> UpdateAddressAsync(Guid addressId, Address address);
        Task<bool> DeleteAddressAsync(Guid addressId);
    }
}