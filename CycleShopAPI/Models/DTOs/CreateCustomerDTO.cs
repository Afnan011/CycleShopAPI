using System.ComponentModel.DataAnnotations;

namespace CycleShopAPI.Models.DTOs
{
    public class CreateCustomerDTO
    {
        [Required, MaxLength(50)]
        public string FirstName { get; set; }

        [Required, MaxLength(50)]
        public string LastName { get; set; }

        [Required, MaxLength(100), EmailAddress]
        public string Email { get; set; }

        [Required, MaxLength(20)]
        public string Phone { get; set; }

        public AddressDTO BillingAddress { get; set; }

        public AddressDTO ShippingAddress { get; set; }
    }
}