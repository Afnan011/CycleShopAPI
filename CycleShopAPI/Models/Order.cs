using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CycleShopAPI.Models
{
    public class Order
    {
        public Guid OrderId { get; set; }
        [Required, MaxLength(20)]
        public string OrderNumber { get; set; }
        [ForeignKey("Employee")]
        public Guid EmployeeId { get; set; }
        public User Employee { get; set; }
        [ForeignKey("Customer")]
        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; }
        public Guid? ShippingAddressId { get; set; }
        public Address ShippingAddress { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public OrderStatus Status { get; set; } = OrderStatus.pending;
        [Column(TypeName = "numeric(10,2)")]
        public decimal Subtotal { get; set; }
        [Column(TypeName = "numeric(10,2)")]
        public decimal Tax { get; set; }
        [Column(TypeName = "numeric(10,2)")]
        public decimal Discount { get; set; } = 0;
        [Column(TypeName = "numeric(10,2)")]
        public decimal TotalAmount { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<OrderItem> OrderItems { get; set; }
    }
}
