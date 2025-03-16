using Microsoft.AspNetCore.Http;

namespace CycleShopAPI.Models.DTOs
{
    public class UpdateCycleDTO
    {
        public string SKU { get; set; }
        public string ModelName { get; set; }
        public Guid? BrandId { get; set; }
        public Guid? TypeId { get; set; }
        public string Description { get; set; }
        public decimal? Price { get; set; }
        public decimal? CostPrice { get; set; }
        public bool? IsActive { get; set; }
        public string ImageUrl { get; set; }
    }
}