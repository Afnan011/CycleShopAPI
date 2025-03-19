using System;
using System.ComponentModel.DataAnnotations;
using CycleShopAPI.Models.DTOs;

namespace CycleShopAPI.Models
{
    public class Address
    {
        public Guid AddressId { get; set; }
        [Required, MaxLength(100)]
        public string StreetLine1 { get; set; }
        [MaxLength(100)]
        public string StreetLine2 { get; set; }
        [Required, MaxLength(50)]
        public string City { get; set; }
        [Required, MaxLength(50)]
        public string State { get; set; }
        [Required, MaxLength(20)]
        public string PostalCode { get; set; }
        [Required, MaxLength(50)]
        public string Country { get; set; } = "India";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
