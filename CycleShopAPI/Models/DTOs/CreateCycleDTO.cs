using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace CycleShopAPI.Models.DTOs
{
    public class CreateCycleDTO
    {
        public string? SKU { get; set; }
        [Required]
        public string ModelName { get; set; } = string.Empty;
        public Guid BrandId { get; set; }
        public Guid TypeId { get; set; }
        [Required]
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal CostPrice { get; set; }
        public bool IsActive { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }
}