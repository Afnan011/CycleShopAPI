using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CycleShopAPI.Models
{
    public class CustomerAddress
    {
        public Guid CustomerAddressId { get; set; }
        
        [Required]
        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; }
        
        [Required]
        public Guid AddressId { get; set; }
        public Address Address { get; set; }
        
        public bool IsDefault { get; set; } = false;
        public string AddressType { get; set; } = "Shipping"; // Shipping, Billing, Both
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
