using System.ComponentModel.DataAnnotations;

namespace CycleShopAPI.Models.DTOs
{
    public class UpdateAddressDTO
    {
        public string? StreetLine1 { get; set; }

        public string? StreetLine2 { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        public string? PostalCode { get; set; }

        public string? Country { get; set; } = "India";
    }
}