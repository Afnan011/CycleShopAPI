using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CycleShopAPI.Models
{
    public class OrderItemDTO
    {
        public Guid OrderItemId { get; set; }
        [ForeignKey("Order")]
        public Guid OrderId { get; set; }
        [ForeignKey("Cycle")]
        public Guid CycleId { get; set; }
        public int Quantity { get; set; }
        [Column(TypeName = "numeric(5,2)")]
        public decimal TaxRate { get; set; } = 0;
    }
}