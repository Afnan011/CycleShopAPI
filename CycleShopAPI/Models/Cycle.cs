using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CycleShopAPI.Models
{
    public class Cycle
    {
        public Guid CycleId { get; set; }
        [Required, MaxLength(50)]
        public string SKU { get; set; }
        [Required, MaxLength(100)]
        public string ModelName { get; set; }
        [ForeignKey("Brand")]
        public Guid BrandId { get; set; }
        public Brand Brand { get; set; }
        [ForeignKey("CycleType")]
        public Guid TypeId { get; set; }
        public CycleType CycleType { get; set; }
        public string Description { get; set; }
        [Column(TypeName = "numeric(10,2)")]
        public decimal Price { get; set; }
        [Column(TypeName = "numeric(10,2)")]
        public decimal? CostPrice { get; set; }
        public int StockQuantity { get; set; }
        public int ReorderThreshold { get; set; } = 5;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }
    }
}
