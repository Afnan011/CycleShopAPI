using System.ComponentModel.DataAnnotations;

namespace CycleShopAPI.Models.DTOs
{
    public class UpdateCustomerDTO
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public UpdateAddressDTO? BillingAddress { get; set; }

        public UpdateAddressDTO? ShippingAddress { get; set; }
    }
}