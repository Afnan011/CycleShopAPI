using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CycleShopAPI.Models;

namespace CycleShopAPI.Models
{
    public class Customer
    {
        public Guid CustomerId { get; set; }
        [Required, MaxLength(50)]
        public string FirstName { get; set; }
        [Required, MaxLength(50)]
        public string LastName { get; set; }
        [MaxLength(100)]
        public string Email { get; set; }
        [MaxLength(20)]
        public string Phone { get; set; }
        public Guid? BillingAddressId { get; set; }
        public Address BillingAddress { get; set; }
        public Guid? ShippingAddressId { get; set; }
        public Address ShippingAddress { get; set; }
        public int LoyaltyPoints { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }
    }
}
