using CycleShopAPI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CycleShopAPI.Services
{
    public interface ICustomerAddressService
    {
        Task<IEnumerable<CustomerAddress>> GetCustomerAddressesAsync(Guid customerId);
        Task<CustomerAddress> GetCustomerAddressByIdAsync(Guid customerAddressId);
        Task<CustomerAddress> AddCustomerAddressAsync(Guid customerId, Address address, bool isDefault = false, string addressType = "Shipping");
        Task<CustomerAddress> UpdateCustomerAddressAsync(Guid customerAddressId, Address address);
        Task<bool> DeleteCustomerAddressAsync(Guid customerAddressId);
        Task<bool> SetDefaultAddressAsync(Guid customerId, Guid customerAddressId);
        Task<IEnumerable<Address>> GetAddressesByCustomerIdAsync(Guid customerId);
    }
}
