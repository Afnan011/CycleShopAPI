using CycleShopAPI.Data;
using CycleShopAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CycleShopAPI.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly CycleShopContext _context;

        public CustomerService(CycleShopContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
        {
            return await _context.Customers
                .Include(c => c.BillingAddress)
                .Include(c => c.ShippingAddress)
                .Where(c => !c.DeletedAt.HasValue)
                .ToListAsync();
        }

        public async Task<Customer> GetCustomerByIdAsync(Guid customerId)
        {
            return await _context.Customers
                .Include(c => c.BillingAddress)
                .Include(c => c.ShippingAddress)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId && !c.DeletedAt.HasValue);
        }

        public async Task<Customer> CreateCustomerAsync(Customer customer)
        {
            customer.CreatedAt = DateTime.UtcNow;
            customer.UpdatedAt = DateTime.UtcNow;

            if (customer.BillingAddress != null)
            {
                customer.BillingAddress.CreatedAt = DateTime.UtcNow;
                _context.Addresses.Add(customer.BillingAddress);
            }

            if (customer.ShippingAddress != null)
            {
                customer.ShippingAddress.CreatedAt = DateTime.UtcNow;
                _context.Addresses.Add(customer.ShippingAddress);
            }

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task<Customer> UpdateCustomerAsync(Guid customerId, Customer customer)
        {
            var existingCustomer = await _context.Customers
                .Include(c => c.BillingAddress)
                .Include(c => c.ShippingAddress)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId && !c.DeletedAt.HasValue);

            if (existingCustomer == null)
                return null;

            existingCustomer.FirstName = customer.FirstName;
            existingCustomer.LastName = customer.LastName;
            existingCustomer.Email = customer.Email;
            existingCustomer.Phone = customer.Phone;
            existingCustomer.UpdatedAt = DateTime.UtcNow;

            // Update billing address
            if (customer.BillingAddress != null)
            {
                if (existingCustomer.BillingAddress == null)
                {
                    customer.BillingAddress.CreatedAt = DateTime.UtcNow;
                    _context.Addresses.Add(customer.BillingAddress);
                    existingCustomer.BillingAddress = customer.BillingAddress;
                }
                else
                {
                    existingCustomer.BillingAddress.StreetLine1 = customer.BillingAddress.StreetLine1;
                    existingCustomer.BillingAddress.StreetLine2 = customer.BillingAddress.StreetLine2;
                    existingCustomer.BillingAddress.City = customer.BillingAddress.City;
                    existingCustomer.BillingAddress.State = customer.BillingAddress.State;
                    existingCustomer.BillingAddress.PostalCode = customer.BillingAddress.PostalCode;
                    existingCustomer.BillingAddress.Country = customer.BillingAddress.Country;
                }
            }

            // Update shipping address
            if (customer.ShippingAddress != null)
            {
                if (existingCustomer.ShippingAddress == null)
                {
                    customer.ShippingAddress.CreatedAt = DateTime.UtcNow;
                    _context.Addresses.Add(customer.ShippingAddress);
                    existingCustomer.ShippingAddress = customer.ShippingAddress;
                }
                else
                {
                    existingCustomer.ShippingAddress.StreetLine1 = customer.ShippingAddress.StreetLine1;
                    existingCustomer.ShippingAddress.StreetLine2 = customer.ShippingAddress.StreetLine2;
                    existingCustomer.ShippingAddress.City = customer.ShippingAddress.City;
                    existingCustomer.ShippingAddress.State = customer.ShippingAddress.State;
                    existingCustomer.ShippingAddress.PostalCode = customer.ShippingAddress.PostalCode;
                    existingCustomer.ShippingAddress.Country = customer.ShippingAddress.Country;
                }
            }

            await _context.SaveChangesAsync();
            return existingCustomer;
        }

        public async Task<bool> DeleteCustomerAsync(Guid customerId)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null || customer.DeletedAt.HasValue)
                return false;

            customer.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Customer> UpdateCustomerLoyaltyPointsAsync(Guid customerId, int points)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null || customer.DeletedAt.HasValue)
                return null;

            customer.LoyaltyPoints += points;
            customer.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return customer;
        }
    }
}