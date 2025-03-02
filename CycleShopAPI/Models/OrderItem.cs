using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CycleShopAPI.Models
{
    public class OrderItem
    {
        public Guid OrderItemId { get; set; }
        [ForeignKey("Order")]
        public Guid OrderId { get; set; }
        public Order Order { get; set; }
        [ForeignKey("Cycle")]
        public Guid CycleId { get; set; }
        public Cycle Cycle { get; set; }
        public int Quantity { get; set; }
        [Column(TypeName = "numeric(10,2)")]
        public decimal UnitPrice { get; set; }
        [Column(TypeName = "numeric(5,2)")]
        public decimal TaxRate { get; set; } = 0;
        [Column(TypeName = "numeric(10,2)")]
        public decimal TotalPrice { get; set; }
    }
}
