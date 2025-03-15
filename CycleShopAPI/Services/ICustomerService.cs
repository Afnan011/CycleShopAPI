using CycleShopAPI.Models;

namespace CycleShopAPI.Services
{
    public interface ICustomerService
    {
        Task<IEnumerable<Customer>> GetAllCustomersAsync();
        Task<Customer> GetCustomerByIdAsync(Guid customerId);
        Task<Customer> CreateCustomerAsync(Customer customer);
        Task<Customer> UpdateCustomerAsync(Guid customerId, Customer customer);
        Task<bool> DeleteCustomerAsync(Guid customerId);
        Task<Customer> UpdateCustomerLoyaltyPointsAsync(Guid customerId, int points);
    }
}